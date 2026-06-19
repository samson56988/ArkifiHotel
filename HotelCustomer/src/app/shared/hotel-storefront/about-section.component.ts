import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { resolveSectionFont } from '../../core/data/storefront-theme-presets';
import type { HotelShowcase } from '../../core/models/hotel-showcase.models';

@Component({
  selector: 'app-about-section',
  standalone: true,
  templateUrl: './about-section.component.html',
  styleUrl: './about-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AboutSectionComponent {
  readonly storefront = input.required<HotelShowcase>();
  readonly fullPage = input(false);

  readonly layout = computed(() => this.storefront().theme.about.layout);

  readonly paragraphs = computed(() => {
    const sf = this.storefront();
    if (sf.aboutStory.length > 0) {
      return sf.aboutStory;
    }
    const text = sf.theme.about.description?.trim();
    return text ? [text] : [];
  });

  readonly titleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.titleFont),
  );

  readonly bodyFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.bodyFont),
  );

  readonly quoteText = computed(() => {
    const about = this.storefront().theme.about;
    return about.quote?.trim() || this.storefront().aboutQuote?.trim() || '';
  });

  readonly quoteBy = computed(() => {
    const about = this.storefront().theme.about;
    return about.quoteBy?.trim() || this.storefront().aboutQuoteBy?.trim() || '';
  });

  readonly showStats = computed(
    () => this.storefront().theme.about.showStats && this.storefront().theme.about.stats.length > 0,
  );
}
