import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError } from '../../core/utils/auth-request-error';
import { getApiResultMessage } from '../../core/utils/http-api-result';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    login: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rememberMe: [false],
  });

  submitted = false;
  loading = false;

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .login(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            const loginId = this.form.controls.login.value.trim();

            if (result.data?.requiresPasswordChange) {
              this.toast.info('Set a new password to finish signing in.', 'Temporary password');
              void this.router.navigateByUrl(
                `/change-default-password?login=${encodeURIComponent(loginId)}`,
              );
              return;
            }

            if (result.data?.requiresTwoFactor && result.data.challengeId) {
              const otpEmail =
                result.data.account?.twoFactorEmail?.trim() ||
                (loginId.includes('@') ? loginId : '');
              this.toast.info('Enter the 6-digit code we sent to your email to finish signing in.', 'Check your inbox');
              const email = encodeURIComponent(otpEmail);
              const challenge = encodeURIComponent(result.data.challengeId);
              const rememberMe = this.form.controls.rememberMe.value ? '1' : '0';
              void this.router.navigateByUrl(
                `/verify-login-otp?email=${email}&challengeId=${challenge}&rememberMe=${rememberMe}`,
              );
              return;
            }

            this.toast.success('Signed in. Welcome back.');
            void this.router.navigateByUrl('/dashboard');
            return;
          }

          if (result.code === 'EmailNotVerified') {
            this.toast.warning('Verify your email before signing in.', 'Email not verified');
            const email = encodeURIComponent(this.form.controls.login.value.trim());
            void this.router.navigateByUrl(`/verify-email?email=${email}`);
            return;
          }

          if (result.code === 'AccountBlocked') {
            this.toast.error(
              getApiResultMessage(result, 'Your account has been blocked. Contact your business administrator.'),
              'Account blocked',
            );
            return;
          }

          this.toast.showFailedApi(result, 'Sign in failed');
        },
        error: (err: unknown) =>
          showAuthRequestError(
            this.toast,
            err,
            'Sign in failed',
            {
              EmailNotVerified: (res, email) => {
                this.toast.warning(
                  getApiResultMessage(res, 'Verify your email before signing in.'),
                  'Email not verified',
                );
                void this.router.navigateByUrl(`/verify-email?email=${encodeURIComponent(email)}`);
              },
              AccountBlocked: (res) => {
                this.toast.error(
                  getApiResultMessage(res, 'Your account has been blocked. Contact your business administrator.'),
                  'Account blocked',
                );
              },
            },
            { email: this.form.controls.login.value.trim() },
          ),
      });
  }

  showError(field: 'login' | 'password'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
