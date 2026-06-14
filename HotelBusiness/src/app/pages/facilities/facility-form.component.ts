import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { FacilityImageDto, PropertyFacilityDetailDto } from '../../core/models/facilities.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { BusinessFacilitiesApiService } from '../../core/services/business-facilities-api.service';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ALLOWED_IMAGE_ACCEPT, filterAllowedImageFiles } from '../../core/utils/image-upload';
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
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: [''],
    locationId: ['', Validators.required],
  });

  readonly facilityId = signal<string | null>(null);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly images = signal<FacilityImageDto[]>([]);
  readonly loading = signal(false);
  readonly locationsLoading = signal(false);
  readonly saving = signal(false);
  readonly pendingPhotoFiles = signal<File[]>([]);
  readonly isArchived = signal(false);

  readonly imageAccept = ALLOWED_IMAGE_ACCEPT;

  get isCreateMode(): boolean {
    return this.facilityId() === null;
  }

  ngOnInit(): void {
    this.loadLocations();
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.initFromRoute(params.get('facilityId'));
    });
  }

  private loadLocations(): void {
    this.locationsLoading.set(true);
    this.locationsApi
      .listLocations()
      .pipe(finalize(() => this.locationsLoading.set(false)))
      .subscribe((res) => {
        if (res.success && res.data) {
          this.locations.set(res.data);
        }
      });
  }

  private initFromRoute(paramId: string | null): void {
    this.resetFormState();
    this.facilityId.set(paramId);

    if (!paramId) {
      return;
    }

    this.loadFacilityForEdit(paramId);
  }

  private resetFormState(): void {
    this.form.reset({ name: '', description: '', locationId: '' });
    this.images.set([]);
    this.pendingPhotoFiles.set([]);
    this.isArchived.set(false);
    this.loading.set(false);
    this.saving.set(false);
  }

  private loadFacilityForEdit(paramId: string): void {
    this.loading.set(true);
    this.api
      .getFacility(paramId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((detail) => {
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
      locationId: raw.locationId.trim(),
    };

    this.saving.set(true);
    const req$ = this.isCreateMode
      ? this.api.createFacility(body)
      : this.api.updateFacility(this.facilityId()!, body);

    req$.subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.saving.set(false);
          this.toast.showFailedApi(res, 'Facility');
          return;
        }

        if (!this.isCreateMode) {
          this.saving.set(false);
          this.patchFromFacility(res.data);
          this.toast.success('Facility details saved.', 'Saved');
          return;
        }

        const newId = res.data.id;
        const files = [...this.pendingPhotoFiles()];
        if (files.length === 0) {
          this.pendingPhotoFiles.set([]);
          this.saving.set(false);
          this.toast.success('Facility created.', 'Done');
          void this.router.navigate(['/facilities', newId], { replaceUrl: true });
          return;
        }

        this.api
          .uploadFacilityImages(newId, files)
          .pipe(
            finalize(() => {
              this.saving.set(false);
              this.pendingPhotoFiles.set([]);
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
        this.saving.set(false);
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

    const { accepted, skipped } = filterAllowedImageFiles(picked);
    if (accepted.length) {
      this.pendingPhotoFiles.update((current) => [...current, ...accepted]);
    }

    if (skipped.length) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'Photos');
    }
  }

  removePendingPhoto(index: number): void {
    this.pendingPhotoFiles.update((files) => files.filter((_, i) => i !== index));
  }

  onFilesSelected(event: Event): void {
    const id = this.facilityId();
    if (!id) {
      return;
    }

    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (picked.length === 0) {
      return;
    }

    const { accepted, skipped } = filterAllowedImageFiles(picked);
    if (skipped.length) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'Photos');
    }

    if (accepted.length === 0) {
      return;
    }

    this.saving.set(true);
    this.api
      .uploadFacilityImages(id, accepted)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe((res) => {
        if (!res.success || !res.data?.length) {
          this.toast.showFailedApi(res, 'Upload failed');
          return;
        }

        this.images.update((current) => [...current, ...res.data!]);
        this.toast.success('Photos uploaded.', 'Facility photos');
      });
  }

  removeImage(img: FacilityImageDto): void {
    const id = this.facilityId();
    if (!id) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteFacilityImage(id, img.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe((res) => {
        if (!res.success) {
          this.toast.showFailedApi(res, 'Photo');
          return;
        }

        this.images.update((current) => current.filter((i) => i.id !== img.id));
        this.toast.info('Photo removed.', 'Facility photos');
      });
  }

  imageUrl(img: FacilityImageDto): string {
    return this.api.resolveImageUrl(img.url);
  }

  archiveFacility(): void {
    const id = this.facilityId();
    if (!id || this.isCreateMode) {
      return;
    }

    if (!globalThis.confirm('Archive this facility? It will disappear from your default list until you restore it.')) {
      return;
    }

    this.saving.set(true);
    this.api
      .archiveFacility(id)
      .pipe(finalize(() => this.saving.set(false)))
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
    const id = this.facilityId();
    if (!id || this.isCreateMode) {
      return;
    }

    this.saving.set(true);
    this.api
      .restoreFacility(id)
      .pipe(finalize(() => this.saving.set(false)))
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
    const id = this.facilityId();
    if (!id || this.isCreateMode) {
      return;
    }

    if (
      !globalThis.confirm(
        'Permanently delete this facility and all of its photos? This cannot be undone.',
      )
    ) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteFacility(id)
      .pipe(finalize(() => this.saving.set(false)))
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
    this.facilityId.set(f.id);
    this.isArchived.set(f.isArchived ?? false);
    this.form.patchValue({
      name: f.name,
      description: f.description ?? '',
      locationId: f.locationId ?? '',
    });
    this.images.set([...f.images].sort((a, b) => a.sortOrder - b.sortOrder));
  }
}
