import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';

@Component({
  selector: 'app-hotel-footer',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './hotel-footer.component.html',
  styleUrl: './hotel-footer.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HotelFooterComponent {
  readonly storefront = input.required<PublicStorefront>();

  readonly year = new Date().getFullYear();

  readonly copyright = computed(() => {
    const sf = this.storefront();
    return sf.theme.footer.copyrightText || `© ${this.year} ${sf.businessName}. All rights reserved.`;
  });
}
