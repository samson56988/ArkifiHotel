import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter, map, startWith, Subscription } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { hotelThemeStyle } from '../../core/utils/hotel-theme';
import { BookingModalComponent } from '../../shared/hotel-storefront/booking-modal.component';
import { HotelNavComponent } from '../../shared/hotel-storefront/hotel-nav.component';

@Component({
  selector: 'app-storefront-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, HotelNavComponent, BookingModalComponent],
  templateUrl: './storefront-layout.component.html',
  styleUrl: './storefront-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontLayoutComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly ctx = inject(StorefrontContextService);

  private sub?: Subscription;

  readonly navSolid = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => {
        const url = this.router.url;
        return url.includes('/rooms') || url.includes('/facilities');
      }),
      startWith(this.router.url.includes('/rooms') || this.router.url.includes('/facilities')),
    ),
    { initialValue: false },
  );

  readonly themeStyle = computed(() => {
    const sf = this.ctx.storefront();
    return sf ? hotelThemeStyle(sf.theme) : {};
  });

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug') ?? '';
      if (slug) {
        this.ctx.load(slug).subscribe();
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.ctx.reset();
  }
}
