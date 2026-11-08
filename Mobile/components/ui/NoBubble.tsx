// Karşılama hero'su: iç içe açık mor daireler üzerinde marka geçişli "NO" konuşma balonu ve
// köşede mercan "dur" eli rozeti (logodaki el). Ölçüler tasarımdaki 212px kompozisyondan.

import { LinearGradient } from 'expo-linear-gradient';
import { HandPalm } from 'phosphor-react-native';
import { StyleSheet, View } from 'react-native';
import Svg, { Path } from 'react-native-svg';

import { Colors, FontWeight, Gradients } from '@/constants/theme';

import { Text } from './Text';

// Kuyruk balonun alt kenarından başlar (içine taşarsa geçişle ek yeri görünür). Rengi, geçişin
// kuyruğun durduğu noktadaki tonu (sol alt, geçişin ~%64'ü).
const BUBBLE_TAIL = '#3E6CF0';

export function NoBubble() {
  return (
    <View style={styles.root}>
      <View style={styles.outerCircle} />
      <View style={styles.innerCircle} />

      <View style={styles.bubbleWrap}>
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
      </View>

      <View style={styles.handBadge}>
        <HandPalm size={26} color={Colors.secondary} weight="fill" />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  root: {
    width: 212,
    height: 212,
    alignItems: 'center',
    justifyContent: 'center',
  },
  outerCircle: {
    ...StyleSheet.absoluteFillObject,
    borderRadius: 106,
    backgroundColor: Colors.indigoSoft,
  },
  innerCircle: {
    position: 'absolute',
    top: 32,
    left: 32,
    right: 32,
    bottom: 32,
    borderRadius: 74,
    backgroundColor: '#DCDDFB',
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
    top: 6,
    right: 4,
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
