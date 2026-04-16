import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/booking-lookup/booking-lookup.component').then((m) => m.BookingLookupComponent),
  },
];
