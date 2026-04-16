import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';

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

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rememberMe: [false],
  });

  submitted = false;
  loading = false;
  apiError: string | null = null;

  onSubmit(): void {
    this.submitted = true;
    this.apiError = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .login(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((result) => {
        if (result.success) {
          if (result.data?.requiresTwoFactor && result.data.challengeId) {
            const email = encodeURIComponent(this.form.controls.email.value.trim());
            const challenge = encodeURIComponent(result.data.challengeId);
            const rememberMe = this.form.controls.rememberMe.value ? '1' : '0';
            void this.router.navigateByUrl(
              `/verify-login-otp?email=${email}&challengeId=${challenge}&rememberMe=${rememberMe}`,
            );
            return;
          }
          void this.router.navigateByUrl('/dashboard');
          return;
        }

        if (result.code === 'EmailNotVerified') {
          const email = encodeURIComponent(this.form.controls.email.value.trim());
          void this.router.navigateByUrl(`/verify-email?email=${email}`);
          return;
        }

        this.apiError = result.message ?? 'Unable to sign in right now.';
      });
  }

  showError(field: 'email' | 'password'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
