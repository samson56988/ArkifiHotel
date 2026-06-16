import { ChangeDetectionStrategy, Component, effect, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HotelUiService } from '../../core/services/hotel-ui.service';
import type { HotelShowcase } from '../../core/models/hotel-showcase.models';
import { formatNaira } from '../../core/utils/hotel-theme';

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
  readonly storefront = input.required<HotelShowcase>();

  checkIn = '';
  checkOut = '';
  selectedRoomId = '';

  constructor() {
    effect(() => {
      const room = this.ui.selectedRoom();
      this.selectedRoomId = room?.id ?? '';
    });
  }

  close(): void {
    this.ui.closeBooking();
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
    this.close();
    this.ui.showToast('Reservation request sent! We will confirm via WhatsApp shortly.');
  }

  formatPrice(amount: number): string {
    return formatNaira(amount);
  }

  todayIso(): string {
    return new Date().toISOString().split('T')[0];
  }
}
