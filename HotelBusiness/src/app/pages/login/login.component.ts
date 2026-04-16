import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';

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
    email: ['', [Validators.required, Validators.email]],
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
            if (result.data?.requiresTwoFactor && result.data.challengeId) {
              this.toast.info('Enter the 6-digit code we sent to your email to finish signing in.', 'Check your inbox');
              const email = encodeURIComponent(this.form.controls.email.value.trim());
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
            const email = encodeURIComponent(this.form.controls.email.value.trim());
            void this.router.navigateByUrl(`/verify-email?email=${email}`);
            return;
          }

          this.toast.showFailedApi(result, 'Sign in failed');
        },
        error: () => {
          this.toast.error('We could not reach the server. Check your connection and try again.', 'Network error');
        },
      });
  }

  showError(field: 'email' | 'password'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
