/** ID for toasts etc. Works on HTTP (no secure context) where crypto.randomUUID may be missing. */
export function createClientId(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 11)}`;
}
