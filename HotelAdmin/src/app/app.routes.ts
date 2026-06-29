import { Routes } from '@angular/router';
import { guestOnlyGuard, platformAuthGuard } from './core/guards/platform-auth.guard';

const workspaceRoutes: Routes = [
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'businesses',
    loadComponent: () =>
      import('./pages/businesses/businesses-list.component').then((m) => m.BusinessesListComponent),
  },
  {
    path: 'businesses/:businessId',
    loadComponent: () =>
      import('./pages/businesses/business-detail.component').then((m) => m.BusinessDetailComponent),
  },
  {
    path: 'subscriptions',
    loadComponent: () =>
      import('./pages/subscriptions/subscriptions.component').then((m) => m.SubscriptionsComponent),
  },
  {
    path: 'activity',
    loadComponent: () => import('./pages/activity/activity.component').then((m) => m.ActivityComponent),
  },
];

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'login',
    canActivate: [guestOnlyGuard],
    loadComponent: () => import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [platformAuthGuard],
    loadComponent: () =>
      import('./layouts/platform-workspace/platform-workspace.component').then(
        (m) => m.PlatformWorkspaceComponent,
      ),
    children: workspaceRoutes,
  },
  { path: '**', redirectTo: 'dashboard' },
];
