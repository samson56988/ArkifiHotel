import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { BusinessRoomSummaryDto } from '../../core/models/rooms.models';
import { BusinessBookingsApiService } from '../../core/services/business-bookings-api.service';
import { BusinessRoomsApiService } from '../../core/services/business-rooms-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

function addDaysIso(base: Date, days: number): string {
  const d = new Date(base.getTime());
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-booking-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent, DecimalPipe],
  templateUrl: './booking-form.component.html',
  styleUrl: './booking-form.component.scss',
})
export class BookingFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly roomsApi = inject(BusinessRoomsApiService);
  private readonly bookingsApi = inject(BusinessBookingsApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    roomId: ['', Validators.required],
    guestName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    guestEmail: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
    guestPhone: [''],
    checkInDate: ['', Validators.required],
    checkOutDate: ['', Validators.required],
    internalNotes: [''],
  });

  rooms: BusinessRoomSummaryDto[] = [];
  loadingRooms = true;
  roomsFailed = false;
  saving = false;

  ngOnInit(): void {
    this.loadRooms();
  }

  loadRooms(): void {
    const today = new Date();
    const checkIn = addDaysIso(today, 1);
    const checkOut = addDaysIso(today, 3);

    this.loadingRooms = true;
    this.roomsFailed = false;
    this.roomsApi
      .listRooms(false)
      .pipe(finalize(() => (this.loadingRooms = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.roomsFailed = true;
            this.toast.showFailedApi(res, 'Rooms');
            return;
          }

          this.rooms = res.data;
          if (this.rooms.length > 0) {
            this.form.patchValue({
              roomId: this.rooms[0].id,
              checkInDate: checkIn,
              checkOutDate: checkOut,
            });
          } else {
            this.form.patchValue({ checkInDate: checkIn, checkOutDate: checkOut });
          }
        },
        error: () => {
          this.roomsFailed = true;
          this.toast.error('Could not load rooms.', 'Rooms');
        },
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.rooms.length === 0) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const checkIn = raw.checkInDate;
    const checkOut = raw.checkOutDate;
    if (checkOut <= checkIn) {
      this.toast.warning('Check-out must be after check-in.', 'Booking');
      return;
    }

    this.saving = true;
    this.bookingsApi
      .createBooking({
        roomId: raw.roomId,
        guestName: raw.guestName.trim(),
        guestEmail: raw.guestEmail.trim(),
        guestPhone: raw.guestPhone.trim() || null,
        checkInDate: checkIn,
        checkOutDate: checkOut,
        internalNotes: raw.internalNotes.trim() || null,
      })
      .subscribe({
        next: (res) => {
          this.saving = false;
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Booking');
            return;
          }

          this.toast.success(`Confirmation code: ${res.data.confirmationCode}`, 'Booking created');
          void this.router.navigateByUrl('/bookings');
        },
        error: (err: unknown) => {
          this.saving = false;
          const r = err as { message?: string };
          this.toast.error(r?.message ?? 'Could not create booking.', 'Booking');
        },
      });
  }
}
