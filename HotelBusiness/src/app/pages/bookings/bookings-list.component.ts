import { DecimalPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BookingStayPhase, BookingSummaryDto, ListBookingsParams } from '../../core/models/bookings.models';
import { BusinessBookingsApiService } from '../../core/services/business-bookings-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

interface BookingFilters {
  checkInFrom: string;
  checkInTo: string;
  checkOutFrom: string;
  checkOutTo: string;
  stayPhase: BookingStayPhase;
  status: string;
}

const EMPTY_FILTERS: BookingFilters = {
  checkInFrom: '',
  checkInTo: '',
  checkOutFrom: '',
  checkOutTo: '',
  stayPhase: 'All',
  status: '',
};

@Component({
  selector: 'app-bookings-list',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent, DecimalPipe, FormsModule],
  templateUrl: './bookings-list.component.html',
  styleUrl: './bookings-list.component.scss',
})
export class BookingsListComponent implements OnInit {
  private readonly api = inject(BusinessBookingsApiService);
  private readonly toast = inject(ToastService);

  readonly bookings = signal<BookingSummaryDto[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly initialLoadDone = signal(false);
  readonly loadFailed = signal(false);
  readonly loading = signal(false);
  readonly statusUpdatingId = signal<string | null>(null);

  readonly draftFilters = signal<BookingFilters>({ ...EMPTY_FILTERS });
  readonly appliedFilters = signal<BookingFilters>({ ...EMPTY_FILTERS });

  readonly statusOptions = ['Pending', 'Confirmed', 'Cancelled', 'Completed'] as const;
  readonly stayPhaseOptions: { value: BookingStayPhase; label: string }[] = [
    { value: 'All', label: 'All stays' },
    { value: 'Active', label: 'Active (checkout in future)' },
    { value: 'Closed', label: 'Closed (checkout today or past)' },
  ];

  readonly hasAppliedFilters = computed(() => this.filtersActive(this.appliedFilters()));
  readonly showingEmptyResults = computed(
    () => this.initialLoadDone() && !this.loading() && this.totalCount() === 0,
  );

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);

    const params = this.buildListParams(this.page(), this.appliedFilters());
    this.api.listBookings(params).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.bookings.set(Array.isArray(res.data.items) ? res.data.items : []);
          this.page.set(res.data.page);
          this.pageSize.set(res.data.pageSize);
          this.totalCount.set(res.data.totalCount);
          this.totalPages.set(res.data.totalPages);
          this.loadFailed.set(false);
        } else {
          this.bookings.set([]);
          this.totalCount.set(0);
          this.totalPages.set(0);
          this.loadFailed.set(true);
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again to manage bookings.', 'Bookings');
          } else {
            this.toast.showFailedApi(res, 'Bookings');
          }
        }

        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.bookings.set([]);
        this.totalCount.set(0);
        this.totalPages.set(0);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        const res = err as ApiResult<unknown>;
        if (res && typeof res === 'object' && 'message' in res) {
          this.toast.showFailedApi(res, 'Bookings');
          return;
        }

        this.toast.error('Could not load bookings.', 'Bookings');
      },
    });
  }

  applyFilters(): void {
    this.appliedFilters.set({ ...this.draftFilters() });
    this.page.set(1);
    this.load();
  }

  resetFilters(): void {
    this.draftFilters.set({ ...EMPTY_FILTERS });
    this.appliedFilters.set({ ...EMPTY_FILTERS });
    this.page.set(1);
    this.load();
  }

  goToPage(nextPage: number): void {
    if (nextPage < 1 || nextPage > this.totalPages() || nextPage === this.page()) {
      return;
    }

    this.page.set(nextPage);
    this.load();
  }

  onStatusChange(booking: BookingSummaryDto, next: string): void {
    if (next === booking.status) {
      return;
    }

    const previous = booking.status;
    this.setBookingStatus(booking.id, next);
    this.statusUpdatingId.set(booking.id);
    this.api.updateStatus(booking.id, next).subscribe({
      next: (res) => {
        this.statusUpdatingId.set(null);
        if (!res.success || !res.data) {
          this.setBookingStatus(booking.id, previous);
          this.toast.showFailedApi(res, 'Bookings');
          return;
        }

        this.setBookingStatus(booking.id, res.data.status ?? next);
        this.toast.success('Status updated.', 'Bookings');
      },
      error: (err: unknown) => {
        this.statusUpdatingId.set(null);
        this.setBookingStatus(booking.id, previous);
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

  pageRangeLabel(): string {
    const total = this.totalCount();
    if (total === 0) {
      return '0 bookings';
    }

    const start = (this.page() - 1) * this.pageSize() + 1;
    const end = Math.min(this.page() * this.pageSize(), total);
    return `${start}–${end} of ${total}`;
  }

  private setBookingStatus(bookingId: string, status: string): void {
    this.bookings.update((rows) =>
      rows.map((row) => (row.id === bookingId ? { ...row, status } : row)),
    );
  }

  private buildListParams(page: number, filters: BookingFilters): ListBookingsParams {
    return {
      page,
      pageSize: this.pageSize(),
      checkInFrom: filters.checkInFrom.trim() || null,
      checkInTo: filters.checkInTo.trim() || null,
      checkOutFrom: filters.checkOutFrom.trim() || null,
      checkOutTo: filters.checkOutTo.trim() || null,
      stayPhase: filters.stayPhase,
      status: filters.status.trim() || null,
    };
  }

  private filtersActive(filters: BookingFilters): boolean {
    return (
      !!filters.checkInFrom.trim() ||
      !!filters.checkInTo.trim() ||
      !!filters.checkOutFrom.trim() ||
      !!filters.checkOutTo.trim() ||
      filters.stayPhase !== 'All' ||
      !!filters.status.trim()
    );
  }
}
