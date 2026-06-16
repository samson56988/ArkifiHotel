import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
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

  readonly room = input.required<ShowcaseRoom>();
  readonly storefront = input.required<HotelShowcase>();
  readonly showPrice = input(true);
  readonly detailMode = input(false);
  readonly variant = input<'card' | 'featured'>('card');

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  bookRoom(): void {
    if (this.room().available) {
      this.ui.openBooking(this.room());
    }
  }
}
