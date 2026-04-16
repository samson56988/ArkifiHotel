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
  { path: '**', redirectTo: '' },
];
