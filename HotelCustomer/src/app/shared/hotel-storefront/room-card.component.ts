import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { HotelShowcase, ShowcaseRoom } from '../../core/models/hotel-showcase.models';
import { formatNaira } from '../../core/utils/hotel-theme';

@Component({
  selector: 'app-room-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './room-card.component.html',
  styleUrl: './room-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoomCardComponent {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(StorefrontContextService);

  readonly room = input.required<ShowcaseRoom>();
  readonly storefront = input.required<HotelShowcase>();
  readonly showPrice = input(true);
  readonly detailMode = input(false);
  readonly variant = input<'card' | 'featured'>('card');

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  onCardClick(): void {
    if (this.detailMode() || this.variant() === 'featured') {
      this.ui.openRoomGallery(this.room());
    }
  }

  bookRoom(event: Event): void {
    event.stopPropagation();
    if (this.room().available) {
      this.ui.openBooking(this.room());
    }
  }
}
