import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';

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

  readonly form = this.fb.nonNullable.group(
    {
      propertyName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, Validators.requiredTrue],
    },
    { validators: [passwordsMatch] },
  );

  submitted = false;

  onSubmit(): void {
    this.submitted = true;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    // Wire to API when ready
  }

  showError(field: 'propertyName' | 'email' | 'password' | 'confirmPassword'): boolean {
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
