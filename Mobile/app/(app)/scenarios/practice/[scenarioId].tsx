// Pratik (chat) sahnesi. Senaryonun karakteriyle text-based roleplay.
//
// Senaryo başlığı/karakteri API'den (useScenarioDetail) gelir; AI cevapları HÂLÂ
// mock script'ten (getAIResponseScript) sırayla geliyor — gerçek LLM entegrasyonu
// sonraki sprint. Script'ler senaryo id'siyle anahtarlı, backend'in uzun id'leriyle
// eşleşmeleri şart (ör. is_001_cuma_mesaji).
//
// Oturum detay ekranında açılır (POST /api/Sessions) ve sessionId param'ıyla gelir;
// mesajlar henüz backend'e yazılmıyor, sessionId Sonuç ekranına taşınır.

import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { PaperPlaneTilt, User, X } from 'phosphor-react-native';
import { useCallback, useRef, useState } from 'react';
import {
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  TextInput,
  View,
} from 'react-native';
import Animated, { FadeInDown } from 'react-native-reanimated';
import { SafeAreaView } from 'react-native-safe-area-context';

import {
  Button,
  ErrorState,
  IconButton,
  MessageBubble,
  Skeleton,
  Text,
} from '@/components/ui';
import {
  Colors,
  Fonts,
  FontSize,
  FontWeight,
  Radius,
  Shadow,
  Spacing,
} from '@/constants/theme';
import { useScenarioDetail } from '@/hooks/queries/useScenarios';
import { toScenarioErrorMessage } from '@/lib/api';
import { contexts, getAIResponseScript } from '@/lib/mockData';
import { getInitials } from '@/lib/utils/initials';

// Zamanlama sabitleri — gerçekçi "yazıyor" hissi için.
const OPENING_DELAY_MS = 500; // mount sonrası ilk typing'in başlama gecikmesi
const AI_TYPING_DELAY_MS = 1500; // AI cevabı vermeden önce typing süresi
const ENTER_ANIM_MS = 300; // yeni balon giriş animasyonu

interface Message {
  id: string;
  role: 'ai' | 'user';
  content: string;
  timestamp: number;
}

let messageCounter = 0;
function nextMessageId(): string {
  messageCounter += 1;
  return `m_${messageCounter}`;
}

export default function PracticeScreen() {
  const { scenarioId, sessionId } = useLocalSearchParams<{
    scenarioId: string;
    sessionId?: string;
  }>();
  const {
    data: scenario,
    isPending,
    isError,
    error,
    refetch,
    isFetching,
  } = useScenarioDetail(scenarioId);

  const script = getAIResponseScript(scenarioId);
  const context = scenario
    ? contexts.find((c) => c.id === scenario.contextId)
    : undefined;

  const contextColor = context?.color ?? Colors.primary;
  const contextSoftColor = context?.softColor ?? Colors.primarySoft;
  const totalRounds = script?.responses.length ?? 0;

  const [messages, setMessages] = useState<Message[]>([]);
  const [currentRound, setCurrentRound] = useState(0);
  const [inputText, setInputText] = useState('');
  const [isAITyping, setIsAITyping] = useState(false);
  const [isCompleted, setIsCompleted] = useState(false);

  const listRef = useRef<FlatList<Message>>(null);
  const timersRef = useRef<ReturnType<typeof setTimeout>[]>([]);

  const schedule = useCallback((fn: () => void, ms: number) => {
    const id = setTimeout(fn, ms);
    timersRef.current.push(id);
  }, []);

  const clearTimers = useCallback(() => {
    timersRef.current.forEach(clearTimeout);
    timersRef.current = [];
  }, []);

  const addMessage = useCallback((role: 'ai' | 'user', content: string) => {
    setMessages((prev) => [
      ...prev,
      { id: nextMessageId(), role, content, timestamp: Date.now() },
    ]);
  }, []);

  // Ekran her odaklandığında state'i sıfırla ve açılış mesajını planla.
  // (Tamamlanmış pratiğe geri dönülürse temiz başlasın.)
  useFocusEffect(
    useCallback(() => {
      if (!script) return;

      setMessages([]);
      setCurrentRound(0);
      setInputText('');
      setIsCompleted(false);
      setIsAITyping(true);

      schedule(() => {
        addMessage('ai', script.openingMessage);
        setIsAITyping(false);
      }, OPENING_DELAY_MS + AI_TYPING_DELAY_MS);

      return () => {
        clearTimers();
        setIsAITyping(false);
      };
      // script referansı senaryoya göre sabit; scenarioId değişimini yakalamak yeterli.
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [scenarioId]),
  );

  const handleSend = useCallback(() => {
    const text = inputText.trim();
    // Boşken, AI yazarken veya pratik bittiğinde gönderme.
    if (!text || isAITyping || isCompleted || !script) return;

    addMessage('user', text);
    setInputText('');
    setIsAITyping(true);

    const roundAtSend = currentRound;
    schedule(() => {
      if (roundAtSend < script.responses.length) {
        addMessage('ai', script.responses[roundAtSend]);
        setCurrentRound(roundAtSend + 1);
      } else {
        addMessage('ai', script.closingMessage);
        setIsCompleted(true);
      }
      setIsAITyping(false);
    }, AI_TYPING_DELAY_MS);
  }, [inputText, isAITyping, isCompleted, script, currentRound, addMessage, schedule]);

  const confirmExit = useCallback(() => {
    Alert.alert(
      'Pratiği bırak',
      'Pratiği bırakmak istediğine emin misin? İlerlemen kaybolacak.',
      [
        { text: 'Vazgeç', style: 'cancel' },
        {
          text: 'Çık',
          style: 'destructive',
          onPress: () => router.back(),
        },
      ],
    );
  }, []);

  const goToResult = useCallback(() => {
    router.replace({
      pathname: '/(app)/scenarios/result/[scenarioId]',
      params: { scenarioId, sessionId },
    });
  }, [scenarioId, sessionId]);

  if (isPending) {
    return (
      <SafeAreaView style={styles.safeArea} edges={['top']}>
        <View style={[styles.header, { backgroundColor: contextSoftColor }]}>
          <View style={styles.headerSide} />
          <View style={styles.headerCenter}>
            <Skeleton width={40} height={40} borderRadius={Radius.full} />
            <View style={styles.headerInfo}>
              <Skeleton width={120} height={16} />
            </View>
          </View>
          <View style={styles.headerSide} />
        </View>
        <View style={styles.listContent}>
          <Skeleton height={72} borderRadius={Radius.lg} />
        </View>
      </SafeAreaView>
    );
  }

  if (isError) {
    return (
      <SafeAreaView style={styles.safeArea} edges={['top']}>
        <View style={styles.flex}>
          <ErrorState
            message={toScenarioErrorMessage(error)}
            onRetry={() => refetch()}
            isRetrying={isFetching}
          />
        </View>
      </SafeAreaView>
    );
  }

  // Fallback: senaryo/script yoksa pratik hazır değil.
  if (!scenario || !script) {
    return (
      <SafeAreaView style={styles.safeArea} edges={['top']}>
        <View style={styles.fallback}>
          <View style={[styles.fallbackIcon, { backgroundColor: contextSoftColor }]}>
            <User size={48} color={contextColor} weight="fill" />
          </View>
          <Text style={styles.fallbackTitle}>Pratik henüz hazır değil</Text>
          <Text style={styles.fallbackText}>
            Bu senaryo için pratik henüz hazırlanmadı.
          </Text>
          <Button variant="primary" onPress={() => router.back()}>
            Geri dön
          </Button>
        </View>
      </SafeAreaView>
    );
  }

  const displayRound = Math.min(currentRound + 1, totalRounds);
  const initials = getInitials(scenario.character.name);

  const renderItem = ({ item, index }: { item: Message; index: number }) => {
    const isAI = item.role === 'ai';
    const prev = index > 0 ? messages[index - 1] : undefined;
    // Avatar: ilk AI mesajında veya önceki mesaj AI değilse göster (gruplama).
    const showAvatar = isAI && (!prev || prev.role !== 'ai');
    // Karakter ismi sadece ilk mesajda (açılış balonu).
    const characterName = isAI && index === 0 ? scenario.character.name : undefined;

    return (
      <Animated.View
        entering={FadeInDown.duration(ENTER_ANIM_MS)}
        style={styles.messageWrapper}
      >
        <MessageBubble
          role={item.role}
          content={item.content}
          characterName={characterName}
          showAvatar={showAvatar}
          contextColor={contextColor}
          avatarLabel={initials}
        />
      </Animated.View>
    );
  };

  const sendDisabled = inputText.trim().length === 0 || isAITyping || isCompleted;

  return (
    <SafeAreaView
      style={[styles.safeArea, { backgroundColor: contextSoftColor }]}
      edges={['top']}
    >
      {/* Header */}
      <View style={[styles.header, { backgroundColor: contextSoftColor }]}>
        <IconButton
          icon={X}
          size={38}
          onPress={confirmExit}
          accessibilityLabel="Pratiği bırak"
        />

        <View style={styles.headerCenter}>
          <View style={[styles.headerAvatar, { backgroundColor: contextColor }]}>
            <Text style={styles.headerAvatarText}>{initials}</Text>
          </View>
          <View style={styles.headerInfo}>
            <Text style={styles.headerName} numberOfLines={1}>
              {scenario.character.name}
            </Text>
            <Text style={styles.headerRole} numberOfLines={1}>
              {scenario.character.role}
            </Text>
          </View>
        </View>

        <View style={styles.roundChip}>
          <Text style={[styles.roundChipText, { color: contextColor }]}>
            {displayRound}/{totalRounds}
          </Text>
        </View>
      </View>

      <KeyboardAvoidingView
        style={styles.chatArea}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      >
        <FlatList
          ref={listRef}
          data={messages}
          renderItem={renderItem}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.listContent}
          showsVerticalScrollIndicator={false}
          onContentSizeChange={() =>
            listRef.current?.scrollToEnd({ animated: true })
          }
          ListFooterComponent={
            isAITyping ? (
              <View style={styles.messageWrapper}>
                <MessageBubble
                  role="ai"
                  showAvatar
                  contextColor={contextColor}
                  avatarLabel={initials}
                  isTyping
                />
              </View>
            ) : null
          }
        />

        {/* Alt alan: tamamlandıysa sonuca geçiş, değilse input */}
        {isCompleted ? (
          <View style={styles.completedBar}>
            <Text style={styles.completedText}>
              🎉 Pratik tamamlandı! Bakalım nasıl geçti.
            </Text>
            <Button variant="primary" size="lg" fullWidth onPress={goToResult}>
              Sonucunu gör
            </Button>
          </View>
        ) : (
          <View style={styles.inputBar}>
            <TextInput
              style={styles.input}
              value={inputText}
              onChangeText={setInputText}
              placeholder="Cevabını yaz..."
              placeholderTextColor={Colors.textMuted}
              multiline
              editable={!isAITyping}
              maxLength={500}
            />
            <Pressable
              onPress={handleSend}
              disabled={sendDisabled}
              accessibilityRole="button"
              accessibilityLabel="Gönder"
              accessibilityState={{ disabled: sendDisabled }}
              style={[
                styles.sendButton,
                sendDisabled ? styles.sendButtonDisabled : Shadow.primary,
              ]}
            >
              <PaperPlaneTilt size={20} color={Colors.surface} weight="fill" />
            </Pressable>
          </View>
        )}
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

// Chat zemini — tasarımda diğer ekranlardan biraz daha soğuk gri.
const CHAT_BACKGROUND = '#F4F6F9';

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: CHAT_BACKGROUND,
  },
  flex: {
    flex: 1,
  },
  chatArea: {
    flex: 1,
    backgroundColor: CHAT_BACKGROUND,
  },

  // Header
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: Spacing.md,
    paddingTop: Spacing.sm,
    paddingBottom: 14,
    gap: 12,
  },
  headerSide: {
    minWidth: 40,
    height: 40,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerCenter: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.sm,
  },
  headerAvatar: {
    width: 42,
    height: 42,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerAvatarText: {
    fontSize: 15,
    fontWeight: FontWeight.black,
    color: Colors.surface,
  },
  headerInfo: {
    flex: 1,
  },
  headerName: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.16,
    color: Colors.text,
  },
  headerRole: {
    fontSize: FontSize.xs,
    color: Colors.textSecondary,
  },
  roundChip: {
    backgroundColor: Colors.surface,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: Radius.full,
  },
  roundChipText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.extrabold,
  },

  // List
  listContent: {
    paddingHorizontal: Spacing.md,
    paddingVertical: 18,
    gap: 14,
  },
  messageWrapper: {
    // gap (listContent) balonlar arası boşluğu yönetir; wrapper tam genişlik.
    width: '100%',
  },

  // Input bar
  inputBar: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    gap: 10,
    paddingHorizontal: Spacing.md,
    paddingVertical: 12,
    backgroundColor: Colors.surface,
    borderTopWidth: 1,
    borderTopColor: Colors.border,
  },
  input: {
    flex: 1,
    minHeight: 46,
    maxHeight: 110, // ~3 satır, sonrası iç scroll
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: 22,
    paddingHorizontal: Spacing.md,
    paddingTop: 13,
    paddingBottom: 13,
    fontSize: FontSize.sm,
    fontFamily: Fonts.regular,
    color: Colors.text,
    backgroundColor: CHAT_BACKGROUND,
  },
  sendButton: {
    width: 46,
    height: 46,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.primary,
  },
  sendButtonDisabled: {
    backgroundColor: Colors.textDisabled,
  },

  // Completed
  completedBar: {
    padding: Spacing.md,
    backgroundColor: Colors.surface,
    borderTopWidth: 1,
    borderTopColor: Colors.border,
    gap: Spacing.md,
  },
  completedText: {
    fontSize: FontSize.sm,
    color: Colors.success,
    fontWeight: FontWeight.bold,
    textAlign: 'center',
  },

  // Fallback
  fallback: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: Spacing.lg,
    gap: Spacing.md,
  },
  fallbackIcon: {
    width: 100,
    height: 100,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  fallbackTitle: {
    fontSize: FontSize.xl,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  fallbackText: {
    fontSize: FontSize.base,
    color: Colors.textSecondary,
    textAlign: 'center',
  },
});
