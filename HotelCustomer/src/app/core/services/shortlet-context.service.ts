import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import { getMockShortlet } from '../data/mock-shortlets.data';
import type { HotelShowcase, ShowcaseLocation } from '../models/hotel-showcase.models';
import type { ShortletShowcase } from '../models/shortlet-showcase.models';
import { shortletAsHotelShowcase, listingToShowcaseRoom } from '../utils/shortlet-adapter';
import { isShortletBusinessType, mapPublicToShortletShowcase } from '../utils/shortlet-mapper';
import { apiLocationId } from '../utils/storefront-path';
import { PublicStorefrontApiService } from './public-storefront-api.service';
import { DocumentIconService } from './document-icon.service';

@Injectable({ providedIn: 'root' })
export class ShortletContextService {
  private readonly storefrontApi = inject(PublicStorefrontApiService);
  private readonly documentIcon = inject(DocumentIconService);

  readonly shortlet = signal<ShortletShowcase | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);
  readonly locationRouteId = signal<string | null>(null);

  private loadedKey: string | null = null;

  load(slug: string, locationRouteId?: string | null): Observable<ShortletShowcase | null> {
    const apiLoc = apiLocationId(locationRouteId ?? null);
    const cacheKey = `${slug}:${locationRouteId ?? 'gate'}`;

    if (this.loadedKey === cacheKey && this.shortlet()) {
      return of(this.shortlet());
    }

    this.loading.set(true);
    this.notFound.set(false);
    this.shortlet.set(null);
    this.locationRouteId.set(locationRouteId ?? null);

    return this.storefrontApi.getBySlug(slug, apiLoc).pipe(
      map((res) => (res.success && res.data ? res.data : null)),
      catchError(() => of(null)),
      switchMap((dto) => {
        if (dto && isShortletBusinessType(dto.businessType)) {
          return of(mapPublicToShortletShowcase(dto));
        }
        return of(getMockShortlet(slug, locationRouteId ?? null));
      }),
      tap((result) => {
        this.loading.set(false);
        if (result) {
          this.loadedKey = cacheKey;
          this.apply(result, locationRouteId);
        } else {
          this.notFound.set(true);
        }
      }),
    );
  }

  apply(data: ShortletShowcase, locationRouteId?: string | null): void {
    this.shortlet.set(data);
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

  path(...segments: string[]): string[] {
    const sl = this.shortlet();
    const slug = sl?.slug;
    const loc = this.locationRouteId() ?? sl?.activeLocationId;
    if (!slug) {
      return ['/', ...segments.filter(Boolean)];
    }
    if (loc) {
      return ['/', slug, 'l', loc, ...segments.filter(Boolean)];
    }
    return ['/', slug, ...segments.filter(Boolean)];
  }

  readonly branchLocations = (): ShowcaseLocation[] => this.shortlet()?.locations ?? [];

  asHotelShowcase(): HotelShowcase | null {
    const sl = this.shortlet();
    return sl ? shortletAsHotelShowcase(sl) : null;
  }

  findListing(id: string) {
    return this.shortlet()?.listings.find((l) => l.id === id) ?? null;
  }

  listingAsRoom(id: string) {
    const listing = this.findListing(id);
    return listing ? listingToShowcaseRoom(listing) : null;
  }

  reset(): void {
    this.loadedKey = null;
    this.shortlet.set(null);
    this.locationRouteId.set(null);
    this.loading.set(false);
    this.notFound.set(false);
  }
}
