import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
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
}
