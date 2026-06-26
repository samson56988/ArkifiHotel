import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter, map, startWith, Subscription } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { StorefrontEntryService } from '../../core/services/storefront-entry.service';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { hotelThemeStyle } from '../../core/utils/hotel-theme';
import { shortletThemeStyle } from '../../core/utils/shortlet-theme';
import { BookingModalComponent } from '../../shared/hotel-storefront/booking-modal.component';
import { HotelNavComponent } from '../../shared/hotel-storefront/hotel-nav.component';
import { ShowcaseGalleryModalComponent } from '../../shared/hotel-storefront/showcase-gallery-modal.component';
import { ShortletNavComponent } from '../../shared/shortlet-storefront/shortlet-nav.component';
import { ShortletFooterComponent } from '../../shared/shortlet-storefront/shortlet-footer.component';

@Component({
  selector: 'app-storefront-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    HotelNavComponent,
    ShortletNavComponent,
    ShortletFooterComponent,
    BookingModalComponent,
    ShowcaseGalleryModalComponent,
  ],
  templateUrl: './storefront-shell.component.html',
  styleUrl: './storefront-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontShellComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly entry = inject(StorefrontEntryService);
  readonly hotelCtx = inject(StorefrontContextService);
  readonly shortletCtx = inject(ShortletContextService);

  private sub?: Subscription;

  readonly navSolid = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => {
        const url = this.router.url;
        return (
          url.includes('/rooms') ||
          url.includes('/facilities') ||
          url.includes('/about') ||
          url.includes('/restaurant') ||
          url.includes('/event-halls') ||
          url.includes('/listings') ||
          url.includes('/amenities') ||
          url.includes('/host')
        );
      }),
      startWith(this.router.url.includes('/rooms') || this.router.url.includes('/listings')),
    ),
    { initialValue: false },
  );

  readonly shortletNavTransparent = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map(() => this.isShortletHome()),
      startWith(this.isShortletHome()),
    ),
    { initialValue: true },
  );

  readonly hotelThemeStyle = computed(() => {
    const sf = this.hotelCtx.storefront();
    return sf ? hotelThemeStyle(sf.theme) : {};
  });

  readonly shortletThemeStyle = computed(() => {
    const sl = this.shortletCtx.shortlet();
    return sl ? shortletThemeStyle(sl.theme) : {};
  });

  readonly hotelForBooking = computed(() => {
    if (this.entry.kind() === 'shortlet') {
      return this.shortletCtx.asHotelShowcase();
    }
    return this.hotelCtx.storefront();
  });

  ngOnInit(): void {
    this.sub = this.route.paramMap.subscribe(() => {
      const slug = this.route.parent?.snapshot.paramMap.get('slug') ?? '';
      const locationId = this.route.snapshot.paramMap.get('locationId');
      if (slug) {
        this.entry.load(slug, locationId).subscribe();
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.entry.reset();
  }

  private isShortletHome(): boolean {
    const url = this.router.url.split('?')[0];
    const slug = this.entry.slug();
    const loc = this.entry.activeLocationId();
    if (!slug || !loc) {
      return false;
    }
    return url === `/${slug}/l/${loc}` || url === `/${slug}/l/${loc}/`;
  }
}
