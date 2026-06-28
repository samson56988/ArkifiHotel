import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';
import type { ShortletAmenity } from '../../core/models/shortlet-showcase.models';
import {
  mapPublicToShortletPreview,
  shortletPreviewThemeStyle,
  type ShortletPreviewPage,
} from '../../core/utils/shortlet-preview.mapper';
import {
  shortletAmenitiesPageSubtitle,
  shortletHeroSubtitle,
  shortletListingsSubtitle,
} from '../../core/utils/shortlet-theme';
import { ShortletPreviewNavComponent } from '../shortlet-storefront/shortlet-preview-nav.component';
import { ShortletPreviewListingCardComponent } from '../shortlet-storefront/shortlet-preview-listing-card.component';
import { ShortletPreviewHostCardComponent } from '../shortlet-storefront/shortlet-preview-host-card.component';

type BedFilter = 'all' | '1' | '2' | '3+';

const MOCK_REVIEWS = [
  { name: 'Chioma A.', date: 'March 2026', text: 'Felt like a real home. Kitchen was fully stocked and Wi‑Fi was fast enough for video calls all day.' },
  { name: 'James O.', date: 'February 2026', text: 'Host was incredibly responsive. Check-in was seamless and the apartment was spotless.' },
  { name: 'Sarah M.', date: 'January 2026', text: 'Perfect for a two-week work trip. Quiet building, great location, would book again.' },
];

@Component({
  selector: 'app-shortlet-guest-preview',
  standalone: true,
  imports: [
    ShortletPreviewNavComponent,
    ShortletPreviewListingCardComponent,
    ShortletPreviewHostCardComponent,
  ],
  templateUrl: './shortlet-guest-preview.component.html',
  styleUrl: './shortlet-guest-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletGuestPreviewComponent {
  readonly storefront = input.required<PublicStorefront>();
  readonly page = input<ShortletPreviewPage>('home');

  readonly shortlet = computed(() => mapPublicToShortletPreview(this.storefront()));
  readonly themeStyle = computed(() => shortletPreviewThemeStyle(this.storefront().theme));
  readonly theme = computed(() => this.shortlet().theme);
  readonly heroSubtitle = computed(() => shortletHeroSubtitle(this.shortlet()));
  readonly listingsSubtitle = computed(() => shortletListingsSubtitle(this.shortlet()));
  readonly amenitiesPageSubtitle = computed(() => shortletAmenitiesPageSubtitle(this.shortlet()));

  readonly featured = computed(() => {
    const fav = this.shortlet().listings.filter((l) => l.featured);
    const pool = fav.length ? fav : this.shortlet().listings;
    return pool.slice(0, 3);
  });

  readonly topAmenities = computed(() => this.shortlet().amenities.slice(0, 8));

  readonly groupedAmenities = computed(() => {
    const map = new Map<string, ShortletAmenity[]>();
    for (const a of this.shortlet().amenities) {
      const list = map.get(a.category) ?? [];
      list.push(a);
      map.set(a.category, list);
    }
    return [...map.entries()];
  });

  readonly bedFilter = signal<BedFilter>('all');

  readonly filteredListings = computed(() => {
    const all = this.shortlet().listings;
    const f = this.bedFilter();
    if (f === 'all') return all;
    if (f === '3+') return all.filter((l) => l.beds >= 3);
    return all.filter((l) => l.beds === Number(f));
  });

  readonly reviews = MOCK_REVIEWS;

  setBedFilter(value: BedFilter): void {
    this.bedFilter.set(value);
  }
}
