import type {
  ColorPresetId,
  GlobalFontId,
  SectionFontRole,
  StorefrontColorPalette,
} from '../models/storefront-theme.models';

export interface FontPreset {
  id: GlobalFontId;
  label: string;
  heading: string;
  body: string;
  accent: string;
}

export const GLOBAL_FONT_PRESETS: FontPreset[] = [
  {
    id: 'modern-sans',
    label: 'Modern Sans',
    heading: '"DM Sans", system-ui, sans-serif',
    body: '"Inter", system-ui, sans-serif',
    accent: '"DM Sans", system-ui, sans-serif',
  },
  {
    id: 'classic-serif',
    label: 'Classic Serif',
    heading: '"Playfair Display", Georgia, serif',
    body: '"Source Serif 4", Georgia, serif',
    accent: '"Playfair Display", Georgia, serif',
  },
  {
    id: 'elegant-display',
    label: 'Elegant Display',
    heading: '"Cormorant Garamond", serif',
    body: '"Lato", sans-serif',
    accent: '"Cormorant Garamond", serif',
  },
  {
    id: 'luxury-contrast',
    label: 'Luxury Contrast',
    heading: '"Cinzel", serif',
    body: '"Montserrat", sans-serif',
    accent: '"Cinzel", serif',
  },
  {
    id: 'warm-hospitality',
    label: 'Warm Hospitality',
    heading: '"Fraunces", serif',
    body: '"Nunito Sans", sans-serif',
    accent: '"Fraunces", serif',
  },
];

export const COLOR_PRESETS: Record<ColorPresetId, StorefrontColorPalette> = {
  'sage-luxe': {
    preset: 'sage-luxe',
    primary: '#5c7a5c',
    accent: '#c8dcc8',
    background: '#faf9f6',
    text: '#1f2a1f',
  },
  'midnight-gold': {
    preset: 'midnight-gold',
    primary: '#1a1f2e',
    accent: '#c9a962',
    background: '#0f1117',
    text: '#f5f3ef',
  },
  'ocean-calm': {
    preset: 'ocean-calm',
    primary: '#2f6b7a',
    accent: '#b8dde8',
    background: '#f4fafb',
    text: '#163038',
  },
  'terracotta-warm': {
    preset: 'terracotta-warm',
    primary: '#9c5b43',
    accent: '#f0d5c8',
    background: '#fdf8f5',
    text: '#3d2318',
  },
  'slate-minimal': {
    preset: 'slate-minimal',
    primary: '#3d4a57',
    accent: '#d6dee8',
    background: '#ffffff',
    text: '#1e2630',
  },
};

export const BANNER_STYLE_OPTIONS = [
  { id: 'grand-hero', label: 'Grand hero', hint: 'Full-width banner with rich overlay.' },
  { id: 'split-showcase', label: 'Split showcase', hint: 'Bold headline beside a visual panel.' },
  { id: 'minimal-serif', label: 'Minimal serif', hint: 'Elegant whitespace and centered type.' },
  { id: 'glass-panel', label: 'Glass panel', hint: 'Frosted card floating on gradient.' },
  { id: 'sunset-gradient', label: 'Sunset gradient', hint: 'Warm gradient backdrop.' },
] as const;

export const SECTION_FONT_OPTIONS: { id: SectionFontRole; label: string }[] = [
  { id: 'display', label: 'Display heading' },
  { id: 'body', label: 'Body text' },
  { id: 'accent', label: 'Accent style' },
];

export function resolveSectionFont(globalFont: GlobalFontId, role: SectionFontRole): string {
  const preset = GLOBAL_FONT_PRESETS.find((p) => p.id === globalFont) ?? GLOBAL_FONT_PRESETS[0];
  switch (role) {
    case 'display':
      return preset.heading;
    case 'accent':
      return preset.accent;
    default:
      return preset.body;
  }
}

export function themeCssVariables(theme: { globalFont: GlobalFontId; colors: StorefrontColorPalette }): Record<string, string> {
  const font = GLOBAL_FONT_PRESETS.find((p) => p.id === theme.globalFont) ?? GLOBAL_FONT_PRESETS[0];
  return {
    '--sf-primary': theme.colors.primary,
    '--sf-accent': theme.colors.accent,
    '--sf-bg': theme.colors.background,
    '--sf-text': theme.colors.text,
    '--sf-font-heading': font.heading,
    '--sf-font-body': font.body,
    '--sf-font-accent': font.accent,
  };
}
