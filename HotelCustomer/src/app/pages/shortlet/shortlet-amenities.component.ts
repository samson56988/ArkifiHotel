import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import type { ShortletAmenity } from '../../core/models/shortlet-showcase.models';

@Component({
  selector: 'app-shortlet-amenities',
  standalone: true,
  templateUrl: './shortlet-amenities.component.html',
  styleUrl: './shortlet-amenities.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletAmenitiesComponent {
  readonly ctx = inject(ShortletContextService);

  readonly shortlet = computed(() => this.ctx.shortlet()!);

  readonly grouped = computed(() => {
    const map = new Map<string, ShortletAmenity[]>();
    for (const a of this.shortlet().amenities) {
      const list = map.get(a.category) ?? [];
      list.push(a);
      map.set(a.category, list);
    }
    return [...map.entries()];
  });
}
