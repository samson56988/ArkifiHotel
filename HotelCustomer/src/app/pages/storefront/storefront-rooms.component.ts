import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import { collectGalleryImages } from '../../core/utils/gallery-images';
import { HotelFooterComponent } from '../../shared/hotel-storefront/hotel-footer.component';
import { RoomCardComponent } from '../../shared/hotel-storefront/room-card.component';

@Component({
  selector: 'app-storefront-rooms',
  standalone: true,
  imports: [RouterLink, RoomCardComponent, HotelFooterComponent],
  templateUrl: './storefront-rooms.component.html',
  styleUrl: './storefront-rooms.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StorefrontRoomsComponent implements OnInit, OnDestroy {
  private readonly ui = inject(HotelUiService);
  readonly ctx = inject(StorefrontContextService);

  readonly heroIndex = signal(0);
  private heroTimer?: ReturnType<typeof setInterval>;

  readonly storefront = computed(() => this.ctx.storefront()!);

  readonly heroSlides = computed(() => {
    const fromRooms = collectGalleryImages(this.storefront().rooms);
    if (fromRooms.length > 0) {
      return fromRooms;
    }
    const sf = this.storefront();
    return sf.heroImages.length > 0 ? sf.heroImages : sf.galleryImages.slice(0, 3);
  });

  readonly allRooms = computed(() => this.storefront().rooms);

  ngOnInit(): void {
    const slides = this.heroSlides();
    if (slides.length > 1) {
      this.heroTimer = setInterval(() => {
        this.heroIndex.update((i) => (i + 1) % slides.length);
      }, 5000);
    }
  }

  ngOnDestroy(): void {
    if (this.heroTimer) {
      clearInterval(this.heroTimer);
    }
  }

  openBooking(): void {
    this.ui.openBooking();
  }
}
