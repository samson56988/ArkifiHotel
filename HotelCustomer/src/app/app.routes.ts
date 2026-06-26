import { Routes } from '@angular/router';
import { hotelStorefrontGuard, shortletStorefrontGuard } from './core/guards/storefront-kind.guard';

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
          import('./pages/storefront/storefront-shell.component').then((m) => m.StorefrontShellComponent),
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./pages/storefront/storefront-home-dispatcher.component').then(
                (m) => m.StorefrontHomeDispatcherComponent,
              ),
          },
          {
            path: 'rooms',
            canActivate: [hotelStorefrontGuard],
            loadComponent: () =>
              import('./pages/storefront/storefront-rooms.component').then((m) => m.StorefrontRoomsComponent),
          },
          {
            path: 'facilities',
            canActivate: [hotelStorefrontGuard],
            loadComponent: () =>
              import('./pages/storefront/storefront-facilities.component').then(
                (m) => m.StorefrontFacilitiesComponent,
              ),
          },
          {
            path: 'about',
            canActivate: [hotelStorefrontGuard],
            loadComponent: () =>
              import('./pages/storefront/storefront-about.component').then((m) => m.StorefrontAboutComponent),
          },
          {
            path: 'restaurant',
            canActivate: [hotelStorefrontGuard],
            loadComponent: () =>
              import('./pages/storefront/storefront-restaurant.component').then(
                (m) => m.StorefrontRestaurantComponent,
              ),
          },
          {
            path: 'event-halls',
            canActivate: [hotelStorefrontGuard],
            loadComponent: () =>
              import('./pages/storefront/storefront-event-halls.component').then(
                (m) => m.StorefrontEventHallsComponent,
              ),
          },
          {
            path: 'event-halls/:hallId',
            canActivate: [hotelStorefrontGuard],
            loadComponent: () =>
              import('./pages/storefront/storefront-event-hall-detail.component').then(
                (m) => m.StorefrontEventHallDetailComponent,
              ),
          },
          {
            path: 'listings',
            canActivate: [shortletStorefrontGuard],
            loadComponent: () =>
              import('./pages/shortlet/shortlet-listings.component').then((m) => m.ShortletListingsComponent),
          },
          {
            path: 'listings/:listingId',
            canActivate: [shortletStorefrontGuard],
            loadComponent: () =>
              import('./pages/shortlet/shortlet-listing-detail.component').then(
                (m) => m.ShortletListingDetailComponent,
              ),
          },
          {
            path: 'amenities',
            canActivate: [shortletStorefrontGuard],
            loadComponent: () =>
              import('./pages/shortlet/shortlet-amenities.component').then((m) => m.ShortletAmenitiesComponent),
          },
          {
            path: 'host',
            canActivate: [shortletStorefrontGuard],
            loadComponent: () =>
              import('./pages/shortlet/shortlet-host.component').then((m) => m.ShortletHostComponent),
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
