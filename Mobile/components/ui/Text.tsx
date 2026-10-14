// Uygulama genelinde kullanılan Text. RN Text'in yerine geçer.
// Satoshi ağırlık başına ayrı dosya olarak yüklendiği için (Expo Go'da tek
// family + fontWeight eşlemesi yok), style'daki fontWeight'i doğru font
// ailesine çevirir ve fontWeight'i sıfırlar — Android'de custom font +
// fontWeight birlikte verilince sistem fontuna düşebiliyor.
// Ekranlar fontWeight token'ı vermeye devam eder; family seçimi burada.

import {
  Text as RNText,
  StyleSheet,
  type TextProps,
  type TextStyle,
} from 'react-native';

import { Colors, Fonts } from '@/constants/theme';

function fontFamilyFor(weight: TextStyle['fontWeight']): string {
  switch (String(weight ?? '400')) {
    case '500':
      return Fonts.medium;
    case '600':
    case '700':
    case 'bold':
      return Fonts.bold;
    case '800':
    case '900':
      return Fonts.black;
    default:
      return Fonts.regular;
  }
}

export function Text({ style, ...rest }: TextProps) {
  const flat = StyleSheet.flatten(style) ?? {};
  // Açıkça fontFamily verilmişse ona dokunma.
  const fontFamily = flat.fontFamily ?? fontFamilyFor(flat.fontWeight);

  return (
    <RNText
      {...rest}
      style={[styles.base, flat, { fontFamily, fontWeight: undefined }]}
    />
  );
}

const styles = StyleSheet.create({
  base: {
    color: Colors.text,
  },
});
