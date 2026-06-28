import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';
import { getApiResultMessage } from '../../core/utils/http-api-result';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
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

    const { email, password, rememberMe } = this.form.getRawValue();
    this.loading = true;
    this.authApi
      .login({ email: email.trim(), password }, rememberMe)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            this.toast.success('Signed in. Welcome back.');
            void this.router.navigateByUrl('/dashboard');
            return;
          }
          this.toast.error(getApiResultMessage(result, 'Sign in failed.'), 'Sign in failed');
        },
        error: (err: unknown) => {
          const result = err as { message?: string };
          this.toast.error(result.message ?? 'Sign in failed.', 'Sign in failed');
        },
      });
  }

  showError(control: 'email' | 'password'): boolean {
    const c = this.form.controls[control];
    return (this.submitted || c.touched) && c.invalid;
  }
}
