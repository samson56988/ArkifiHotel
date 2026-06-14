import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError, showAuthSuccessMessage } from '../../core/utils/auth-request-error';

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
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  });

  submitted = false;
  loading = false;

  constructor() {
    const email = this.route.snapshot.queryParamMap.get('email');
    if (email) {
      this.form.controls.email.setValue(email);
    }
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .verifyEmailOtp(this.form.getRawValue())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            showAuthSuccessMessage(
              this.toast,
              result,
              'Your email is verified. You can sign in now.',
              'Email verified',
            );
            setTimeout(() => {
              void this.router.navigateByUrl('/login');
            }, 900);
            return;
          }

          this.toast.showFailedApi(result, 'Verification failed');
        },
        error: (err: unknown) => showAuthRequestError(this.toast, err, 'Verification failed'),
      });
  }

  showError(field: 'email' | 'otp'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
