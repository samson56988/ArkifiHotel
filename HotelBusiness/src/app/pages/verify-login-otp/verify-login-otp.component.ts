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
    rememberMe: [false],
  });

  submitted = false;
  loading = false;

  constructor() {
    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const challengeId = this.route.snapshot.queryParamMap.get('challengeId') ?? '';
    const rememberMe = this.route.snapshot.queryParamMap.get('rememberMe') === '1';
    this.form.patchValue({ email, challengeId, rememberMe });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .verifyLoginOtp(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            this.toast.success('Signed in. Welcome back.');
            void this.router.navigateByUrl('/dashboard');
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
