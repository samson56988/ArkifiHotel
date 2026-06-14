export interface CountryDialCode {
  iso2: string;
  name: string;
  dialCode: string;
}

/** Common dial codes for reception bookings (Nigeria default). */
export const COUNTRY_DIAL_CODES: CountryDialCode[] = [
  { iso2: 'NG', name: 'Nigeria', dialCode: '+234' },
  { iso2: 'GH', name: 'Ghana', dialCode: '+233' },
  { iso2: 'KE', name: 'Kenya', dialCode: '+254' },
  { iso2: 'ZA', name: 'South Africa', dialCode: '+27' },
  { iso2: 'GB', name: 'United Kingdom', dialCode: '+44' },
  { iso2: 'US', name: 'United States', dialCode: '+1' },
  { iso2: 'CA', name: 'Canada', dialCode: '+1' },
  { iso2: 'AE', name: 'United Arab Emirates', dialCode: '+971' },
  { iso2: 'IN', name: 'India', dialCode: '+91' },
  { iso2: 'FR', name: 'France', dialCode: '+33' },
  { iso2: 'DE', name: 'Germany', dialCode: '+49' },
  { iso2: 'NL', name: 'Netherlands', dialCode: '+31' },
  { iso2: 'BE', name: 'Belgium', dialCode: '+32' },
  { iso2: 'ES', name: 'Spain', dialCode: '+34' },
  { iso2: 'IT', name: 'Italy', dialCode: '+39' },
  { iso2: 'PT', name: 'Portugal', dialCode: '+351' },
  { iso2: 'IE', name: 'Ireland', dialCode: '+353' },
  { iso2: 'CH', name: 'Switzerland', dialCode: '+41' },
  { iso2: 'SE', name: 'Sweden', dialCode: '+46' },
  { iso2: 'NO', name: 'Norway', dialCode: '+47' },
  { iso2: 'DK', name: 'Denmark', dialCode: '+45' },
  { iso2: 'CN', name: 'China', dialCode: '+86' },
  { iso2: 'JP', name: 'Japan', dialCode: '+81' },
  { iso2: 'AU', name: 'Australia', dialCode: '+61' },
  { iso2: 'NZ', name: 'New Zealand', dialCode: '+64' },
  { iso2: 'BR', name: 'Brazil', dialCode: '+55' },
  { iso2: 'MX', name: 'Mexico', dialCode: '+52' },
  { iso2: 'SA', name: 'Saudi Arabia', dialCode: '+966' },
  { iso2: 'QA', name: 'Qatar', dialCode: '+974' },
  { iso2: 'EG', name: 'Egypt', dialCode: '+20' },
  { iso2: 'RW', name: 'Rwanda', dialCode: '+250' },
  { iso2: 'TZ', name: 'Tanzania', dialCode: '+255' },
  { iso2: 'UG', name: 'Uganda', dialCode: '+256' },
  { iso2: 'CM', name: 'Cameroon', dialCode: '+237' },
  { iso2: 'CI', name: "Côte d'Ivoire", dialCode: '+225' },
  { iso2: 'SN', name: 'Senegal', dialCode: '+221' },
];

export const DEFAULT_DIAL_CODE = '+234';

export function dialCodeLabel(entry: CountryDialCode): string {
  return `${entry.dialCode} ${entry.name}`;
}
