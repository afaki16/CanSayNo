// Form altında gösterilen hata bandı. Alert yerine bunu kullan: kullanıcı
// mesajı okurken formu görmeye devam eder ve ekran okuyucu duyurur.

import { Warning } from 'phosphor-react-native';
import { StyleSheet, View } from 'react-native';

import { Text } from './Text';

import { Colors, FontSize, Radius, Spacing } from '@/constants/theme';

export interface FormErrorProps {
  // null/undefined ise hiçbir şey render edilmez.
  message?: string | null;
}

export function FormError({ message }: FormErrorProps) {
  if (!message) {
    return null;
  }

  return (
    <View style={styles.container} accessibilityLiveRegion="polite">
      <Warning size={18} color={Colors.error} weight="fill" />
      <Text style={styles.text}>{message}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: Spacing.sm,
    backgroundColor: Colors.errorSoft,
    borderRadius: Radius.md,
    paddingVertical: Spacing.sm,
    paddingHorizontal: Spacing.md,
    marginBottom: Spacing.md,
  },
  text: {
    flex: 1,
    fontSize: FontSize.sm,
    color: Colors.error,
    lineHeight: FontSize.sm * 1.4,
  },
});
