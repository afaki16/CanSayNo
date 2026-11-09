// Veri çekilemediğinde gösterilen ortalanmış hata bloğu + "Tekrar Dene".
// Senaryo ekranlarının üçü de aynı görünümü paylaşsın diye ayrı bileşen.

import { CloudWarning } from 'phosphor-react-native';
import { useTranslation } from 'react-i18next';
import { StyleSheet, View } from 'react-native';

import { Text } from './Text';

import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';

import { Button } from './Button';

export interface ErrorStateProps {
  message: string;
  onRetry: () => void;
  // İstek hâlâ uçuyorken butonu kilitle (arka arkaya retry'ı engeller).
  isRetrying?: boolean;
}

export function ErrorState({ message, onRetry, isRetrying }: ErrorStateProps) {
  const { t } = useTranslation();
  return (
    <View style={styles.container}>
      <View style={styles.icon}>
        <CloudWarning size={48} color={Colors.error} weight="duotone" />
      </View>
      <Text style={styles.title}>{t('practice.errorState.title')}</Text>
      <Text style={styles.message}>{message}</Text>
      {/* Button tam genişlik değilken alignSelf: 'flex-start' alır; sarmalayıcı onu ortalar. */}
      <View>
        <Button variant="primary" onPress={onRetry} loading={isRetrying}>
          {t('common.retry')}
        </Button>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    // Yer varsa dikeyde ortalanır; ScrollView içinde içerik kadar kalır (flex: 1 orada çöker).
    flexGrow: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.sm,
    paddingVertical: Spacing.xxl,
    paddingHorizontal: Spacing.lg,
  },
  icon: {
    width: 96,
    height: 96,
    borderRadius: Radius.full,
    backgroundColor: Colors.errorSoft,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  title: {
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  message: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    textAlign: 'center',
    marginBottom: Spacing.sm,
  },
});
