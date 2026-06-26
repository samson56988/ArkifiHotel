import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { formatNaira } from '../../core/utils/shortlet-theme';
import type { ShortletListing } from '../../core/models/shortlet-showcase.models';
import { listingToShowcaseRoom } from '../../core/utils/shortlet-adapter';

@Component({
  selector: 'app-shortlet-listing-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './shortlet-listing-card.component.html',
  styleUrl: './shortlet-listing-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletListingCardComponent {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(ShortletContextService);

  readonly listing = input.required<ShortletListing>();
  readonly compact = input(false);

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  openGallery(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.ui.openRoomGallery(listingToShowcaseRoom(this.listing()));
  }

  book(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.ui.openBooking(listingToShowcaseRoom(this.listing()));
  }
}
