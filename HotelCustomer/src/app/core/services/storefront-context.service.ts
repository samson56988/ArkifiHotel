import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import { getMockHotel } from '../data/mock-hotels.data';
import type { HotelShowcase, ShowcaseLocation } from '../models/hotel-showcase.models';
import { mapPublicToShowcase } from '../utils/storefront-mapper';
import { apiLocationId } from '../utils/storefront-path';
import { PublicStorefrontApiService } from './public-storefront-api.service';
import { DocumentIconService } from './document-icon.service';

@Injectable({ providedIn: 'root' })
export class StorefrontContextService {
  private readonly storefrontApi = inject(PublicStorefrontApiService);
  private readonly documentIcon = inject(DocumentIconService);

  readonly storefront = signal<HotelShowcase | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);
  readonly locationRouteId = signal<string | null>(null);

  private loadedKey: string | null = null;

  /** Load storefront for branch gate (no location) or with a selected branch. */
  load(slug: string, locationRouteId?: string | null): Observable<HotelShowcase | null> {
    const apiLoc = apiLocationId(locationRouteId ?? null);
    const cacheKey = `${slug}:${locationRouteId ?? 'gate'}`;

    if (this.loadedKey === cacheKey && this.storefront()) {
      return of(this.storefront());
    }

    this.loading.set(true);
    this.notFound.set(false);
    this.storefront.set(null);
    this.locationRouteId.set(locationRouteId ?? null);

    return this.storefrontApi.getBySlug(slug, apiLoc).pipe(
      map((res) => (res.success && res.data ? mapPublicToShowcase(res.data) : null)),
      catchError(() => of(null)),
      switchMap((fromApi) => {
        if (fromApi) {
          return of(fromApi);
        }
        return of(getMockHotel(slug, locationRouteId ?? null));
      }),
      tap((data) => {
        this.loading.set(false);
        if (data) {
          this.loadedKey = cacheKey;
          this.storefront.set(data);
          this.locationRouteId.set(locationRouteId ?? data.activeLocationId ?? null);
          const branch = data.branchName ? ` — ${data.branchName}` : '';
          document.title = `${data.businessName}${branch} — ArkifiStay`;
          this.documentIcon.applyPropertyIcon({
            logoUrl: data.logoUrl,
            businessName: data.businessName,
            primaryColor: data.theme.colors.primary,
            accentColor: data.theme.colors.accent,
          });
        } else {
          this.notFound.set(true);
        }
      }),
    );
  }

  path(...segments: string[]): string[] {
    const sf = this.storefront();
    const slug = sf?.slug;
    const loc = this.locationRouteId() ?? sf?.activeLocationId;
    if (!slug) {
      return ['/', ...segments.filter(Boolean)];
    }
    if (loc) {
      return ['/', slug, 'l', loc, ...segments.filter(Boolean)];
    }
    return ['/', slug, ...segments.filter(Boolean)];
  }

  readonly branchLocations = (): ShowcaseLocation[] => this.storefront()?.locations ?? [];

  reset(): void {
    this.loadedKey = null;
    this.storefront.set(null);
    this.locationRouteId.set(null);
    this.loading.set(false);
    this.notFound.set(false);
  }

  apply(data: HotelShowcase, locationRouteId?: string | null): void {
    this.storefront.set(data);
    this.locationRouteId.set(locationRouteId ?? data.activeLocationId ?? null);
    this.loading.set(false);
    this.notFound.set(false);
    this.documentIcon.applyPropertyIcon({
      logoUrl: data.logoUrl,
      businessName: data.businessName,
      primaryColor: data.theme.colors.primary,
      accentColor: data.theme.colors.accent,
    });
  }
}
