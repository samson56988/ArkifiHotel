import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { GuestRoomAvailabilityService } from '../../core/services/guest-room-availability.service';
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
  readonly availability = inject(GuestRoomAvailabilityService);

  readonly room = input.required<ShowcaseRoom>();
  readonly storefront = input.required<HotelShowcase>();

  readonly canBook = computed(() => this.availability.isRoomAvailable(this.room().id));
  readonly availLabel = computed(() => this.availability.availabilityLabel(this.room().id));

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  bookRoom(): void {
    if (this.canBook()) {
      this.ui.openBooking(this.room());
    }
  }
}
