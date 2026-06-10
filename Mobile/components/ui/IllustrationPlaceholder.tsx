// Karşılama ekranı için geçici illustration placeholder'ı.
// İleride buraya Figma'dan gerçek bir illustration gelecek; şimdilik soft
// arka planlı bir kutu + ortada Phosphor ikon + çevresinde dekoratif daireler.
//
// Boyut prop ile ayarlanabilir (default 240); ikon ve daireler buna göre ölçeklenir.

import { HandWaving, type Icon } from 'phosphor-react-native';
import { View } from 'react-native';

import { Colors, Radius, Shadow } from '@/constants/theme';

export interface IllustrationPlaceholderProps {
  size?: number;
  // Ortadaki ikon; default "el sallama" jesti (sıcak karşılama hissi).
  icon?: Icon;
}

// Dekoratif daireler: kutuya göre relative konum + boyut + renk.
// opacity 0.5 ile arka planda hafif kalır, dikkat dağıtmaz.
const DECOR_CIRCLES = [
  { size: 0.18, top: -0.04, left: -0.03, color: Colors.secondarySoft },
  { size: 0.12, top: 0.08, right: -0.02, color: Colors.warningSoft },
  { size: 0.22, bottom: -0.05, right: 0.08, color: Colors.successSoft },
  { size: 0.1, bottom: 0.1, left: -0.04, color: Colors.secondary },
] as const;

export function IllustrationPlaceholder({
  size = 240,
  icon: IconComponent = HandWaving,
}: IllustrationPlaceholderProps) {
  return (
    <View style={{ width: size, height: size }}>
      {/* Ana soft kutu */}
      <View
        style={{
          width: size,
          height: size,
          borderRadius: Radius.xl,
          backgroundColor: Colors.primarySoft,
          alignItems: 'center',
          justifyContent: 'center',
          ...Shadow.sm,
        }}
      >
        <IconComponent
          size={size * 0.42}
          color={Colors.primary}
          weight="duotone"
        />
      </View>

      {/* Dekoratif daireler — kutunun üstüne taşan, oyunsu detaylar */}
      {DECOR_CIRCLES.map((circle, index) => {
        const diameter = size * circle.size;
        return (
          <View
            key={index}
            pointerEvents="none"
            style={{
              position: 'absolute',
              width: diameter,
              height: diameter,
              borderRadius: Radius.full,
              backgroundColor: circle.color,
              opacity: 0.5,
              top: 'top' in circle ? size * circle.top : undefined,
              bottom: 'bottom' in circle ? size * circle.bottom : undefined,
              left: 'left' in circle ? size * circle.left : undefined,
              right: 'right' in circle ? size * circle.right : undefined,
            }}
          />
        );
      })}
    </View>
  );
}
