import { InjectionToken } from '@angular/core';

/** Base URL of the ArkifiHotel API (no trailing slash). Must match CORS + launchSettings. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => 'https://localhost:7058',
});
