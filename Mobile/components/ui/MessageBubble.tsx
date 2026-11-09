// Chat mesaj balonu. AI (sol) ve kullanıcı (sağ) varyantları + "yazıyor" göstergesi.
// AI balonlarında sol tarafta karakterin baş harfleriyle yuvarlak avatar yer alır;
// gruplanmış ardışık AI mesajlarında avatar gizlenip yerine boşluk bırakılır (gutter korunur).

import { LinearGradient } from 'expo-linear-gradient';
import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { StyleSheet, View } from 'react-native';

import { Text } from './Text';
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
  Gradients,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';

const AVATAR_SIZE = 30;
const DOT_BOUNCE_PX = 4;
const DOT_DURATION_MS = 300;
const DOT_STAGGER_MS = 150;

export interface MessageBubbleProps {
  role: 'ai' | 'user';
  content?: string;
  characterName?: string; // AI mesajı için; genelde sadece ilk mesajda verilir.
  showAvatar?: boolean; // AI mesajının yanında avatar göster (gruplama için kapatılabilir).
  contextColor?: string; // Avatar arka plan rengi (bağlam rengi).
  avatarLabel?: string; // Avatar içindeki baş harfler (örn: "MB").
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
  const { t } = useTranslation();
  return (
    <View
      style={styles.dotsRow}
      accessibilityRole="text"
      accessibilityLabel={t('practice.chat.typing')}
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
  avatarLabel,
  isTyping = false,
}: MessageBubbleProps) {
  const isAI = role === 'ai';

  if (isAI) {
    return (
      <View style={styles.aiRow}>
        {/* Avatar gutter — avatar gizliyse hizayı koruyan boşluk bırakılır. */}
        {showAvatar ? (
          <View style={[styles.avatar, { backgroundColor: contextColor }]}>
            <Text style={styles.avatarText}>{avatarLabel}</Text>
          </View>
        ) : (
          <View style={styles.avatarSpacer} />
        )}

        <View style={styles.aiColumn}>
          {characterName ? (
            <Text style={styles.characterName}>{characterName}</Text>
          ) : null}
          <View style={styles.aiBubble}>
            {isTyping ? (
              <TypingIndicator />
            ) : (
              <Text style={styles.aiText} selectable>
                {content}
              </Text>
            )}
          </View>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.userRow}>
      <LinearGradient
        colors={Gradients.brand.colors}
        start={Gradients.brand.start}
        end={Gradients.brand.end}
        style={styles.userBubble}
      >
        <Text style={styles.userText} selectable>
          {content}
        </Text>
      </LinearGradient>
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
    maxWidth: '88%',
  },
  avatar: {
    width: AVATAR_SIZE,
    height: AVATAR_SIZE,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarText: {
    fontSize: 11,
    fontWeight: FontWeight.black,
    color: Colors.surface,
  },
  avatarSpacer: {
    width: AVATAR_SIZE,
  },
  aiColumn: {
    flexShrink: 1,
  },
  aiBubble: {
    backgroundColor: Colors.surface,
    borderRadius: 18,
    borderBottomLeftRadius: 5, // klasik chat bubble görünümü
    paddingHorizontal: 15,
    paddingVertical: 13,
    ...Shadow.sm,
  },
  characterName: {
    marginLeft: Spacing.xs,
    marginBottom: Spacing.xs,
    fontSize: 11,
    fontWeight: FontWeight.bold,
    color: Colors.textMuted,
  },
  aiText: {
    fontSize: FontSize.sm,
    color: Colors.text,
    lineHeight: 21,
  },

  // Kullanıcı (sağ)
  userRow: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    alignSelf: 'flex-end',
    maxWidth: '80%',
  },
  // Zemin marka geçişi (LinearGradient); düz renk yalnızca yedek
  userBubble: {
    flexShrink: 1,
    backgroundColor: Colors.primary,
    borderRadius: 18,
    borderBottomRightRadius: 5,
    paddingHorizontal: 15,
    paddingVertical: 13,
  },
  userText: {
    fontSize: FontSize.sm,
    color: Colors.surface,
    lineHeight: 21,
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
    backgroundColor: Colors.textDisabled,
  },
});
