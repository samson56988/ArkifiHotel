import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-email.component.html',
  styleUrl: './verify-email.component.scss',
})
export class VerifyEmailComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  submitted = false;
  loading = false;
  apiError: string | null = null;
  apiSuccess: string | null = null;

  constructor() {
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email) {
      this.form.controls.email.setValue(email);
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.apiError = null;
    this.apiSuccess = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .verifyEmailOtp(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((result) => {
        if (result.success) {
          this.apiSuccess = result.message ?? 'Email verified successfully.';
          setTimeout(() => {
            void this.router.navigateByUrl('/login');
          }, 900);
          return;
        }

        this.apiError = result.message ?? 'Unable to verify OTP.';
      });
  }

  showError(field: 'email' | 'otp'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
