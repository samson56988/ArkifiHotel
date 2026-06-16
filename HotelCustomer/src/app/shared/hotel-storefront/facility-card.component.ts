import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { ShowcaseFacility } from '../../core/models/hotel-showcase.models';

@Component({
  selector: 'app-facility-card',
  standalone: true,
  templateUrl: './facility-card.component.html',
  styleUrl: './facility-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FacilityCardComponent {
  private readonly ui = inject(HotelUiService);

  readonly facility = input.required<ShowcaseFacility>();
  readonly dark = input(false);
  readonly variant = input<'grid' | 'list'>('grid');
  readonly clickable = input(false);

  onCardClick(): void {
    if (this.clickable()) {
      this.ui.openFacilityGallery(this.facility());
    }
  }
}
