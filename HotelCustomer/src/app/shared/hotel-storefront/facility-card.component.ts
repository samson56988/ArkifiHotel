import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { facilityEmoji } from '../../core/utils/hotel-theme';
import type { PublicStorefrontFacility } from '../../core/models/storefront-theme.models';

@Component({
  selector: 'app-facility-card',
  standalone: true,
  templateUrl: './facility-card.component.html',
  styleUrl: './facility-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FacilityCardComponent {
  readonly facility = input.required<PublicStorefrontFacility>();
  readonly dark = input(false);

  emoji(name: string): string {
    return facilityEmoji(name);
  }
}
