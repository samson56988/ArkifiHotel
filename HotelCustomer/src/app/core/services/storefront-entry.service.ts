import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap } from 'rxjs';
import { getMockHotel } from '../data/mock-hotels.data';
import { getMockShortlet } from '../data/mock-shortlets.data';
import type { HotelShowcase } from '../models/hotel-showcase.models';
import type { ShortletShowcase } from '../models/shortlet-showcase.models';
import { mapPublicToShowcase } from '../utils/storefront-mapper';
import { isShortletBusinessType, mapPublicToShortletShowcase } from '../utils/shortlet-mapper';
import { apiLocationId } from '../utils/storefront-path';
import { PublicStorefrontApiService } from './public-storefront-api.service';
import { ShortletContextService } from './shortlet-context.service';
import { StorefrontContextService } from './storefront-context.service';
import { GuestRoomAvailabilityService } from './guest-room-availability.service';
import { DocumentIconService } from './document-icon.service';

export type StorefrontKind = 'hotel' | 'shortlet';

@Injectable({ providedIn: 'root' })
export class StorefrontEntryService {
  private readonly api = inject(PublicStorefrontApiService);
  private readonly hotelCtx = inject(StorefrontContextService);
  private readonly shortletCtx = inject(ShortletContextService);
  private readonly availability = inject(GuestRoomAvailabilityService);
  private readonly documentIcon = inject(DocumentIconService);

  readonly kind = signal<StorefrontKind | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);
  readonly slug = signal('');
  readonly locationRouteId = signal<string | null>(null);

  private loadedKey: string | null = null;

  load(slug: string, locationRouteId?: string | null): Observable<StorefrontKind | null> {
    const apiLoc = apiLocationId(locationRouteId ?? null);
    const cacheKey = `${slug}:${locationRouteId ?? 'gate'}`;

    if (this.loadedKey === cacheKey && this.kind()) {
      return of(this.kind());
    }

    this.loading.set(true);
    this.notFound.set(false);
    this.kind.set(null);
    this.slug.set(slug);
    this.locationRouteId.set(locationRouteId ?? null);
    this.hotelCtx.reset();
    this.shortletCtx.reset();

    return this.api.getBySlug(slug, apiLoc).pipe(
      map((res) => (res.success && res.data ? res.data : null)),
      catchError(() => of(null)),
      switchMap((dto) => {
        if (dto) {
          if (isShortletBusinessType(dto.businessType)) {
            const shortlet = mapPublicToShortletShowcase(dto);
            this.applyShortlet(shortlet, locationRouteId);
            return of<StorefrontKind>('shortlet');
          }
          const hotel = mapPublicToShowcase(dto);
          this.applyHotel(hotel, locationRouteId);
          return of<StorefrontKind>('hotel');
        }

        const mockShortlet = getMockShortlet(slug, locationRouteId ?? null);
        if (mockShortlet) {
          this.applyShortlet(mockShortlet, locationRouteId);
          return of<StorefrontKind>('shortlet');
        }

        const mockHotel = getMockHotel(slug, locationRouteId ?? null);
        if (mockHotel) {
          this.applyHotel(mockHotel, locationRouteId);
          return of<StorefrontKind>('hotel');
        }

        return of(null);
      }),
      tap((result) => {
        this.loading.set(false);
        if (result) {
          this.loadedKey = cacheKey;
          this.kind.set(result);
          const loc = this.locationRouteId() ?? this.activeLocationId();
          if (loc) {
            this.availability.ensureDefaultDates();
            this.availability.refresh(slug, loc);
          }
        } else {
          this.notFound.set(true);
        }
      }),
    );
  }

  activeLocationId(): string | null {
    return (
      this.locationRouteId() ??
      this.hotelCtx.storefront()?.activeLocationId ??
      this.shortletCtx.shortlet()?.activeLocationId ??
      null
    );
  }

  requiresBranchSelection(): boolean {
    return (
      this.hotelCtx.storefront()?.requiresBranchSelection ??
      this.shortletCtx.shortlet()?.requiresBranchSelection ??
      false
    );
  }

  businessName(): string {
    return (
      this.hotelCtx.storefront()?.businessName ??
      this.shortletCtx.shortlet()?.businessName ??
      ''
    );
  }

  branchLocations() {
    return (
      this.hotelCtx.branchLocations().length > 0
        ? this.hotelCtx.branchLocations()
        : this.shortletCtx.branchLocations()
    );
  }

  reset(): void {
    this.loadedKey = null;
    this.kind.set(null);
    this.slug.set('');
    this.locationRouteId.set(null);
    this.loading.set(false);
    this.notFound.set(false);
    this.hotelCtx.reset();
    this.shortletCtx.reset();
    this.availability.reset();
    this.documentIcon.resetToDefault();
  }

  private applyHotel(data: HotelShowcase, locationRouteId?: string | null): void {
    this.hotelCtx.apply(data, locationRouteId);
    const branch = data.branchName ? ` — ${data.branchName}` : '';
    document.title = `${data.businessName}${branch} — ArkifiStay`;
    this.documentIcon.applyPropertyIcon({
      logoUrl: data.logoUrl,
      businessName: data.businessName,
      primaryColor: data.theme.colors.primary,
      accentColor: data.theme.colors.accent,
    });
  }

  private applyShortlet(data: ShortletShowcase, locationRouteId?: string | null): void {
    this.shortletCtx.apply(data, locationRouteId);
    const branch = data.branchName ? ` — ${data.branchName}` : '';
    document.title = `${data.businessName}${branch} — ArkifiStay`;
    this.documentIcon.applyPropertyIcon({
      logoUrl: data.logoUrl,
      businessName: data.businessName,
      primaryColor: data.theme.colors.primary,
      accentColor: data.theme.colors.accent,
    });
  }
}
