import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-verify-login-otp',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-login-otp.component.html',
  styleUrl: './verify-login-otp.component.scss',
})
export class VerifyLoginOtpComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    challengeId: ['', [Validators.required]],
    otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  submitted = false;
  loading = false;

  /** From login query `rememberMe=1|0` — not bound to a hidden input (avoids string/boolean quirks). */
  private rememberMeFromLogin = false;

  constructor() {
    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const challengeId = this.route.snapshot.queryParamMap.get('challengeId') ?? '';
    this.rememberMeFromLogin = this.route.snapshot.queryParamMap.get('rememberMe') === '1';
    this.form.patchValue({ email, challengeId });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const raw = this.form.getRawValue();
    this.authApi
      .verifyLoginOtp({
        email: raw.email,
        challengeId: raw.challengeId,
        otp: raw.otp,
        rememberMe: this.rememberMeFromLogin,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success && result.data?.accessToken) {
            this.authApi.saveSessionFromLogin(this.rememberMeFromLogin, result.data);
            this.toast.success('Signed in. Welcome back.');
            void this.router.navigateByUrl('/dashboard');
            return;
          }

          if (result.success) {
            this.toast.error('Sign-in did not return a token. Try again or contact support.', 'Login verification failed');
            return;
          }

          this.toast.showFailedApi(result, 'Login verification failed');
        },
        error: () => {
          this.toast.error('We could not reach the server. Check your connection and try again.', 'Network error');
        },
      });
  }

  showError(field: 'email' | 'challengeId' | 'otp'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
