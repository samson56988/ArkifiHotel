import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { BusinessBookingPaymentsApiService } from '../../core/services/business-booking-payments-api.service';
import { BusinessBookingsApiService } from '../../core/services/business-bookings-api.service';
import { ToastService } from '../../core/services/toast.service';
import type { BookingSummaryDto } from '../../core/models/bookings.models';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-booking-payment-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './booking-payment-form.component.html',
  styleUrl: './booking-payment-form.component.scss',
})
export class BookingPaymentFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly paymentsApi = inject(BusinessBookingPaymentsApiService);
  private readonly bookingsApi = inject(BusinessBookingsApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    bookingId: ['', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    currency: ['NGN', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
    status: ['Pending', Validators.required],
    gateway: ['None', Validators.required],
    externalReference: [''],
    notes: [''],
  });

  bookings: BookingSummaryDto[] = [];
  loadingBookings = true;
  bookingsFailed = false;
  saving = false;

  readonly statusOptions = ['Pending', 'Completed', 'Failed', 'Refunded', 'Cancelled'] as const;
  readonly gatewayOptions = ['None', 'Paystack', 'Flutterwave'] as const;

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.loadingBookings = true;
    this.bookingsFailed = false;
    this.bookingsApi
      .listBookings()
      .pipe(finalize(() => (this.loadingBookings = false)))
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.bookings = Array.isArray(res.data) ? res.data! : [];
            if (this.bookings.length > 0) {
              this.form.patchValue({ bookingId: this.bookings[0].id });
            }
            return;
          }

          this.bookingsFailed = true;
          this.toast.showFailedApi(res, 'Bookings');
        },
        error: () => {
          this.bookingsFailed = true;
          this.toast.error('Could not load bookings.', 'Bookings');
        },
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.bookings.length === 0) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.saving = true;
    this.paymentsApi
      .createPayment({
        bookingId: raw.bookingId,
        amount: Number(raw.amount),
        currency: raw.currency.trim().toUpperCase(),
        status: raw.status,
        gateway: raw.gateway,
        externalReference: raw.externalReference.trim() || null,
        notes: raw.notes.trim() || null,
      })
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Payments');
            return;
          }

          this.toast.success('Payment recorded.', 'Payments');
          void this.router.navigateByUrl('/booking-payments');
        },
        error: (err: unknown) => {
          const r = err as { message?: string };
          this.toast.error(r?.message ?? 'Could not save.', 'Payments');
        },
      });
  }
}
