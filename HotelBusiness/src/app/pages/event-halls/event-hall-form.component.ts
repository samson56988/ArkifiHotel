import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, concatMap, from, last } from 'rxjs';
import type { EventHallDetailDto, EventHallImageDto } from '../../core/models/event-hall.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { BusinessEventHallsApiService } from '../../core/services/business-event-halls-api.service';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ALLOWED_IMAGE_ACCEPT, filterAllowedImageFiles } from '../../core/utils/image-upload';

@Component({
  selector: 'app-event-hall-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './event-hall-form.component.html',
  styleUrl: './event-hall-form.component.scss',
})
export class EventHallFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessEventHallsApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: [''],
    rentalPrice: [0, [Validators.required, Validators.min(0)]],
    maxCapacity: [null as number | null],
    locationId: ['', Validators.required],
  });

  readonly eventHallId = signal<string | null>(null);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly images = signal<EventHallImageDto[]>([]);
  readonly loading = signal(false);
  readonly locationsLoading = signal(false);
  readonly saving = signal(false);
  readonly pendingPhotoFiles = signal<File[]>([]);
  readonly isArchived = signal(false);

  readonly imageAccept = ALLOWED_IMAGE_ACCEPT;

  get isCreateMode(): boolean {
    return this.eventHallId() === null;
  }

  ngOnInit(): void {
    this.loadLocations();
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.initFromRoute(params.get('eventHallId'));
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
    this.eventHallId.set(paramId);
    if (!paramId) {
      return;
    }
    this.loadHallForEdit(paramId);
  }

  private resetFormState(): void {
    this.form.reset({ name: '', description: '', rentalPrice: 0, maxCapacity: null, locationId: '' });
    this.images.set([]);
    this.pendingPhotoFiles.set([]);
    this.isArchived.set(false);
    this.loading.set(false);
    this.saving.set(false);
  }

  private loadHallForEdit(paramId: string): void {
    this.loading.set(true);
    this.api
      .get(paramId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((detail) => {
        if (!detail.success || !detail.data) {
          this.toast.error(detail.message ?? 'Event hall not found.', 'Event hall');
          return;
        }
        this.patchFromHall(detail.data);
      });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      name: (raw.name ?? '').trim(),
      description: (raw.description ?? '').trim() || null,
      rentalPrice: Number(raw.rentalPrice ?? 0),
      maxCapacity: raw.maxCapacity && raw.maxCapacity > 0 ? raw.maxCapacity : null,
      locationId: (raw.locationId ?? '').trim(),
    };

    this.saving.set(true);
    const req$ = this.isCreateMode
      ? this.api.create(body)
      : this.api.update(this.eventHallId()!, body);

    req$.subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.saving.set(false);
          this.toast.showFailedApi(res, 'Event hall');
          return;
        }

        if (!this.isCreateMode) {
          this.saving.set(false);
          this.toast.success('Event hall saved.', 'Saved');
          void this.router.navigate(['/event-halls'], { replaceUrl: true });
          return;
        }

        const newId = res.data.id;
        const files = [...this.pendingPhotoFiles()];
        if (files.length === 0) {
          this.pendingPhotoFiles.set([]);
          this.saving.set(false);
          this.toast.success('Event hall created.', 'Done');
          void this.router.navigate(['/event-halls'], { replaceUrl: true });
          return;
        }

        from(files)
          .pipe(
            concatMap((file) => this.api.uploadImage(newId, file)),
            last(),
            finalize(() => {
              this.saving.set(false);
              this.pendingPhotoFiles.set([]);
            }),
          )
          .subscribe({
            next: (res) => {
              if (!res?.success) {
                this.toast.warning(
                  'Event hall was created, but some photos did not upload. Add them from the event halls list.',
                  'Photos',
                );
              } else {
                this.toast.success('Event hall created with photos.', 'Done');
              }
              void this.router.navigate(['/event-halls'], { replaceUrl: true });
            },
            error: () => {
              this.toast.warning(
                'Event hall was created, but photos did not upload. Add them from the event halls list.',
                'Photos',
              );
              void this.router.navigate(['/event-halls'], { replaceUrl: true });
            },
          });
      },
      error: () => {
        this.saving.set(false);
        this.toast.error('Could not save event hall.', 'Network');
      },
    });
  }

  onPendingPhotosSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (!picked.length) {
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
    const id = this.eventHallId();
    if (!id) {
      return;
    }
    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    if (!picked.length) {
      return;
    }
    const { accepted, skipped } = filterAllowedImageFiles(picked);
    if (skipped.length) {
      this.toast.warning(`Skipped: ${skipped.join('; ')}.`, 'Photos');
    }
    if (!accepted.length) {
      return;
    }

    this.saving.set(true);
    from(accepted)
      .pipe(
        concatMap((file) => this.api.uploadImage(id, file)),
        last(),
        finalize(() => this.saving.set(false)),
      )
      .subscribe((res) => {
        if (!res?.success || !res.data) {
          this.toast.showFailedApi(res ?? { success: false, message: 'Upload failed' }, 'Upload');
          return;
        }
        this.patchFromHall(res.data);
        this.toast.success('Photos uploaded.', 'Event hall');
      });
  }

  removeImage(img: EventHallImageDto): void {
    const id = this.eventHallId();
    if (!id) {
      return;
    }
    this.saving.set(true);
    this.api
      .deleteImage(id, img.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe((res) => {
        if (!res.success) {
          this.toast.showFailedApi(res, 'Photo');
          return;
        }
        this.images.update((current) => current.filter((i) => i.id !== img.id));
        this.toast.info('Photo removed.', 'Event hall');
      });
  }

  imageUrl(img: EventHallImageDto): string {
    return this.api.resolveImageUrl(img.url);
  }

  archiveHall(): void {
    const id = this.eventHallId();
    if (!id || this.isCreateMode) {
      return;
    }
    if (!globalThis.confirm('Archive this event hall? It will be hidden from guests until restored.')) {
      return;
    }
    this.saving.set(true);
    this.api
      .archive(id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Archive');
            return;
          }
          this.isArchived.set(true);
          this.toast.success('Event hall archived.', 'Property');
        },
        error: () => this.toast.error('Could not archive event hall.', 'Network'),
      });
  }

  restoreHall(): void {
    const id = this.eventHallId();
    if (!id || this.isCreateMode) {
      return;
    }
    this.saving.set(true);
    this.api
      .restore(id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Restore');
            return;
          }
          this.isArchived.set(false);
          this.toast.success('Event hall restored.', 'Property');
        },
        error: () => this.toast.error('Could not restore event hall.', 'Network'),
      });
  }

  deleteHall(): void {
    const id = this.eventHallId();
    if (!id || this.isCreateMode) {
      return;
    }
    if (!globalThis.confirm('Permanently delete this event hall and all photos? This cannot be undone.')) {
      return;
    }
    this.saving.set(true);
    this.api
      .delete(id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Delete');
            return;
          }
          this.toast.success('Event hall deleted.', 'Property');
          void this.router.navigate(['/event-halls']);
        },
        error: () => this.toast.error('Could not delete event hall.', 'Network'),
      });
  }

  private patchFromHall(h: EventHallDetailDto): void {
    this.eventHallId.set(h.id);
    this.isArchived.set(h.isArchived ?? false);
    this.form.patchValue({
      name: h.name,
      description: h.description ?? '',
      rentalPrice: h.rentalPrice,
      maxCapacity: h.maxCapacity,
      locationId: h.locationId ?? '',
    });
    this.images.set([...h.images].sort((a, b) => a.sortOrder - b.sortOrder));
  }
}
