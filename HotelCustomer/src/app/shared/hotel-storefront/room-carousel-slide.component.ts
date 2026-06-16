import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { HotelShowcase, ShowcaseRoom } from '../../core/models/hotel-showcase.models';
import { formatNaira } from '../../core/utils/hotel-theme';

@Component({
  selector: 'app-room-carousel-slide',
  standalone: true,
  templateUrl: './room-carousel-slide.component.html',
  styleUrl: './room-carousel-slide.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoomCarouselSlideComponent {
  private readonly ui = inject(HotelUiService);

  readonly room = input.required<ShowcaseRoom>();
  readonly storefront = input.required<HotelShowcase>();

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  bookRoom(): void {
    if (this.room().available) {
      this.ui.openBooking(this.room());
    }
  }
}
