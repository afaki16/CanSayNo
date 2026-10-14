// Kare ikon butonu — geri / kapat gibi üst bar aksiyonları için.
// Tasarımda 40x40, beyaz zemin, 13px köşe; açık zeminde ince kenarlı varyant.

import type { Icon } from 'phosphor-react-native';
import { Pressable } from 'react-native';

import { Colors } from '@/constants/theme';

export interface IconButtonProps {
  icon: Icon;
  onPress: () => void;
  accessibilityLabel: string;
  size?: number;
  // Beyaz zeminli ekranlarda ayrışması için ince kenar çizgisi.
  bordered?: boolean;
}

export function IconButton({
  icon: IconComponent,
  onPress,
  accessibilityLabel,
  size = 40,
  bordered = false,
}: IconButtonProps) {
  return (
    <Pressable
      onPress={onPress}
      hitSlop={6}
      accessibilityRole="button"
      accessibilityLabel={accessibilityLabel}
      style={({ pressed }) => ({
        width: size,
        height: size,
        borderRadius: Math.round(size * 0.32),
        backgroundColor: Colors.surface,
        borderWidth: bordered ? 1 : 0,
        borderColor: Colors.border,
        alignItems: 'center',
        justifyContent: 'center',
        opacity: pressed ? 0.7 : 1,
      })}
    >
      <IconComponent size={Math.round(size * 0.45)} color={Colors.text} weight="bold" />
    </Pressable>
  );
}
