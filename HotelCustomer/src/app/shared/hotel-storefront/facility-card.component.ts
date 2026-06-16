import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import type { ShowcaseFacility } from '../../core/models/hotel-showcase.models';

@Component({
  selector: 'app-facility-card',
  standalone: true,
  templateUrl: './facility-card.component.html',
  styleUrl: './facility-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FacilityCardComponent {
  readonly facility = input.required<ShowcaseFacility>();
  readonly dark = input(false);
  readonly variant = input<'grid' | 'list'>('grid');
}
