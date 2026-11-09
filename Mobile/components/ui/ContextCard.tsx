// Bağlam kartı — Senaryolar tab'ındaki grid'in tek hücresi.
// Beyaz kart, bağlam renginde soft ikon kutusu. Premium bağlamlarda kilitli
// "Premium" chip'i, ücretsizlerde "tamamlanan/toplam" ilerleme chip'i.

import { LockSimple } from 'phosphor-react-native';
import { useTranslation } from 'react-i18next';
import { Pressable, StyleSheet, View } from 'react-native';

import { Text } from './Text';

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
} from '@/constants/theme';
import { getContextIcon } from '@/lib/icons';
import type { Context, ContextProgress } from '@/types/scenario';

export interface ContextCardProps {
  context: Context;
  // Sayılar API'den türetilir, metadata'da tutulmaz — çağıran ekran verir.
  progress: ContextProgress;
  // Premium bağlam + aboneliği olmayan kullanıcı. Abonelik bilgisi çağıran ekrandan gelir.
  isLocked: boolean;
  onPress: () => void;
}

// Senaryolar ekranındaki skeleton bu yüksekliği kullanır — layout kaymasın.
export const CONTEXT_CARD_MIN_HEIGHT = 190;

export function ContextCard({ context, progress, isLocked, onPress }: ContextCardProps) {
  const { t } = useTranslation();
  const Icon = getContextIcon(context.iconName);

  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={context.name}
      style={({ pressed }) => [styles.card, { opacity: pressed ? 0.85 : 1 }]}
    >
      <View style={[styles.iconBox, { backgroundColor: context.softColor }]}>
        <Icon size={26} color={context.color} weight="fill" />
      </View>

      <Text style={styles.title} numberOfLines={2}>
        {context.name}
      </Text>
      <Text style={styles.description} numberOfLines={2}>
        {context.description}
      </Text>

      {isLocked ? (
        <View style={styles.chip}>
          <LockSimple size={13} color={Colors.textMuted} weight="fill" />
          <Text style={styles.chipText}>{t('practice.scenarios.premium')}</Text>
        </View>
      ) : (
        <View style={[styles.chip, { backgroundColor: context.softColor }]}>
          <Text style={[styles.chipText, { color: context.color }]}>
            {t('practice.scenarios.cardProgress', {
              completed: progress.completedCount,
              total: progress.scenariosCount,
            })}
          </Text>
        </View>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    flex: 1,
    minHeight: CONTEXT_CARD_MIN_HEIGHT,
    padding: 18,
    borderRadius: Radius.xl,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  iconBox: {
    width: 50,
    height: 50,
    borderRadius: 15,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 14,
  },
  title: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.32,
    lineHeight: 19,
    color: Colors.text,
  },
  description: {
    marginTop: 5,
    minHeight: 32,
    fontSize: FontSize.xs,
    lineHeight: 16,
    color: Colors.textMuted,
  },
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    alignSelf: 'flex-start',
    gap: 5,
    marginTop: 12,
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: Radius.full,
    backgroundColor: Colors.chip,
  },
  chipText: {
    fontSize: 11,
    fontWeight: FontWeight.bold,
    color: Colors.textMuted,
  },
});
