import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { resolveSectionFont } from '../../core/data/storefront-theme-presets';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { AboutSectionComponent } from '../../shared/hotel-storefront/about-section.component';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';

@Component({
  selector: 'app-storefront-about',
  standalone: true,
  imports: [RouterLink, AboutSectionComponent, HotelFooterComponent],
  templateUrl: './storefront-about.component.html',
  styleUrl: './storefront-about.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontAboutComponent {
  readonly ctx = inject(StorefrontContextService);

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly heroImage = computed(() => {
    const sf = this.storefront();
    return sf.aboutImageUrl ?? sf.heroImages[0] ?? null;
  });

  readonly pageTitleFont = computed(() =>
    resolveSectionFont(this.storefront().theme.globalFont, this.storefront().theme.about.titleFont),
  );
}
