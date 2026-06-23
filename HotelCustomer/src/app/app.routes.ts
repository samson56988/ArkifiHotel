import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'lookup',
    loadComponent: () =>
      import('./pages/booking-lookup/booking-lookup.component').then((m) => m.BookingLookupComponent),
  },
  {
    path: ':slug',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./pages/storefront/storefront-branch-gateway.component').then(
            (m) => m.StorefrontBranchGatewayComponent,
          ),
      },
      {
        path: 'l/:locationId',
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
          {
            path: 'about',
            loadComponent: () =>
              import('./pages/storefront/storefront-about.component').then((m) => m.StorefrontAboutComponent),
          },
          {
            path: 'restaurant',
            loadComponent: () =>
              import('./pages/storefront/storefront-restaurant.component').then(
                (m) => m.StorefrontRestaurantComponent,
              ),
          },
          {
            path: 'event-halls',
            loadComponent: () =>
              import('./pages/storefront/storefront-event-halls.component').then(
                (m) => m.StorefrontEventHallsComponent,
              ),
          },
          {
            path: 'restaurant/payment/verify',
            loadComponent: () =>
              import('./pages/storefront/restaurant-order-payment-verify.component').then(
                (m) => m.RestaurantOrderPaymentVerifyComponent,
              ),
          },
          {
            path: 'booking/payment/verify',
            loadComponent: () =>
              import('./pages/storefront/booking-payment-verify.component').then(
                (m) => m.BookingPaymentVerifyComponent,
              ),
          },
        ],
      },
    ],
  },
  {
    path: '',
    redirectTo: 'lekki-suites',
    pathMatch: 'full',
  },
];
