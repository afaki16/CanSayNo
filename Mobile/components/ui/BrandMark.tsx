// CanSayNo logo işareti: mavi yuvarlak kare içinde yükselen chevron.
// `withName` ile yanına marka adı yazılır (karşılama ekranı üst bar).

import { View } from 'react-native';
import Svg, { Path } from 'react-native-svg';

import { Colors, FontWeight } from '@/constants/theme';

import { Text } from './Text';

export interface BrandMarkProps {
  size?: number;
  withName?: boolean;
}

export function BrandMark({ size = 28, withName = false }: BrandMarkProps) {
  const iconWidth = size * 0.57;
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', gap: 9 }}>
      <View
        style={{
          width: size,
          height: size,
          borderRadius: size * 0.32,
          backgroundColor: Colors.primary,
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Svg width={iconWidth} height={iconWidth * 0.75} viewBox="0 0 16 12" fill="none">
          <Path
            d="M2 10 L8 3 L14 10"
            stroke="#FFFFFF"
            strokeWidth={2.8}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </Svg>
      </View>
      {withName ? (
        <Text style={{ fontSize: 18, fontWeight: FontWeight.bold, letterSpacing: -0.36 }}>
          CanSayNo
        </Text>
      ) : null}
    </View>
  );
}
