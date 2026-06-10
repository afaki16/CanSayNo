// Pratik (chat) sahnesi. Senaryonun karakteriyle text-based roleplay; AI cevapları
// mock script'ten (getAIResponseScript) sırayla gelir. 3-4 raunt sonunda kapanış mesajı
// + "Pratik Tamamlandı" banner'ı gösterilir. Backend bağlanınca mock yerini API'ye bırakır.

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
  Text,
  TextInput,
  View,
} from 'react-native';
import Animated, { FadeInDown } from 'react-native-reanimated';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button, MessageBubble } from '@/components/ui';
import {
  Colors,
  FontSize,
  FontWeight,
  Radius,
  Spacing,
} from '@/constants/theme';
import {
  getAIResponseScript,
  getScenarioById,
  mockContexts,
} from '@/lib/mockData';

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
  const { scenarioId } = useLocalSearchParams<{ scenarioId: string }>();
  const scenario = getScenarioById(scenarioId);
  const script = getAIResponseScript(scenarioId);
  const context = scenario
    ? mockContexts.find((c) => c.id === scenario.contextId)
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

  const goToScenarios = useCallback(() => {
    router.replace('/(app)/(tabs)/scenarios');
  }, []);

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
            Geri Dön
          </Button>
        </View>
      </SafeAreaView>
    );
  }

  const displayRound = Math.min(currentRound + 1, totalRounds);

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
        />
      </Animated.View>
    );
  };

  const sendDisabled = inputText.trim().length === 0 || isAITyping || isCompleted;

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      {/* Header */}
      <View style={[styles.header, { backgroundColor: contextSoftColor }]}>
        <Pressable
          onPress={confirmExit}
          accessibilityRole="button"
          accessibilityLabel="Pratiği bırak"
          hitSlop={8}
          style={styles.headerSide}
        >
          <X size={22} color={Colors.text} weight="bold" />
        </Pressable>

        <View style={styles.headerCenter}>
          <View style={[styles.headerAvatar, { backgroundColor: contextColor }]}>
            <User size={20} color={Colors.surface} weight="fill" />
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

        <View style={styles.headerSide}>
          <View style={styles.roundChip}>
            <Text style={styles.roundChipText}>
              {displayRound}/{totalRounds}
            </Text>
          </View>
        </View>
      </View>

      <KeyboardAvoidingView
        style={styles.flex}
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
                <MessageBubble role="ai" showAvatar contextColor={contextColor} isTyping />
              </View>
            ) : null
          }
        />

        {/* Alt alan: tamamlandıysa banner, değilse input */}
        {isCompleted ? (
          <View style={styles.completedBar}>
            <View style={styles.completedBanner}>
              <Text style={styles.completedText}>
                🎉 Pratik tamamlandı! Hayır demeyi pratik ettiğin için tebrikler.
              </Text>
            </View>
            <Button variant="primary" size="lg" fullWidth onPress={goToScenarios}>
              Senaryolara Dön
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
                {
                  backgroundColor: sendDisabled ? Colors.border : Colors.primary,
                  opacity: sendDisabled ? 0.5 : 1,
                },
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

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  flex: {
    flex: 1,
  },

  // Header
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: Spacing.md,
    paddingVertical: Spacing.sm,
    gap: Spacing.sm,
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
    width: 40,
    height: 40,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerInfo: {
    flex: 1,
  },
  headerName: {
    fontSize: FontSize.base,
    fontWeight: FontWeight.semibold,
    color: Colors.text,
  },
  headerRole: {
    fontSize: FontSize.xs,
    color: Colors.textSecondary,
  },
  roundChip: {
    backgroundColor: Colors.surface,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
    borderRadius: Radius.full,
  },
  roundChipText: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.bold,
    color: Colors.textSecondary,
  },

  // List
  listContent: {
    paddingHorizontal: Spacing.md,
    paddingVertical: Spacing.lg,
    gap: Spacing.md,
  },
  messageWrapper: {
    // gap (listContent) balonlar arası boşluğu yönetir; wrapper tam genişlik.
    width: '100%',
  },

  // Input bar
  inputBar: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    gap: Spacing.sm,
    padding: Spacing.md,
    backgroundColor: Colors.surface,
    borderTopWidth: 1,
    borderTopColor: Colors.border,
  },
  input: {
    flex: 1,
    minHeight: 44,
    maxHeight: 110, // ~3 satır, sonrası iç scroll
    borderWidth: 1,
    borderColor: Colors.border,
    borderRadius: Radius.lg,
    paddingHorizontal: Spacing.md,
    paddingTop: Spacing.sm,
    paddingBottom: Spacing.sm,
    fontSize: FontSize.base,
    color: Colors.text,
    backgroundColor: Colors.background,
  },
  sendButton: {
    width: 44,
    height: 44,
    borderRadius: Radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },

  // Completed
  completedBar: {
    padding: Spacing.md,
    backgroundColor: Colors.surface,
    borderTopWidth: 1,
    borderTopColor: Colors.border,
    gap: Spacing.md,
  },
  completedBanner: {
    backgroundColor: Colors.successSoft,
    borderRadius: Radius.lg,
    padding: Spacing.md,
  },
  completedText: {
    fontSize: FontSize.sm,
    color: Colors.success,
    fontWeight: FontWeight.medium,
    lineHeight: FontSize.sm * 1.5,
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
