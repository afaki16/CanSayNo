// Yükleniyor placeholder'ı. Gerçek içerikle AYNI ölçüde kullanılır ki veri
// gelince layout zıplamasın — boyutları çağıran ekran verir.
//
// Nabız animasyonu Reanimated ile UI thread'de döner (JS bridge trafiği yok).

import { useEffect } from 'react';
import type { DimensionValue, ViewStyle } from 'react-native';
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withRepeat,
  withTiming,
} from 'react-native-reanimated';

import { Colors, Radius } from '@/constants/theme';

const PULSE_DURATION_MS = 1000;
const PULSE_MIN_OPACITY = 0.5;

export interface SkeletonProps {
  width?: DimensionValue;
  height?: DimensionValue;
  borderRadius?: number;
  style?: ViewStyle;
}

export function Skeleton({
  width = '100%',
  height = 16,
  borderRadius = Radius.sm,
  style,
}: SkeletonProps) {
  const opacity = useSharedValue(PULSE_MIN_OPACITY);

  useEffect(() => {
    // -1 = sonsuz tekrar, true = ters yönde geri dön (0.5 ↔ 1).
    opacity.value = withRepeat(
      withTiming(1, { duration: PULSE_DURATION_MS }),
      -1,
      true,
    );
  }, [opacity]);

  const animatedStyle = useAnimatedStyle(() => ({ opacity: opacity.value }));

  return (
    <Animated.View
      accessibilityRole="progressbar"
      accessibilityLabel="Yükleniyor"
      style={[
        { width, height, borderRadius, backgroundColor: Colors.border },
        style,
        animatedStyle,
      ]}
    />
  );
}
