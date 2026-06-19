import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import {
  PublicRestaurantOrderApiService,
  type GuestRestaurantOrderVerifyResultDto,
} from '../../core/services/public-restaurant-order-api.service';
import { hotelThemeStyle, formatNaira } from '../../core/utils/hotel-theme';
import type { ApiResult } from '../../core/models/api-result.model';

@Component({
  selector: 'app-restaurant-order-payment-verify',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './restaurant-order-payment-verify.component.html',
  styleUrl: './restaurant-order-payment-verify.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RestaurantOrderPaymentVerifyComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly ctx = inject(StorefrontContextService);
  private readonly orderApi = inject(PublicRestaurantOrderApiService);

  readonly loading = signal(true);
  readonly result = signal<GuestRestaurantOrderVerifyResultDto | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly themeStyle = signal<Record<string, string>>({});

  ngOnInit(): void {
    const slug = this.route.parent?.parent?.snapshot.paramMap.get('slug') ?? '';
    const locationId = this.route.parent?.snapshot.paramMap.get('locationId') ?? null;

    this.ctx.load(slug, locationId).subscribe(() => {
      const sf = this.ctx.storefront();
      if (sf) {
        this.themeStyle.set(hotelThemeStyle(sf.theme));
      }

      const reference = this.resolveReference();
      if (!reference || !slug) {
        this.loading.set(false);
        this.errorMessage.set('Payment reference is missing from the return URL.');
        return;
      }

      this.orderApi.verifyPayment(slug, reference).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.result.set(res.data);
          } else {
            this.errorMessage.set(res.message ?? 'Payment could not be verified.');
          }
        },
        error: (err: unknown) => {
          this.loading.set(false);
          const r = err as ApiResult<GuestRestaurantOrderVerifyResultDto>;
          this.errorMessage.set(r?.message ?? 'Could not verify payment. Please contact the hotel.');
        },
      });
    });
  }

  formatPrice(amount: number, currency: string): string {
    if (currency === 'NGN') {
      return formatNaira(amount);
    }
    return `${currency} ${amount.toLocaleString()}`;
  }

  guestTypeLabel(type: string): string {
    return type === 'roomGuest' ? 'Room guest' : 'Dining in restaurant';
  }

  private resolveReference(): string | null {
    const q = this.route.snapshot.queryParamMap;
    return (
      q.get('reference') ??
      q.get('trxref') ??
      q.get('tx_ref') ??
      q.get('paymentReference') ??
      q.get('transactionReference')
    );
  }
}
