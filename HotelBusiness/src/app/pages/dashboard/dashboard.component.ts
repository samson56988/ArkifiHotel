import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

type Kpi = { label: string; value: string; delta: string; trend: 'up' | 'down' };
type Booking = { guest: string; unit: string; checkIn: string; nights: number; status: string };
type UnitPerformance = { name: string; occupancy: number; revenue: string };

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, BusinessWorkspaceComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  readonly kpis: Kpi[] = [
    { label: 'Monthly Revenue', value: 'NGN 8.4M', delta: '+14.2%', trend: 'up' },
    { label: 'Occupancy Rate', value: '78%', delta: '+6.1%', trend: 'up' },
    { label: 'Active Listings', value: '26', delta: '+3', trend: 'up' },
    { label: 'Pending Payouts', value: 'NGN 1.2M', delta: '-8.4%', trend: 'down' },
  ];

  readonly bookings: Booking[] = [
    { guest: 'Ada Nwosu', unit: 'Deluxe Suite A12', checkIn: 'Apr 19', nights: 3, status: 'Confirmed' },
    { guest: 'Tunde Adeyemi', unit: 'City View 2B', checkIn: 'Apr 20', nights: 2, status: 'Checked-in' },
    { guest: 'Grace Okafor', unit: 'Executive Loft 4C', checkIn: 'Apr 22', nights: 5, status: 'Pending' },
    { guest: 'Samuel Bello', unit: 'Shortlet 1BR L9', checkIn: 'Apr 23', nights: 1, status: 'Confirmed' },
  ];

  readonly units: UnitPerformance[] = [
    { name: 'Deluxe Suites', occupancy: 86, revenue: 'NGN 3.1M' },
    { name: 'Executive Lofts', occupancy: 72, revenue: 'NGN 2.4M' },
    { name: 'Shortlet 1BR', occupancy: 79, revenue: 'NGN 1.6M' },
    { name: 'Family Rooms', occupancy: 65, revenue: 'NGN 1.3M' },
  ];
}
