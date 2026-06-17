import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { GuestRoomAvailabilityService } from '../../core/services/guest-room-availability.service';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { HotelShowcase } from '../../core/models/hotel-showcase.models';
import { galleryImages } from '../../core/utils/gallery-images';
import { formatNaira } from '../../core/utils/hotel-theme';

@Component({
  selector: 'app-showcase-gallery-modal',
  standalone: true,
  templateUrl: './showcase-gallery-modal.component.html',
  styleUrl: './showcase-gallery-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShowcaseGalleryModalComponent {
  readonly ui = inject(HotelUiService);
  readonly availability = inject(GuestRoomAvailabilityService);
  readonly storefront = input.required<HotelShowcase>();

  readonly images = computed(() => {
    const kind = this.ui.galleryKind();
    if (kind === 'room') {
      const room = this.ui.galleryRoom();
      return room ? galleryImages(room) : [];
    }
    const facility = this.ui.galleryFacility();
    return facility ? galleryImages(facility) : [];
  });

  readonly title = computed(() => {
    if (this.ui.galleryKind() === 'room') {
      return this.ui.galleryRoom()?.name ?? '';
    }
    return this.ui.galleryFacility()?.name ?? '';
  });

  readonly room = computed(() => this.ui.galleryRoom());
  readonly facility = computed(() => this.ui.galleryFacility());
  readonly showPrice = computed(() => this.storefront().theme.rooms.showPrice);
  readonly canBookRoom = computed(() => {
    const room = this.room();
    return room ? this.availability.isRoomAvailable(room.id) : false;
  });

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  close(): void {
    this.ui.closeGallery();
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close();
    }
  }

  prev(): void {
    this.ui.prevGallerySlide(this.images().length);
  }

  next(): void {
    this.ui.nextGallerySlide(this.images().length);
  }

  goTo(index: number): void {
    this.ui.goToGallerySlide(index);
  }

  bookRoom(): void {
    const room = this.room();
    if (!room || !this.canBookRoom()) {
      return;
    }
    this.close();
    this.ui.openBooking(room);
  }
}
