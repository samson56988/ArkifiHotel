import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { ShortletHostCardComponent } from '../../shared/shortlet-storefront/shortlet-host-card.component';
import { ShortletListingCardComponent } from '../../shared/shortlet-storefront/shortlet-listing-card.component';

@Component({
  selector: 'app-shortlet-home',
  standalone: true,
  imports: [RouterLink, ShortletListingCardComponent, ShortletHostCardComponent],
  templateUrl: './shortlet-home.component.html',
  styleUrl: './shortlet-home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletHomeComponent {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(ShortletContextService);

  readonly shortlet = computed(() => this.ctx.shortlet()!);
  readonly featured = computed(() => this.shortlet().listings.filter((l) => l.featured).slice(0, 3));
  readonly topAmenities = computed(() => this.shortlet().amenities.slice(0, 8));

  openBooking(): void {
    this.ui.openBooking();
  }
}
