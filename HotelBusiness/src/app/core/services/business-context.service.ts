import { Injectable, inject, signal } from '@angular/core';
import { SubscriptionApiService } from './subscription-api.service';
import type { BusinessTypeOption } from '../models/subscription.models';

@Injectable({ providedIn: 'root' })
export class BusinessContextService {
  private readonly subscriptionApi = inject(SubscriptionApiService);

  readonly businessType = signal<BusinessTypeOption>('Hotel');
  readonly loaded = signal(false);

  private loadStarted = false;

  ensureLoaded(): void {
    if (this.loadStarted) {
      return;
    }
    this.loadStarted = true;
    this.subscriptionApi.getCurrent().subscribe({
      next: (res) => {
        if (res.success && res.data?.businessType) {
          this.businessType.set(res.data.businessType);
        }
        this.loaded.set(true);
      },
      error: () => this.loaded.set(true),
    });
  }

  isShortlet(): boolean {
    return this.businessType() === 'Shortlet';
  }

  isHotel(): boolean {
    return this.businessType() === 'Hotel';
  }

  setType(type: BusinessTypeOption): void {
    this.businessType.set(type);
    this.loaded.set(true);
  }
}
