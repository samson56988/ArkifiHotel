import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import { PublicGuestBookingApiService } from '../../core/services/public-guest-booking-api.service';
import type { HotelShowcase } from '../../core/models/hotel-showcase.models';
import { formatNaira } from '../../core/utils/hotel-theme';
import type { ApiResult } from '../../core/models/api-result.model';
import type { GuestBookingCheckoutDto } from '../../core/services/public-guest-booking-api.service';

@Component({
  selector: 'app-booking-modal',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './booking-modal.component.html',
  styleUrl: './booking-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingModalComponent {
  readonly ui = inject(HotelUiService);
  private readonly bookingApi = inject(PublicGuestBookingApiService);
  readonly storefront = input.required<HotelShowcase>();

  checkIn = '';
  checkOut = '';
  selectedRoomId = '';
  guestName = '';
  guestEmail = '';
  guestPhone = '';
  readonly submitting = signal(false);
  readonly formError = signal<string | null>(null);

  constructor() {
    effect(() => {
      const room = this.ui.selectedRoom();
      this.selectedRoomId = room?.id ?? '';
    });
  }

  close(): void {
    if (this.submitting()) {
      return;
    }
    this.ui.closeBooking();
    this.formError.set(null);
  }

  onOverlayClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.close();
    }
  }

  onRoomChange(roomId: string): void {
    this.selectedRoomId = roomId;
    const room = this.storefront().rooms.find((r) => r.id === roomId) ?? null;
    this.ui.selectedRoom.set(room);
  }

  selectedRoomName(): string {
    return this.ui.selectedRoom()?.name ?? 'Reserve a Room';
  }

  selectedRoomPrice(): string | null {
    const room = this.ui.selectedRoom();
    return room ? `Starting from ${formatNaira(room.basePricePerNight)} per night` : null;
  }

  availableRooms() {
    return this.storefront().rooms.filter((r) => r.available);
  }

  estimatedTotal(): string {
    const room = this.ui.selectedRoom();
    if (!room || !this.checkIn || !this.checkOut) {
      return 'Select dates';
    }

    const nights = Math.max(
      1,
      Math.round((new Date(this.checkOut).getTime() - new Date(this.checkIn).getTime()) / 86_400_000),
    );
    return `${formatNaira(room.basePricePerNight * nights)} (${nights} night${nights > 1 ? 's' : ''})`;
  }

  submit(event: Event): void {
    event.preventDefault();
    this.formError.set(null);

    const sf = this.storefront();
    const room = this.ui.selectedRoom();
    const locationId = sf.activeLocationId;

    if (!room || !this.selectedRoomId) {
      this.formError.set('Please select a room.');
      return;
    }

    if (!locationId) {
      this.formError.set('Select a hotel branch before booking.');
      return;
    }

    if (!this.checkIn || !this.checkOut) {
      this.formError.set('Please choose check-in and check-out dates.');
      return;
    }

    if (new Date(this.checkOut) <= new Date(this.checkIn)) {
      this.formError.set('Check-out must be after check-in.');
      return;
    }

    const name = this.guestName.trim();
    const email = this.guestEmail.trim();
    const phone = this.guestPhone.trim().replace(/\s/g, '');

    if (name.length < 2 || !email.includes('@') || !phone.startsWith('+')) {
      this.formError.set('Enter your name, email, and phone in international format (e.g. +2348034567890).');
      return;
    }

    this.submitting.set(true);

    this.bookingApi
      .createCheckout(sf.slug, {
        locationId,
        roomId: room.id,
        guestName: name,
        guestEmail: email,
        guestPhone: phone,
        checkInDate: this.checkIn,
        checkOutDate: this.checkOut,
      })
      .subscribe({
        next: (res) => {
          this.submitting.set(false);
          if (res.success && res.data?.paymentUrl) {
            this.close();
            window.location.href = res.data.paymentUrl;
            return;
          }

          this.formError.set(res.message ?? 'Could not start checkout.');
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          const r = err as ApiResult<GuestBookingCheckoutDto>;
          if (r?.code === 'PaymentNotConfigured') {
            this.formError.set('Online payment is not set up for this hotel. Please contact them directly.');
            return;
          }
          if (r?.code === 'RoomUnavailable') {
            this.formError.set('This room is no longer available for those dates.');
            return;
          }
          this.formError.set(r?.message ?? 'Could not start booking. Please try again.');
        },
      });
  }

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  todayIso(): string {
    return new Date().toISOString().split('T')[0];
  }
}
