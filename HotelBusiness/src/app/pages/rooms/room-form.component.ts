import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import type {
  AmenitiesApiResponse,
  AmenityDto,
  BusinessRoomDetailDto,
  RoomDetailApiResponse,
  RoomImageDto,
} from '../../core/models/rooms.models';
import { BusinessRoomsApiService } from '../../core/services/business-rooms-api.service';
import { ToastService } from '../../core/services/toast.service';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

@Component({
  selector: 'app-room-form',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './room-form.component.html',
  styleUrl: './room-form.component.scss',
})
export class RoomFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessRoomsApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: [''],
    maxOccupancy: [2, [Validators.required, Validators.min(1), Validators.max(50)]],
    basePricePerNight: [0, [Validators.required, Validators.min(0)]],
  });

  amenities: AmenityDto[] = [];
  readonly selectedAmenityIds = new Set<string>();
  customName = '';
  customCategory = '';

  roomId: string | null = null;
  images: RoomImageDto[] = [];
  /** Full-page spinner only while loading an existing room for edit. */
  initialLoading = false;
  /** Amenity catalog fetch (create: does not block the rest of the form). */
  amenitiesLoading = false;
  saving = false;

  /** Queued on “Add room”; uploaded right after the room is created. */
  pendingPhotoFiles: File[] = [];

  /** Edit mode: soft-archived rooms are omitted from the default list until restored. */
  isArchived = false;

  get isCreateMode(): boolean {
    return this.roomId === null;
  }

  ngOnInit(): void {
    const paramId = this.route.snapshot.paramMap.get('roomId');
    this.roomId = paramId ?? null;

    if (!this.roomId) {
      this.loadAmenitiesForCreate();
      return;
    }

    this.initialLoading = true;
    const room$ = this.api.getRoom(this.roomId).pipe(
      timeout(25_000),
      catchError(() =>
        of<RoomDetailApiResponse>({
          success: false,
          data: null,
          message: 'Could not load this room. Check that the API is running and you are still signed in.',
          code: 'NetworkOrTimeout',
          validationErrors: null,
        }),
      ),
    );

    const amenities$ = this.api.listAmenities().pipe(
      timeout(25_000),
      catchError(() =>
        of<AmenitiesApiResponse>({
          success: false,
          data: null,
          message: 'Could not load amenities (timeout or network).',
          code: 'NetworkOrTimeout',
          validationErrors: null,
        }),
      ),
    );

    forkJoin({ amenities: amenities$, room: room$ })
      .pipe(finalize(() => (this.initialLoading = false)))
      .subscribe(({ amenities, room }) => {
        if (amenities.success && amenities.data) {
          this.amenities = amenities.data;
        } else {
          this.toast.warning(
            amenities.message ??
              'Could not load the amenity list. You can still edit the room and try again later.',
            'Amenities',
          );
        }

        if (!room.success || !room.data) {
          this.toast.error(room.message ?? 'Room not found.', 'Room');
          return;
        }

        this.patchFromRoom(room.data);
      });
  }

  private loadAmenitiesForCreate(): void {
    this.amenitiesLoading = true;
    this.api
      .listAmenities()
      .pipe(
        timeout(25_000),
        catchError(() =>
          of<AmenitiesApiResponse>({
            success: false,
            data: null,
            message:
              'Could not reach the API to load amenities. Start the ArkifiHub API (e.g. https://localhost:7058), ensure CORS is enabled, and sign in again.',
            code: 'NetworkOrTimeout',
            validationErrors: null,
          }),
        ),
        finalize(() => (this.amenitiesLoading = false)),
      )
      .subscribe((res) => {
        if (res.success && res.data) {
          this.amenities = res.data;
          return;
        }

        this.toast.warning(
          res.message ??
            'Amenities could not be loaded. You can still create the room and add custom amenities after the API is available.',
          'Amenities',
        );
      });
  }

  groupedCatalog(): { category: string; items: AmenityDto[] }[] {
    const catalog = this.amenities.filter((a) => !a.isCustom);
    const map = new Map<string, AmenityDto[]>();
    for (const a of catalog) {
      const key = a.category?.trim() || 'General';
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

  customAmenities(): AmenityDto[] {
    return this.amenities.filter((a) => a.isCustom).sort((a, b) => a.name.localeCompare(b.name));
  }

  toggleAmenity(id: string): void {
    if (this.selectedAmenityIds.has(id)) {
      this.selectedAmenityIds.delete(id);
    } else {
      this.selectedAmenityIds.add(id);
    }
  }

  isSelected(id: string): boolean {
    return this.selectedAmenityIds.has(id);
  }

  addCustomAmenity(): void {
    const name = this.customName.trim();
    if (name.length < 2) {
      return;
    }

    this.saving = true;
    this.api
      .createCustomAmenity({ name, category: this.customCategory.trim() || null })
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success || !res.data) {
          this.toast.showFailedApi(res, 'Amenity');
          return;
        }

        this.toast.success(`${res.data.name} was added to your list.`, 'Custom amenity');
        this.amenities = [...this.amenities, res.data];
        this.selectedAmenityIds.add(res.data.id);
        this.customName = '';
        this.customCategory = '';
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
      maxOccupancy: raw.maxOccupancy,
      basePricePerNight: raw.basePricePerNight,
      amenityIds: [...this.selectedAmenityIds],
    };

    this.saving = true;
    const req$ = this.isCreateMode
      ? this.api.createRoom(body)
      : this.api.updateRoom(this.roomId!, body);

    req$.subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.saving = false;
          this.toast.showFailedApi(res, 'Room');
          return;
        }

        if (!this.isCreateMode) {
          this.saving = false;
          this.patchFromRoom(res.data);
          this.toast.success('Room details saved.', 'Saved');
          return;
        }

        const newId = res.data.id;
        const files = [...this.pendingPhotoFiles];
        if (files.length === 0) {
          this.pendingPhotoFiles = [];
          this.saving = false;
          this.toast.success('Room created.', 'Done');
          void this.router.navigate(['/rooms', newId], { replaceUrl: true });
          return;
        }

        this.api
          .uploadRoomImages(newId, files)
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
                    'Room was created, but photos did not upload. You can add them on the next screen.',
                  'Photos',
                );
              } else {
                this.toast.success('Room created and photos uploaded.', 'Done');
              }

              void this.router.navigate(['/rooms', newId], { replaceUrl: true });
            },
            error: () => {
              this.toast.warning(
                'Room was created, but photos did not upload. You can add them on the next screen.',
                'Photos',
              );
              void this.router.navigate(['/rooms', newId], { replaceUrl: true });
            },
          });
      },
      error: () => {
        this.saving = false;
        this.toast.error('Could not reach the API to save the room.', 'Network');
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
    if (!this.roomId) {
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
      .uploadRoomImages(this.roomId, files)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success || !res.data?.length) {
          this.toast.showFailedApi(res, 'Upload failed');
          return;
        }

        this.images = [...this.images, ...res.data];
        this.toast.success('Photos uploaded.', 'Room photos');
      });
  }

  removeImage(image: RoomImageDto): void {
    if (!this.roomId) {
      return;
    }

    this.saving = true;
    this.api
      .deleteRoomImage(this.roomId, image.id)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((res) => {
        if (!res.success) {
          this.toast.showFailedApi(res, 'Photo');
          return;
        }

        this.images = this.images.filter((i) => i.id !== image.id);
        this.toast.info('Photo removed.', 'Room photos');
      });
  }

  imageUrl(img: RoomImageDto): string {
    return this.api.resolveImageUrl(img.url);
  }

  archiveRoom(): void {
    if (!this.roomId || this.isCreateMode) {
      return;
    }

    if (!globalThis.confirm('Archive this room? It will disappear from your default rooms list until you restore it.')) {
      return;
    }

    this.saving = true;
    this.api
      .archiveRoom(this.roomId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Archive');
            return;
          }

          this.patchFromRoom(res.data);
          this.toast.success('Room archived.', 'Inventory');
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  restoreRoom(): void {
    if (!this.roomId || this.isCreateMode) {
      return;
    }

    this.saving = true;
    this.api
      .restoreRoom(this.roomId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.showFailedApi(res, 'Restore');
            return;
          }

          this.patchFromRoom(res.data);
          this.toast.success('Room restored to your inventory.', 'Inventory');
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  deleteRoom(): void {
    if (!this.roomId || this.isCreateMode) {
      return;
    }

    if (
      !globalThis.confirm(
        'Permanently delete this room and all of its photos? This cannot be undone.',
      )
    ) {
      return;
    }

    this.saving = true;
    this.api
      .deleteRoom(this.roomId)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.showFailedApi(res, 'Delete room');
            return;
          }

          this.toast.success('Room deleted.', 'Inventory');
          void this.router.navigate(['/rooms']);
        },
        error: () => {
          this.toast.error('Could not reach the API.', 'Network');
        },
      });
  }

  private patchFromRoom(room: BusinessRoomDetailDto): void {
    this.roomId = room.id;
    this.isArchived = room.isArchived ?? false;
    this.form.patchValue({
      name: room.name,
      description: room.description ?? '',
      maxOccupancy: room.maxOccupancy,
      basePricePerNight: room.basePricePerNight,
    });
    this.images = [...room.images].sort((a, b) => a.sortOrder - b.sortOrder);
    this.selectedAmenityIds.clear();
    for (const a of room.amenities) {
      this.selectedAmenityIds.add(a.id);
    }
  }
}
