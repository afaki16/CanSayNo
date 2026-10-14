// Karşılama hero'su: iç içe mavi daireler üzerinde "NO" konuşma balonu ve
// köşede "dur" eli rozeti. Ölçüler tasarımdaki 212px kompozisyondan.

import { LinearGradient } from 'expo-linear-gradient';
import { HandPalm } from 'phosphor-react-native';
import { StyleSheet, View } from 'react-native';
import Svg, { Path } from 'react-native-svg';

import { Colors, FontWeight } from '@/constants/theme';

import { Text } from './Text';

export function NoBubble() {
  return (
    <View style={styles.root}>
      <View style={styles.outerCircle} />
      <View style={styles.innerCircle} />

      <View style={styles.bubbleWrap}>
        <LinearGradient
          colors={['#5398FF', Colors.primary, Colors.primaryDark]}
          locations={[0, 0.52, 1]}
          start={{ x: 0.25, y: 0 }}
          end={{ x: 0.75, y: 1 }}
          style={styles.bubble}
        >
          <Text style={styles.no}>NO</Text>
        </LinearGradient>
        {/* Balon kuyruğu */}
        <Svg width={42} height={32} viewBox="0 0 42 32" style={styles.tail}>
          <Path d="M0 0 H42 L11 31 Z" fill={Colors.primaryDark} />
        </Svg>
      </View>

      <View style={styles.handBadge}>
        <HandPalm size={26} color={Colors.primary} weight="fill" />
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
    backgroundColor: Colors.primarySoft,
  },
  innerCircle: {
    position: 'absolute',
    top: 32,
    left: 32,
    right: 32,
    bottom: 32,
    borderRadius: 74,
    backgroundColor: '#D4E5FE',
  },
  bubbleWrap: {
    width: 152,
  },
  bubble: {
    height: 116,
    borderRadius: 42,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: Colors.primaryDark,
    shadowOpacity: 0.4,
    shadowRadius: 21,
    shadowOffset: { width: 0, height: 22 },
    elevation: 10,
  },
  tail: {
    position: 'absolute',
    bottom: -15,
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
    shadowColor: Colors.primaryDark,
    shadowOpacity: 0.26,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 10 },
    elevation: 8,
  },
});
