// CanSayNo logosu: mor→mavi geçişli yuvarlak kare, içinde beyaz konuşma balonu ve mercan "dur" eli.
// Uygulama ikonuyla aynı çizim. Geometrinin kaynağı design/brand/brand.mjs (1024'lük tuval) —
// orada bir şey değişirse burayı da güncelle. `withName` ile yanına marka adı yazılır.

import { useId } from 'react';
import { View } from 'react-native';
import Svg, { Defs, G, Line, LinearGradient, Path, Rect, Stop } from 'react-native-svg';

import { Colors, FontWeight, Gradients } from '@/constants/theme';

import { Text } from './Text';

export interface BrandMarkProps {
  size?: number;
  withName?: boolean;
}

const FINGERS: readonly (readonly [number, number])[] = [
  [-117, -262], [-39, -310], [39, -294], [117, -236],
];
const FINGER_WIDTH = 62;

export function BrandMark({ size = 28, withName = false }: BrandMarkProps) {
  // Aynı ekranda birden fazla logo olursa gradient id'leri çakışmasın.
  const gradientId = `brand-${useId().replace(/:/g, '')}`;
  const [from, to] = Gradients.brand.colors;

  return (
    <View
      style={{ flexDirection: 'row', alignItems: 'center', gap: 9 }}
      accessibilityRole="image"
      accessibilityLabel="CanSayNo"
    >
      <Svg width={size} height={size} viewBox="0 0 1024 1024">
        <Defs>
          <LinearGradient id={gradientId} x1="0" y1="0" x2="1" y2="1">
            <Stop offset="0" stopColor={from} />
            <Stop offset="1" stopColor={to} />
          </LinearGradient>
        </Defs>
        <Rect width={1024} height={1024} rx={229} fill={`url(#${gradientId})`} />
        <G transform="translate(512 512) scale(0.86) translate(-512 -512)">
          {/* Konuşma balonu + sol alt kuyruk */}
          <Rect x={182} y={176} width={660} height={590} rx={270} fill="#FFFFFF" />
          <Path
            d="M352 640 L262 858 L500 740 Z"
            fill="#FFFFFF"
            stroke="#FFFFFF"
            strokeWidth={44}
            strokeLinejoin="round"
          />
          {/* "Dur" eli */}
          <G transform="translate(512 471) scale(0.8) translate(30 70)">
            {FINGERS.map(([x, top]) => (
              <Line
                key={x}
                x1={x}
                y1={top + FINGER_WIDTH / 2}
                x2={x}
                y2={0}
                stroke={Colors.secondary}
                strokeWidth={FINGER_WIDTH}
                strokeLinecap="round"
              />
            ))}
            <Path
              d="M-148 -60 H148 V40 A130 130 0 0 1 18 170 H-18 A130 130 0 0 1 -148 40 Z"
              fill={Colors.secondary}
            />
            <Line
              x1={-104}
              y1={78}
              x2={-200}
              y2={-16}
              stroke={Colors.secondary}
              strokeWidth={82}
              strokeLinecap="round"
            />
          </G>
        </G>
      </Svg>
      {withName ? (
        <Text style={{ fontSize: 18, fontWeight: FontWeight.bold, letterSpacing: -0.36 }}>
          CanSayNo
        </Text>
      ) : null}
    </View>
  );
}
