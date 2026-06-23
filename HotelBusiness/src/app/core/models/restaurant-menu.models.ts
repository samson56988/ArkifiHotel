import type { ApiResult } from './api-result.model';

export interface RestaurantMenuSettingsDto {
  enabled: boolean;
  navLabel: string;
  heroEyebrow: string;
  heroTitle: string;
  heroSubtitle: string;
  mealsSectionTitle: string;
  drinksSectionTitle: string;
  heroImageUrl: string | null;
  locationId?: string;
}

export interface UpdateRestaurantMenuSettingsRequest {
  enabled: boolean;
  navLabel: string;
  heroEyebrow: string;
  heroTitle: string;
  heroSubtitle: string;
  mealsSectionTitle: string;
  drinksSectionTitle: string;
}

export interface RestaurantMenuCategoryDto {
  id: string;
  name: string;
  section: 'food' | 'drink';
  sortOrder: number;
  isArchived: boolean;
  itemCount: number;
}

export interface CreateRestaurantMenuCategoryRequest {
  name: string;
  section: 'food' | 'drink';
  sortOrder: number;
}

export interface UpdateRestaurantMenuCategoryRequest {
  name: string;
  sortOrder: number;
}

export interface RestaurantMenuItemDto {
  id: string;
  categoryId: string;
  name: string;
  description: string | null;
  price: number;
  tags: string[];
  imageUrl: string | null;
  sortOrder: number;
  isArchived: boolean;
  isAvailable: boolean;
}

export interface CreateRestaurantMenuItemRequest {
  name: string;
  description?: string | null;
  price: number;
  tags?: string[];
  sortOrder: number;
}

export interface UpdateRestaurantMenuItemRequest {
  name: string;
  description?: string | null;
  price: number;
  tags?: string[];
  sortOrder: number;
}

export type RestaurantMenuSettingsApiResponse = ApiResult<RestaurantMenuSettingsDto>;
export type RestaurantMenuCategoriesApiResponse = ApiResult<RestaurantMenuCategoryDto[]>;
export type RestaurantMenuCategoryApiResponse = ApiResult<RestaurantMenuCategoryDto>;
export type RestaurantMenuItemsApiResponse = ApiResult<RestaurantMenuItemDto[]>;
export type RestaurantMenuItemApiResponse = ApiResult<RestaurantMenuItemDto>;
