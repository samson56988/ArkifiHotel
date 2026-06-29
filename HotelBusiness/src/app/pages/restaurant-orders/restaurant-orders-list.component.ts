import { DatePipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import type { RestaurantOrderListItemDto } from '../../core/models/restaurant-order.models';
import { BusinessRestaurantOrdersApiService } from '../../core/services/business-restaurant-orders-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-restaurant-orders-list',
  standalone: true,
  imports: [ DatePipe, FormsModule, RouterLink],
  templateUrl: './restaurant-orders-list.component.html',
  styleUrl: './restaurant-orders-list.component.scss',
})
export class RestaurantOrdersListComponent implements OnInit {
  private readonly api = inject(BusinessRestaurantOrdersApiService);
  private readonly toast = inject(ToastService);

  readonly orders = signal<RestaurantOrderListItemDto[]>([]);
  readonly selectedOrderId = signal<string | null>(null);
  readonly detailLoading = signal(false);
  readonly detail = signal<import('../../core/models/restaurant-order.models').RestaurantOrderDetailDto | null>(null);

  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  readonly statusFilter = signal('');
  readonly loading = signal(false);
  readonly loadFailed = signal(false);
  readonly initialLoadDone = signal(false);

  readonly statusOptions = ['Pending', 'Paid', 'Cancelled'] as const;

  readonly pageRangeLabel = computed(() => {
    const total = this.totalCount();
    if (total === 0) {
      return '0 orders';
    }
    const start = (this.page() - 1) * this.pageSize() + 1;
    const end = Math.min(this.page() * this.pageSize(), total);
    return `${start}–${end} of ${total}`;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);

    this.api
      .list({
        page: this.page(),
        pageSize: this.pageSize(),
        status: this.statusFilter() || undefined,
      })
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.orders.set(res.data.items ?? []);
            this.page.set(res.data.page);
            this.pageSize.set(res.data.pageSize);
            this.totalCount.set(res.data.totalCount);
            this.totalPages.set(res.data.totalPages);
          } else {
            this.orders.set([]);
            this.loadFailed.set(true);
            this.toast.showFailedApi(res, 'Orders');
          }
          this.initialLoadDone.set(true);
          this.loading.set(false);
        },
        error: () => {
          this.orders.set([]);
          this.loadFailed.set(true);
          this.initialLoadDone.set(true);
          this.loading.set(false);
          this.toast.error('Could not load restaurant orders.', 'Orders');
        },
      });
  }

  applyStatusFilter(): void {
    this.page.set(1);
    this.load();
  }

  resetFilters(): void {
    this.statusFilter.set('');
    this.page.set(1);
    this.load();
  }

  goToPage(next: number): void {
    if (next < 1 || next > this.totalPages()) {
      return;
    }
    this.page.set(next);
    this.load();
  }

  openDetail(orderId: string): void {
    this.selectedOrderId.set(orderId);
    this.detailLoading.set(true);
    this.detail.set(null);

    this.api.get(orderId).subscribe({
      next: (res) => {
        this.detailLoading.set(false);
        if (res.success && res.data) {
          this.detail.set(res.data);
        } else {
          this.toast.showFailedApi(res, 'Orders');
          this.selectedOrderId.set(null);
        }
      },
      error: () => {
        this.detailLoading.set(false);
        this.selectedOrderId.set(null);
        this.toast.error('Could not load order details.', 'Orders');
      },
    });
  }

  closeDetail(): void {
    this.selectedOrderId.set(null);
    this.detail.set(null);
  }

  guestTypeLabel(type: string): string {
    return type === 'roomGuest' ? 'Room guest' : 'In restaurant';
  }

  formatPrice(amount: number, currency: string): string {
    if (currency === 'NGN') {
      return `₦${amount.toLocaleString('en-NG', { maximumFractionDigits: 0 })}`;
    }
    return `${currency} ${amount.toLocaleString()}`;
  }
}
