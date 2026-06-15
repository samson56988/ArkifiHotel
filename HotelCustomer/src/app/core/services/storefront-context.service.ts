import { inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import type { PublicStorefront } from '../models/storefront-theme.models';
import { PublicStorefrontApiService } from './public-storefront-api.service';

@Injectable({ providedIn: 'root' })
export class StorefrontContextService {
  private readonly api = inject(PublicStorefrontApiService);

  readonly storefront = signal<PublicStorefront | null>(null);
  readonly loading = signal(false);
  readonly notFound = signal(false);

  private loadedSlug: string | null = null;

  load(slug: string): Observable<PublicStorefront | null> {
    if (this.loadedSlug === slug && this.storefront()) {
      return of(this.storefront());
    }

    this.loading.set(true);
    this.notFound.set(false);
    this.storefront.set(null);

    return new Observable<PublicStorefront | null>((subscriber) => {
      this.api.getBySlug(slug).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.loadedSlug = slug;
            this.storefront.set(res.data);
            document.title = `${res.data.businessName} — ArkifiStay`;
            subscriber.next(res.data);
            subscriber.complete();
            return;
          }

          this.notFound.set(true);
          subscriber.next(null);
          subscriber.complete();
        },
        error: () => {
          this.loading.set(false);
          this.notFound.set(true);
          subscriber.next(null);
          subscriber.complete();
        },
      });
    });
  }

  reset(): void {
    this.loadedSlug = null;
    this.storefront.set(null);
    this.loading.set(false);
    this.notFound.set(false);
  }
}
