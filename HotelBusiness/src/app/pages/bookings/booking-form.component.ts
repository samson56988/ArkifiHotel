import { DecimalPipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { combineLatest, of } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, map, startWith, switchMap } from 'rxjs/operators';
import type { BusinessRoomSummaryDto } from '../../core/models/rooms.models';
import type { RoomAvailabilityDto } from '../../core/models/bookings.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { BusinessBookingsApiService } from '../../core/services/business-bookings-api.service';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { BusinessRoomsApiService } from '../../core/services/business-rooms-api.service';
import { ToastService } from '../../core/services/toast.service';
import { COUNTRY_DIAL_CODES, DEFAULT_DIAL_CODE, dialCodeLabel } from '../../core/data/country-dial-codes';
import {
  buildInternationalPhone,
  guestPhoneLocalValidator,
  isValidLocalPhoneDigits,
  normalizeLocalPhoneDigits,
} from '../../core/utils/phone-number';
import { BusinessContextService } from '../../core/services/business-context.service';
import { calculateStayTotal, hasWeeklyRate } from '../../core/utils/room-pricing';

function addDaysIso(base: Date, days: number): string {
  const d = new Date(base.getTime());
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-booking-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe],
  templateUrl: './booking-form.component.html',
  styleUrl: './booking-form.component.scss',
})
export class BookingFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly roomsApi = inject(BusinessRoomsApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly bookingsApi = inject(BusinessBookingsApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  readonly biz = inject(BusinessContextService);

  readonly form = this.fb.nonNullable.group({
    locationFilterId: ['', Validators.required],
    roomId: ['', Validators.required],
    guestName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    guestEmail: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
    guestPhoneCountryCode: [DEFAULT_DIAL_CODE, Validators.required],
    guestPhoneLocal: ['', guestPhoneLocalValidator],
    checkInDate: ['', Validators.required],
    checkOutDate: ['', Validators.required],
    paymentMethod: ['Cash', Validators.required],
    paymentConfirmed: [false, Validators.requiredTrue],
    internalNotes: [''],
  });

  readonly allRooms = signal<BusinessRoomSummaryDto[]>([]);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly availability = signal<RoomAvailabilityDto | null>(null);
  readonly availabilityByRoom = signal<Map<string, RoomAvailabilityDto>>(new Map());
  readonly loadingRooms = signal(true);
  readonly loadingLocations = signal(true);
  readonly roomsFailed = signal(false);
  readonly checkingAvailability = signal(false);
  readonly saving = signal(false);

  readonly countryDialCodes = COUNTRY_DIAL_CODES;
  readonly dialCodeLabel = dialCodeLabel;
  readonly normalizeLocalPhoneDigits = normalizeLocalPhoneDigits;
  readonly isValidLocalPhoneDigits = isValidLocalPhoneDigits;

  readonly paymentMethodOptions = [
    { value: 'Cash', label: 'Cash', hint: 'Guest paid cash at reception.' },
    { value: 'BankTransfer', label: 'Bank transfer', hint: 'Transfer received and verified at reception.' }] as const;

  ngOnInit(): void {
    this.loadLocations();
    this.loadRooms();
    this.watchAvailability();
  }

  filteredRooms(): BusinessRoomSummaryDto[] {
    const locationId = this.form.controls.locationFilterId.value.trim();
    if (!locationId) {
      return [];
    }

    return this.allRooms().filter((r) => r.locationId === locationId);
  }

  selectedRoom(): BusinessRoomSummaryDto | null {
    const id = this.form.controls.roomId.value;
    return this.allRooms().find((r) => r.id === id) ?? null;
  }

  selectedRoomAvailability(): RoomAvailabilityDto | null {
    return this.availability();
  }

  estimatedTotal(): number | null {
    const avail = this.availability();
    const raw = this.form.getRawValue();
    if (!avail || !raw.checkInDate || !raw.checkOutDate || raw.checkOutDate <= raw.checkInDate) {
      return null;
    }

    const nights =
      (Date.parse(raw.checkOutDate) - Date.parse(raw.checkInDate)) / (1000 * 60 * 60 * 24);
    if (nights < 1) {
      return null;
    }

    return calculateStayTotal(avail.basePricePerNight, avail.basePricePerWeek, nights);
  }

  roomOptionLabel(room: BusinessRoomSummaryDto): string {
    const map = this.availabilityByRoom();
    const avail = map.get(room.id);
    const loc = room.locationName ? ` @ ${room.locationName}` : '';
    const weekly =
      hasWeeklyRate(avail?.basePricePerWeek ?? room.basePricePerWeek) &&
      ` · ${(avail?.basePricePerWeek ?? room.basePricePerWeek)!.toFixed(2)} / week`;

    if (!avail) {
      return `${room.name}${loc} — ${room.basePricePerNight.toFixed(2)} / night${weekly ?? ''}`;
    }

    if (!avail.isAvailable) {
      return `${room.name}${loc} — not available for these dates`;
    }

    const qty =
      avail.totalQuantity > 1 ? ` (${avail.availableUnits} of ${avail.totalQuantity} free)` : '';
    const weekLabel = hasWeeklyRate(avail.basePricePerWeek)
      ? ` · ${avail.basePricePerWeek!.toFixed(2)} / week`
      : '';
    return `${room.name}${loc}${qty} — ${avail.basePricePerNight.toFixed(2)} / night${weekLabel}`;
  }

  isRoomSelectable(roomId: string): boolean {
    const avail = this.availabilityByRoom().get(roomId);
    return avail?.isAvailable ?? true;
  }

  loadLocations(): void {
    this.loadingLocations.set(true);
    this.locationsApi
      .listLocations()
      .pipe(finalize(() => this.loadingLocations.set(false)))
      .subscribe((res) => {
        if (res.success && res.data) {
          this.locations.set(res.data);
          if (res.data.length === 1) {
            this.form.patchValue({ locationFilterId: res.data[0].id });
            this.onLocationFilterChange();
          }
        }
      });
  }

  loadRooms(): void {
    const today = new Date();
    const checkIn = addDaysIso(today, 1);
    const checkOut = addDaysIso(today, 3);

    this.loadingRooms.set(true);
    this.roomsFailed.set(false);
    this.roomsApi
      .listRooms(false)
      .pipe(finalize(() => this.loadingRooms.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.roomsFailed.set(true);
            this.toast.showFailedApi(res, 'Rooms');
            return;
          }

          this.allRooms.set(res.data);
          this.form.patchValue({
            roomId: '',
            checkInDate: checkIn,
            checkOutDate: checkOut,
          });
          this.onLocationFilterChange();
        },
        error: () => {
          this.roomsFailed.set(true);
          this.toast.error('Could not load rooms.', 'Rooms');
        },
      });
  }

  onLocationFilterChange(): void {
    const filtered = this.filteredRooms();
    const currentRoomId = this.form.controls.roomId.value;
    if (!filtered.some((r) => r.id === currentRoomId)) {
      this.form.patchValue({ roomId: filtered[0]?.id ?? '' });
    }
  }

  private watchAvailability(): void {
    combineLatest([
      this.form.controls.locationFilterId.valueChanges.pipe(startWith(this.form.controls.locationFilterId.value)),
      this.form.controls.roomId.valueChanges.pipe(startWith(this.form.controls.roomId.value)),
      this.form.controls.checkInDate.valueChanges.pipe(startWith(this.form.controls.checkInDate.value)),
      this.form.controls.checkOutDate.valueChanges.pipe(startWith(this.form.controls.checkOutDate.value))])
      .pipe(
        debounceTime(300),
        distinctUntilChanged(
          (a, b) => a[0] === b[0] && a[1] === b[1] && a[2] === b[2] && a[3] === b[3],
        ),
        switchMap(([locationFilterId, roomId, checkIn, checkOut]) => {
          const locationId = locationFilterId.trim();
          if (!locationId || !checkIn || !checkOut || checkOut <= checkIn) {
            this.availability.set(null);
            this.availabilityByRoom.set(new Map());
            return of(null);
          }

          this.checkingAvailability.set(true);
          return this.bookingsApi.getAvailability(checkIn, checkOut, null, locationId).pipe(
            catchError(() => of({ success: false, data: null, message: null, code: null, validationErrors: null })),
            finalize(() => this.checkingAvailability.set(false)),
            map((res) => (res.success && res.data ? res.data : [])),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((rows) => {
        if (!rows) {
          return;
        }

        const map = new Map(rows.map((r) => [r.roomId, r]));
        this.availabilityByRoom.set(map);

        const roomId = this.form.controls.roomId.value;
        this.availability.set(map.get(roomId) ?? null);

        if (roomId && map.has(roomId) && !map.get(roomId)!.isAvailable) {
          const fallback = rows.find((r) => r.isAvailable);
          if (fallback) {
            this.form.patchValue({ roomId: fallback.roomId });
            this.availability.set(fallback);
          }
        }
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.allRooms().length === 0) {
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

    if (!raw.paymentConfirmed) {
      this.toast.warning('Confirm that payment has been received before creating the booking.', 'Payment');
      return;
    }

    const guestPhone = buildInternationalPhone(raw.guestPhoneCountryCode, raw.guestPhoneLocal);
    if (!guestPhone) {
      this.toast.warning('Enter the guest’s phone number.', 'Guest');
      return;
    }

    const avail = this.availability();
    if (!avail?.isAvailable) {
      this.toast.warning(
        'This room is not available for the selected dates. Choose another room or different dates.',
        'Not available',
      );
      return;
    }

    this.saving.set(true);
    this.bookingsApi
      .createBooking({
        locationId: raw.locationFilterId.trim(),
        roomId: raw.roomId,
        guestName: raw.guestName.trim(),
        guestEmail: raw.guestEmail.trim(),
        guestPhone,
        checkInDate: checkIn,
        checkOutDate: checkOut,
        paymentMethod: raw.paymentMethod,
        paymentConfirmed: raw.paymentConfirmed,
        internalNotes: raw.internalNotes.trim() || null,
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Booking');
            return;
          }

          this.toast.success(`Confirmation code: ${res.data.confirmationCode}`, 'Booking created');
          void this.router.navigateByUrl('/bookings');
        },
        error: (err: unknown) => {
          this.saving.set(false);
          const r = err as { message?: string };
          this.toast.error(r?.message ?? 'Could not create booking.', 'Booking');
        },
      });
  }
}
