import { Injectable, computed, signal } from '@angular/core';
import type { BusinessAccountDto } from '../models/auth.models';

const ACCOUNT_KEY = 'arkifihub_business_account';

export type OrganizationModuleCode =
  | 'dashboard'
  | 'rooms'
  | 'locations'
  | 'amenities'
  | 'facilities'
  | 'event_halls'
  | 'restaurant_menu'
  | 'restaurant_orders'
  | 'bookings'
  | 'payment_configuration'
  | 'customers'
  | 'booking_payments'
  | 'subscription'
  | 'profile'
  | 'social_profile'
  | 'storefront_designer'
  | 'team'
  | 'audit';

@Injectable({ providedIn: 'root' })
export class OrganizationAccessService {
  private readonly account = signal<BusinessAccountDto | null>(this.readStoredAccount());

  readonly isSuperAdmin = computed(() => this.account()?.isSuperAdmin === true);
  readonly hasAllModuleAccess = computed(
    () => this.isSuperAdmin() || this.account()?.hasAllModuleAccess === true,
  );
  readonly moduleCodes = computed(() => this.account()?.moduleCodes ?? []);

  hydrateFromStorage(): void {
    this.account.set(this.readStoredAccount());
  }

  setAccount(dto: BusinessAccountDto | null): void {
    this.account.set(dto);
  }

  canAccess(module: OrganizationModuleCode | string): boolean {
    if (this.isSuperAdmin() || this.hasAllModuleAccess()) {
      return true;
    }

    const codes = this.moduleCodes();
    return codes.some((c) => c.toLowerCase() === module.toLowerCase());
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
