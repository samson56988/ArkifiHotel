import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'verify-email',
    loadComponent: () =>
      import('./pages/verify-email/verify-email.component').then((m) => m.VerifyEmailComponent),
  },
  {
    path: 'verify-login-otp',
    loadComponent: () =>
      import('./pages/verify-login-otp/verify-login-otp.component').then(
        (m) => m.VerifyLoginOtpComponent,
      ),
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./pages/forgot-password/forgot-password.component').then((m) => m.ForgotPasswordComponent),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./pages/reset-password/reset-password.component').then((m) => m.ResetPasswordComponent),
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'rooms',
    loadComponent: () =>
      import('./pages/rooms/rooms-list.component').then((m) => m.RoomsListComponent),
  },
  {
    path: 'amenities',
    loadComponent: () =>
      import('./pages/amenities/amenities-list.component').then((m) => m.AmenitiesListComponent),
  },
  {
    path: 'locations',
    loadComponent: () =>
      import('./pages/locations/locations-list.component').then((m) => m.LocationsListComponent),
  },
  {
    path: 'rooms/new',
    loadComponent: () =>
      import('./pages/rooms/room-form.component').then((m) => m.RoomFormComponent),
  },
  {
    path: 'rooms/:roomId',
    loadComponent: () =>
      import('./pages/rooms/room-form.component').then((m) => m.RoomFormComponent),
  },
  {
    path: 'facilities',
    loadComponent: () =>
      import('./pages/facilities/facilities-list.component').then((m) => m.FacilitiesListComponent),
  },
  {
    path: 'facilities/new',
    loadComponent: () =>
      import('./pages/facilities/facility-form.component').then((m) => m.FacilityFormComponent),
  },
  {
    path: 'facilities/:facilityId',
    loadComponent: () =>
      import('./pages/facilities/facility-form.component').then((m) => m.FacilityFormComponent),
  },
  {
    path: 'bookings/new',
    loadComponent: () =>
      import('./pages/bookings/booking-form.component').then((m) => m.BookingFormComponent),
  },
  {
    path: 'bookings',
    loadComponent: () =>
      import('./pages/bookings/bookings-list.component').then((m) => m.BookingsListComponent),
  },
  {
    path: 'payment-configuration',
    loadComponent: () =>
      import('./pages/payment-configuration/payment-configuration.component').then(
        (m) => m.PaymentConfigurationComponent,
      ),
  },
  {
    path: 'customers/new',
    loadComponent: () =>
      import('./pages/customers/customer-form.component').then((m) => m.CustomerFormComponent),
  },
  {
    path: 'customers/:customerId',
    loadComponent: () =>
      import('./pages/customers/customer-form.component').then((m) => m.CustomerFormComponent),
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./pages/customers/customers-list.component').then((m) => m.CustomersListComponent),
  },
  {
    path: 'booking-payments/new',
    loadComponent: () =>
      import('./pages/booking-payments/booking-payment-form.component').then(
        (m) => m.BookingPaymentFormComponent,
      ),
  },
  {
    path: 'booking-payments',
    loadComponent: () =>
      import('./pages/booking-payments/booking-payments-list.component').then(
        (m) => m.BookingPaymentsListComponent,
      ),
  },
  {
    path: 'profile',
    loadComponent: () =>
      import('./pages/business-profile/business-profile.component').then((m) => m.BusinessProfileComponent),
  },
  {
    path: 'storefront-designer',
    loadComponent: () =>
      import('./pages/storefront-designer/storefront-designer.component').then((m) => m.StorefrontDesignerComponent),
  },
  { path: '**', redirectTo: '' },
];
