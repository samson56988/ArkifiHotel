import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type {
  BusinessSubscriptionDto,
  BusinessSubscriptionPaymentHistoryDto,
  SubscriptionPlanOptionDto,
} from '../../core/models/subscription.models';
import { SubscriptionApiService } from '../../core/services/subscription-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-subscription',
  standalone: true,
  imports: [ DecimalPipe],
  templateUrl: './subscription.component.html',
  styleUrl: './subscription.component.scss',
})
export class SubscriptionComponent implements OnInit {
  private readonly api = inject(SubscriptionApiService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly loadingPayments = signal(true);
  readonly verifying = signal(false);
  readonly paying = signal(false);
  readonly current = signal<BusinessSubscriptionDto | null>(null);
  readonly planOptions = signal<SubscriptionPlanOptionDto[]>([]);
  readonly paymentHistory = signal<BusinessSubscriptionPaymentHistoryDto[]>([]);

  readonly displayPlans = computed(() => {
    const all = this.planOptions();
    const free = all.find((p) => p.code === 'free');
    const proMonthly = all.find((p) => p.code === 'pro-monthly');
    const proYearly = all.find((p) => p.code === 'pro-yearly');
    const cards: SubscriptionPlanOptionDto[] = [];
    if (free) cards.push(free);
    if (proMonthly) cards.push(proMonthly);
    if (proYearly) cards.push(proYearly);
    return cards.length > 0 ? cards : all;
  });

  ngOnInit(): void {
    this.loadSubscription();
    this.loadPlanOptions();
    this.loadPaymentHistory();

    const reference = this.resolvePaymentReference();
    if (reference) {
      this.verifyPaymentReturn(reference);
    }
  }

  loadSubscription(): void {
    this.loading.set(true);
    this.api
      .getCurrent()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.current.set(res.data);
          } else {
            this.toast.showFailedApi(res, 'Could not load subscription');
          }
        },
        error: () => this.toast.error('Could not load subscription.', 'Subscription'),
      });
  }

  loadPlanOptions(): void {
    this.api.listPlanOptions().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.planOptions.set(res.data);
        }
      },
    });
  }

  loadPaymentHistory(): void {
    this.loadingPayments.set(true);
    this.api
      .listPaymentHistory()
      .pipe(finalize(() => this.loadingPayments.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.paymentHistory.set(res.data);
          }
        },
      });
  }

  selectPlan(plan: SubscriptionPlanOptionDto): void {
    if (!plan.canSelect) {
      return;
    }

    if (plan.requiresPayment) {
      this.paying.set(true);
      this.api
        .initializePayment(plan.code)
        .pipe(finalize(() => this.paying.set(false)))
        .subscribe({
          next: (res) => {
            if (res.success && res.data?.paymentUrl) {
              window.location.href = res.data.paymentUrl;
              return;
            }
            this.toast.showFailedApi(res, 'Could not start payment');
          },
          error: (err: unknown) => {
            const message =
              (err as { message?: string })?.message ?? 'Could not start Paystack checkout.';
            this.toast.error(message, 'Payment');
          },
        });
      return;
    }

    this.paying.set(true);
    this.api
      .changePlan(plan.code)
      .pipe(finalize(() => this.paying.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.current.set(res.data);
            this.loadPlanOptions();
            this.loadPaymentHistory();
            this.toast.success('Your plan has been updated.', 'Subscription');
            return;
          }
          this.toast.showFailedApi(res, 'Could not change plan');
        },
        error: (err: unknown) => {
          const message = (err as { message?: string })?.message ?? 'Could not change plan.';
          this.toast.error(message, 'Subscription');
        },
      });
  }

  planActionLabel(plan: SubscriptionPlanOptionDto): string {
    if (plan.changeType === 'Current') {
      return 'Current plan';
    }
    if (plan.changeType === 'Upgrade') {
      return plan.requiresPayment ? 'Upgrade with Paystack' : 'Upgrade';
    }
    if (plan.changeType === 'Renew') {
      return 'Renew with Paystack';
    }
    if (plan.changeType === 'Downgrade') {
      return plan.requiresPayment ? 'Switch with Paystack' : 'Switch to Free';
    }
    return 'Select';
  }

  formatPrice(plan: SubscriptionPlanOptionDto): string {
    if (plan.priceAmount <= 0) {
      return 'Free';
    }
    const formatted = new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: plan.currency || 'NGN',
      maximumFractionDigits: 0,
    }).format(plan.priceAmount);
    if (plan.billingInterval === 'Monthly') {
      return `${formatted}/mo`;
    }
    if (plan.billingInterval === 'Yearly') {
      return `${formatted}/yr`;
    }
    return formatted;
  }

  statusLabel(status: BusinessSubscriptionDto['status']): string {
    switch (status) {
      case 'GracePeriod':
        return 'Grace period';
      case 'Expired':
        return 'Expired';
      default:
        return 'Active';
    }
  }

  formatDate(iso: string | null | undefined): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  formatDateTime(iso: string | null | undefined): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
    });
  }

  paymentStatusClass(status: BusinessSubscriptionPaymentHistoryDto['status']): string {
    switch (status) {
      case 'Completed':
        return 'pay-status pay-status--ok';
      case 'Failed':
        return 'pay-status pay-status--bad';
      default:
        return 'pay-status pay-status--pending';
    }
  }

  private verifyPaymentReturn(reference: string): void {
    this.verifying.set(true);
    this.api
      .verifyPayment(reference)
      .pipe(finalize(() => this.verifying.set(false)))
      .subscribe({
        next: (res) => {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { reference: null, trxref: null },
            queryParamsHandling: 'merge',
            replaceUrl: true,
          });

          if (res.success && res.data) {
            this.current.set(res.data);
            this.loadPlanOptions();
            this.loadPaymentHistory();
            this.toast.success('Payment confirmed. Your subscription is updated.', 'Subscription');
            return;
          }
          this.toast.showFailedApi(res, 'Payment verification failed');
        },
        error: (err: unknown) => {
          const message =
            (err as { message?: string })?.message ?? 'Payment could not be verified.';
          this.toast.error(message, 'Payment');
        },
      });
  }

  private resolvePaymentReference(): string | null {
    const q = this.route.snapshot.queryParamMap;
    return q.get('reference') ?? q.get('trxref');
  }
}
