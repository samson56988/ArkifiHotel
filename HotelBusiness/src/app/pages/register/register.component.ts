import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { debounceTime, distinctUntilChanged, finalize, switchMap } from 'rxjs/operators';
import { buildStorefrontUrl, CUSTOMER_STOREFRONT_BASE_URL } from '../../core/constants/storefront';
import { AuthApiService } from '../../core/services/auth-api.service';
import { RegistrationApiService } from '../../core/services/registration-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError } from '../../core/utils/auth-request-error';
import {
  isValidBusinessSlug,
  normalizeBusinessSlug,
  suggestSlugFromBusinessName,
} from '../../core/utils/business-slug';

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
  private readonly registrationApi = inject(RegistrationApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly storefrontBase = CUSTOMER_STOREFRONT_BASE_URL;

  readonly form = this.fb.nonNullable.group(
    {
      propertyName: ['', [Validators.required, Validators.minLength(2)]],
      slug: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(128)]],
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
  slugChecking = false;
  slugAvailable: boolean | null = null;
  slugTouched = false;

  constructor() {
    this.form.controls.propertyName.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((name) => {
      if (!this.form.controls.slug.dirty) {
        this.form.controls.slug.setValue(suggestSlugFromBusinessName(name));
      }
    });

    this.form.controls.slug.valueChanges
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        switchMap((raw) => {
          const slug = normalizeBusinessSlug(raw);
          if (slug !== raw) {
            this.form.controls.slug.setValue(slug, { emitEvent: false });
          }

          if (!isValidBusinessSlug(slug)) {
            this.slugAvailable = null;
            return of(null);
          }

          this.slugChecking = true;
          return this.registrationApi.checkSlug(slug).pipe(finalize(() => (this.slugChecking = false)));
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((res) => {
        if (!res?.success || !res.data) {
          return;
        }

        this.slugAvailable = res.data.available;
        if (res.data.slug !== this.form.controls.slug.value) {
          this.form.controls.slug.setValue(res.data.slug, { emitEvent: false });
        }
      });
  }

  storefrontPreview(): string {
    return buildStorefrontUrl(this.form.controls.slug.value);
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const slug = normalizeBusinessSlug(this.form.controls.slug.value);
    if (!isValidBusinessSlug(slug)) {
      this.toast.warning('Enter a valid hotel slug (letters, numbers, hyphens).', 'Registration');
      return;
    }

    if (this.slugAvailable === false) {
      this.toast.warning('That hotel slug is already taken.', 'Registration');
      return;
    }

    const raw = this.form.getRawValue();
    this.loading = true;
    this.authApi
      .register({
        businessName: raw.propertyName.trim(),
        slug,
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
              slug: '',
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
        error: (err: unknown) => showAuthRequestError(this.toast, err, 'Registration failed'),
      });
  }

  showError(
    field: 'propertyName' | 'slug' | 'firstName' | 'lastName' | 'email' | 'phoneNumber' | 'password' | 'confirmPassword',
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
