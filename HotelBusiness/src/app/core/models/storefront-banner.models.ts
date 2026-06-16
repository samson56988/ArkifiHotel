export interface StorefrontBannerImageDto {
  id: string;
  url: string;
  originalFileName: string | null;
  sortOrder: number;
  locationId: string | null;
  locationName: string | null;
}

export const MAX_BANNER_IMAGES = 3;
