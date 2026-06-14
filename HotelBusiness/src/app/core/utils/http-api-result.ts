import { HttpErrorResponse } from '@angular/common/http';
import type { ApiResult } from '../models/api-result.model';

/** True for a parsed standard API envelope (2xx body or normalized HTTP error). */
export function isApiResultBody(value: unknown): value is ApiResult<unknown> {
  if (!value || typeof value !== 'object') {
    return false;
  }

  const record = value as Record<string, unknown>;
  return typeof readBool(record, 'success', 'Success') === 'boolean';
}

/** Prefer server `message`, then validation errors, then fallback. */
export function getApiResultMessage(
  result: Pick<ApiResult<unknown>, 'message' | 'validationErrors'>,
  fallback = 'Something went wrong.',
): string {
  const parts: string[] = [];
  if (result.message?.trim()) {
    parts.push(result.message.trim());
  }

  const ve = result.validationErrors?.filter((s) => s?.trim()) ?? [];
  if (ve.length) {
    parts.push(ve.join(' '));
  }

  return parts.length ? parts.join(' ') : fallback;
}

/**
 * Normalizes any API JSON body into camelCase `ApiResult<T>` (envelope + nested `data`).
 * Use on successful HTTP responses so edit/list views always read `success` and `data.*`.
 */
export function normalizeApiResult<T>(raw: unknown): ApiResult<T> {
  const parsed = tryParseApiResultBody<T>(raw);
  if (parsed) {
    return {
      ...parsed,
      data: parsed.data != null ? (deepCamelCase(parsed.data) as T) : null,
    };
  }

  return {
    success: false,
    data: null,
    message: 'Unexpected API response shape.',
    code: 'InvalidResponse',
    validationErrors: null,
  };
}

/**
 * Reads `ApiResult` from a failed HTTP response (401, 400, 404, etc.).
 * Supports camelCase JSON from ASP.NET Core and PascalCase fallbacks.
 */
export function parseHttpApiResult<T>(err: HttpErrorResponse): ApiResult<T> {
  const fromBody = normalizeApiResult<T>(err.error);
  if (fromBody.success || fromBody.code !== 'InvalidResponse') {
    return fromBody;
  }

  return {
    success: false,
    data: null,
    message: err.status === 0 ? 'Network error. Is the API running?' : err.statusText || err.message || 'Request failed.',
    code: err.status === 0 ? 'NetworkError' : 'HttpError',
    validationErrors: null,
  };
}

function tryParseApiResultBody<T>(raw: unknown): ApiResult<T> | null {
  let value: unknown = raw;

  if (typeof raw === 'string' && raw.trim().startsWith('{')) {
    try {
      value = JSON.parse(raw) as unknown;
    } catch {
      return null;
    }
  }

  if (!value || typeof value !== 'object') {
    return null;
  }

  const record = value as Record<string, unknown>;
  const success = readBool(record, 'success', 'Success');
  if (success === undefined) {
    return null;
  }

  return {
    success,
    data: (readValue(record, 'data', 'Data') ?? null) as T | null,
    message: readString(record, 'message', 'Message'),
    code: readString(record, 'code', 'Code'),
    validationErrors: readStringArray(record, 'validationErrors', 'ValidationErrors'),
  };
}

function readValue(record: Record<string, unknown>, ...keys: string[]): unknown {
  for (const key of keys) {
    if (key in record) {
      return record[key];
    }
  }

  return undefined;
}

function readBool(record: Record<string, unknown>, ...keys: string[]): boolean | undefined {
  const v = readValue(record, ...keys);
  return typeof v === 'boolean' ? v : undefined;
}

function readString(record: Record<string, unknown>, ...keys: string[]): string | null {
  const v = readValue(record, ...keys);
  return typeof v === 'string' ? v : null;
}

function readStringArray(record: Record<string, unknown>, ...keys: string[]): string[] | null {
  const v = readValue(record, ...keys);
  if (!Array.isArray(v)) {
    return null;
  }

  return v.filter((item): item is string => typeof item === 'string');
}

function deepCamelCase(value: unknown): unknown {
  if (Array.isArray(value)) {
    return value.map((item) => deepCamelCase(item));
  }

  if (!value || typeof value !== 'object') {
    return value;
  }

  const out: Record<string, unknown> = {};
  for (const [key, nested] of Object.entries(value as Record<string, unknown>)) {
    const camelKey = key.length ? key.charAt(0).toLowerCase() + key.slice(1) : key;
    out[camelKey] = deepCamelCase(nested);
  }

  return out;
}
