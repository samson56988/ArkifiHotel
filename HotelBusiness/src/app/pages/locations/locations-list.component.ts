import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import type { ApiResult } from '../../core/models/api-result.model';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-locations-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './locations-list.component.html',
  styleUrl: './locations-list.component.scss',
})
export class LocationsListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessLocationsApiService);
  private readonly toast = inject(ToastService);

  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly initialLoadDone = signal(false);
  readonly loadFailed = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editingId = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    address: [''],
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
    this.api.listLocations().subscribe({
      next: (res) => {
        if (res.success) {
          this.locations.set(Array.isArray(res.data) ? [...res.data!] : []);
          this.loadFailed.set(false);
        } else {
          this.locations.set([]);
          this.loadFailed.set(true);
          this.toast.showFailedApi(res, 'Locations');
        }

        this.initialLoadDone.set(true);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.locations.set([]);
        this.loadFailed.set(true);
        this.initialLoadDone.set(true);
        this.loading.set(false);
        const res = err as ApiResult<BusinessLocationDto[]>;
        if (res && typeof res === 'object' && 'message' in res) {
          this.toast.showFailedApi(res, 'Locations');
          return;
        }

        this.toast.error('Could not load locations.', 'Locations');
      },
    });
  }

  startEdit(location: BusinessLocationDto): void {
    this.editingId.set(location.id);
    this.form.patchValue({
      name: location.name,
      address: location.address ?? '',
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', address: '' });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const body = {
      name: raw.name.trim(),
      address: raw.address.trim() || null,
    };

    this.saving.set(true);
    const editingId = this.editingId();
    const req$ = editingId
      ? this.api.updateLocation(editingId, body)
      : this.api.createLocation(body);

    req$
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Location');
            return;
          }

          if (editingId) {
            this.locations.update((list) =>
              list.map((l) => (l.id === res.data!.id ? res.data! : l)),
            );
            this.toast.success('Location updated.', 'Saved');
          } else {
            this.locations.update((list) =>
              [...list, res.data!].sort((a, b) => a.name.localeCompare(b.name)),
            );
            this.toast.success(`${res.data.name} was added.`, 'Location');
          }

          this.cancelEdit();
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  deleteLocation(location: BusinessLocationDto): void {
    if (
      !globalThis.confirm(
        `Delete "${location.name}"? Rooms or facilities using this location must be updated first.`,
      )
    ) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteLocation(location.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Delete location');
            return;
          }

          this.locations.update((list) => list.filter((l) => l.id !== location.id));
          if (this.editingId() === location.id) {
            this.cancelEdit();
          }

          this.toast.success('Location deleted.', 'Locations');
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }
}
