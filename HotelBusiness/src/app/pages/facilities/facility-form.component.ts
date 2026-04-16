import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, of } from 'rxjs';
import { finalize } from 'rxjs/operators';
import type { FacilityDetailApiResponse, FacilityImageDto, PropertyFacilityDetailDto } from '../../core/models/facilities.models';
import { BusinessFacilitiesApiService } from '../../core/services/business-facilities-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-facility-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './facility-form.component.html',
  styleUrl: './facility-form.component.scss',
})
export class FacilityFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessFacilitiesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: [''],
  });

  facilityId: string | null = null;
  images: FacilityImageDto[] = [];
  loading = false;
  saving = false;

  /** Queued on “Add facility”; uploaded right after the facility is created. */
  pendingPhotoFiles: File[] = [];

  isArchived = false;

  get isCreateMode(): boolean {
    return this.facilityId === null;
  }

  ngOnInit(): void {
    const paramId = this.route.snapshot.paramMap.get('facilityId');
    this.facilityId = paramId ?? null;

    this.loading = true;
    const detail$: Observable<FacilityDetailApiResponse> = this.facilityId
      ? this.api.getFacility(this.facilityId)
      : of<FacilityDetailApiResponse>({
          success: true,
          data: null,
          message: null,
          code: null,
          validationErrors: null,
        });

    forkJoin({ detail: detail$ })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(({ detail }) => {
        if (!this.facilityId) {
          return;
        }

        if (!detail.success || !detail.data) {
          this.toast.error(detail.message ?? 'Facility not found.', 'Facility');
          return;
        }

        this.patchFromFacility(detail.data);
      });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      name: raw.name.trim(),
      description: raw.description.trim() || null,
    };

    this.saving = true;
    const req$ = this.isCreateMode
      ? this.api.createFacility(body)
      : this.api.updateFacility(this.facilityId!, body);

    req$.subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.saving = false;
          this.toast.showFailedApi(res, 'Facility');
          return;
        }

        if (!this.isCreateMode) {
          this.saving = false;
          this.patchFromFacility(res.data);
          this.toast.success('Facility details saved.', 'Saved');
          return;
        }

        const newId = res.data.id;
        const files = [...this.pendingPhotoFiles];
        if (files.length === 0) {
          this.pendingPhotoFiles = [];
          this.saving = false;
          this.toast.success('Facility created.', 'Done');
          void this.router.navigate(['/facilities', newId], { replaceUrl: true });
          return;
        }

        this.api
          .uploadFacilityImages(newId, files)
          .pipe(
            finalize(() => {
              this.saving = false;
              this.pendingPhotoFiles = [];
            }),
          )
          .subscribe({
            next: (up) => {
              if (!up.success || !up.data?.length) {
                this.toast.warning(
                  up.message ??
                    'Facility was created, but photos did not upload. You can add them on the next screen.',
                  'Photos',
                );
              } else {
                this.toast.success('Facility created and photos uploaded.', 'Done');
              }

              void this.router.navigate(['/facilities', newId], { replaceUrl: true });
            },
            error: () => {
              this.toast.warning(
                'Facility was created, but photos did not upload. You can add them on the next screen.',
                'Photos',
              );
              void this.router.navigate(['/facilities', newId], { replaceUrl: true });
            },
          });
      },
      error: () => {
        this.saving = false;
        this.toast.error('Could not reach the API to save the facility.', 'Network');
      },
    });
  }

  onPendingPhotosSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (picked.length === 0) {
      return;
    }

    const maxEach = 8 * 1024 * 1024;
    const allowed = /^image\/(jpeg|png|webp|gif)$/i;
    const skipped: string[] = [];
    for (const f of picked) {
      if (f.size > maxEach) {
        skipped.push(`${f.name} (over 8MB)`);
        continue;
      }

      if (!allowed.test(f.type)) {
        skipped.push(`${f.name} (not JPEG/PNG/WebP/GIF)`);
        continue;
      }

      this.pendingPhotoFiles = [...this.pendingPhotoFiles, f];
    }

    if (skipped.length) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'Photos');
    }
  }

  removePendingPhoto(index: number): void {
    this.pendingPhotoFiles = this.pendingPhotoFiles.filter((_, i) => i !== index);
  }

  onFilesSelected(event: Event): void {
    if (!this.facilityId) {
      return;
    }

    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (files.length === 0) {
      return;
    }

    this.saving = true;
    this.api
      .uploadFacilityImages(this.facilityId, files)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success || !res.data?.length) {
          this.toast.showFailedApi(res, 'Upload failed');
          return;
        }

        this.images = [...this.images, ...res.data];
        this.toast.success('Photos uploaded.', 'Facility photos');
      });
  }

  removeImage(img: FacilityImageDto): void {
    if (!this.facilityId) {
      return;
    }

    this.saving = true;
    this.api
      .deleteFacilityImage(this.facilityId, img.id)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success) {
          this.toast.showFailedApi(res, 'Photo');
          return;
        }

        this.images = this.images.filter((i) => i.id !== img.id);
        this.toast.info('Photo removed.', 'Facility photos');
      });
  }

  imageUrl(img: FacilityImageDto): string {
    return this.api.resolveImageUrl(img.url);
  }

  archiveFacility(): void {
    if (!this.facilityId || this.isCreateMode) {
      return;
    }

    if (!globalThis.confirm('Archive this facility? It will disappear from your default list until you restore it.')) {
      return;
    }

    this.saving = true;
    this.api
      .archiveFacility(this.facilityId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Archive');
            return;
          }

          this.patchFromFacility(res.data);
          this.toast.success('Facility archived.', 'Property');
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  restoreFacility(): void {
    if (!this.facilityId || this.isCreateMode) {
      return;
    }

    this.saving = true;
    this.api
      .restoreFacility(this.facilityId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Restore');
            return;
          }

          this.patchFromFacility(res.data);
          this.toast.success('Facility restored to your list.', 'Property');
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  deleteFacility(): void {
    if (!this.facilityId || this.isCreateMode) {
      return;
    }

    if (
      !globalThis.confirm(
        'Permanently delete this facility and all of its photos? This cannot be undone.',
      )
    ) {
      return;
    }

    this.saving = true;
    this.api
      .deleteFacility(this.facilityId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Delete facility');
            return;
          }

          this.toast.success('Facility deleted.', 'Property');
          void this.router.navigate(['/facilities']);
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  private patchFromFacility(f: PropertyFacilityDetailDto): void {
    this.facilityId = f.id;
    this.isArchived = f.isArchived ?? false;
    this.form.patchValue({
      name: f.name,
      description: f.description ?? '',
    });
    this.images = [...f.images].sort((a, b) => a.sortOrder - b.sortOrder);
  }
}
