import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import { BusinessPaymentApiService } from '../../core/services/business-payment-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-payment-configuration',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './payment-configuration.component.html',
  styleUrl: './payment-configuration.component.scss',
})
export class PaymentConfigurationComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessPaymentApiService);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    provider: this.fb.nonNullable.control('None', { validators: [Validators.required] }),
    secretKey: [''],
  });

  /** True while fetching current settings (form stays usable). */
  hydrating = false;
  loadFailed = false;
  saving = false;
  /** Server had a key on last successful load (before edits). */
  hadSecretOnLoad = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.hydrating = true;
    this.loadFailed = false;
    this.api
      .getConfiguration()
      .pipe(finalize(() => (this.hydrating = false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.loadFailed = true;
            if (res.code === 'Unauthorized' || res.message?.includes('401')) {
              this.toast.warning('Please sign in again.', 'Payments');
              return;
            }

            this.toast.showFailedApi(res, 'Payments');
            return;
          }

          const d = res.data;
          const p = d?.provider ?? 'None';
          const provider: 'None' | 'Paystack' | 'Flutterwave' =
            p === 'Paystack' ? 'Paystack' : p === 'Flutterwave' ? 'Flutterwave' : 'None';
          this.form.patchValue({
            provider,
            secretKey: '',
          });
          this.hadSecretOnLoad = d?.hasSecretKey ?? false;
        },
        error: (err: unknown) => {
          this.loadFailed = true;
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

    if (raw.provider !== 'None' && !secret && !this.hadSecretOnLoad) {
      this.toast.warning('Enter your secret key from Paystack or Flutterwave.', 'Payments');
      return;
    }

    this.saving = true;
    this.api
      .updateConfiguration({
        provider: raw.provider,
        secretKey: secret || null,
      })
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Payments');
            return;
          }

          this.form.patchValue({ secretKey: '' });
          this.hadSecretOnLoad = res.data.hasSecretKey;
          this.toast.success('Payment configuration saved.', 'Payments');
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
}
