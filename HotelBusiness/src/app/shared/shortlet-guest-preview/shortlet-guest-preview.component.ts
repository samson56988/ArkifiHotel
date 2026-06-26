import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';
import {
  formatNaira,
  mapPublicToShortletPreview,
  shortletPreviewThemeStyle,
  type ShortletPreviewPage,
} from '../../core/utils/shortlet-preview.mapper';

@Component({
  selector: 'app-shortlet-guest-preview',
  standalone: true,
  templateUrl: './shortlet-guest-preview.component.html',
  styleUrl: './shortlet-guest-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletGuestPreviewComponent {
  readonly storefront = input.required<PublicStorefront>();
  readonly page = input<ShortletPreviewPage>('home');

  readonly data = computed(() => mapPublicToShortletPreview(this.storefront()));
  readonly themeStyle = computed(() => shortletPreviewThemeStyle(this.storefront().theme));
  readonly formatPrice = formatNaira;

  readonly featured = computed(() => this.data().listings.filter((_, i) => i < 3));
}
