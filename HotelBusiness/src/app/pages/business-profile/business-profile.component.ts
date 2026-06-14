import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, finalize, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { buildStorefrontUrl, CUSTOMER_STOREFRONT_BASE_URL } from '../../core/constants/storefront';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BusinessProfileDto } from '../../core/models/business-profile.models';
import { BusinessProfileApiService } from '../../core/services/business-profile-api.service';
import { ToastService } from '../../core/services/toast.service';
import {
  ALLOWED_IMAGE_ACCEPT,
  filterAllowedImageFiles,
} from '../../core/utils/image-upload';
import {
  isValidBusinessSlug,
  normalizeBusinessSlug,
  suggestSlugFromBusinessName,
} from '../../core/utils/business-slug';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-business-profile',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './business-profile.component.html',
  styleUrl: './business-profile.component.scss',
})
export class BusinessProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessProfileApiService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly storefrontBase = CUSTOMER_STOREFRONT_BASE_URL;
  readonly allowedLogoAccept = ALLOWED_IMAGE_ACCEPT;

  readonly form = this.fb.nonNullable.group({
    businessName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(256)]],
    slug: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(128)]],
  });

  readonly profile = signal<BusinessProfileDto | null>(null);
  readonly loading = signal(true);
  readonly loadFailed = signal(false);
  readonly saving = signal(false);
  readonly uploadingLogo = signal(false);
  readonly slugChecking = signal(false);
  readonly slugAvailable = signal<boolean | null>(null);
  readonly logoPreviewUrl = signal<string | null>(null);
  readonly pendingLogoFile = signal<File | null>(null);

  readonly storefrontPreview = computed(() => buildStorefrontUrl(this.form.controls.slug.value));

  readonly liveStorefrontUrl = computed(() => {
    const slug = this.profile()?.slug ?? this.form.controls.slug.value;
    return slug && isValidBusinessSlug(normalizeBusinessSlug(slug))
      ? buildStorefrontUrl(slug)
      : null;
  });

  ngOnInit(): void {
    this.load();

    this.form.controls.businessName.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((name) => {
        const slugControl = this.form.controls.slug;
        if (!slugControl.dirty && !this.profile()?.slug) {
          slugControl.setValue(suggestSlugFromBusinessName(name));
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
            this.slugAvailable.set(null);
            return of(null);
          }

          const current = this.profile()?.slug;
          if (current && current === slug) {
            this.slugAvailable.set(true);
            return of(null);
          }

          this.slugChecking.set(true);
          return this.api.checkSlug(slug).pipe(finalize(() => this.slugChecking.set(false)));
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((res) => {
        if (!res) {
          return;
        }

        if (res.success && res.data) {
          this.slugAvailable.set(res.data.available);
          if (res.data.slug !== this.form.controls.slug.value) {
            this.form.controls.slug.setValue(res.data.slug, { emitEvent: false });
          }
        }
      });
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.api
      .getProfile()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.loadFailed.set(true);
            this.toast.showFailedApi(res, 'Profile');
            return;
          }

          this.applyProfile(res.data);
        },
        error: (err: unknown) => {
          this.loadFailed.set(true);
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Profile');
            return;
          }

          this.toast.error('Could not load profile.', 'Profile');
        },
      });
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    input.value = '';

    if (files.length === 0) {
      return;
    }

    const { accepted, skipped } = filterAllowedImageFiles(files);
    if (skipped.length > 0) {
      this.toast.warning(skipped.join('; '), 'Logo');
    }

    const file = accepted[0];
    if (!file) {
      return;
    }

    this.pendingLogoFile.set(file);
    const reader = new FileReader();
    reader.onload = () => this.logoPreviewUrl.set(typeof reader.result === 'string' ? reader.result : null);
    reader.readAsDataURL(file);
  }

  uploadLogo(): void {
    const file = this.pendingLogoFile();
    if (!file) {
      this.toast.warning('Choose a JPEG or PNG logo first.', 'Logo');
      return;
    }

    this.uploadingLogo.set(true);
    this.api
      .uploadLogo(file)
      .pipe(finalize(() => this.uploadingLogo.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Logo');
            return;
          }

          this.pendingLogoFile.set(null);
          this.applyProfile(res.data);
          this.toast.success('Logo updated.', 'Profile');
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Logo');
            return;
          }

          this.toast.error('Could not upload logo.', 'Logo');
        },
      });
  }

  removeLogo(): void {
    if (!globalThis.confirm('Remove your business logo?')) {
      return;
    }

    this.uploadingLogo.set(true);
    this.api
      .removeLogo()
      .pipe(finalize(() => this.uploadingLogo.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Logo');
            return;
          }

          this.pendingLogoFile.set(null);
          this.applyProfile(res.data);
          this.toast.success('Logo removed.', 'Profile');
        },
        error: () => this.toast.error('Could not remove logo.', 'Logo'),
      });
  }

  onSubmit(): void {
    const raw = this.form.getRawValue();
    const slug = normalizeBusinessSlug(raw.slug);

    if (this.form.invalid || !isValidBusinessSlug(slug)) {
      this.form.markAllAsTouched();
      this.toast.warning('Enter a valid hotel slug (letters, numbers, hyphens).', 'Profile');
      return;
    }

    if (this.slugAvailable() === false) {
      this.toast.warning('That hotel slug is already taken.', 'Profile');
      return;
    }

    this.saving.set(true);
    this.api
      .updateProfile({
        businessName: raw.businessName.trim(),
        slug,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Profile');
            return;
          }

          this.applyProfile(res.data);
          this.toast.success('Profile updated.', 'Profile');
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Profile');
            return;
          }

          this.toast.error('Could not save profile.', 'Profile');
        },
      });
  }

  private applyProfile(data: BusinessProfileDto): void {
    this.profile.set(data);
    this.form.patchValue({
      businessName: data.businessName,
      slug: data.slug ?? '',
    });
    this.form.controls.slug.markAsPristine();
    this.slugAvailable.set(data.slug ? true : null);
    this.logoPreviewUrl.set(data.logoUrl);
  }
}
