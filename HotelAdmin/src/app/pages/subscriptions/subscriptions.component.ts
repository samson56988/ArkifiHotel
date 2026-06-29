import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs/operators';
import type { PlatformSubscriptionPayment, SubscriptionPlan } from '../../core/models/platform.models';
import { PlatformApiService } from '../../core/services/platform-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './subscriptions.component.html',
  styleUrl: './subscriptions.component.scss',
})
export class SubscriptionsComponent implements OnInit {
  private readonly api = inject(PlatformApiService);
  private readonly toast = inject(ToastService);

  readonly plans = signal<SubscriptionPlan[]>([]);
  readonly payments = signal<PlatformSubscriptionPayment[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    let pending = 2;
    const done = () => {
      pending -= 1;
      if (pending === 0) {
        this.loading.set(false);
      }
    };

    this.api
      .listPlans()
      .pipe(finalize(done))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.plans.set(result.data);
          }
        },
        error: () => this.toast.error('Could not load plans.', 'Subscriptions'),
      });

    this.api
      .listPayments()
      .pipe(finalize(done))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.payments.set(result.data);
          }
        },
        error: () => this.toast.error('Could not load payments.', 'Subscriptions'),
      });
  }
}
