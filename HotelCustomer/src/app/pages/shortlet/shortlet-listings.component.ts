import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { ShortletListingCardComponent } from '../../shared/shortlet-storefront/shortlet-listing-card.component';
import { shortletListingsSubtitle } from '../../core/utils/shortlet-theme';

type BedFilter = 'all' | '1' | '2' | '3+';

@Component({
  selector: 'app-shortlet-listings',
  standalone: true,
  imports: [ShortletListingCardComponent],
  templateUrl: './shortlet-listings.component.html',
  styleUrl: './shortlet-listings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletListingsComponent {
  readonly ctx = inject(ShortletContextService);

  readonly filter = signal<BedFilter>('all');

  readonly shortlet = computed(() => this.ctx.shortlet()!);
  readonly theme = computed(() => this.shortlet().theme);
  readonly listingsSubtitle = computed(() => shortletListingsSubtitle(this.shortlet()));

  readonly filtered = computed(() => {
    const all = this.shortlet().listings;
    const f = this.filter();
    if (f === 'all') return all;
    if (f === '3+') return all.filter((l) => l.beds >= 3);
    return all.filter((l) => l.beds === Number(f));
  });

  setFilter(value: BedFilter): void {
    this.filter.set(value);
  }
}
