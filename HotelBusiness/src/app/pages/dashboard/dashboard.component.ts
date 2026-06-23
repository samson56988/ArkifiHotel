import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type {
  BusinessDashboardDto,
  DashboardDateRange,
  DashboardRangePreset,
} from '../../core/models/dashboard.models';
import type { BusinessSubscriptionDto } from '../../core/models/subscription.models';
import { DashboardApiService } from '../../core/services/dashboard-api.service';
import { SubscriptionApiService } from '../../core/services/subscription-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';
import { DashboardRevenueChartComponent } from './dashboard-revenue-chart.component';

type KpiCard = {
  label: string;
  value: string;
  delta: string | null;
  trend: 'up' | 'down' | 'flat';
  hint?: string;
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, FormsModule, BusinessWorkspaceComponent, DashboardRevenueChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly subscriptionApi = inject(SubscriptionApiService);
  private readonly toast = inject(ToastService);

  readonly rangePreset = signal<DashboardRangePreset>('thisMonth');
  customFrom = '';
  customTo = '';

  readonly dashboard = signal<BusinessDashboardDto | null>(null);
  readonly dashboardLoading = signal(true);
  readonly subscription = signal<BusinessSubscriptionDto | null>(null);
  readonly subscriptionLoading = signal(true);

  readonly periodLabel = computed(() => {
    const data = this.dashboard();
    if (!data) {
      return '';
    }
    return `${this.formatPeriodDate(data.periodStart)} – ${this.formatPeriodDate(data.periodEnd)}`;
  });

  readonly kpis = computed<KpiCard[]>(() => {
    const data = this.dashboard();
    if (!data) {
      return [];
    }

    return [
      {
        label: 'Total income',
        value: this.formatMoney(data.totalRevenue, data.currency),
        delta: this.formatDelta(data.totalRevenueChangePercent),
        trend: this.deltaTrend(data.totalRevenueChangePercent),
        hint: `Bookings ${this.formatMoney(data.bookingRevenue, data.currency)} · Restaurant ${this.formatMoney(data.restaurantRevenue, data.currency)}`,
      },
      {
        label: 'Occupancy today',
        value: `${data.occupancyRatePercent}%`,
        delta: this.formatDelta(data.occupancyChangePercent, 'pts'),
        trend: this.deltaTrend(data.occupancyChangePercent),
        hint: `${data.activeStaysToday} active stay(s)`,
      },
      {
        label: 'Room inventory',
        value: `${data.totalRoomUnits}`,
        delta: null,
        trend: 'flat',
        hint: `${data.roomTypesCount} room type(s)`,
      },
      {
        label: 'Pending items',
        value: `${data.pendingRestaurantOrders + data.pendingEventHallRequests}`,
        delta: null,
        trend: 'flat',
        hint: `${data.pendingRestaurantOrders} order(s) · ${data.pendingEventHallRequests} event request(s)`,
      },
    ];
  });

  ngOnInit(): void {
    const initial = this.resolvePresetRange('thisMonth');
    this.customFrom = initial.from;
    this.customTo = initial.to;
    this.loadDashboard(initial);

    this.subscriptionApi
      .getCurrent()
      .pipe(finalize(() => this.subscriptionLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.subscription.set(res.data);
          }
        },
      });
  }

  setRangePreset(preset: DashboardRangePreset): void {
    this.rangePreset.set(preset);
    if (preset === 'custom') {
      return;
    }

    const range = this.resolvePresetRange(preset);
    this.customFrom = range.from;
    this.customTo = range.to;
    this.loadDashboard(range);
  }

  applyCustomRange(): void {
    const from = this.customFrom.trim();
    const to = this.customTo.trim();

    if (!from || !to) {
      this.toast.error('Choose both a start and end date.', 'Dashboard');
      return;
    }

    if (to < from) {
      this.toast.error('End date must be on or after start date.', 'Dashboard');
      return;
    }

    this.rangePreset.set('custom');
    this.loadDashboard({ from, to });
  }

  loadDashboard(range?: DashboardDateRange): void {
    this.dashboardLoading.set(true);
    this.dashboardApi
      .getDashboard(range)
      .pipe(finalize(() => this.dashboardLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.dashboard.set(res.data);
            return;
          }
          this.toast.showFailedApi(res, 'Could not load dashboard');
        },
        error: () => this.toast.error('Could not load dashboard data.', 'Dashboard'),
      });
  }

  subscriptionStatusLabel(sub: BusinessSubscriptionDto): string {
    switch (sub.status) {
      case 'GracePeriod':
        return 'Grace period';
      case 'Expired':
        return 'Expired';
      default:
        return 'Active';
    }
  }

  formatExpiry(iso: string | null | undefined): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  formatPeriodDate(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }

  formatCheckIn(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    });
  }

  formatMoney(amount: number, currency = 'NGN'): string {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  bookingStatusClass(status: string): string {
    switch (status) {
      case 'Confirmed':
      case 'Completed':
        return 'status status--ok';
      case 'Cancelled':
        return 'status status--bad';
      default:
        return 'status status--pending';
    }
  }

  private resolvePresetRange(preset: DashboardRangePreset): DashboardDateRange {
    const today = startOfUtcDay(new Date());
    const to = formatDateOnly(today);

    switch (preset) {
      case 'last7': {
        const fromDate = addDays(today, -6);
        return { from: formatDateOnly(fromDate), to };
      }
      case 'last30': {
        const fromDate = addDays(today, -29);
        return { from: formatDateOnly(fromDate), to };
      }
      case 'thisMonth':
      default: {
        const fromDate = new Date(Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), 1));
        return { from: formatDateOnly(fromDate), to };
      }
    }
  }

  private formatDelta(change: number | null | undefined, suffix = '%'): string | null {
    if (change === null || change === undefined) {
      return null;
    }
    const sign = change > 0 ? '+' : '';
    return `${sign}${change}${suffix} vs previous period`;
  }

  private deltaTrend(change: number | null | undefined): 'up' | 'down' | 'flat' {
    if (change === null || change === undefined || change === 0) {
      return 'flat';
    }
    return change > 0 ? 'up' : 'down';
  }
}

function startOfUtcDay(date: Date): Date {
  return new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()));
}

function addDays(date: Date, days: number): Date {
  const next = new Date(date);
  next.setUTCDate(next.getUTCDate() + days);
  return next;
}

function formatDateOnly(date: Date): string {
  const year = date.getUTCFullYear();
  const month = String(date.getUTCMonth() + 1).padStart(2, '0');
  const day = String(date.getUTCDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
