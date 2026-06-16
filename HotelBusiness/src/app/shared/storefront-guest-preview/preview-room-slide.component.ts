import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { PublicStorefront, PublicStorefrontRoom } from '../../core/models/storefront-theme.models';
import { formatNaira } from '../../core/utils/hotel-theme';

@Component({
  selector: 'app-preview-room-slide',
  standalone: true,
  templateUrl: './preview-room-slide.component.html',
  styleUrl: './preview-room-slide.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PreviewRoomSlideComponent {
  readonly room = input.required<PublicStorefrontRoom>();
  readonly storefront = input.required<PublicStorefront>();

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }
}
