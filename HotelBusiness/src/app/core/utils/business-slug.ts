export function normalizeBusinessSlug(raw: string): string {
  return raw
    .trim()
    .toLowerCase()
    .replace(/\s+/g, '-')
    .replace(/[^a-z0-9-]/g, '')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '');
}

export function suggestSlugFromBusinessName(name: string): string {
  return normalizeBusinessSlug(name);
}

export function isValidBusinessSlug(slug: string): boolean {
  return slug.length >= 3 && slug.length <= 128 && /^[a-z0-9]+(?:-[a-z0-9]+)*$/.test(slug);
}
