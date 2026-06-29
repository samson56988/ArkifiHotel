import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BookingSummaryDto } from '../../core/models/bookings.models';
import { BusinessBookingPaymentsApiService } from '../../core/services/business-booking-payments-api.service';
import { BusinessBookingsApiService } from '../../core/services/business-bookings-api.service';
import { BusinessPaymentApiService } from '../../core/services/business-payment-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-booking-payment-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './booking-payment-form.component.html',
  styleUrl: './booking-payment-form.component.scss',
})
export class BookingPaymentFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly paymentsApi = inject(BusinessBookingPaymentsApiService);
  private readonly bookingsApi = inject(BusinessBookingsApiService);
  private readonly paymentConfigApi = inject(BusinessPaymentApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    bookingId: ['', Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    currency: ['NGN', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
    status: ['Pending', Validators.required],
    method: ['Cash', Validators.required],
    gateway: ['None', Validators.required],
    externalReference: [''],
    notes: [''],
  });

  readonly bookings = signal<BookingSummaryDto[]>([]);
  readonly loadingBookings = signal(true);
  readonly bookingsFailed = signal(false);
  readonly saving = signal(false);
  readonly configuredGateway = signal('None');

  readonly statusOptions = ['Pending', 'Completed', 'Failed', 'Refunded', 'Cancelled'] as const;
  readonly methodOptions = [
    { value: 'Cash', label: 'Cash', hint: 'Paid at reception on arrival or checkout.' },
    { value: 'BankTransfer', label: 'Bank transfer', hint: 'Guest transferred to the hotel account; confirm when received.' },
    { value: 'Gateway', label: 'Online gateway', hint: 'Card payment via Paystack, Flutterwave, or Monify.' }] as const;

  ngOnInit(): void {
    this.loadBookings();
    this.loadPaymentConfig();
    this.form.controls.method.valueChanges.subscribe((method) => this.applyMethodDefaults(method));
  }

  loadBookings(): void {
    this.loadingBookings.set(true);
    this.bookingsFailed.set(false);
    this.bookingsApi
      .listBookings({ page: 1, pageSize: 100 })
      .pipe(finalize(() => this.loadingBookings.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            const items = Array.isArray(res.data.items) ? res.data.items : [];
            this.bookings.set(items);
            if (items.length > 0) {
              this.form.patchValue({
                bookingId: items[0].id,
                amount: items[0].totalAmount,
                currency: items[0].currency,
              });
            }
            return;
          }

          this.bookingsFailed.set(true);
          this.toast.showFailedApi(res, 'Bookings');
        },
        error: () => {
          this.bookingsFailed.set(true);
          this.toast.error('Could not load bookings.', 'Bookings');
        },
      });
  }

  onBookingChange(): void {
    const id = this.form.controls.bookingId.value;
    const booking = this.bookings().find((b) => b.id === id);
    if (booking) {
      this.form.patchValue({ amount: booking.totalAmount, currency: booking.currency });
    }
  }

  private loadPaymentConfig(): void {
    this.paymentConfigApi.getConfiguration().subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.provider !== 'None') {
          this.configuredGateway.set(res.data.provider);
          if (this.form.controls.method.value === 'Gateway') {
            this.form.patchValue({ gateway: res.data.provider });
          }
        }
      },
    });
  }

  private applyMethodDefaults(method: string): void {
    if (method === 'Cash') {
      this.form.patchValue({ gateway: 'None', status: 'Completed' });
      return;
    }

    if (method === 'BankTransfer') {
      this.form.patchValue({ gateway: 'None', status: 'Pending' });
      return;
    }

    if (method === 'Gateway') {
      const gw = this.configuredGateway() !== 'None' ? this.configuredGateway() : 'Paystack';
      this.form.patchValue({ gateway: gw, status: 'Pending' });
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.bookings().length === 0) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (raw.method === 'Gateway' && raw.gateway === 'None') {
      this.toast.warning('Configure a payment gateway under Payment configuration.', 'Gateway');
      return;
    }

    this.saving.set(true);
    this.paymentsApi
      .createPayment({
        bookingId: raw.bookingId,
        amount: Number(raw.amount),
        currency: raw.currency.trim().toUpperCase(),
        status: raw.status,
        method: raw.method,
        gateway: raw.method === 'Gateway' ? raw.gateway : 'None',
        externalReference: raw.externalReference.trim() || null,
        notes: raw.notes.trim() || null,
      })
      .pipe(finalize(() => this.saving.set(false)))
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
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Payments');
            return;
          }

          this.toast.error('Could not save.', 'Payments');
        },
      });
  }
}
