import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import type {
  RestaurantMenuCategoryDto,
  RestaurantMenuItemDto,
} from '../../core/models/restaurant-menu.models';
import type { BusinessLocationDto } from '../../core/models/locations.models';
import { BusinessLocationsApiService } from '../../core/services/business-locations-api.service';
import { OrganizationLocationService } from '../../core/services/organization-location.service';
import { BusinessRestaurantMenuApiService } from '../../core/services/business-restaurant-menu-api.service';
import { ToastService } from '../../core/services/toast.service';
import { ALLOWED_IMAGE_ACCEPT, filterAllowedImageFiles } from '../../core/utils/image-upload';
import { BusinessWorkspaceComponent } from '../../layouts/business-workspace/business-workspace.component';

type MenuSection = 'food' | 'drink';

@Component({
  selector: 'app-restaurant-menu',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BusinessWorkspaceComponent],
  templateUrl: './restaurant-menu.component.html',
  styleUrl: './restaurant-menu.component.scss',
})
export class RestaurantMenuComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(BusinessRestaurantMenuApiService);
  private readonly locationsApi = inject(BusinessLocationsApiService);
  private readonly orgLocation = inject(OrganizationLocationService);
  private readonly toast = inject(ToastService);

  readonly imageAccept = ALLOWED_IMAGE_ACCEPT;
  readonly loading = signal(true);
  readonly savingSettings = signal(false);
  readonly activeSection = signal<MenuSection>('food');
  readonly categories = signal<RestaurantMenuCategoryDto[]>([]);
  readonly selectedCategoryId = signal<string | null>(null);
  readonly items = signal<RestaurantMenuItemDto[]>([]);
  readonly itemsLoading = signal(false);
  readonly editingItemId = signal<string | null>(null);
  readonly locations = signal<BusinessLocationDto[]>([]);
  readonly locationId = signal<string | null>(null);

  readonly settingsForm = this.fb.nonNullable.group({
    enabled: [false],
    navLabel: ['Restaurant & menu', [Validators.required, Validators.maxLength(120)]],
    heroEyebrow: ['Dining', Validators.maxLength(120)],
    heroTitle: ['Restaurant & bar', [Validators.required, Validators.maxLength(200)]],
    heroSubtitle: [''],
    mealsSectionTitle: ['Meals', Validators.maxLength(120)],
    drinksSectionTitle: ['Drinks', Validators.maxLength(120)],
  });

  readonly categoryForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    sortOrder: [0],
  });

  readonly itemForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: [''],
    price: [0, [Validators.required, Validators.min(0)]],
    tags: [''],
    sortOrder: [0],
  });

  heroImageUrl: string | null = null;

  ngOnInit(): void {
    this.orgLocation.hydrateFromStorage();
    this.locationsApi.listLocations().subscribe({
      next: (res) => {
        if (!res.success || !res.data?.length) {
          this.loading.set(false);
          return;
        }
        const allowed = this.orgLocation.filterLocations(res.data);
        if (allowed.length === 0) {
          this.loading.set(false);
          return;
        }
        this.locations.set(allowed);
        this.locationId.set(this.orgLocation.primaryLocationId(allowed));
        this.loadMenuForLocation();
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Could not load locations.', 'Restaurant');
      },
    });
  }

  onLocationChange(locationId: string): void {
    if (!locationId || locationId === this.locationId()) {
      return;
    }
    this.locationId.set(locationId);
    this.selectedCategoryId.set(null);
    this.items.set([]);
    this.loadMenuForLocation();
  }

  private locationIdOrNull(): string | null {
    const id = this.locationId();
    return id && id.length > 0 ? id : null;
  }

  loadMenuForLocation(): void {
    const locationId = this.locationIdOrNull();
    if (!locationId) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.api.getSettings(locationId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.settingsForm.patchValue(res.data);
          this.heroImageUrl = res.data.heroImageUrl;
        }
        this.loading.set(false);
        this.loadCategories();
      },
      error: () => {
        this.loading.set(false);
        this.toast.error('Could not load menu settings.', 'Restaurant');
      },
    });
  }

  saveSettings(): void {
    const locationId = this.locationIdOrNull();
    if (!locationId || this.settingsForm.invalid) {
      this.settingsForm.markAllAsTouched();
      return;
    }
    this.savingSettings.set(true);
    this.api.updateSettings(locationId, this.settingsForm.getRawValue()).subscribe({
      next: (res) => {
        this.savingSettings.set(false);
        if (res.success) {
          this.heroImageUrl = res.data?.heroImageUrl ?? this.heroImageUrl;
          this.toast.success('Menu settings saved.', 'Restaurant');
        } else {
          this.toast.showFailedApi(res, 'Restaurant');
        }
      },
      error: () => {
        this.savingSettings.set(false);
        this.toast.error('Could not save settings.', 'Restaurant');
      },
    });
  }

  onHeroImageSelected(event: Event): void {
    const locationId = this.locationIdOrNull();
    if (!locationId) {
      return;
    }
    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    const file = filterAllowedImageFiles(picked).accepted[0];
    if (!file) {
      return;
    }
    this.api.uploadHeroImage(locationId, file).subscribe((res) => {
      if (res.success && res.data) {
        this.heroImageUrl = res.data.heroImageUrl;
        this.toast.success('Hero photo updated.', 'Restaurant');
      } else {
        this.toast.showFailedApi(res, 'Restaurant');
      }
    });
  }

  removeHeroImage(): void {
    const locationId = this.locationIdOrNull();
    if (!locationId) {
      return;
    }
    this.api.deleteHeroImage(locationId).subscribe((res) => {
      if (res.success) {
        this.heroImageUrl = null;
        this.toast.success('Hero photo removed.', 'Restaurant');
      }
    });
  }

  setSection(section: MenuSection): void {
    this.activeSection.set(section);
    this.selectedCategoryId.set(null);
    this.items.set([]);
    this.loadCategories();
  }

  loadCategories(): void {
    const locationId = this.locationIdOrNull();
    if (!locationId) {
      return;
    }
    this.api.listCategories(locationId, this.activeSection()).subscribe((res) => {
      if (res.success && res.data) {
        this.categories.set(res.data);
      }
    });
  }

  addCategory(): void {
    const locationId = this.locationIdOrNull();
    if (!locationId || this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }
    const raw = this.categoryForm.getRawValue();
    this.api
      .createCategory(locationId, {
        name: raw.name,
        section: this.activeSection(),
        sortOrder: raw.sortOrder,
      })
      .subscribe((res) => {
        if (res.success) {
          this.categoryForm.reset({ name: '', sortOrder: 0 });
          this.loadCategories();
          this.toast.success('Category added.', 'Restaurant');
        } else {
          this.toast.showFailedApi(res, 'Restaurant');
        }
      });
  }

  selectCategory(categoryId: string): void {
    this.selectedCategoryId.set(categoryId);
    this.editingItemId.set(null);
    this.itemForm.reset({ name: '', description: '', price: 0, tags: '', sortOrder: 0 });
    this.loadItems(categoryId);
  }

  loadItems(categoryId: string): void {
    const locationId = this.locationIdOrNull();
    if (!locationId) {
      return;
    }
    this.itemsLoading.set(true);
    this.api.listItems(locationId, categoryId, true).subscribe((res) => {
      this.itemsLoading.set(false);
      if (res.success && res.data) {
        this.items.set(res.data);
      }
    });
  }

  startEditItem(item: RestaurantMenuItemDto): void {
    this.editingItemId.set(item.id);
    this.itemForm.patchValue({
      name: item.name,
      description: item.description ?? '',
      price: item.price,
      tags: (item.tags ?? []).join(', '),
      sortOrder: item.sortOrder,
    });
  }

  cancelEditItem(): void {
    this.editingItemId.set(null);
    this.itemForm.reset({ name: '', description: '', price: 0, tags: '', sortOrder: 0 });
  }

  saveItem(): void {
    const locationId = this.locationIdOrNull();
    const categoryId = this.selectedCategoryId();
    if (!locationId || !categoryId || this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }
    const raw = this.itemForm.getRawValue();
    const body = {
      name: raw.name,
      description: raw.description || null,
      price: raw.price,
      tags: raw.tags
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean),
      sortOrder: raw.sortOrder,
    };
    const editId = this.editingItemId();
    const req = editId
      ? this.api.updateItem(locationId, editId, body)
      : this.api.createItem(locationId, categoryId, body);

    req.subscribe((res) => {
      if (res.success) {
        this.cancelEditItem();
        this.loadItems(categoryId);
        this.loadCategories();
        this.toast.success(editId ? 'Item updated.' : 'Item added.', 'Restaurant');
      } else {
        this.toast.showFailedApi(res, 'Restaurant');
      }
    });
  }

  toggleItemArchive(item: RestaurantMenuItemDto): void {
    const locationId = this.locationIdOrNull();
    const categoryId = this.selectedCategoryId();
    if (!locationId || !categoryId) {
      return;
    }
    const req = item.isArchived
      ? this.api.restoreItem(locationId, item.id)
      : this.api.archiveItem(locationId, item.id);
    req.subscribe((res) => {
      if (res.success) {
        this.loadItems(categoryId);
        this.loadCategories();
      }
    });
  }

  toggleItemAvailability(item: RestaurantMenuItemDto): void {
    const locationId = this.locationIdOrNull();
    const categoryId = this.selectedCategoryId();
    if (!locationId || !categoryId) {
      return;
    }
    this.api.setItemAvailability(locationId, item.id, !item.isAvailable).subscribe((res) => {
      if (res.success) {
        this.loadItems(categoryId);
        this.toast.success(
          item.isAvailable ? 'Item marked unavailable for guests.' : 'Item is available to order again.',
          'Restaurant',
        );
      } else {
        this.toast.showFailedApi(res, 'Restaurant');
      }
    });
  }

  onItemImageSelected(item: RestaurantMenuItemDto, event: Event): void {
    const locationId = this.locationIdOrNull();
    const input = event.target as HTMLInputElement;
    const picked = input.files ? Array.from(input.files) : [];
    input.value = '';
    const file = filterAllowedImageFiles(picked).accepted[0];
    const categoryId = this.selectedCategoryId();
    if (!file || !locationId || !categoryId) {
      return;
    }
    this.api.uploadItemImage(locationId, item.id, file).subscribe((res) => {
      if (res.success) {
        this.loadItems(categoryId);
        this.toast.success('Item photo updated.', 'Restaurant');
      } else {
        this.toast.showFailedApi(res, 'Restaurant');
      }
    });
  }

  imageUrl(path: string | null | undefined): string {
    return this.api.resolveImageUrl(path);
  }

  formatPrice(amount: number): string {
    return `₦${amount.toLocaleString('en-NG', { maximumFractionDigits: 0 })}`;
  }
}
