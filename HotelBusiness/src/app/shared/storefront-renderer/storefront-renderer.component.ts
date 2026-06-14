import { Component, computed, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { resolveSectionFont, themeCssVariables } from '../../core/data/storefront-theme-presets';
import type { PublicStorefront } from '../../core/models/storefront-theme.models';

@Component({
  selector: 'app-storefront-renderer',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  templateUrl: './storefront-renderer.component.html',
  styleUrl: './storefront-renderer.component.scss',
})
export class StorefrontRendererComponent {
  readonly storefront = input.required<PublicStorefront>();
  readonly compact = input(false);
  /** When true, nav and CTAs scroll; footer shows booking lookup link. */
  readonly interactive = input(false);

  readonly cssVars = computed(() => themeCssVariables(this.storefront().theme));

  readonly bannerHeadlineFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.banner.headlineFont),
  );

  readonly bannerSubFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.banner.subheadlineFont),
  );

  readonly aboutTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.titleFont),
  );

  readonly aboutBodyFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.bodyFont),
  );

  readonly roomsTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.rooms.titleFont),
  );

  readonly facilitiesTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.facilities.titleFont),
  );

  scrollTo(sectionId: string): void {
    if (!this.interactive()) {
      return;
    }

    document.getElementById(sectionId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
