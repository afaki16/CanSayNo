// Chat mesaj balonu. AI (sol) ve kullanıcı (sağ) varyantları + "yazıyor" göstergesi.
// AI balonlarında sol tarafta yuvarlak avatar placeholder (Phosphor User) yer alır;
// gruplanmış ardışık AI mesajlarında avatar gizlenip yerine boşluk bırakılır (gutter korunur).

import { User } from 'phosphor-react-native';
import { useEffect } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import Animated, {
  Easing,
  useAnimatedStyle,
  useSharedValue,
  withDelay,
  withRepeat,
  withSequence,
  withTiming,
} from 'react-native-reanimated';

import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';

const AVATAR_SIZE = 32;
const DOT_BOUNCE_PX = 4;
const DOT_DURATION_MS = 300;
const DOT_STAGGER_MS = 150;

export interface MessageBubbleProps {
  role: 'ai' | 'user';
  content?: string;
  characterName?: string; // AI mesajı için; genelde sadece ilk mesajda verilir.
  showAvatar?: boolean; // AI mesajının yanında avatar göster (gruplama için kapatılabilir).
  contextColor?: string; // Avatar arka plan rengi (bağlam rengi).
  isTyping?: boolean; // true ise content yerine "yazıyor" animasyonu gösterilir.
}

// Tek bir zıplayan nokta. delay ile sıralı (stagger) animasyon sağlanır.
function TypingDot({ delay }: { delay: number }) {
  const translateY = useSharedValue(0);

  useEffect(() => {
    translateY.value = withDelay(
      delay,
      withRepeat(
        withSequence(
          withTiming(-DOT_BOUNCE_PX, {
            duration: DOT_DURATION_MS,
            easing: Easing.inOut(Easing.ease),
          }),
          withTiming(0, {
            duration: DOT_DURATION_MS,
            easing: Easing.inOut(Easing.ease),
          }),
        ),
        -1,
        false,
      ),
    );
  }, [delay, translateY]);

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ translateY: translateY.value }],
  }));

  return <Animated.View style={[styles.dot, animatedStyle]} />;
}

function TypingIndicator() {
  return (
    <View
      style={styles.dotsRow}
      accessibilityRole="text"
      accessibilityLabel="Yazıyor"
    >
      <TypingDot delay={0} />
      <TypingDot delay={DOT_STAGGER_MS} />
      <TypingDot delay={DOT_STAGGER_MS * 2} />
    </View>
  );
}

export function MessageBubble({
  role,
  content,
  characterName,
  showAvatar = false,
  contextColor = Colors.primary,
  isTyping = false,
}: MessageBubbleProps) {
  const isAI = role === 'ai';

  if (isAI) {
    return (
      <View style={styles.aiRow}>
        {/* Avatar gutter — avatar gizliyse hizayı koruyan boşluk bırakılır. */}
        {showAvatar ? (
          <View style={[styles.avatar, { backgroundColor: contextColor }]}>
            <User size={18} color={Colors.surface} weight="fill" />
          </View>
        ) : (
          <View style={styles.avatarSpacer} />
        )}

        <View style={styles.aiBubble}>
          {characterName ? (
            <Text style={styles.characterName}>{characterName}</Text>
          ) : null}
          {isTyping ? (
            <TypingIndicator />
          ) : (
            <Text style={styles.aiText} selectable>
              {content}
            </Text>
          )}
        </View>
      </View>
    );
  }

  return (
    <View style={styles.userRow}>
      <View style={styles.userBubble}>
        <Text style={styles.userText} selectable>
          {content}
        </Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  // AI (sol)
  aiRow: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    alignSelf: 'flex-start',
    gap: Spacing.sm,
    maxWidth: '85%',
  },
  avatar: {
    width: AVATAR_SIZE,
    height: AVATAR_SIZE,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarSpacer: {
    width: AVATAR_SIZE,
  },
  aiBubble: {
    flexShrink: 1,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    borderBottomLeftRadius: Radius.sm, // klasik chat bubble görünümü
    paddingHorizontal: Spacing.md,
    paddingVertical: Spacing.sm,
    gap: 2,
  },
  characterName: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.semibold,
    color: Colors.primary,
  },
  aiText: {
    fontSize: FontSize.base,
    color: Colors.text,
    lineHeight: FontSize.base * 1.5,
  },

  // Kullanıcı (sağ)
  userRow: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    alignSelf: 'flex-end',
    maxWidth: '75%',
  },
  userBubble: {
    flexShrink: 1,
    backgroundColor: Colors.primary,
    borderRadius: Radius.lg,
    borderBottomRightRadius: Radius.sm,
    paddingHorizontal: Spacing.md,
    paddingVertical: Spacing.sm,
  },
  userText: {
    fontSize: FontSize.base,
    color: Colors.surface,
    lineHeight: FontSize.base * 1.5,
  },

  // Typing indicator
  dotsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    paddingVertical: 6,
    paddingHorizontal: 2,
  },
  dot: {
    width: 7,
    height: 7,
    borderRadius: Radius.full,
    backgroundColor: Colors.textMuted,
  },
});
