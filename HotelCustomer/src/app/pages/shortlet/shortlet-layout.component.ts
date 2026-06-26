import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter, map, startWith, Subscription } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { GuestRoomAvailabilityService } from '../../core/services/guest-room-availability.service';
import { shortletThemeStyle } from '../../core/utils/shortlet-theme';
import { BookingModalComponent } from '../../shared/hotel-storefront/booking-modal.component';
import { ShowcaseGalleryModalComponent } from '../../shared/hotel-storefront/showcase-gallery-modal.component';
import { ShortletNavComponent } from '../../shared/shortlet-storefront/shortlet-nav.component';
import { ShortletFooterComponent } from '../../shared/shortlet-storefront/shortlet-footer.component';

@Component({
  selector: 'app-shortlet-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    ShortletNavComponent,
    ShortletFooterComponent,
    BookingModalComponent,
    ShowcaseGalleryModalComponent,
  ],
  templateUrl: './shortlet-layout.component.html',
  styleUrl: './shortlet-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletLayoutComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly ctx = inject(ShortletContextService);
  private readonly availability = inject(GuestRoomAvailabilityService);

  private sub?: Subscription;

  readonly navTransparent = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => this.router.url.split('?')[0].endsWith(this.homeSuffix())),
      startWith(this.router.url.split('?')[0].endsWith(this.homeSuffix())),
    ),
    { initialValue: true },
  );

  readonly themeStyle = computed(() => {
    const sl = this.ctx.shortlet();
    return sl ? shortletThemeStyle(sl.theme) : {};
  });

  readonly hotelForBooking = computed(() => this.ctx.asHotelShowcase());

  private homeSuffix(): string {
    const sl = this.ctx.shortlet();
    const loc = this.ctx.locationRouteId() ?? sl?.activeLocationId;
    if (sl && loc) {
      return `/${sl.slug}/l/${loc}`;
    }
    return '';
  }

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe(() => {
      const slug = this.resolveSlug();
      const locationId = this.route.snapshot.paramMap.get('locationId');
      if (slug) {
        this.ctx.load(slug, locationId).subscribe((sl) => {
          if (sl?.activeLocationId) {
            this.availability.ensureDefaultDates();
            this.availability.refresh(sl.slug, sl.activeLocationId);
          }
        });
      }
    });
  }

  private resolveSlug(): string {
    return (
      this.route.parent?.snapshot.paramMap.get('slug') ??
      this.route.parent?.snapshot.url[0]?.path ??
      ''
    );
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.availability.reset();
    this.ctx.reset();
  }
}
