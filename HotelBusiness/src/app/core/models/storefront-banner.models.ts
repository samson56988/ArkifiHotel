export interface StorefrontBannerImageDto {
  id: string;
  url: string;
  originalFileName: string | null;
  sortOrder: number;
}

export const MAX_BANNER_IMAGES = 3;
