import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatNaira } from '../../core/utils/hotel-theme';
import type { PublicStorefront, PublicStorefrontRoom } from '../../core/models/storefront-theme.models';

@Component({
  selector: 'app-room-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './room-card.component.html',
  styleUrl: './room-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoomCardComponent {
  readonly room = input.required<PublicStorefrontRoom>();
  readonly storefront = input.required<PublicStorefront>();
  readonly showPrice = input(true);

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }
}
