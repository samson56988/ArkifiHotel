export const ALLOWED_IMAGE_ACCEPT = 'image/jpeg,image/png,.jpg,.jpeg,.png';

export const ALLOWED_IMAGE_MAX_BYTES = 8 * 1024 * 1024;

const allowedMime = /^image\/(jpeg|png)$/i;
const allowedExt = /\.(jpe?g|png)$/i;

export function isAllowedImageFile(file: File): boolean {
  if (allowedMime.test(file.type)) {
    return true;
  }

  return allowedExt.test(file.name);
}

/** Client-side filter before upload (JPEG / PNG / JPG only, max 8MB each). */
export function filterAllowedImageFiles(files: File[]): { accepted: File[]; skipped: string[] } {
  const accepted: File[] = [];
  const skipped: string[] = [];

  for (const f of files) {
    if (f.size > ALLOWED_IMAGE_MAX_BYTES) {
      skipped.push(`${f.name} (over 8MB)`);
      continue;
    }

    if (!isAllowedImageFile(f)) {
      skipped.push(`${f.name} (not JPEG or PNG)`);
      continue;
    }

    accepted.push(f);
  }

  return { accepted, skipped };
}
