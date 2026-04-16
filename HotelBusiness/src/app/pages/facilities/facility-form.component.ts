import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, of } from 'rxjs';
import { finalize } from 'rxjs/operators';
import type { FacilityDetailApiResponse, FacilityImageDto, PropertyFacilityDetailDto } from '../../core/models/facilities.models';
import { BusinessFacilitiesApiService } from '../../core/services/business-facilities-api.service';
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

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: [''],
  });

  facilityId: string | null = null;
  images: FacilityImageDto[] = [];
  loading = false;
  saving = false;
  error: string | null = null;
  uploadMessage: string | null = null;

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
          this.error = detail.message ?? 'Facility not found.';
          return;
        }

        this.patchFromFacility(detail.data);
      });
  }

  onSubmit(): void {
    this.error = null;
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

    req$.pipe(finalize(() => (this.saving = false))).subscribe((res) => {
      if (!res.success || !res.data) {
        this.error = res.message ?? 'Could not save facility.';
        return;
      }

      if (this.isCreateMode) {
        void this.router.navigate(['/facilities', res.data.id], { replaceUrl: true });
        return;
      }

      this.patchFromFacility(res.data);
    });
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

    this.uploadMessage = null;
    this.saving = true;
    this.api
      .uploadFacilityImages(this.facilityId, files)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success || !res.data?.length) {
          this.uploadMessage = res.message ?? 'Upload failed.';
          return;
        }

        this.images = [...this.images, ...res.data];
        this.uploadMessage = 'Photos uploaded.';
      });
  }

  removeImage(img: FacilityImageDto): void {
    if (!this.facilityId) {
      return;
    }

    this.saving = true;
    this.uploadMessage = null;
    this.api
      .deleteFacilityImage(this.facilityId, img.id)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success) {
          this.uploadMessage = res.message ?? 'Could not remove image.';
          return;
        }

        this.images = this.images.filter((i) => i.id !== img.id);
      });
  }

  imageUrl(img: FacilityImageDto): string {
    return this.api.resolveImageUrl(img.url);
  }

  private patchFromFacility(f: PropertyFacilityDetailDto): void {
    this.facilityId = f.id;
    this.form.patchValue({
      name: f.name,
      description: f.description ?? '',
    });
    this.images = [...f.images].sort((a, b) => a.sortOrder - b.sortOrder);
  }
}
