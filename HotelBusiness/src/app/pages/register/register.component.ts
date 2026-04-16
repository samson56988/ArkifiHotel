import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthApiService } from '../../core/services/auth-api.service';
import { ToastService } from '../../core/services/toast.service';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  if (password && confirm && password !== confirm) {
    return { mismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group(
    {
      propertyName: ['', [Validators.required, Validators.minLength(2)]],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.minLength(7)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, Validators.requiredTrue],
    },
    { validators: [passwordsMatch] },
  );

  submitted = false;
  loading = false;

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.loading = true;
    this.authApi
      .register({
        businessName: raw.propertyName.trim(),
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        email: raw.email.trim(),
        phoneNumber: raw.phoneNumber.trim(),
        password: raw.password,
        acceptTerms: raw.acceptTerms,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            this.toast.success('We sent a verification code to your email.', 'Account created');
            this.form.reset({
              propertyName: '',
              firstName: '',
              lastName: '',
              email: '',
              phoneNumber: '',
              password: '',
              confirmPassword: '',
              acceptTerms: false,
            });
            this.submitted = false;
            setTimeout(() => {
              const email = encodeURIComponent(raw.email.trim());
              void this.router.navigateByUrl(`/verify-email?email=${email}`);
            }, 700);
            return;
          }

          this.toast.showFailedApi(result, 'Registration failed');
        },
        error: () => {
          this.toast.error('We could not reach the server. Check your connection and try again.', 'Network error');
        },
      });
  }

  showError(
    field: 'propertyName' | 'firstName' | 'lastName' | 'email' | 'phoneNumber' | 'password' | 'confirmPassword',
  ): boolean {
    const c = this.form.controls[field];
    return (c.touched || this.submitted) && c.invalid;
  }

  showMismatch(): boolean {
    return (
      (this.form.touched || this.submitted) &&
      this.form.hasError('mismatch') &&
      this.form.controls.confirmPassword.value.length > 0
    );
  }

  showTermsError(): boolean {
    const c = this.form.controls.acceptTerms;
    return (c.touched || this.submitted) && c.invalid;
  }
}
