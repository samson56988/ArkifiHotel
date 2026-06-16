import { Injectable, signal } from '@angular/core';
import type { ShowcaseRoom } from '../models/hotel-showcase.models';

@Injectable({ providedIn: 'root' })
export class HotelUiService {
  readonly bookingOpen = signal(false);
  readonly selectedRoom = signal<ShowcaseRoom | null>(null);
  readonly toastMessage = signal<string | null>(null);

  openBooking(room?: ShowcaseRoom | null): void {
    this.selectedRoom.set(room ?? null);
    this.bookingOpen.set(true);
    document.body.style.overflow = 'hidden';
  }

  closeBooking(): void {
    this.bookingOpen.set(false);
    this.selectedRoom.set(null);
    document.body.style.overflow = '';
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
