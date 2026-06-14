import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError } from '../../core/utils/auth-request-error';
import { getApiResultMessage } from '../../core/utils/http-api-result';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  submitted = false;
  loading = false;

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const email = this.form.controls.email.value.trim();
    this.loading = true;
    this.authApi
      .requestPasswordReset({ email })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (!result.success) {
            this.toast.showFailedApi(result, 'Password reset');
            return;
          }

          this.toast.info(
            getApiResultMessage(
              result,
              'If an account exists for this email, we sent a 6-digit reset code. Check your inbox.',
            ),
            'Check your inbox',
          );

          const challengeId = result.data?.challengeId;
          if (challengeId) {
            const qEmail = encodeURIComponent(email);
            const qChallenge = encodeURIComponent(challengeId);
            void this.router.navigateByUrl(`/reset-password?email=${qEmail}&challengeId=${qChallenge}`);
          }
        },
        error: (err: unknown) => showAuthRequestError(this.toast, err, 'Password reset'),
      });
  }

  showError(field: 'email'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
