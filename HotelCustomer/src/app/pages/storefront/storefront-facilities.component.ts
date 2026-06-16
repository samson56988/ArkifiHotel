import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import type { ShowcaseFacility } from '../../core/models/hotel-showcase.models';
import { FacilityCardComponent } from '../../shared/hotel-storefront/facility-card.component';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';

@Component({
  selector: 'app-storefront-facilities',
  standalone: true,
  imports: [RouterLink, FacilityCardComponent, HotelFooterComponent],
  templateUrl: './storefront-facilities.component.html',
  styleUrl: './storefront-facilities.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontFacilitiesComponent {
  readonly ctx = inject(StorefrontContextService);

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly heroImage = computed(() => {
    const sf = this.storefront();
    const featured = sf.facilities.find((f) => f.featured && f.primaryImageUrl);
    return featured?.primaryImageUrl ?? sf.galleryImages[1] ?? sf.heroImages[0] ?? null;
  });

  readonly featuredFacility = computed(() => {
    const facilities = this.storefront().facilities;
    return facilities.find((f) => f.featured) ?? facilities.find((f) => f.primaryImageUrl) ?? facilities[0] ?? null;
  });

  readonly categoryGroups = computed(() => {
    const groups = new Map<string, ShowcaseFacility[]>();
    for (const f of this.storefront().facilities) {
      if (f.featured && f.id === this.featuredFacility()?.id) {
        continue;
      }
      const list = groups.get(f.category) ?? [];
      list.push(f);
      groups.set(f.category, list);
    }
    return [...groups.entries()].map(([category, items]) => ({ category, items }));
  });

  readonly facilityCount = computed(() => this.storefront().facilities.length);
}
