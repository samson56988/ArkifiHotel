import { Component, computed, input } from '@angular/core';
import type { DashboardRevenueTrendPointDto } from '../../core/models/dashboard.models';

type ChartBar = {
  date: string;
  label: string;
  bookingHeight: number;
  restaurantHeight: number;
  total: number;
  booking: number;
  restaurant: number;
  showLabel: boolean;
};

@Component({
  selector: 'app-dashboard-revenue-chart',
  standalone: true,
  templateUrl: './dashboard-revenue-chart.component.html',
  styleUrl: './dashboard-revenue-chart.component.scss',
})
export class DashboardRevenueChartComponent {
  readonly points = input.required<DashboardRevenueTrendPointDto[]>();
  readonly currency = input('NGN');

  readonly bars = computed<ChartBar[]>(() => {
    const points = this.points();
    if (points.length === 0) {
      return [];
    }

    const max = Math.max(...points.map((p) => p.totalRevenue), 1);
    const labelEvery = points.length <= 10 ? 1 : Math.ceil(points.length / 8);

    return points.map((point, index) => ({
      date: point.date,
      label: this.formatAxisLabel(point.date),
      bookingHeight: (point.bookingRevenue / max) * 100,
      restaurantHeight: (point.restaurantRevenue / max) * 100,
      total: point.totalRevenue,
      booking: point.bookingRevenue,
      restaurant: point.restaurantRevenue,
      showLabel: index % labelEvery === 0 || index === points.length - 1,
    }));
  });

  readonly hasData = computed(() => this.bars().some((bar) => bar.total > 0));

  formatMoney(amount: number): string {
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: this.currency(),
      maximumFractionDigits: 0,
    }).format(amount);
  }

  private formatAxisLabel(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
    });
  }
}
