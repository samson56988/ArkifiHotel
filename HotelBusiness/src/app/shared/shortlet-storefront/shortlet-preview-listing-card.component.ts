import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { ShortletListing } from '../../core/models/shortlet-showcase.models';
import { formatNaira } from '../../core/utils/shortlet-theme';

const PLACEHOLDER =
  'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800&q=80';

@Component({
  selector: 'app-shortlet-preview-listing-card',
  standalone: true,
  templateUrl: './shortlet-preview-listing-card.component.html',
  styleUrl: './shortlet-preview-listing-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletPreviewListingCardComponent {
  readonly listing = input.required<ShortletListing>();
  readonly compact = input(false);

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  coverImage(listing: ShortletListing): string {
    return listing.images[0] ?? PLACEHOLDER;
  }
}
