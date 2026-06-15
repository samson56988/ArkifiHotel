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
      import('./pages/storefront/storefront-layout.component').then((m) => m.StorefrontLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/storefront/storefront-home.component').then((m) => m.StorefrontHomeComponent),
      },
      {
        path: 'rooms',
        loadComponent: () =>
          import('./pages/storefront/storefront-rooms.component').then((m) => m.StorefrontRoomsComponent),
      },
      {
        path: 'facilities',
        loadComponent: () =>
          import('./pages/storefront/storefront-facilities.component').then(
            (m) => m.StorefrontFacilitiesComponent,
          ),
      },
    ],
  },
  {
    path: '',
    redirectTo: 'lookup',
    pathMatch: 'full',
  },
];
