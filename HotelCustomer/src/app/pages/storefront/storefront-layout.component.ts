import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter, map, startWith, Subscription } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { GuestRoomAvailabilityService } from '../../core/services/guest-room-availability.service';
import { hotelThemeStyle } from '../../core/utils/hotel-theme';
import { BookingModalComponent } from '../../shared/hotel-storefront/booking-modal.component';
import { HotelNavComponent } from '../../shared/hotel-storefront/hotel-nav.component';
import { ShowcaseGalleryModalComponent } from '../../shared/hotel-storefront/showcase-gallery-modal.component';

@Component({
  selector: 'app-storefront-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, HotelNavComponent, BookingModalComponent, ShowcaseGalleryModalComponent],
  templateUrl: './storefront-layout.component.html',
  styleUrl: './storefront-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontLayoutComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly ctx = inject(StorefrontContextService);
  private readonly availability = inject(GuestRoomAvailabilityService);

  private sub?: Subscription;

  readonly navSolid = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => {
        const url = this.router.url;
        return url.includes('/rooms') || url.includes('/facilities') || url.includes('/about') || url.includes('/restaurant');
      }),
      startWith(
        this.router.url.includes('/rooms') ||
          this.router.url.includes('/facilities') ||
          this.router.url.includes('/about') ||
          this.router.url.includes('/restaurant'),
      ),
    ),
    { initialValue: false },
  );

  readonly themeStyle = computed(() => {
    const sf = this.ctx.storefront();
    return sf ? hotelThemeStyle(sf.theme) : {};
  });

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe((params) => {
      const slug = this.route.parent?.snapshot.paramMap.get('slug') ?? params.get('slug') ?? '';
      const locationId = params.get('locationId');
      if (slug) {
        this.ctx.load(slug, locationId).subscribe((sf) => {
          if (sf?.activeLocationId) {
            this.availability.ensureDefaultDates();
            this.availability.refresh(sf.slug, sf.activeLocationId);
          }
        });
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.availability.reset();
    this.ctx.reset();
  }
}
