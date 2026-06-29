import { Routes } from '@angular/router';

const workspaceRoutes: Routes = [
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
    path: 'event-halls',
    loadComponent: () =>
      import('./pages/event-halls/event-halls-list.component').then((m) => m.EventHallsListComponent),
  },
  {
    path: 'event-halls/new',
    loadComponent: () =>
      import('./pages/event-halls/event-hall-form.component').then((m) => m.EventHallFormComponent),
  },
  {
    path: 'event-halls/:eventHallId',
    loadComponent: () =>
      import('./pages/event-halls/event-hall-form.component').then((m) => m.EventHallFormComponent),
  },
  {
    path: 'event-hall-requests',
    loadComponent: () =>
      import('./pages/event-halls/event-hall-requests-list.component').then(
        (m) => m.EventHallRequestsListComponent,
      ),
  },
  {
    path: 'restaurant-menu',
    loadComponent: () =>
      import('./pages/restaurant-menu/restaurant-menu.component').then((m) => m.RestaurantMenuComponent),
  },
  {
    path: 'restaurant-orders',
    loadComponent: () =>
      import('./pages/restaurant-orders/restaurant-orders-list.component').then(
        (m) => m.RestaurantOrdersListComponent,
      ),
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
    path: 'subscription',
    loadComponent: () =>
      import('./pages/subscription/subscription.component').then((m) => m.SubscriptionComponent),
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
  {
    path: 'social-profile',
    loadComponent: () =>
      import('./pages/social-profile/social-profile.component').then((m) => m.SocialProfileComponent),
  },
  {
    path: 'team',
    loadComponent: () =>
      import('./pages/team/team-list.component').then((m) => m.TeamListComponent),
  },
  {
    path: 'team/invites',
    loadComponent: () =>
      import('./pages/team/team-invites-list.component').then((m) => m.TeamInvitesListComponent),
  },
  {
    path: 'audit',
    loadComponent: () =>
      import('./pages/audit/audit-list.component').then((m) => m.AuditListComponent),
  },
];

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
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
    path: 'change-default-password',
    loadComponent: () =>
      import('./pages/change-default-password/change-default-password.component').then(
        (m) => m.ChangeDefaultPasswordComponent,
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
    path: '',
    loadComponent: () =>
      import('./layouts/business-workspace/business-workspace.component').then(
        (m) => m.BusinessWorkspaceComponent,
      ),
    children: workspaceRoutes,
  },
  { path: '**', redirectTo: '' },
];
