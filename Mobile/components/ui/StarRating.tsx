// Zorluk göstergesi: 5 yıldızın `count` kadarı amber dolu, kalanı açık gri çizgi.
// Senaryo kartı, liste başlıkları ve detay ekranında ortak kullanılır.

import { Star } from 'phosphor-react-native';
import { useTranslation } from 'react-i18next';
import { StyleSheet, View } from 'react-native';

import { Colors } from '@/constants/theme';

export interface StarRatingProps {
  count: number; // dolu yıldız sayısı (1-5)
  size?: number;
  color?: string;
  emptyColor?: string;
  total?: number;
}

export function StarRating({
  count,
  size = 14,
  color = Colors.amber,
  emptyColor = '#D8D5CE',
  total = 5,
}: StarRatingProps) {
  const { t } = useTranslation();
  return (
    <View
      style={styles.row}
      accessibilityLabel={t('practice.difficulty.a11y', { value: count, total })}
    >
      {Array.from({ length: total }).map((_, i) => (
        <Star
          key={i}
          size={size}
          color={i < count ? color : emptyColor}
          weight={i < count ? 'fill' : 'regular'}
        />
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 1,
  },
});
