import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BusinessSocialProfileDto } from '../../core/models/business-social-profile.models';
import { BusinessSocialProfileApiService } from '../../core/services/business-social-profile-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-social-profile',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './social-profile.component.html',
  styleUrl: './social-profile.component.scss',
})
export class SocialProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessSocialProfileApiService);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    facebookUrl: [''],
    instagramUrl: [''],
    tikTokUrl: [''],
    xUrl: [''],
    contactEmail: ['', Validators.email],
    contactPhone: ['', Validators.maxLength(32)],
  });

  readonly loading = signal(true);
  readonly loadFailed = signal(false);
  readonly saving = signal(false);

  ngOnInit(): void {
    this.load();
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
            this.toast.showFailedApi(res, 'Social & contact');
            return;
          }

          this.applyProfile(res.data);
        },
        error: (err: unknown) => {
          this.loadFailed.set(true);
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Social & contact');
            return;
          }

          this.toast.error('Could not load social profile.', 'Social & contact');
        },
      });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.warning('Fix the highlighted fields before saving.', 'Social & contact');
      return;
    }

    const raw = this.form.getRawValue();
    this.saving.set(true);
    this.api
      .updateProfile({
        facebookUrl: raw.facebookUrl.trim() || null,
        instagramUrl: raw.instagramUrl.trim() || null,
        tikTokUrl: raw.tikTokUrl.trim() || null,
        xUrl: raw.xUrl.trim() || null,
        contactEmail: raw.contactEmail.trim() || null,
        contactPhone: raw.contactPhone.trim() || null,
      })
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Social & contact');
            return;
          }

          this.applyProfile(res.data);
          this.toast.success('Social and contact details saved.', 'Social & contact');
        },
        error: (err: unknown) => {
          const r = err as ApiResult<unknown>;
          if (r && typeof r === 'object' && 'message' in r) {
            this.toast.showFailedApi(r, 'Social & contact');
            return;
          }

          this.toast.error('Could not save social profile.', 'Social & contact');
        },
      });
  }

  private applyProfile(data: BusinessSocialProfileDto): void {
    this.form.patchValue({
      facebookUrl: data.facebookUrl ?? '',
      instagramUrl: data.instagramUrl ?? '',
      tikTokUrl: data.tikTokUrl ?? '',
      xUrl: data.xUrl ?? '',
      contactEmail: data.contactEmail ?? '',
      contactPhone: data.contactPhone ?? '',
    });
    this.form.markAsPristine();
  }
}
