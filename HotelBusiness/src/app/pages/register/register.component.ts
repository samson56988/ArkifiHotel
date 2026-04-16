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
  apiError: string | null = null;
  apiSuccess: string | null = null;

  onSubmit(): void {
    this.submitted = true;
    this.apiError = null;
    this.apiSuccess = null;

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
      .subscribe((result) => {
        if (result.success) {
          this.apiSuccess = 'Registration successful. We sent an OTP to verify your email.';
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

        this.apiError = result.message ?? 'Unable to complete registration right now.';
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
