import { Routes } from '@angular/router';
import { guestOnlyGuard, platformAuthGuard } from './core/guards/platform-auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'login',
    canActivate: [guestOnlyGuard],
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'dashboard',
    canActivate: [platformAuthGuard],
    loadComponent: () => import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'businesses',
    canActivate: [platformAuthGuard],
    loadComponent: () =>
      import('./pages/businesses/businesses-list.component').then((m) => m.BusinessesListComponent),
  },
  {
    path: 'businesses/:businessId',
    canActivate: [platformAuthGuard],
    loadComponent: () =>
      import('./pages/businesses/business-detail.component').then((m) => m.BusinessDetailComponent),
  },
  {
    path: 'subscriptions',
    canActivate: [platformAuthGuard],
    loadComponent: () =>
      import('./pages/subscriptions/subscriptions.component').then((m) => m.SubscriptionsComponent),
  },
  {
    path: 'activity',
    canActivate: [platformAuthGuard],
    loadComponent: () => import('./pages/activity/activity.component').then((m) => m.ActivityComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
