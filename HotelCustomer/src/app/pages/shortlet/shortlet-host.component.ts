import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ShortletContextService } from '../../core/services/shortlet-context.service';

const MOCK_REVIEWS = [
  { name: 'Chioma A.', date: 'March 2026', text: 'Felt like a real home. Kitchen was fully stocked and Wi‑Fi was fast enough for video calls all day.' },
  { name: 'James O.', date: 'February 2026', text: 'Host was incredibly responsive. Check-in was seamless and the apartment was spotless.' },
  { name: 'Sarah M.', date: 'January 2026', text: 'Perfect for a two-week work trip. Quiet building, great location, would book again.' },
];

@Component({
  selector: 'app-shortlet-host',
  standalone: true,
  templateUrl: './shortlet-host.component.html',
  styleUrl: './shortlet-host.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletHostComponent {
  readonly ctx = inject(ShortletContextService);

  readonly shortlet = computed(() => this.ctx.shortlet()!);
  readonly reviews = MOCK_REVIEWS;
}
