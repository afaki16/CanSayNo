// Zorluk göstergesi: 5 yıldızın `count` kadarı dolu (weight="fill"), kalanı ince çizgi.
// Senaryo kartı, liste başlıkları ve detay ekranında ortak kullanılır.

import { Star } from 'phosphor-react-native';
import { StyleSheet, View } from 'react-native';

import { Colors } from '@/constants/theme';

export interface StarRatingProps {
  count: number; // dolu yıldız sayısı (1-5)
  size?: number;
  color?: string;
  total?: number;
}

export function StarRating({
  count,
  size = 14,
  color = Colors.warning,
  total = 5,
}: StarRatingProps) {
  return (
    <View style={styles.row} accessibilityLabel={`Zorluk ${count}/${total}`}>
      {Array.from({ length: total }).map((_, i) => (
        <Star
          key={i}
          size={size}
          color={color}
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
