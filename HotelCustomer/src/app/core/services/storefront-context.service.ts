import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import { getMockHotel } from '../data/mock-hotels.data';
import type { HotelShowcase } from '../models/hotel-showcase.models';
import { mapPublicToShowcase } from '../utils/storefront-mapper';
import { PublicStorefrontApiService } from './public-storefront-api.service';

@Injectable({ providedIn: 'root' })
export class StorefrontContextService {
  private readonly storefrontApi = inject(PublicStorefrontApiService);

  readonly storefront = signal<HotelShowcase | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);

  private loadedSlug: string | null = null;

  load(slug: string): Observable<HotelShowcase | null> {
    if (this.loadedSlug === slug && this.storefront()) {
      return of(this.storefront());
    }

    this.loading.set(true);
    this.notFound.set(false);
    this.storefront.set(null);

    return this.storefrontApi.getBySlug(slug).pipe(
      map((res) => (res.success && res.data ? mapPublicToShowcase(res.data) : null)),
      catchError(() => of(null)),
      switchMap((fromApi) => {
        if (fromApi) {
          return of(fromApi);
        }
        return of(getMockHotel(slug));
      }),
      tap((data) => {
        this.loading.set(false);
        if (data) {
          this.loadedSlug = slug;
          this.storefront.set(data);
          document.title = `${data.businessName} — ArkifiStay`;
        } else {
          this.notFound.set(true);
        }
      }),
    );
  }

  reset(): void {
    this.loadedSlug = null;
    this.storefront.set(null);
    this.loading.set(false);
    this.notFound.set(false);
  }
}
