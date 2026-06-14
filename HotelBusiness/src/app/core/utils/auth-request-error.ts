import type { ApiResult } from '../models/api-result.model';
import type { ToastService } from '../services/toast.service';
import { getApiResultMessage, isApiResultBody } from './http-api-result';

export interface AuthErrorContext {
  /** Current form email — used for EmailNotVerified redirect. */
  email?: string;
}

export interface AuthErrorHandlers {
  EmailNotVerified?: (result: ApiResult<unknown>, email: string) => void;
}

/** Shows API `message` / `validationErrors` from a failed auth request (HTTP 4xx with ApiResult body). */
export function showAuthRequestError(
  toast: ToastService,
  err: unknown,
  title: string,
  handlers?: AuthErrorHandlers,
  context?: AuthErrorContext,
): void {
  if (!isApiResultBody(err)) {
    toast.error('We could not reach the server. Check your connection and try again.', 'Network error');
    return;
  }

  if (err.code === 'EmailNotVerified' && handlers?.EmailNotVerified && context?.email) {
    handlers.EmailNotVerified(err, context.email);
    return;
  }

  toast.showFailedApi(err, title);
}

/** Success path: use server message when present. */
export function showAuthSuccessMessage(
  toast: ToastService,
  result: ApiResult<unknown>,
  fallback: string,
  toastTitle?: string,
): void {
  toast.success(getApiResultMessage(result, fallback), toastTitle);
}
