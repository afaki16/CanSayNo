// Phosphor ikon adı (string) → ikon bileşeni map'i.
// Mock data'da ikonlar string olarak tutulur (iconName); UI'da bileşene çevirmek için.

import {
  Briefcase,
  ChatCircleText,
  DeviceMobile,
  GraduationCap,
  Heart,
  HouseLine,
  type Icon,
  ShoppingBag,
  SmileyWink,
  UsersThree,
} from 'phosphor-react-native';

const ICON_MAP: Record<string, Icon> = {
  Briefcase,
  UsersThree,
  SmileyWink,
  Heart,
  ShoppingBag,
  HouseLine,
  GraduationCap,
  DeviceMobile,
};

// Bilinmeyen bir ad gelirse güvenli fallback ile her zaman geçerli bir ikon döner.
export function getContextIcon(iconName: string): Icon {
  return ICON_MAP[iconName] ?? ChatCircleText;
}
