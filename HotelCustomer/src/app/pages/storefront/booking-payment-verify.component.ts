import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { StorefrontContextService } from '../../core/services/storefront-context.service';
import {
  PublicGuestBookingApiService,
  type GuestPaymentVerifyResultDto,
} from '../../core/services/public-guest-booking-api.service';
import { hotelThemeStyle, formatNaira } from '../../core/utils/hotel-theme';
import type { ApiResult } from '../../core/models/api-result.model';

@Component({
  selector: 'app-booking-payment-verify',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './booking-payment-verify.component.html',
  styleUrl: './booking-payment-verify.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BookingPaymentVerifyComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  readonly ctx = inject(StorefrontContextService);
  private readonly bookingApi = inject(PublicGuestBookingApiService);

  readonly loading = signal(true);
  readonly result = signal<GuestPaymentVerifyResultDto | null>(null);
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

      this.bookingApi.verifyPayment(slug, reference).subscribe({
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
          const r = err as ApiResult<GuestPaymentVerifyResultDto>;
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
