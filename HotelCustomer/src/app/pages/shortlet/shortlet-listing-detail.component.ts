import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { ShortletContextService } from '../../core/services/shortlet-context.service';
import { formatNaira } from '../../core/utils/shortlet-theme';
import { listingToShowcaseRoom } from '../../core/utils/shortlet-adapter';
import { ShortletHostCardComponent } from '../../shared/shortlet-storefront/shortlet-host-card.component';

@Component({
  selector: 'app-shortlet-listing-detail',
  standalone: true,
  imports: [RouterLink, ShortletHostCardComponent],
  templateUrl: './shortlet-listing-detail.component.html',
  styleUrl: './shortlet-listing-detail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShortletListingDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(ShortletContextService);

  readonly listingId = signal('');
  readonly activeImage = signal(0);

  readonly listing = computed(() => this.ctx.findListing(this.listingId()));
  readonly shortlet = computed(() => this.ctx.shortlet());

  ngOnInit(): void {
    this.route.paramMap.subscribe((p) => {
      this.listingId.set(p.get('listingId') ?? '');
      this.activeImage.set(0);
    });
  }

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  selectImage(index: number): void {
    this.activeImage.set(index);
  }

  openGallery(index = 0): void {
    const l = this.listing();
    if (l) {
      this.ui.openRoomGallery(listingToShowcaseRoom(l), index);
    }
  }

  book(): void {
    const l = this.listing();
    if (l) {
      this.ui.openBooking(listingToShowcaseRoom(l));
    }
  }
}
