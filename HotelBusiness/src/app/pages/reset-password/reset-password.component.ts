import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError, showAuthSuccessMessage } from '../../core/utils/auth-request-error';

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const password = group.get('newPassword')?.value;
  const confirm = group.get('confirmPassword')?.value;
  if (!password || !confirm) {
    return null;
  }

  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      challengeId: ['', [Validators.required]],
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]],
    },
    { validators: passwordsMatch },
  );

  submitted = false;
  loading = false;

  constructor() {
    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const challengeId = this.route.snapshot.queryParamMap.get('challengeId') ?? '';
    this.form.patchValue({ email, challengeId });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.loading = true;
    this.authApi
      .resetPassword({
        email: raw.email.trim(),
        challengeId: raw.challengeId,
        otp: raw.otp,
        newPassword: raw.newPassword,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            showAuthSuccessMessage(
              this.toast,
              result,
              'Password updated. You can sign in with your new password.',
              'Password reset',
            );
            setTimeout(() => {
              void this.router.navigateByUrl('/login');
            }, 900);
            return;
          }

          this.toast.showFailedApi(result, 'Password reset');
        },
        error: (err: unknown) => showAuthRequestError(this.toast, err, 'Password reset'),
      });
  }

  showError(field: 'email' | 'challengeId' | 'otp' | 'newPassword' | 'confirmPassword'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }

  passwordMismatch(): boolean {
    return (this.submitted || this.form.controls.confirmPassword.touched) && this.form.hasError('passwordMismatch');
  }
}
