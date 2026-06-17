import { Injectable, inject, signal } from '@angular/core';
import { finalize } from 'rxjs/operators';
import type { GuestRoomAvailabilityDto } from '../models/room-availability.models';
import { PublicGuestBookingApiService } from './public-guest-booking-api.service';
import { defaultGuestStayDates } from '../../core/utils/stay-dates';

@Injectable({ providedIn: 'root' })
export class GuestRoomAvailabilityService {
  private readonly api = inject(PublicGuestBookingApiService);

  readonly checkIn = signal('');
  readonly checkOut = signal('');
  readonly byRoom = signal<Map<string, GuestRoomAvailabilityDto>>(new Map());
  readonly isLoadingAvailability = signal(false);
  readonly hasResult = signal(false);

  private refreshToken = 0;

  ensureDefaultDates(): void {
    if (!this.checkIn() || !this.checkOut()) {
      const dates = defaultGuestStayDates();
      this.checkIn.set(dates.checkIn);
      this.checkOut.set(dates.checkOut);
    }
  }

  reset(): void {
    this.refreshToken++;
    this.checkIn.set('');
    this.checkOut.set('');
    this.byRoom.set(new Map());
    this.isLoadingAvailability.set(false);
    this.hasResult.set(false);
  }

  setDates(checkIn: string, checkOut: string, slug: string, locationId: string | null): void {
    this.checkIn.set(checkIn);
    this.checkOut.set(checkOut);
    this.refresh(slug, locationId);
  }

  refresh(slug: string, locationId: string | null): void {
    const checkIn = this.checkIn();
    const checkOut = this.checkOut();

    if (!slug || !locationId || !checkIn || !checkOut || checkOut <= checkIn) {
      this.byRoom.set(new Map());
      this.hasResult.set(false);
      return;
    }

    const token = ++this.refreshToken;
    this.isLoadingAvailability.set(true);

    this.api
      .getRoomAvailability(slug, locationId, checkIn, checkOut)
      .pipe(finalize(() => this.isLoadingAvailability.set(false)))
      .subscribe({
        next: (res) => {
          if (token !== this.refreshToken) {
            return;
          }
          if (res.success && res.data) {
            this.byRoom.set(new Map(res.data.map((row) => [row.roomId, row])));
            this.hasResult.set(true);
          } else {
            this.byRoom.set(new Map());
            this.hasResult.set(false);
          }
        },
        error: () => {
          if (token !== this.refreshToken) {
            return;
          }
          this.byRoom.set(new Map());
          this.hasResult.set(false);
        },
      });
  }

  availabilityFor(roomId: string): GuestRoomAvailabilityDto | null {
    return this.byRoom().get(roomId) ?? null;
  }

  isRoomAvailable(roomId: string): boolean {
    if (!this.hasResult()) {
      return false;
    }
    return this.byRoom().get(roomId)?.isAvailable ?? false;
  }

  availabilityLabel(roomId: string): string {
    if (this.isLoadingAvailability()) {
      return 'Checking…';
    }
    if (!this.hasResult()) {
      return 'Check dates';
    }
    const row = this.byRoom().get(roomId);
    if (!row) {
      return 'Unavailable';
    }
    if (!row.isAvailable) {
      return '✗ Booked';
    }
    if (row.totalQuantity > 1) {
      return `✓ ${row.availableUnits} left`;
    }
    return '✓ Available';
  }
}
