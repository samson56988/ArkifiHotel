import { DecimalPipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BookingSummaryDto } from '../../core/models/bookings.models';
import { BusinessBookingsApiService } from '../../core/services/business-bookings-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-bookings-list',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent, DecimalPipe],
  templateUrl: './bookings-list.component.html',
  styleUrl: './bookings-list.component.scss',
})
export class BookingsListComponent implements OnInit {
  private readonly api = inject(BusinessBookingsApiService);
  private readonly toast = inject(ToastService);

  bookings: BookingSummaryDto[] = [];
  /** True after the first list request finishes (success or failure). */
  initialLoadDone = false;
  loadFailed = false;
  statusUpdatingId: string | null = null;

  readonly statusOptions = ['Pending', 'Confirmed', 'Cancelled', 'Completed'] as const;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loadFailed = false;
    this.api.listBookings().subscribe({
      next: (res) => {
        if (res.success) {
          this.bookings = Array.isArray(res.data) ? res.data! : [];
          this.loadFailed = false;
        } else {
          this.bookings = [];
          this.loadFailed = true;
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again to manage bookings.', 'Bookings');
          } else {
            this.toast.showFailedApi(res, 'Bookings');
          }
        }

        this.initialLoadDone = true;
      },
      error: (err: unknown) => {
        this.bookings = [];
        this.loadFailed = true;
        this.initialLoadDone = true;
        const res = err as ApiResult<BookingSummaryDto[]>;
        if (res && typeof res === 'object' && 'message' in res) {
          this.toast.showFailedApi(res, 'Bookings');
          return;
        }

        this.toast.error('Could not load bookings.', 'Bookings');
      },
    });
  }

  onStatusChange(booking: BookingSummaryDto, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const next = select.value;
    if (next === booking.status) {
      return;
    }

    this.statusUpdatingId = booking.id;
    this.api.updateStatus(booking.id, next).subscribe({
      next: (res) => {
        this.statusUpdatingId = null;
        if (!res.success || !res.data) {
          select.value = booking.status;
          this.toast.showFailedApi(res, 'Bookings');
          return;
        }

        booking.status = res.data.status;
        this.toast.success('Status updated.', 'Bookings');
      },
      error: (err: unknown) => {
        this.statusUpdatingId = null;
        select.value = booking.status;
        const res = err as ApiResult<unknown>;
        if (res && typeof res === 'object' && 'message' in res) {
          this.toast.showFailedApi(res, 'Bookings');
          return;
        }

        this.toast.error('Could not update status.', 'Bookings');
      },
    });
  }

  formatDate(isoDate: string): string {
    if (!isoDate) {
      return '';
    }

    const d = new Date(isoDate + 'T12:00:00');
    return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
