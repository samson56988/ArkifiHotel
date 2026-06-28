import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';

import { FormsModule } from '@angular/forms';

import { HotelUiService } from '../../core/services/hotel-ui.service';

import { GuestRoomAvailabilityService } from '../../core/services/guest-room-availability.service';

import { PublicGuestBookingApiService } from '../../core/services/public-guest-booking-api.service';

import type { HotelShowcase } from '../../core/models/hotel-showcase.models';

import { formatNaira } from '../../core/utils/hotel-theme';
import { calculateStayTotal, hasWeeklyRate } from '../../core/utils/room-pricing';

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

  readonly availability = inject(GuestRoomAvailabilityService);

  readonly storefront = input.required<HotelShowcase>();



  guestName = '';

  guestEmail = '';

  guestPhone = '';

  readonly submitting = signal(false);

  readonly formError = signal<string | null>(null);



  readonly checkIn = computed(() => this.availability.checkIn());

  readonly checkOut = computed(() => this.availability.checkOut());



  readonly selectedRoomId = computed(() => this.ui.selectedRoom()?.id ?? '');



  readonly canSubmit = computed(() => {

    if (this.submitting() || this.availability.isLoadingAvailability()) {

      return false;

    }

    const roomId = this.selectedRoomId();

    if (!roomId || !this.checkIn() || !this.checkOut()) {

      return false;

    }

    if (!this.availability.hasResult()) {

      return false;

    }

    return this.availability.isRoomAvailable(roomId);

  });



  constructor() {

    effect(() => {

      if (!this.ui.bookingOpen()) {

        return;

      }

      this.availability.ensureDefaultDates();

      const sf = this.storefront();

      this.availability.refresh(sf.slug, sf.activeLocationId);

    });

    effect(() => {
      if (!this.ui.bookingOpen() || !this.availability.hasResult()) {
        return;
      }
      const current = this.ui.selectedRoom();
      if (current && this.availability.isRoomAvailable(current.id)) {
        return;
      }
      const first = this.roomOptions()[0];
      if (first) {
        this.ui.selectedRoom.set(first);
      }
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

    const room = this.storefront().rooms.find((r) => r.id === roomId) ?? null;

    this.ui.selectedRoom.set(room);

  }



  onCheckInChange(value: string): void {

    const sf = this.storefront();

    this.availability.setDates(value, this.checkOut(), sf.slug, sf.activeLocationId);

  }



  onCheckOutChange(value: string): void {

    const sf = this.storefront();

    this.availability.setDates(this.checkIn(), value, sf.slug, sf.activeLocationId);

  }



  selectedRoomName(): string {

    return this.ui.selectedRoom()?.name ?? 'Reserve a Room';

  }



  selectedRoomPrice(): string | null {

    const room = this.ui.selectedRoom();

    if (!room) {
      return null;
    }
    const nightly = `From ${formatNaira(room.basePricePerNight)} / night`;
    if (hasWeeklyRate(room.basePricePerWeek)) {
      return `${nightly} · ${formatNaira(room.basePricePerWeek)} / week`;
    }
    return nightly;

  }



  roomOptions() {

    const rooms = this.storefront().rooms;

    if (!this.availability.hasResult()) {

      return rooms;

    }

    return rooms.filter((r) => this.availability.isRoomAvailable(r.id));

  }



  roomOptionLabel(roomId: string, name: string, price: number, weekly?: number | null): string {

    const row = this.availability.availabilityFor(roomId);
    const priceLabel = hasWeeklyRate(weekly)
      ? `${formatNaira(price)}/night · ${formatNaira(weekly)}/week`
      : `${formatNaira(price)}/night`;

    if (!row) {

      return `${name} — ${priceLabel}`;

    }

    if (!row.isAvailable) {

      return `${name} — not available for these dates`;

    }

    const qty =

      row.totalQuantity > 1 ? ` (${row.availableUnits} of ${row.totalQuantity} free)` : '';

    return `${name}${qty} — ${priceLabel}`;

  }



  allRoomOptions() {

    return this.storefront().rooms.map((r) => ({

      room: r,

      selectable: !this.availability.hasResult() || this.availability.isRoomAvailable(r.id),

      label: this.roomOptionLabel(r.id, r.name, r.basePricePerNight, r.basePricePerWeek),

    }));

  }



  estimatedTotal(): string {

    const room = this.ui.selectedRoom();

    const checkIn = this.checkIn();

    const checkOut = this.checkOut();

    if (!room || !checkIn || !checkOut) {

      return 'Select dates';

    }



    const nights = Math.max(

      1,

      Math.round((Date.parse(checkOut) - Date.parse(checkIn)) / 86_400_000),

    );

    const total = calculateStayTotal(room.basePricePerNight, room.basePricePerWeek, nights);
    const weeklyNote =
      nights >= 7 && hasWeeklyRate(room.basePricePerWeek) ? ' (weekly rate applied)' : '';

    return `${formatNaira(total)} (${nights} night${nights > 1 ? 's' : ''}${weeklyNote})`;

  }



  availabilityHint(): string | null {

    if (this.availability.isLoadingAvailability()) {

      return 'Checking room availability for your dates…';

    }

    if (!this.checkIn() || !this.checkOut() || this.checkOut() <= this.checkIn()) {

      return 'Choose valid check-in and check-out dates to see available rooms.';

    }

    if (!this.availability.hasResult()) {

      return 'Could not load availability. Adjust dates and try again.';

    }

    const roomId = this.selectedRoomId();

    if (roomId && !this.availability.isRoomAvailable(roomId)) {

      return 'This room is not available for the selected dates. Pick another room or change dates.';

    }

    if (this.roomOptions().length === 0) {

      return 'No rooms are available for these dates. Try different dates.';

    }

    return null;

  }



  submit(event: Event): void {

    event.preventDefault();

    if (!this.canSubmit()) {

      this.formError.set(this.availabilityHint() ?? 'Room not available for these dates.');

      return;

    }

    if (this.submitting()) {

      return;

    }

    this.formError.set(null);



    const sf = this.storefront();

    const room = this.ui.selectedRoom();

    const locationId = sf.activeLocationId;

    const checkIn = this.checkIn();

    const checkOut = this.checkOut();



    if (!room || !locationId || !checkIn || !checkOut) {

      this.formError.set('Complete details are incomplete.');

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

        checkInDate: checkIn,

        checkOutDate: checkOut,

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

            this.availability.refresh(sf.slug, locationId);

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

    return new Date().toISOString().slice(0, 10);

  }

}

