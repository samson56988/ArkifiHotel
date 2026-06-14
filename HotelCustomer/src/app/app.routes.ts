import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'lookup',
    loadComponent: () =>
      import('./pages/booking-lookup/booking-lookup.component').then((m) => m.BookingLookupComponent),
  },
  {
    path: ':slug',
    loadComponent: () =>
      import('./pages/storefront/storefront-page.component').then((m) => m.StorefrontPageComponent),
  },
  {
    path: '',
    redirectTo: 'lookup',
    pathMatch: 'full',
  },
];
