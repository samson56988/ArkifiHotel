import { ChangeDetectionStrategy, Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { SubscriptionPlanDto } from '../../core/models/subscription.models';
import { SubscriptionApiService } from '../../core/services/subscription-api.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {
  private readonly subscriptionApi = inject(SubscriptionApiService);

  readonly year = new Date().getFullYear();
  readonly navScrolled = signal(false);
  readonly plans = signal<SubscriptionPlanDto[]>([]);
  readonly plansLoading = signal(true);

  readonly displayPlans = signal<SubscriptionPlanDto[]>([]);

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.navScrolled.set(window.scrollY > 60);
  }

  ngOnInit(): void {
    this.subscriptionApi.listPublicPlans().subscribe({
      next: (res) => {
        this.plansLoading.set(false);
        if (!res.success || !res.data) {
          return;
        }
        this.plans.set(res.data);
        const free = res.data.find((p) => p.code === 'free');
        const proMonthly = res.data.find((p) => p.code === 'pro-monthly');
        const proYearly = res.data.find((p) => p.code === 'pro-yearly');
        const cards = [free, proMonthly, proYearly].filter((p): p is SubscriptionPlanDto => !!p);
        this.displayPlans.set(cards.length > 0 ? cards : res.data);
      },
      error: () => this.plansLoading.set(false),
    });
  }

  formatPlanPrice(plan: SubscriptionPlanDto): string {
    if (plan.priceAmount <= 0) {
      return 'Free';
    }
    const amount = new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: plan.currency || 'NGN',
      maximumFractionDigits: 0,
    }).format(plan.priceAmount);
    if (plan.billingInterval === 'Monthly') {
      return `${amount}/mo`;
    }
    if (plan.billingInterval === 'Yearly') {
      return `${amount}/yr`;
    }
    return amount;
  }

  planTitle(plan: SubscriptionPlanDto): string {
    if (plan.code === 'pro-yearly') {
      return 'Pro (Yearly)';
    }
    return plan.name;
  }
}
