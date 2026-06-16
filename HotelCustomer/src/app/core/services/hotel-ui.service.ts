import { Injectable, signal } from '@angular/core';
import type { ShowcaseFacility, ShowcaseRoom } from '../models/hotel-showcase.models';

export type GalleryKind = 'room' | 'facility';

@Injectable({ providedIn: 'root' })
export class HotelUiService {
  readonly bookingOpen = signal(false);
  readonly selectedRoom = signal<ShowcaseRoom | null>(null);
  readonly toastMessage = signal<string | null>(null);

  readonly galleryOpen = signal(false);
  readonly galleryKind = signal<GalleryKind>('room');
  readonly galleryRoom = signal<ShowcaseRoom | null>(null);
  readonly galleryFacility = signal<ShowcaseFacility | null>(null);
  readonly gallerySlideIndex = signal(0);

  openBooking(room?: ShowcaseRoom | null): void {
    this.closeGallery();
    this.selectedRoom.set(room ?? null);
    this.bookingOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeBooking(): void {
    this.bookingOpen.set(false);
    this.selectedRoom.set(null);
    if (!this.galleryOpen()) {
      document.body.style.overflow = '';
    }
  }

  openRoomGallery(room: ShowcaseRoom, slideIndex = 0): void {
    this.galleryKind.set('room');
    this.galleryRoom.set(room);
    this.galleryFacility.set(null);
    this.gallerySlideIndex.set(slideIndex);
    this.galleryOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  openFacilityGallery(facility: ShowcaseFacility, slideIndex = 0): void {
    this.galleryKind.set('facility');
    this.galleryFacility.set(facility);
    this.galleryRoom.set(null);
    this.gallerySlideIndex.set(slideIndex);
    this.galleryOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeGallery(): void {
    this.galleryOpen.set(false);
    this.galleryRoom.set(null);
    this.galleryFacility.set(null);
    this.gallerySlideIndex.set(0);
    if (!this.bookingOpen()) {
      document.body.style.overflow = '';
    }
  }

  nextGallerySlide(total: number): void {
    if (total <= 1) {
      return;
    }
    this.gallerySlideIndex.update((i) => (i + 1) % total);
  }

  prevGallerySlide(total: number): void {
    if (total <= 1) {
      return;
    }
    this.gallerySlideIndex.update((i) => (i - 1 + total) % total);
  }

  goToGallerySlide(index: number): void {
    this.gallerySlideIndex.set(index);
  }

  showToast(message: string): void {
    this.toastMessage.set(message);
    window.setTimeout(() => {
      if (this.toastMessage() === message) {
        this.toastMessage.set(null);
      }
    }, 4000);
  }
}
