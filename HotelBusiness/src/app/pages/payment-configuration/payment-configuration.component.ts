import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { PaymentConfigurationDto, PaymentProvider } from '../../core/models/payment.models';
import { BusinessPaymentApiService } from '../../core/services/business-payment-api.service';
import { ToastService } from '../../core/services/toast.service';

function parseProvider(value: string | null | undefined): PaymentProvider {
  switch (value) {
    case 'Paystack':
    case 'Flutterwave':
    case 'Monify':
      return value;
    default:
      return 'None';
  }
}

const GATEWAY_HINTS: Record<Exclude<PaymentProvider, 'None'>, string> = {
  Paystack: 'Accept card and bank payments via Paystack.',
  Flutterwave: 'Accept card and bank payments via Flutterwave.',
  Monify: 'Accept payments via Monify (API key, secret, and contract code).',
};

@Component({
  selector: 'app-payment-configuration',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './payment-configuration.component.html',
  styleUrl: './payment-configuration.component.scss',
})
export class PaymentConfigurationComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessPaymentApiService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = this.fb.nonNullable.group({
    provider: this.fb.nonNullable.control<PaymentProvider>('None', { validators: [Validators.required] }),
    secretKey: [''],
    apiKey: [''],
    contractCode: [''],
  });

  readonly currentConfig = signal<PaymentConfigurationDto | null>(null);
  readonly editing = signal(false);
  readonly hydrating = signal(false);
  readonly loadFailed = signal(false);
  readonly saving = signal(false);
  readonly hadSecretOnLoad = signal(false);
  readonly hadApiKeyOnLoad = signal(false);
  readonly hadContractCodeOnLoad = signal(false);

  readonly hasActiveConfig = computed(() => {
    const c = this.currentConfig();
    return !!c?.isConfigured && parseProvider(c.provider) !== 'None';
  });

  ngOnInit(): void {
    this.load();
    this.form.controls.provider.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((provider) => {
      this.syncCredentialFlagsForProvider(provider);
    });
  }

  gatewayHint(provider: string): string {
    const parsed = parseProvider(provider);
    if (parsed === 'None') {
      return '';
    }

    return GATEWAY_HINTS[parsed];
  }

  credentialItems(config: PaymentConfigurationDto): { label: string; saved: boolean }[] {
    const provider = parseProvider(config.provider);
    if (provider === 'Monify') {
      return [
        { label: 'API key', saved: config.hasApiKey },
        { label: 'Secret key', saved: config.hasSecretKey },
        { label: 'Contract code', saved: config.hasContractCode }];
    }

    return [{ label: 'Secret key', saved: config.hasSecretKey }];
  }

  startEdit(): void {
    const config = this.currentConfig();
    if (!config) {
      this.editing.set(true);
      return;
    }

    this.applyConfigToForm(config);
    this.editing.set(true);
  }

  cancelEdit(): void {
    const config = this.currentConfig();
    if (config && this.hasActiveConfig()) {
      this.applyConfigToForm(config);
      this.editing.set(false);
      return;
    }

    this.editing.set(true);
  }

  load(): void {
    this.hydrating.set(true);
    this.loadFailed.set(false);
    this.api
      .getConfiguration()
      .pipe(finalize(() => this.hydrating.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.loadFailed.set(true);
            if (res.code === 'Unauthorized' || res.message?.includes('401')) {
              this.toast.warning('Please sign in again.', 'Payments');
              return;
            }

            this.toast.showFailedApi(res, 'Payments');
            return;
          }

          const d = res.data ?? null;
          this.currentConfig.set(d);
          this.applyConfigToForm(d);
          const active = !!d?.isConfigured && parseProvider(d.provider) !== 'None';
          this.editing.set(!active);
        },
        error: (err: unknown) => {
          this.loadFailed.set(true);
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Payments');
            return;
          }

          this.toast.error('Could not load payment settings.', 'Payments');
        },
      });
  }

  onSubmit(): void {
    const raw = this.form.getRawValue();
    const secret = raw.secretKey.trim();
    const apiKey = raw.apiKey.trim();
    const contractCode = raw.contractCode.trim();

    if (raw.provider === 'Paystack' || raw.provider === 'Flutterwave') {
      if (!secret && !this.hadSecretOnLoad()) {
        this.toast.warning('Enter your secret key.', 'Payments');
        return;
      }
    }

    if (raw.provider === 'Monify') {
      if (!secret && !this.hadSecretOnLoad()) {
        this.toast.warning('Enter your Monify secret key.', 'Payments');
        return;
      }

      if (!apiKey && !this.hadApiKeyOnLoad()) {
        this.toast.warning('Enter your Monify API key.', 'Payments');
        return;
      }

      if (!contractCode && !this.hadContractCodeOnLoad()) {
        this.toast.warning('Enter your Monify contract code.', 'Payments');
        return;
      }
    }

    this.saving.set(true);
    this.api
      .updateConfiguration({
        provider: raw.provider,
        secretKey: secret || null,
        apiKey: apiKey || null,
        contractCode: contractCode || null,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Payments');
            return;
          }

          this.currentConfig.set(res.data);
          this.applyConfigToForm(res.data);
          const active = res.data.isConfigured && parseProvider(res.data.provider) !== 'None';
          this.editing.set(!active);
          this.toast.success(
            active ? 'Payment configuration updated.' : 'Payment gateway removed.',
            'Payments',
          );
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Payments');
            return;
          }

          this.toast.error('Could not save payment settings.', 'Payments');
        },
      });
  }

  private applyConfigToForm(d: PaymentConfigurationDto | null): void {
    this.form.patchValue({
      provider: parseProvider(d?.provider),
      secretKey: '',
      apiKey: '',
      contractCode: '',
    });
    this.syncCredentialFlagsForProvider(parseProvider(d?.provider));
  }

  private syncCredentialFlagsForProvider(provider: PaymentProvider): void {
    const savedProvider = parseProvider(this.currentConfig()?.provider);
    const config = this.currentConfig();

    if (provider !== savedProvider || !config?.isConfigured) {
      this.hadSecretOnLoad.set(false);
      this.hadApiKeyOnLoad.set(false);
      this.hadContractCodeOnLoad.set(false);
      return;
    }

    this.hadSecretOnLoad.set(config.hasSecretKey);
    this.hadApiKeyOnLoad.set(config.hasApiKey);
    this.hadContractCodeOnLoad.set(config.hasContractCode);
  }
}
