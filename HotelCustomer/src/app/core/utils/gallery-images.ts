export interface GalleryImageSource {
  primaryImageUrl: string | null;
  imageUrls?: string[];
}

export function galleryImages(item: GalleryImageSource): string[] {
  if (item.imageUrls?.length) {
    return item.imageUrls;
  }
  if (item.primaryImageUrl) {
    return [item.primaryImageUrl];
  }
  return [];
}

export function collectGalleryImages(items: GalleryImageSource[]): string[] {
  const seen = new Set<string>();
  const urls: string[] = [];
  for (const item of items) {
    for (const url of galleryImages(item)) {
      if (!seen.has(url)) {
        seen.add(url);
        urls.push(url);
      }
    }
  }
  return urls;
}
