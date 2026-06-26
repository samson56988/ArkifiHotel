import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StorefrontEntryService } from '../services/storefront-entry.service';

function storefrontHomeTree(entry: StorefrontEntryService): string[] {
  const slug = entry.slug();
  const loc = entry.activeLocationId();
  if (slug && loc) {
    return ['/', slug, 'l', loc];
  }
  return ['/', slug];
}

export const hotelStorefrontGuard: CanActivateFn = () => {
  const entry = inject(StorefrontEntryService);
  if (entry.kind() === 'hotel') {
    return true;
  }
  return inject(Router).createUrlTree(storefrontHomeTree(entry));
};

export const shortletStorefrontGuard: CanActivateFn = () => {
  const entry = inject(StorefrontEntryService);
  if (entry.kind() === 'shortlet') {
    return true;
  }
  return inject(Router).createUrlTree(storefrontHomeTree(entry));
};
