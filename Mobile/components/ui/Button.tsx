// Genel amaçlı Button bileşeni. 4 varyant (primary/secondary/outline/ghost)
// ve 3 boyut (sm/md/lg) destekler. Loading / disabled / icon slot'ları içerir.
// Pressable kullanır; pressed state'inde opacity ile feedback verir.
// Primary varyantta tasarımdaki mavi renkli gölge (Shadow.primary) var.

import {
  ActivityIndicator,
  Pressable,
  View,
  type GestureResponderEvent,
} from 'react-native';

import { Text } from './Text';

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';

export type ButtonVariant = 'primary' | 'secondary' | 'outline' | 'ghost';
export type ButtonSize = 'sm' | 'md' | 'lg';

export interface ButtonProps {
  variant?: ButtonVariant;
  size?: ButtonSize;
  children: React.ReactNode;
  onPress?: (event: GestureResponderEvent) => void;
  disabled?: boolean;
  loading?: boolean;
  fullWidth?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
}

type VariantStyle = {
  backgroundColor: string;
  borderColor: string;
  borderWidth: number;
  textColor: string;
  // Sadece primary varyantında (renkli) shadow var.
  hasShadow: boolean;
};

const VARIANT_STYLES: Record<ButtonVariant, VariantStyle> = {
  primary: {
    backgroundColor: Colors.primary,
    borderColor: Colors.primary,
    borderWidth: 0,
    textColor: Colors.surface,
    hasShadow: true,
  },
  secondary: {
    backgroundColor: Colors.secondary,
    borderColor: Colors.secondary,
    borderWidth: 0,
    textColor: Colors.surface,
    hasShadow: false,
  },
  // Beyaz zemin + ince kenar (örn. sonuç ekranındaki 'Tekrar dene')
  outline: {
    backgroundColor: Colors.surface,
    borderColor: Colors.border,
    borderWidth: 1,
    textColor: Colors.text,
    hasShadow: false,
  },
  ghost: {
    backgroundColor: 'transparent',
    borderColor: 'transparent',
    borderWidth: 0,
    textColor: Colors.textSecondary,
    hasShadow: false,
  },
};

const SIZE_STYLES: Record<
  ButtonSize,
  { paddingVertical: number; paddingHorizontal: number; fontSize: number }
> = {
  sm: { paddingVertical: 10, paddingHorizontal: 16, fontSize: FontSize.sm },
  md: { paddingVertical: 15, paddingHorizontal: 20, fontSize: FontSize.base },
  lg: { paddingVertical: 17, paddingHorizontal: 24, fontSize: 17 },
};

export function Button({
  variant = 'primary',
  size = 'md',
  children,
  onPress,
  disabled = false,
  loading = false,
  fullWidth = false,
  leftIcon,
  rightIcon,
}: ButtonProps) {
  const variantStyle = VARIANT_STYLES[variant];
  const sizeStyle = SIZE_STYLES[size];
  const isInteractive = !disabled && !loading;

  return (
    <Pressable
      onPress={isInteractive ? onPress : undefined}
      disabled={!isInteractive}
      accessibilityRole="button"
      accessibilityState={{ disabled: !isInteractive, busy: loading }}
      style={({ pressed }) => ({
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'center',
        paddingVertical: sizeStyle.paddingVertical,
        paddingHorizontal: sizeStyle.paddingHorizontal,
        borderRadius: Radius.lg,
        backgroundColor: variantStyle.backgroundColor,
        borderColor: variantStyle.borderColor,
        borderWidth: variantStyle.borderWidth,
        alignSelf: fullWidth ? 'stretch' : 'flex-start',
        opacity: disabled ? 0.45 : pressed ? 0.8 : 1,
        ...(variantStyle.hasShadow && !disabled ? Shadow.primary : null),
      })}
    >
      {loading ? (
        <ActivityIndicator
          size="small"
          color={variantStyle.textColor}
          accessibilityLabel="Yükleniyor"
        />
      ) : (
        <>
          {leftIcon ? (
            <View style={{ marginRight: Spacing.sm }}>{leftIcon}</View>
          ) : null}
          <Text
            style={{
              color: variantStyle.textColor,
              fontSize: sizeStyle.fontSize,
              fontWeight: FontWeight.bold,
            }}
          >
            {children}
          </Text>
          {rightIcon ? (
            <View style={{ marginLeft: Spacing.sm }}>{rightIcon}</View>
          ) : null}
        </>
      )}
    </Pressable>
  );
}
