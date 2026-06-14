import { COUNTRY_DIAL_CODES, DEFAULT_DIAL_CODE } from '../data/country-dial-codes';
import type { AbstractControl, ValidationErrors } from '@angular/forms';

const DIAL_CODES_LONGEST_FIRST = [...COUNTRY_DIAL_CODES]
  .map((c) => c.dialCode)
  .sort((a, b) => b.length - a.length);

/** Digits only from local part (strips spaces, dashes, leading zero). */
export function normalizeLocalPhoneDigits(local: string): string {
  let digits = local.replace(/\D/g, '');
  if (digits.startsWith('0')) {
    digits = digits.replace(/^0+/, '');
  }

  return digits;
}

/** Builds stored guest phone e.g. +2348012345678, or null when empty. */
export function buildInternationalPhone(countryCode: string, local: string): string | null {
  const digits = normalizeLocalPhoneDigits(local);
  if (!digits) {
    return null;
  }

  const code = countryCode.trim();
  if (!code.startsWith('+')) {
    return null;
  }

  return `${code}${digits}`;
}

export function isValidLocalPhoneDigits(digits: string): boolean {
  return digits.length >= 7 && digits.length <= 15;
}

/** Required local phone digits (7–15) for reactive forms. */
export function guestPhoneLocalValidator(control: AbstractControl): ValidationErrors | null {
  const digits = normalizeLocalPhoneDigits(String(control.value ?? ''));
  if (!digits) {
    return { required: true };
  }

  if (!isValidLocalPhoneDigits(digits)) {
    return { phoneInvalid: true };
  }

  return null;
}

/** Splits a stored +234… value back into code + local (for edit forms). */
export function parseInternationalPhone(stored: string | null | undefined): {
  countryCode: string;
  local: string;
} {
  const value = stored?.trim() ?? '';
  if (!value.startsWith('+')) {
    return { countryCode: DEFAULT_DIAL_CODE, local: normalizeLocalPhoneDigits(value) };
  }

  const matchedCode =
    DIAL_CODES_LONGEST_FIRST.find((code) => value.startsWith(code)) ?? DEFAULT_DIAL_CODE;
  const local = normalizeLocalPhoneDigits(value.slice(matchedCode.length));

  return { countryCode: matchedCode, local };
}
