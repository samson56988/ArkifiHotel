import { Component, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import type { AmenitiesApiResponse } from '../../core/models/amenities.models';
import type { AmenityDto } from '../../core/models/amenities.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import type {
  BusinessRoomDetailDto,
  RoomDetailApiResponse,
  RoomImageDto,
} from '../../core/models/rooms.models';
import { BusinessAmenitiesApiService } from '../../core/services/business-amenities-api.service';
import { BusinessContextService } from '../../core/services/business-context.service';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { BusinessRoomsApiService } from '../../core/services/business-rooms-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ALLOWED_IMAGE_ACCEPT, filterAllowedImageFiles } from '../../core/utils/image-upload';

@Component({
  selector: 'app-room-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './room-form.component.html',
  styleUrl: './room-form.component.scss',
})
export class RoomFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessRoomsApiService);
  private readonly amenitiesApi = inject(BusinessAmenitiesApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly businessContext = inject(BusinessContextService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    tagline: [''],
    description: [''],
    maxOccupancy: [2, [Validators.required, Validators.min(1), Validators.max(50)]],
    bedroomCount: [1, [Validators.min(1), Validators.max(20)]],
    bathroomCount: [1, [Validators.min(1), Validators.max(20)]],
    isGuestFavorite: [false],
    quantity: [1, [Validators.required, Validators.min(1), Validators.max(500)]],
    basePricePerNight: [0, [Validators.required, Validators.min(0)]],
    basePricePerWeek: [null as number | null, [Validators.min(0)]],
    locationId: ['', Validators.required],
  });

  readonly isShortlet = this.businessContext.isShortlet.bind(this.businessContext);

  constructor() {
    effect(() => {
      if (this.businessContext.loaded()) {
        this.applyBusinessTypeValidators();
      }
    });
  }

  readonly amenities = signal<AmenityDto[]>([]);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly selectedAmenityIds = signal<string[]>([]);

  readonly roomId = signal<string | null>(null);
  readonly images = signal<RoomImageDto[]>([]);
  readonly initialLoading = signal(false);
  readonly amenitiesLoading = signal(false);
  readonly locationsLoading = signal(false);
  readonly saving = signal(false);
  readonly isArchived = signal(false);

  /** Queued on “Add room”; uploaded right after the room is created. */
  readonly pendingPhotoFiles = signal<File[]>([]);

  readonly imageAccept = ALLOWED_IMAGE_ACCEPT;

  /** Starter taglines for shortlet listings (UI-only; not from API). */
  readonly taglinePresets: readonly string[] = [
    'Floor-to-ceiling windows & city skyline',
    'Walk to restaurants, nightlife & the beach',
    'Peaceful retreat with lagoon views',
    'Fully furnished & move-in ready',
    'Perfect for remote work & weekend stays',
    'Sun-filled living with private balcony',
    'Premium linens, fast Wi-Fi & 24/7 power',
    'Stylish studio in the heart of the city',
    'Spacious layout ideal for families',
    'Quiet neighbourhood, minutes from downtown'];

  get isCreateMode(): boolean {
    return this.roomId() === null;
  }

  ngOnInit(): void {
    this.businessContext.ensureLoaded();
    this.applyBusinessTypeValidators();
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      this.initFromRoute(params.get('roomId'));
    });
  }

  private applyBusinessTypeValidators(): void {
    const shortlet = this.businessContext.isShortlet();
    const tagline = this.form.controls.tagline;
    const bedroom = this.form.controls.bedroomCount;
    const bathroom = this.form.controls.bathroomCount;
    const quantity = this.form.controls.quantity;

    if (shortlet) {
      tagline.setValidators([Validators.required, Validators.minLength(5), Validators.maxLength(300)]);
      bedroom.setValidators([Validators.required, Validators.min(1), Validators.max(20)]);
      bathroom.setValidators([Validators.required, Validators.min(1), Validators.max(20)]);
      quantity.clearValidators();
      quantity.setValue(1, { emitEvent: false });
    } else {
      tagline.clearValidators();
      bedroom.clearValidators();
      bathroom.clearValidators();
      quantity.setValidators([Validators.required, Validators.min(1), Validators.max(500)]);
    }

    tagline.updateValueAndValidity({ emitEvent: false });
    bedroom.updateValueAndValidity({ emitEvent: false });
    bathroom.updateValueAndValidity({ emitEvent: false });
    quantity.updateValueAndValidity({ emitEvent: false });
  }

  private isShortletListing(): boolean {
    return this.businessContext.businessType() === 'Shortlet';
  }

  private coerceRoomCount(value: number): number {
    if (!Number.isFinite(value) || value < 1) {
      return 1;
    }

    return Math.min(20, Math.floor(value));
  }

  private initFromRoute(paramId: string | null): void {
    this.resetFormState();
    this.roomId.set(paramId);

    if (!paramId) {
      this.loadAmenitiesForCreate();
      this.loadLocations();
      return;
    }

    this.loadRoomForEdit(paramId);
  }

  private resetFormState(): void {
    this.form.reset({
      name: '',
      tagline: '',
      description: '',
      maxOccupancy: 2,
      bedroomCount: 1,
      bathroomCount: 1,
      isGuestFavorite: false,
      quantity: 1,
      basePricePerNight: 0,
      basePricePerWeek: null,
      locationId: '',
    });
    this.amenities.set([]);
    this.selectedAmenityIds.set([]);
    this.images.set([]);
    this.pendingPhotoFiles.set([]);
    this.isArchived.set(false);
    this.initialLoading.set(false);
    this.amenitiesLoading.set(false);
    this.locationsLoading.set(false);
    this.saving.set(false);
  }

  private loadLocations(): void {
    this.locationsLoading.set(true);
    this.locationsApi
      .listLocations()
      .pipe(
        timeout(25_000),
        finalize(() => this.locationsLoading.set(false)),
      )
      .subscribe((res) => {
        if (res.success && res.data) {
          this.locations.set(res.data);
        }
      });
  }

  private loadRoomForEdit(paramId: string): void {
    this.initialLoading.set(true);
    const room$ = this.api.getRoom(paramId).pipe(
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

    const amenities$ = this.amenitiesApi.listAmenities().pipe(
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

    const locations$ = this.locationsApi.listLocations().pipe(
      timeout(25_000),
      catchError(() =>
        of({
          success: false,
          data: null,
          message: 'Could not load locations.',
          code: 'NetworkOrTimeout',
          validationErrors: null,
        }),
      ),
    );

    forkJoin({ amenities: amenities$, room: room$, locations: locations$ })
      .pipe(finalize(() => this.initialLoading.set(false)))
      .subscribe(({ amenities, room, locations }) => {
        if (amenities.success && amenities.data) {
          this.amenities.set(amenities.data);
        } else {
          this.toast.warning(
            amenities.message ??
              'Could not load the amenity list. You can still edit the room and try again later.',
            'Amenities',
          );
        }

        if (locations.success && locations.data) {
          this.locations.set(locations.data);
        }

        if (!room.success || !room.data) {
          this.toast.error(room.message ?? 'Room not found.', 'Room');
          return;
        }

        this.patchFromRoom(room.data);
      });
  }

  private loadAmenitiesForCreate(): void {
    this.amenitiesLoading.set(true);
    this.amenitiesApi
      .listAmenities()
      .pipe(
        timeout(25_000),
        catchError(() =>
          of<AmenitiesApiResponse>({
            success: false,
            data: null,
            message:
              'Could not reach the API to load amenities. Standard amenities should appear automatically — refresh or try again.',
            code: 'NetworkOrTimeout',
            validationErrors: null,
          }),
        ),
        finalize(() => this.amenitiesLoading.set(false)),
      )
      .subscribe((res) => {
        if (res.success && res.data) {
          this.amenities.set(res.data);
          return;
        }

        this.toast.warning(
          res.message ??
            'Amenities could not be loaded. You can still save the room and assign amenities after they are set up.',
          'Amenities',
        );
      });
  }

  groupedAmenities(): { category: string; items: AmenityDto[] }[] {
    const map = new Map<string, AmenityDto[]>();
    for (const a of this.amenities()) {
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

  toggleAmenity(id: string): void {
    const current = this.selectedAmenityIds();
    if (current.includes(id)) {
      this.selectedAmenityIds.set(current.filter((x) => x !== id));
    } else {
      this.selectedAmenityIds.set([...current, id]);
    }
  }

  isSelected(id: string): boolean {
    return this.selectedAmenityIds().includes(id);
  }

  applyTaglinePreset(preset: string): void {
    this.form.controls.tagline.setValue(preset);
    this.form.controls.tagline.markAsTouched();
    this.form.controls.tagline.updateValueAndValidity();
  }

  isTaglinePresetSelected(preset: string): boolean {
    return this.form.controls.tagline.value.trim() === preset;
  }

  onSubmit(): void {
    this.applyBusinessTypeValidators();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      const label = this.businessContext.isShortlet() ? 'apartment listing' : 'room';
      this.toast.warning(`Please fix the highlighted fields before saving this ${label}.`, 'Form');
      return;
    }

    const raw = this.form.getRawValue();
    const shortlet = this.isShortletListing();
    const body = {
      name: raw.name.trim(),
      tagline: shortlet ? raw.tagline.trim() : raw.tagline.trim() || null,
      description: raw.description.trim() || null,
      maxOccupancy: raw.maxOccupancy,
      bedroomCount: shortlet ? this.coerceRoomCount(raw.bedroomCount) : null,
      bathroomCount: shortlet ? this.coerceRoomCount(raw.bathroomCount) : null,
      isGuestFavorite: shortlet ? raw.isGuestFavorite : false,
      quantity: shortlet ? 1 : raw.quantity,
      basePricePerNight: raw.basePricePerNight,
      basePricePerWeek:
        shortlet && raw.basePricePerWeek && raw.basePricePerWeek > 0 ? raw.basePricePerWeek : null,
      locationId: raw.locationId.trim(),
      amenityIds: [...this.selectedAmenityIds()],
    };

    this.saving.set(true);
    const req$ = this.isCreateMode
      ? this.api.createRoom(body)
      : this.api.updateRoom(this.roomId()!, body);

    req$.subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.saving.set(false);
          this.toast.showFailedApi(res, 'Room');
          return;
        }

        if (!this.isCreateMode) {
          this.saving.set(false);
          this.toast.success(
            this.businessContext.isShortlet() ? 'Apartment listing saved.' : 'Room details saved.',
            'Saved',
          );
          void this.router.navigate(['/rooms'], { replaceUrl: true });
          return;
        }

        const newId = res.data.id;
        const files = [...this.pendingPhotoFiles()];
        if (files.length === 0) {
          this.pendingPhotoFiles.set([]);
          this.saving.set(false);
          this.toast.success('Room created.', 'Done');
          void this.router.navigate(['/rooms'], { replaceUrl: true });
          return;
        }

        this.api
          .uploadRoomImages(newId, files)
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
                    'Room was created, but photos did not upload. You can add them on the next screen.',
                  'Photos',
                );
              } else {
                this.toast.success('Room created and photos uploaded.', 'Done');
              }

              void this.router.navigate(['/rooms'], { replaceUrl: true });
            },
            error: () => {
              this.toast.warning(
                'Room was created, but photos did not upload. You can add them from the rooms list.',
                'Photos',
              );
              void this.router.navigate(['/rooms'], { replaceUrl: true });
            },
          });
      },
      error: () => {
        this.saving.set(false);
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
    const id = this.roomId();
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
      .uploadRoomImages(id, accepted)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe((res) => {
        if (!res.success || !res.data?.length) {
          this.toast.showFailedApi(res, 'Upload failed');
          return;
        }

        this.images.update((current) => [...current, ...res.data!]);
        this.toast.success('Photos uploaded.', 'Room photos');
      });
  }

  removeImage(image: RoomImageDto): void {
    const id = this.roomId();
    if (!id) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteRoomImage(id, image.id)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe((res) => {
        if (!res.success) {
          this.toast.showFailedApi(res, 'Photo');
          return;
        }

        this.images.update((current) => current.filter((i) => i.id !== image.id));
        this.toast.info('Photo removed.', 'Room photos');
      });
  }

  imageUrl(img: RoomImageDto): string {
    return this.api.resolveImageUrl(img.url);
  }

  archiveRoom(): void {
    const id = this.roomId();
    if (!id || this.isCreateMode) {
      return;
    }

    if (!globalThis.confirm('Archive this room? It will disappear from your default rooms list until you restore it.')) {
      return;
    }

    this.saving.set(true);
    this.api
      .archiveRoom(id)
      .pipe(finalize(() => this.saving.set(false)))
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
    const id = this.roomId();
    if (!id || this.isCreateMode) {
      return;
    }

    this.saving.set(true);
    this.api
      .restoreRoom(id)
      .pipe(finalize(() => this.saving.set(false)))
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
    const id = this.roomId();
    if (!id || this.isCreateMode) {
      return;
    }

    if (
      !globalThis.confirm(
        'Permanently delete this room and all of its photos? This cannot be undone.',
      )
    ) {
      return;
    }

    this.saving.set(true);
    this.api
      .deleteRoom(id)
      .pipe(finalize(() => this.saving.set(false)))
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
    this.roomId.set(room.id);
    this.isArchived.set(room.isArchived ?? false);
    this.form.patchValue({
      name: room.name,
      tagline: room.tagline ?? '',
      description: room.description ?? '',
      maxOccupancy: room.maxOccupancy,
      bedroomCount: room.bedroomCount ?? 1,
      bathroomCount: room.bathroomCount ?? 1,
      isGuestFavorite: room.isGuestFavorite ?? false,
      quantity: room.quantity ?? 1,
      basePricePerNight: room.basePricePerNight,
      basePricePerWeek: room.basePricePerWeek ?? null,
      locationId: room.locationId ?? '',
    });
    this.images.set([...room.images].sort((a, b) => a.sortOrder - b.sortOrder));

    const allowed = new Set(this.amenities().map((a) => a.id));
    const selected: string[] = [];
    const orphaned: string[] = [];
    for (const a of room.amenities) {
      if (allowed.has(a.id)) {
        selected.push(a.id);
      } else {
        orphaned.push(a.name);
      }
    }

    this.selectedAmenityIds.set(selected);

    if (orphaned.length > 0) {
      this.toast.warning(
        `Some amenities on this room are not on your list anymore (${orphaned.join(', ')}). Re-select from your amenities and save.`,
        'Amenities',
      );
    }

    this.applyBusinessTypeValidators();
    this.form.updateValueAndValidity();
  }
}
