// Genel amaçlı Button bileşeni. 4 varyant (primary/secondary/outline/ghost)
// ve 3 boyut (sm/md/lg) destekler. Loading / disabled / icon slot'ları içerir.
// Pressable kullanır; pressed state'inde opacity ile feedback verir.

import {
  ActivityIndicator,
  Pressable,
  Text,
  View,
  type GestureResponderEvent,
} from 'react-native';

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
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
  // Sadece primary varyantında shadow var; diğerlerinde 0.
  shadowOpacity: number;
  elevation: number;
};

const VARIANT_STYLES: Record<ButtonVariant, VariantStyle> = {
  primary: {
    backgroundColor: Colors.primary,
    borderColor: Colors.primary,
    borderWidth: 0,
    textColor: Colors.surface,
    shadowOpacity: 0.15,
    elevation: 2,
  },
  secondary: {
    backgroundColor: Colors.secondary,
    borderColor: Colors.secondary,
    borderWidth: 0,
    textColor: Colors.surface,
    shadowOpacity: 0,
    elevation: 0,
  },
  outline: {
    backgroundColor: 'transparent',
    borderColor: Colors.primary,
    borderWidth: 1,
    textColor: Colors.primary,
    shadowOpacity: 0,
    elevation: 0,
  },
  ghost: {
    backgroundColor: 'transparent',
    borderColor: 'transparent',
    borderWidth: 0,
    textColor: Colors.primary,
    shadowOpacity: 0,
    elevation: 0,
  },
};

const SIZE_STYLES: Record<
  ButtonSize,
  { paddingVertical: number; paddingHorizontal: number; fontSize: number }
> = {
  sm: { paddingVertical: 8, paddingHorizontal: 16, fontSize: FontSize.sm },
  md: { paddingVertical: 12, paddingHorizontal: 20, fontSize: FontSize.base },
  lg: { paddingVertical: 16, paddingHorizontal: 24, fontSize: FontSize.lg },
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
        borderRadius: Radius.md,
        backgroundColor: variantStyle.backgroundColor,
        borderColor: variantStyle.borderColor,
        borderWidth: variantStyle.borderWidth,
        alignSelf: fullWidth ? 'stretch' : 'flex-start',
        opacity: disabled ? 0.5 : pressed ? 0.7 : 1,
        // iOS shadow
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: variantStyle.shadowOpacity,
        shadowRadius: 4,
        // Android shadow
        elevation: variantStyle.elevation,
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
              fontWeight: FontWeight.semibold,
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
