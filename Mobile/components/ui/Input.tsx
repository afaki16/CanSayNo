// Form alanları için genel Input bileşeni. Label, error, helperText ve
// sol/sağ icon slot'larını destekler. Focus durumunda border rengi primary,
// error durumunda error rengi olur.

import { useState } from 'react';
import {
  Text,
  TextInput,
  View,
  type TextInputProps,
  type ViewStyle,
} from 'react-native';

// onFocus/onBlur event tipleri RN 0.81'de FocusEvent/BlurEvent oldu. Bunları
// doğrudan import edemediğimiz için TextInputProps'tan türetiyoruz.
type FocusHandler = NonNullable<TextInputProps['onFocus']>;
type BlurHandler = NonNullable<TextInputProps['onBlur']>;
type FocusArg = Parameters<FocusHandler>[0];
type BlurArg = Parameters<BlurHandler>[0];

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';

export interface InputProps extends TextInputProps {
  label?: string;
  error?: string;
  helperText?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  containerStyle?: ViewStyle;
}

export function Input({
  label,
  error,
  helperText,
  leftIcon,
  rightIcon,
  containerStyle,
  onFocus,
  onBlur,
  style,
  ...rest
}: InputProps) {
  const [isFocused, setIsFocused] = useState(false);

  const handleFocus = (e: FocusArg) => {
    setIsFocused(true);
    onFocus?.(e);
  };

  const handleBlur = (e: BlurArg) => {
    setIsFocused(false);
    onBlur?.(e);
  };

  // Sıralama önemli: error > focus > default. Error focus'tan baskın olmalı
  // ki kullanıcı düzeltirken bile hatanın görsel sinyali görünür kalsın.
  const borderColor = error
    ? Colors.error
    : isFocused
    ? Colors.primary
    : Colors.border;

  return (
    <View style={containerStyle}>
      {label ? (
        <Text
          style={{
            marginBottom: Spacing.xs,
            fontSize: FontSize.sm,
            fontWeight: FontWeight.medium,
            color: Colors.textSecondary,
          }}
        >
          {label}
        </Text>
      ) : null}

      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          backgroundColor: Colors.surface,
          borderWidth: 1,
          borderColor,
          borderRadius: Radius.md,
          paddingHorizontal: Spacing.md,
        }}
      >
        {leftIcon ? (
          <View style={{ marginRight: Spacing.sm }}>{leftIcon}</View>
        ) : null}

        <TextInput
          {...rest}
          onFocus={handleFocus}
          onBlur={handleBlur}
          placeholderTextColor={Colors.textMuted}
          style={[
            {
              flex: 1,
              paddingVertical: Spacing.md,
              fontSize: FontSize.base,
              color: Colors.text,
            },
            style,
          ]}
        />

        {rightIcon ? (
          <View style={{ marginLeft: Spacing.sm }}>{rightIcon}</View>
        ) : null}
      </View>

      {error ? (
        <Text
          style={{
            marginTop: Spacing.xs,
            fontSize: FontSize.xs,
            color: Colors.error,
          }}
        >
          {error}
        </Text>
      ) : helperText ? (
        <Text
          style={{
            marginTop: Spacing.xs,
            fontSize: FontSize.xs,
            color: Colors.textMuted,
          }}
        >
          {helperText}
        </Text>
      ) : null}
    </View>
  );
}
