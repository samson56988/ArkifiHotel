export type ShortletPreviewPage = 'home' | 'listings' | 'amenities' | 'host';

export type { ShortletShowcase as ShortletPreviewData } from '../models/shortlet-showcase.models';

export { isShortletBusinessType, mapPublicToShortletShowcase as mapPublicToShortletPreview } from './shortlet-mapper';
export { formatNaira, shortletThemeStyle as shortletPreviewThemeStyle } from './shortlet-theme';
