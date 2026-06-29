import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BookingPaymentSummaryDto } from '../../core/models/booking-payments.models';
import { BusinessBookingPaymentsApiService } from '../../core/services/business-booking-payments-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-booking-payments-list',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './booking-payments-list.component.html',
  styleUrl: './booking-payments-list.component.scss',
})
export class BookingPaymentsListComponent implements OnInit {
  private readonly api = inject(BusinessBookingPaymentsApiService);
  private readonly toast = inject(ToastService);

  readonly payments = signal<BookingPaymentSummaryDto[]>([]);
  readonly initialLoadDone = signal(false);
  readonly loadFailed = signal(false);
  readonly loading = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.api.listPayments().subscribe({
      next: (res) => {
        if (res.success) {
          this.payments.set(Array.isArray(res.data) ? res.data! : []);
          this.loadFailed.set(false);
        } else {
          this.payments.set([]);
          this.loadFailed.set(true);
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again.', 'Payments');
          } else {
            this.toast.showFailedApi(res, 'Payments');
          }
        }

        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.payments.set([]);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        const r = err as ApiResult<BookingPaymentSummaryDto[]>;
        if (r && typeof r === 'object' && 'message' in r) {
          this.toast.showFailedApi(r, 'Payments');
          return;
        }

        this.toast.error('Could not load booking payments.', 'Payments');
      },
    });
  }

  formatDate(iso: string): string {
    if (!iso) {
      return '';
    }

    const d = new Date(iso);
    return d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
  }
}
