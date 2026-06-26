import { Injectable, computed, signal } from '@angular/core';
import type { BusinessAccountDto } from '../models/auth.models';
import type { BusinessLocationDto } from '../models/locations.models';

const ACCOUNT_KEY = 'arkifihub_business_account';

@Injectable({ providedIn: 'root' })
export class OrganizationLocationService {
  private readonly account = signal<BusinessAccountDto | null>(this.readStoredAccount());

  readonly isSuperAdmin = computed(() => this.account()?.isSuperAdmin === true);
  readonly hasAllLocationAccess = computed(
    () => this.isSuperAdmin() || this.account()?.hasAllLocationAccess === true,
  );
  readonly locationIds = computed(() => this.account()?.locationIds ?? []);
  readonly defaultLocationId = computed(() => this.account()?.defaultLocationId ?? null);

  hydrateFromStorage(): void {
    this.account.set(this.readStoredAccount());
  }

  setAccount(dto: BusinessAccountDto | null): void {
    this.account.set(dto);
  }

  filterLocations(locations: BusinessLocationDto[]): BusinessLocationDto[] {
    if (this.hasAllLocationAccess()) {
      return locations;
    }

    const allowed = new Set(this.locationIds().map((id) => id.toLowerCase()));
    return locations.filter((l) => allowed.has(l.id.toLowerCase()));
  }

  canAccessLocation(locationId: string | null | undefined): boolean {
    if (!locationId) {
      return this.hasAllLocationAccess();
    }

    if (this.hasAllLocationAccess()) {
      return true;
    }

    return this.locationIds().some((id) => id.toLowerCase() === locationId.toLowerCase());
  }

  primaryLocationId(locations: BusinessLocationDto[]): string | null {
    const filtered = this.filterLocations(locations);
    if (filtered.length === 0) {
      return null;
    }

    const preferred = this.defaultLocationId();
    if (preferred && filtered.some((l) => l.id === preferred)) {
      return preferred;
    }

    return filtered[0]?.id ?? null;
  }

  private readStoredAccount(): BusinessAccountDto | null {
    const raw = localStorage.getItem(ACCOUNT_KEY) ?? sessionStorage.getItem(ACCOUNT_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as BusinessAccountDto;
    } catch {
      return null;
    }
  }
}
