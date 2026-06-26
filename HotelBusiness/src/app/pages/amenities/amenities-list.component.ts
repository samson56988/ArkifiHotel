import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { AmenityDto } from '../../core/models/amenities.models';
import { BusinessAmenitiesApiService } from '../../core/services/business-amenities-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-amenities-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './amenities-list.component.html',
  styleUrl: './amenities-list.component.scss',
})
export class AmenitiesListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessAmenitiesApiService);
  private readonly toast = inject(ToastService);

  readonly amenities = signal<AmenityDto[]>([]);
  readonly catalogAmenities = computed(() => this.amenities().filter((a) => !a.isCustom));
  readonly customAmenities = computed(() => this.amenities().filter((a) => a.isCustom));
  readonly initialLoadDone = signal(false);
  readonly loadFailed = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editingId = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(128)]],
    category: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  get isEditing(): boolean {
    return this.editingId() !== null;
  }

  load(): void {
    this.loading.set(true);
    this.loadFailed.set(false);
    this.api.listAmenities().subscribe({
      next: (res) => {
        if (res.success) {
          this.amenities.set(Array.isArray(res.data) ? [...res.data!] : []);
          this.loadFailed.set(false);
        } else {
          this.amenities.set([]);
          this.loadFailed.set(true);
          if (res.code === 'Unauthorized' || res.message?.includes('401')) {
            this.toast.warning('Please sign in again to manage amenities.', 'Amenities');
          } else {
            this.toast.showFailedApi(res, 'Amenities');
          }
        }

        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.amenities.set([]);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        const res = err as ApiResult<AmenityDto[]>;
        if (res && typeof res === 'object' && 'message' in res) {
          this.toast.showFailedApi(res, 'Amenities');
          return;
        }

        this.toast.error('Could not load amenities.', 'Amenities');
      },
    });
  }

  startEdit(amenity: AmenityDto): void {
    if (!amenity.isCustom) {
      return;
    }

    this.editingId.set(amenity.id);
    this.form.patchValue({
      name: amenity.name,
      category: amenity.category ?? '',
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', category: '' });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      name: raw.name.trim(),
      category: raw.category.trim() || null,
    };

    this.saving.set(true);
    const editingId = this.editingId();
    const req$ = editingId
      ? this.api.updateAmenity(editingId, body)
      : this.api.createAmenity(body);

    req$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Amenity');
            return;
          }

          if (editingId) {
            this.amenities.update((list) =>
              list.map((a) => (a.id === res.data!.id ? res.data! : a)),
            );
            this.toast.success('Amenity updated.', 'Saved');
          } else {
            this.amenities.update((list) =>
              [...list, res.data!].sort(
                (a, b) =>
                  (a.category ?? '').localeCompare(b.category ?? '') || a.name.localeCompare(b.name),
              ),
            );
            this.toast.success(`${res.data.name} was added.`, 'Amenity');
          }

          this.cancelEdit();
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  deleteAmenity(amenity: AmenityDto): void {
    if (!amenity.isCustom) {
      return;
    }

    if (
      !globalThis.confirm(
        `Delete "${amenity.name}"? Rooms that use this amenity must be updated first.`,
      )
    ) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteAmenity(amenity.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Delete amenity');
            return;
          }

          this.amenities.update((list) => list.filter((a) => a.id !== amenity.id));
          if (this.editingId() === amenity.id) {
            this.cancelEdit();
          }

          this.toast.success('Amenity deleted.', 'Amenities');
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  categoryLabel(amenity: AmenityDto): string {
    return amenity.category?.trim() || 'General';
  }

  groupedCatalogAmenities(): { category: string; items: AmenityDto[] }[] {
    const map = new Map<string, AmenityDto[]>();
    for (const a of this.catalogAmenities()) {
      const key = this.categoryLabel(a);
      if (!map.has(key)) {
        map.set(key, []);
      }

      map.get(key)!.push(a);
    }

    return [...map.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([category, items]) => ({
        category,
        items: items.sort((x, y) => x.name.localeCompare(y.name)),
      }));
  }
}
