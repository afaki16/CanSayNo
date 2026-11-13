// Karşılama hero'su: ortada marka geçişli "NO" balonu, köşede mercan "dur" eli (logodaki el).
// Arka plan ürünü anlatır: karakterlerin ısrar balonları sırayla belirir ve "NO"ya çarpıp
// geri itilir; balonun arkasından marka renginde halkalar yayılır, el ara ara "dur" der.
// Sistemde "hareketi azalt" açıksa her şey sabit çizilir.

import { LinearGradient } from 'expo-linear-gradient';
import { HandPalm } from 'phosphor-react-native';
import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { StyleSheet, View } from 'react-native';
import Animated, {
  Easing,
  cancelAnimation,
  useAnimatedStyle,
  useReducedMotion,
  useSharedValue,
  withDelay,
  withRepeat,
  withSequence,
  withTiming,
} from 'react-native-reanimated';
import Svg, { Path } from 'react-native-svg';

import { Colors, FontWeight, Gradients } from '@/constants/theme';

import { Text } from './Text';

// Kuyruk balonun alt kenarından başlar (içine taşarsa geçişle ek yeri görünür). Rengi, geçişin
// kuyruğun durduğu noktadaki tonu (sol alt, geçişin ~%64'ü).
const BUBBLE_TAIL = '#3E6CF0';

const WIDTH = 300;
const HEIGHT = 236;
const RING_SIZE = 200;
const RING_PERIOD = 3600;
const RING_COUNT = 3;

// Israr balonları: her biri CHIP_PERIOD içinde bir kez belirir ve itilir; aralarında CHIP_GAP var.
const CHIP_PERIOD = 6600;
const CHIP_GAP = 2200;
const CHIP_IN = 450;
const CHIP_HOLD = 1500;
const CHIP_OUT = 550;

type ChipPlacement = {
  key: 'pressure1' | 'pressure2' | 'pressure3';
  style: object;
  /** Geri itilirken gideceği yön (merkezden dışarı). */
  push: { x: number; y: number };
  /** Kuyruk hangi alt köşede (balon merkeze bakar). */
  tail: 'left' | 'right';
};

const CHIPS: ChipPlacement[] = [
  { key: 'pressure1', style: { top: 6, left: 0 }, push: { x: -14, y: -6 }, tail: 'right' },
  { key: 'pressure2', style: { top: 118, right: 0 }, push: { x: 14, y: 4 }, tail: 'left' },
  { key: 'pressure3', style: { bottom: 18, left: 2 }, push: { x: -14, y: 6 }, tail: 'right' },
];

export function NoBubble() {
  const reduceMotion = useReducedMotion();

  return (
    <View
      style={styles.root}
      accessibilityElementsHidden
      importantForAccessibility="no-hide-descendants"
    >
      <View style={styles.baseCircle} />
      {!reduceMotion &&
        Array.from({ length: RING_COUNT }, (_, i) => (
          <PulseRing key={i} delay={(RING_PERIOD / RING_COUNT) * i} />
        ))}

      {CHIPS.map((chip, i) => (
        <PressureChip key={chip.key} chip={chip} delay={600 + CHIP_GAP * i} still={reduceMotion} />
      ))}

      <FloatingBubble still={reduceMotion} />
      <HandBadge still={reduceMotion} />
    </View>
  );
}

function PulseRing({ delay }: { delay: number }) {
  const progress = useSharedValue(0);

  useEffect(() => {
    progress.value = withDelay(
      delay,
      withRepeat(withTiming(1, { duration: RING_PERIOD, easing: Easing.out(Easing.quad) }), -1),
    );
    return () => cancelAnimation(progress);
  }, [delay, progress]);

  const style = useAnimatedStyle(() => ({
    opacity: 0.55 * (1 - progress.value),
    transform: [{ scale: 0.62 + 0.58 * progress.value }],
  }));

  return <Animated.View style={[styles.ring, style]} />;
}

function PressureChip({ chip, delay, still }: { chip: ChipPlacement; delay: number; still: boolean }) {
  const { t } = useTranslation();
  const opacity = useSharedValue(still ? 1 : 0);
  const pushed = useSharedValue(0);

  useEffect(() => {
    if (still) return;
    const rest = CHIP_PERIOD - CHIP_IN - CHIP_HOLD - CHIP_OUT;
    opacity.value = withDelay(
      delay,
      withRepeat(
        withSequence(
          withTiming(1, { duration: CHIP_IN, easing: Easing.out(Easing.cubic) }),
          withDelay(CHIP_HOLD, withTiming(0, { duration: CHIP_OUT, easing: Easing.in(Easing.quad) })),
          withTiming(0, { duration: rest }),
        ),
        -1,
      ),
    );
    pushed.value = withDelay(
      delay,
      withRepeat(
        withSequence(
          withTiming(0, { duration: CHIP_IN + CHIP_HOLD }),
          withTiming(1, { duration: CHIP_OUT, easing: Easing.in(Easing.quad) }),
          withTiming(0, { duration: rest }),
        ),
        -1,
      ),
    );
    return () => {
      cancelAnimation(opacity);
      cancelAnimation(pushed);
    };
  }, [delay, still, opacity, pushed]);

  const style = useAnimatedStyle(() => ({
    opacity: opacity.value,
    transform: [
      { translateX: chip.push.x * pushed.value },
      { translateY: chip.push.y * pushed.value + 6 * (1 - opacity.value) * (1 - pushed.value) },
      { scale: 1 - 0.12 * pushed.value },
    ],
  }));

  return (
    <Animated.View
      style={[
        styles.chip,
        chip.tail === 'left' ? styles.chipTailLeft : styles.chipTailRight,
        chip.style,
        style,
      ]}
    >
      <Text style={styles.chipText}>{t(`auth.welcome.${chip.key}`)}</Text>
    </Animated.View>
  );
}

function FloatingBubble({ still }: { still: boolean }) {
  const lift = useSharedValue(0);

  useEffect(() => {
    if (still) return;
    lift.value = withRepeat(
      withTiming(1, { duration: 2400, easing: Easing.inOut(Easing.sin) }),
      -1,
      true,
    );
    return () => cancelAnimation(lift);
  }, [still, lift]);

  const style = useAnimatedStyle(() => ({ transform: [{ translateY: -5 * lift.value }] }));

  return (
    <Animated.View style={[styles.bubbleWrap, style]}>
      <LinearGradient
        colors={Gradients.brand.colors}
        start={Gradients.brand.start}
        end={Gradients.brand.end}
        style={styles.bubble}
      >
        <Text style={styles.no}>NO</Text>
      </LinearGradient>
      {/* Balon kuyruğu */}
      <Svg width={42} height={32} viewBox="0 0 42 32" style={styles.tail}>
        <Path d="M0 0 H42 L11 31 Z" fill={BUBBLE_TAIL} />
      </Svg>
    </Animated.View>
  );
}

function HandBadge({ still }: { still: boolean }) {
  const angle = useSharedValue(0);

  useEffect(() => {
    if (still) return;
    const swing = { duration: 130, easing: Easing.inOut(Easing.quad) };
    angle.value = withDelay(
      1200,
      withRepeat(
        withSequence(
          withTiming(-14, swing),
          withTiming(12, swing),
          withTiming(-8, swing),
          withTiming(0, swing),
          withTiming(0, { duration: 2800 }),
        ),
        -1,
      ),
    );
    return () => cancelAnimation(angle);
  }, [still, angle]);

  const style = useAnimatedStyle(() => ({ transform: [{ rotate: `${angle.value}deg` }] }));

  return (
    <View style={styles.handBadge}>
      <Animated.View style={style}>
        <HandPalm size={26} color={Colors.secondary} weight="fill" />
      </Animated.View>
    </View>
  );
}

const styles = StyleSheet.create({
  root: {
    width: WIDTH,
    height: HEIGHT,
    alignItems: 'center',
    justifyContent: 'center',
  },
  baseCircle: {
    position: 'absolute',
    width: 168,
    height: 168,
    borderRadius: 84,
    backgroundColor: Colors.indigoSoft,
  },
  ring: {
    position: 'absolute',
    width: RING_SIZE,
    height: RING_SIZE,
    borderRadius: RING_SIZE / 2,
    borderWidth: 2,
    borderColor: '#C9C4F6',
    backgroundColor: 'rgba(236, 234, 252, 0.6)',
  },
  chip: {
    position: 'absolute',
    maxWidth: 128,
    paddingHorizontal: 11,
    paddingVertical: 7,
    borderRadius: 14,
    backgroundColor: Colors.surface,
    shadowColor: Colors.indigoDark,
    shadowOpacity: 0.16,
    shadowRadius: 8,
    shadowOffset: { width: 0, height: 6 },
    elevation: 4,
  },
  chipTailLeft: {
    borderBottomLeftRadius: 4,
  },
  chipTailRight: {
    borderBottomRightRadius: 4,
  },
  chipText: {
    fontSize: 12.5,
    fontWeight: FontWeight.semibold,
    color: Colors.textSecondary,
  },
  bubbleWrap: {
    width: 152,
  },
  bubble: {
    height: 116,
    borderRadius: 42,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: Colors.indigoDark,
    shadowOpacity: 0.4,
    shadowRadius: 21,
    shadowOffset: { width: 0, height: 22 },
    elevation: 10,
  },
  tail: {
    position: 'absolute',
    bottom: -29,
    left: 36,
  },
  no: {
    fontSize: 58,
    fontWeight: FontWeight.black,
    color: '#FFFFFF',
    letterSpacing: -3.5,
    lineHeight: 64,
  },
  handBadge: {
    position: 'absolute',
    top: 16,
    right: 52,
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: Colors.surface,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: Colors.indigoDark,
    shadowOpacity: 0.26,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 10 },
    elevation: 8,
  },
});
