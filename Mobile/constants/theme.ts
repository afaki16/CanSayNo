// Tasarım sistemi: renk, spacing, tipografi, radius ve shadow tokenları.
// Kaynak: Claude Design "CanSayNo Ekranlar" — logodaki yukarı-ok mavisi ana renk,
// sıcak mercan vurgu enerji ve seriyi (streak) taşır. Tipografi Satoshi.
// Light theme için tasarlandı; dark theme ileride eklenecek.

export const Colors = {
  // Brand — yukarı-ok mavisi + sıcak mercan vurgu
  primary: '#2D7FF9',
  primaryDark: '#1B63D6',
  primaryLight: '#5C9DFF',
  // Marka geçişinin başı (mor). Geçiş: Gradients.brand. Logo: components/ui/BrandMark.tsx
  indigo: '#5B4BE0',
  indigoDark: '#4636C4',
  secondary: '#FF7043', // mercan — seri, ipucu, enerji, logodaki "dur" eli
  amber: '#F7B500', // XP, yıldız, premium

  // Backgrounds
  background: '#F8F7F4', // sıcak kırık beyaz zemin
  surface: '#FFFFFF',
  chip: '#F4F2EE', // nötr chip / pill zemini
  ink: '#16181D', // koyu yüzey (koç notu, premium banner, paywall)

  // Text
  text: '#16181D',
  textSecondary: '#5A6472',
  textMuted: '#98A1AE',
  textDisabled: '#C3CAD4',

  // States
  success: '#1FA971',
  warning: '#F7A93B',
  error: '#F04438',

  // Borders
  border: '#ECEDF0', // input, buton kenarı
  divider: '#F0EFEA', // kart kenarı

  // Soft variants — chip, badge, banner ve açık arka planlar için
  primarySoft: '#E7F0FE',
  indigoSoft: '#ECEAFC',
  secondarySoft: '#FFEAE2',
  amberSoft: '#FEF3D6',
  successSoft: '#E4F5EE',
  warningSoft: '#FEF1DF',
  errorSoft: '#FDE8E6',
  tipSoft: '#FFF7ED', // ipucu / "bir dahakine dene" kartı

  // Track — progress bar arka planı
  track: '#EEF0F3',

  // Overlay / scrim
  overlay: 'rgba(22, 24, 29, 0.5)',
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
  xl: 22,
  xxl: 28,
  hero: 33,
  display: 46,
} as const;

// Ağırlıklar Satoshi dosyalarına <Text> içinde eşlenir (bkz. components/ui/Text.tsx):
// 400 Regular · 500 Medium · 600/700 Bold · 800/900 Black.
export const FontWeight = {
  regular: '400',
  medium: '500',
  semibold: '600',
  bold: '700',
  extrabold: '800',
  black: '900',
} as const;

// Yüklenen font aileleri — app/_layout.tsx'te useFonts ile kaydedilir.
export const Fonts = {
  regular: 'Satoshi-Regular',
  medium: 'Satoshi-Medium',
  bold: 'Satoshi-Bold',
  black: 'Satoshi-Black',
} as const;

export const Radius = {
  sm: 8,
  md: 14, // input, ikon kutusu
  lg: 16, // buton
  xl: 20, // kart
  full: 9999,
} as const;

// Yükseklik hissi için shadow ölçeği. iOS shadow* + Android elevation birlikte.
export const Shadow = {
  sm: {
    shadowColor: '#141E32',
    shadowOpacity: 0.05,
    shadowRadius: 2,
    shadowOffset: { width: 0, height: 1 },
    elevation: 1,
  },
  md: {
    shadowColor: '#141E32',
    shadowOpacity: 0.06,
    shadowRadius: 12,
    shadowOffset: { width: 0, height: 6 },
    elevation: 3,
  },
  lg: {
    shadowColor: '#141E32',
    shadowOpacity: 0.12,
    shadowRadius: 20,
    shadowOffset: { width: 0, height: 10 },
    elevation: 8,
  },
  // Renkli gölge — primary buton ve marka geçişli kartlar (geçişin ortası)
  primary: {
    shadowColor: '#4463EE',
    shadowOpacity: 0.32,
    shadowRadius: 16,
    shadowOffset: { width: 0, height: 10 },
    elevation: 6,
  },
} as const;

// Marka geçişi: mor (sol üst) → mavi (sağ alt). Kaynak design/brand/brand.mjs (ikon, site, panel
// aynı geçişi kullanır). Büyük marka yüzeyleri (birincil buton, başlık kartları, karşılama
// görseli, logo) bununla boyanır; küçük etkileşim öğeleri (link, ikon, seçim) Colors.primary kalır.
// `as const` tuple: expo-linear-gradient `colors` en az iki eleman ister.
export const Gradients = {
  brand: {
    colors: ['#5B4BE0', '#2D7FF9'] as const,
    start: { x: 0, y: 0 },
    end: { x: 1, y: 1 },
  },
  // Koyu marka yüzeyi (Sonuç ekranı başlığı gibi): aynı yönde bir ton koyu
  brandDeep: {
    colors: ['#4636C4', '#1B63D6'] as const,
    start: { x: 0, y: 0 },
    end: { x: 1, y: 1 },
  },
} as const;
