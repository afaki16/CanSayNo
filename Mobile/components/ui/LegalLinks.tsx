// "Gizlilik Politikası · Kullanım Şartları" satırı — paywall (Apple abonelik kuralı) ve Profil.
// Sayfalar uygulama içi tarayıcıda açılır (lib/utils/openLegalPage.ts).

import { useTranslation } from 'react-i18next';
import { Pressable, StyleSheet, View } from 'react-native';

import { Colors, FontSize, FontWeight } from '@/constants/theme';
import { openLegalPage } from '@/lib/utils/openLegalPage';

import { Text } from './Text';

export interface LegalLinksProps {
  /** Koyu zeminde (ör. paywall) açık renk metin. */
  onDark?: boolean;
}

export function LegalLinks({ onDark = false }: LegalLinksProps) {
  const { t } = useTranslation();
  const color = onDark ? 'rgba(255,255,255,0.7)' : Colors.textSecondary;
  return (
    <View style={styles.row}>
      <Pressable onPress={() => openLegalPage('privacy')} hitSlop={8} accessibilityRole="link">
        <Text style={[styles.link, { color }]}>{t('account.legal.privacy')}</Text>
      </Pressable>
      <Text style={[styles.dot, { color }]}>·</Text>
      <Pressable onPress={() => openLegalPage('terms')} hitSlop={8} accessibilityRole="link">
        <Text style={[styles.link, { color }]}>{t('account.legal.terms')}</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    gap: 8,
  },
  link: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.semibold,
    textDecorationLine: 'underline',
  },
  dot: {
    fontSize: FontSize.xs,
  },
});
