/** Build customer storefront routes with optional branch segment. */
export function storefrontPath(
  slug: string,
  locationRouteId: string | null | undefined,
  ...segments: string[]
): string[] {
  const tail = segments.filter(Boolean);
  if (locationRouteId) {
    return ['/', slug, 'l', locationRouteId, ...tail];
  }
  return ['/', slug, ...tail];
}

export function apiLocationId(locationRouteId: string | null | undefined): string | undefined {
  if (!locationRouteId || locationRouteId === 'default') {
    return undefined;
  }
  return locationRouteId;
}

export function routeLocationId(locationRouteId: string | null | undefined): string | null {
  if (!locationRouteId || locationRouteId === 'default') {
    return null;
  }
  return locationRouteId;
}
