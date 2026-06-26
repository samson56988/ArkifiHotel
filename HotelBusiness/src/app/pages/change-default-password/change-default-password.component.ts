import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError } from '../../core/utils/auth-request-error';

@Component({
  selector: 'app-change-default-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './change-default-password.component.html',
  styleUrl: './change-default-password.component.scss',
})
export class ChangeDefaultPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authApi = inject(AuthApiService);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    login: [this.route.snapshot.queryParamMap.get('login') ?? '', [Validators.required, Validators.minLength(3)]],
    currentPassword: ['', [Validators.required, Validators.minLength(8)]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]],
  });

  submitted = false;
  loading = false;

  onSubmit(): void {
    this.submitted = true;
    const raw = this.form.getRawValue();
    if (raw.newPassword !== raw.confirmPassword) {
      this.toast.warning('New passwords do not match.', 'Password');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.authApi
      .changeDefaultPassword({
        login: raw.login.trim(),
        currentPassword: raw.currentPassword,
        newPassword: raw.newPassword,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            this.toast.success('Password updated. Sign in with your new password.', 'Password');
            void this.router.navigateByUrl('/login');
            return;
          }

          if (result.code === 'AccountBlocked') {
            this.toast.error(
              result.message ?? 'Your account has been blocked. Contact your business administrator.',
              'Account blocked',
            );
            return;
          }

          this.toast.showFailedApi(result, 'Password');
        },
        error: (err: unknown) =>
          showAuthRequestError(this.toast, err, 'Password', {
            AccountBlocked: (res) => {
              this.toast.error(
                res.message ?? 'Your account has been blocked. Contact your business administrator.',
                'Account blocked',
              );
            },
          }),
      });
  }

  showError(field: 'login' | 'currentPassword' | 'newPassword' | 'confirmPassword'): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }
}
