// Tasarım sistemi: renk, spacing, tipografi, radius ve shadow tokenları.
// UI dili: friendly playful (Duolingo/Headspace benzeri — yumuşak, sıcak, olgun).
// Light theme için tasarlandı; dark theme ileride eklenecek.

export const Colors = {
  // Brand — sakin, güven veren teal + sıcak vurgu
  primary: '#2A9D8F',
  primaryDark: '#1F7268',
  secondary: '#F4A261',

  // Backgrounds
  background: '#FAFAF7',
  surface: '#FFFFFF',

  // Text
  text: '#1F2933',
  textSecondary: '#52606D',
  textMuted: '#9AA5B1',

  // States
  success: '#4CAF50',
  warning: '#F2C94C',
  error: '#E76F51',

  // Borders
  border: '#E4E7EB',
  divider: '#F0F2F5',

  // Soft variants — chip, badge, banner ve açık arka planlar için
  primarySoft: '#E8F5F3', // teal'in açık tonu
  secondarySoft: '#FDF0E5', // coral'ın açık tonu
  successSoft: '#E6F4EA',
  warningSoft: '#FEF5E7',
  errorSoft: '#FDECE8',

  // Overlay / scrim
  overlay: 'rgba(31, 41, 51, 0.5)',
} as const;

export const Spacing = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  xxl: 48,
} as const;

export const FontSize = {
  xs: 12,
  sm: 14,
  base: 16,
  lg: 18,
  xl: 22, // önceden 24'tü; ölçek yeniden dengelendi
  xxl: 28, // YENİ
  hero: 36, // YENİ — büyük başlıklar için
  display: 48,
} as const;

export const FontWeight = {
  regular: '400',
  medium: '500',
  semibold: '600',
  bold: '700',
} as const;

export const Radius = {
  sm: 4,
  md: 8,
  lg: 16,
  xl: 24, // YENİ — kart, modal, illustration için
  full: 9999,
} as const;

// Yükseklik hissi için shadow ölçeği. iOS shadow* + Android elevation birlikte.
export const Shadow = {
  sm: {
    shadowColor: '#000',
    shadowOpacity: 0.05,
    shadowRadius: 4,
    shadowOffset: { width: 0, height: 2 },
    elevation: 2,
  },
  md: {
    shadowColor: '#000',
    shadowOpacity: 0.08,
    shadowRadius: 8,
    shadowOffset: { width: 0, height: 4 },
    elevation: 4,
  },
  lg: {
    shadowColor: '#000',
    shadowOpacity: 0.12,
    shadowRadius: 16,
    shadowOffset: { width: 0, height: 8 },
    elevation: 8,
  },
} as const;
