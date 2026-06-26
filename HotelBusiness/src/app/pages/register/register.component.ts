import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
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
import type { PlanTierOption, SubscriptionPlanDto } from '../../core/models/subscription.models';
import { AuthApiService } from '../../core/services/auth-api.service';
import { RegistrationApiService } from '../../core/services/registration-api.service';
import { SubscriptionApiService } from '../../core/services/subscription-api.service';
import { ToastService } from '../../core/services/toast.service';
import { showAuthRequestError } from '../../core/utils/auth-request-error';
import {
  filterAllowedImageFiles,
  ALLOWED_IMAGE_ACCEPT,
} from '../../core/utils/image-upload';
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
export class RegisterComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly registrationApi = inject(RegistrationApiService);
  private readonly subscriptionApi = inject(SubscriptionApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly storefrontBase = CUSTOMER_STOREFRONT_BASE_URL;
  readonly plans = signal<SubscriptionPlanDto[]>([]);
  readonly plansLoading = signal(true);

  readonly form = this.fb.nonNullable.group(
    {
      propertyName: ['', [Validators.required, Validators.minLength(2)]],
      businessType: ['Hotel' as 'Hotel' | 'Shortlet', Validators.required],
      planTier: ['free' as PlanTierOption, Validators.required],
      proBilling: ['monthly' as 'monthly' | 'yearly'],
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

  readonly freePlan = computed(() => this.plans().find((p) => p.code === 'free') ?? null);
  readonly proMonthlyPlan = computed(() => this.plans().find((p) => p.code === 'pro-monthly') ?? null);
  readonly proYearlyPlan = computed(() => this.plans().find((p) => p.code === 'pro-yearly') ?? null);

  submitted = false;
  loading = false;
  slugChecking = false;
  slugAvailable: boolean | null = null;
  slugTouched = false;

  readonly allowedLogoAccept = ALLOWED_IMAGE_ACCEPT;
  readonly logoPreviewUrl = signal<string | null>(null);
  private pendingLogoFile: File | null = null;

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

  ngOnInit(): void {
    this.subscriptionApi
      .listPublicPlans()
      .pipe(finalize(() => this.plansLoading.set(false)))
      .subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.plans.set(res.data);
          }
        },
      });
  }

  storefrontPreview(): string {
    return buildStorefrontUrl(this.form.controls.slug.value);
  }

  formatPlanPrice(plan: SubscriptionPlanDto | null): string {
    if (!plan || plan.priceAmount <= 0) {
      return 'Free · 30-day trial';
    }
    const amount = new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: plan.currency || 'NGN',
      maximumFractionDigits: 0,
    }).format(plan.priceAmount);
    return plan.billingInterval === 'Yearly' ? `${amount}/year` : `${amount}/month`;
  }

  selectPlanTier(tier: PlanTierOption): void {
    this.form.controls.planTier.setValue(tier);
  }

  selectProBilling(interval: 'monthly' | 'yearly'): void {
    this.form.controls.proBilling.setValue(interval);
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (picked.length === 0) {
      return;
    }

    const { accepted, skipped } = filterAllowedImageFiles(picked);
    if (skipped.length > 0) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'Logo');
    }

    const file = accepted[0];
    if (!file) {
      return;
    }

    this.pendingLogoFile = file;
    const reader = new FileReader();
    reader.onload = () =>
      this.logoPreviewUrl.set(typeof reader.result === 'string' ? reader.result : null);
    reader.readAsDataURL(file);
  }

  clearLogo(): void {
    this.pendingLogoFile = null;
    this.logoPreviewUrl.set(null);
  }

  private selectedPlanCode(): string {
    const tier = this.form.controls.planTier.value;
    if (tier === 'free') {
      return 'free';
    }
    return this.form.controls.proBilling.value === 'yearly' ? 'pro-yearly' : 'pro-monthly';
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
      .register(
        {
          businessName: raw.propertyName.trim(),
          slug,
          firstName: raw.firstName.trim(),
          lastName: raw.lastName.trim(),
          email: raw.email.trim(),
          phoneNumber: raw.phoneNumber.trim(),
          password: raw.password,
          acceptTerms: raw.acceptTerms,
          businessType: raw.businessType,
          planCode: this.selectedPlanCode(),
        },
        this.pendingLogoFile,
      )
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (result) => {
          if (result.success) {
            this.toast.success('We sent a verification code to your email.', 'Account created');
            this.form.reset({
              propertyName: '',
              businessType: 'Hotel',
              planTier: 'free',
              proBilling: 'monthly',
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
            this.clearLogo();
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
    field:
      | 'propertyName'
      | 'slug'
      | 'firstName'
      | 'lastName'
      | 'email'
      | 'phoneNumber'
      | 'password'
      | 'confirmPassword',
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
