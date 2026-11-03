// Pratik (chat) sahnesi. Senaryonun karakteriyle text-based roleplay.
//
// Oturum detay ekranında açılır (POST /api/Sessions) ve sessionId param'ıyla gelir.
// Mesajlar oturumdan okunur (useSession — start mutation cache'i doldurur); ilk
// mesaj senaryonun sabit açılış mesajıdır. Her kullanıcı mesajı POST
// /api/Sessions/{id}/messages ile gider, karakterin cevabı (LLM) güncellenmiş oturumla
// döner. Son rauntta oturum "completed" olur ve Sonuç ekranına geçilir.
//
// Senaryo başlığı/karakteri/bağlam rengi useScenarioDetail'den gelir.

import { router, useLocalSearchParams } from 'expo-router';
import { PaperPlaneTilt, User, X } from 'phosphor-react-native';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
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
  FormError,
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
import { useSendMessage, useSession } from '@/hooks/queries/useSessions';
import {
  toScenarioErrorMessage,
  toSendMessageErrorMessage,
  toSessionErrorMessage,
} from '@/lib/api';
import { contexts } from '@/lib/mockData';
import { getInitials } from '@/lib/utils/initials';
import type { ApiSessionMessage } from '@/types/api/session';

// Yeni başlamış pratikte açılış mesajı, kısa bir "yazıyor" animasyonundan sonra görünür.
const OPENING_TYPING_MS = 1200;
const ENTER_ANIM_MS = 300; // yeni balon giriş animasyonu
// Backend de doğrular; buradaki sınır kullanıcıyı erken durdurmak için.
const MAX_MESSAGE_LENGTH = 500;

// Gönderilmekte olan mesajın geçici id'si (sunucu cevabı gelince gerçek liste gelir).
const PENDING_MESSAGE_ID = 'pending-user-message';

export default function PracticeScreen() {
  const { scenarioId, sessionId } = useLocalSearchParams<{
    scenarioId: string;
    sessionId?: string;
  }>();

  const scenarioQuery = useScenarioDetail(scenarioId);
  const sessionQuery = useSession(sessionId);
  const sendMessage = useSendMessage(sessionId);

  const scenario = scenarioQuery.data;
  const session = sessionQuery.data;

  const context = scenario
    ? contexts.find((c) => c.id === scenario.contextId)
    : undefined;
  const contextColor = context?.color ?? Colors.primary;
  const contextSoftColor = context?.softColor ?? Colors.primarySoft;

  const [inputText, setInputText] = useState('');
  const [pendingText, setPendingText] = useState<string | null>(null);
  const [sendError, setSendError] = useState<string | null>(null);
  // Hiç mesaj atılmamış oturumda açılış balonu animasyonla gelir; devam edilen oturumda hemen.
  const [openingRevealed, setOpeningRevealed] = useState(false);

  const listRef = useRef<FlatList<ApiSessionMessage>>(null);

  const isFreshSession = !!session && session.currentRound === 0;
  useEffect(() => {
    if (!session || openingRevealed) return;
    if (!isFreshSession) {
      setOpeningRevealed(true);
      return;
    }
    const timer = setTimeout(() => setOpeningRevealed(true), OPENING_TYPING_MS);
    return () => clearTimeout(timer);
  }, [session, isFreshSession, openingRevealed]);

  const messages = useMemo<ApiSessionMessage[]>(() => {
    if (!session || !openingRevealed) return [];
    if (pendingText === null) return session.messages;
    return [
      ...session.messages,
      {
        id: PENDING_MESSAGE_ID,
        role: 'user',
        content: pendingText,
        timestamp: new Date().toISOString(),
      },
    ];
  }, [session, openingRevealed, pendingText]);

  const isAITyping = sendMessage.isPending || (!!session && !openingRevealed);
  const isCompleted = session?.status === 'completed';

  const handleSend = useCallback(() => {
    const text = inputText.trim();
    // Boşken, AI yazarken veya pratik bittiğinde gönderme.
    if (!text || isAITyping || isCompleted || !sessionId) return;

    setSendError(null);
    setPendingText(text);
    setInputText('');

    sendMessage.mutate(text, {
      onSuccess: () => setPendingText(null),
      onError: (error) => {
        // Raunt ilerlemedi: mesajı geri ver, kullanıcı düzeltip tekrar gönderebilsin.
        setPendingText(null);
        setInputText(text);
        setSendError(toSendMessageErrorMessage(error));
      },
    });
  }, [inputText, isAITyping, isCompleted, sessionId, sendMessage]);

  const confirmExit = useCallback(() => {
    if (isCompleted) {
      router.back();
      return;
    }
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
  }, [isCompleted]);

  const goToResult = useCallback(() => {
    router.replace({
      pathname: '/(app)/scenarios/result/[scenarioId]',
      params: { scenarioId, sessionId },
    });
  }, [scenarioId, sessionId]);

  // sessionId olmadan bu ekrana gelinmemeli (pratik her zaman oturumla başlar).
  if (!sessionId) {
    return (
      <SafeAreaView style={styles.safeArea} edges={['top']}>
        <View style={styles.fallback}>
          <View style={[styles.fallbackIcon, { backgroundColor: contextSoftColor }]}>
            <User size={48} color={contextColor} weight="fill" />
          </View>
          <Text style={styles.fallbackTitle}>Pratik başlatılamadı</Text>
          <Text style={styles.fallbackText}>
            Senaryo sayfasına dönüp pratiği yeniden başlat.
          </Text>
          <Button variant="primary" onPress={() => router.back()}>
            Geri dön
          </Button>
        </View>
      </SafeAreaView>
    );
  }

  if (scenarioQuery.isPending || sessionQuery.isPending) {
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

  if (scenarioQuery.isError || sessionQuery.isError || !scenario || !session) {
    const message = scenarioQuery.isError
      ? toScenarioErrorMessage(scenarioQuery.error)
      : toSessionErrorMessage(sessionQuery.error);
    return (
      <SafeAreaView style={styles.safeArea} edges={['top']}>
        <View style={styles.flex}>
          <ErrorState
            message={message}
            onRetry={() => {
              scenarioQuery.refetch();
              sessionQuery.refetch();
            }}
            isRetrying={scenarioQuery.isFetching || sessionQuery.isFetching}
          />
        </View>
      </SafeAreaView>
    );
  }

  const totalRounds = session.maxRounds;
  const displayRound = Math.min(session.currentRound + 1, totalRounds);
  const initials = getInitials(scenario.character.name);

  const renderItem = ({ item, index }: { item: ApiSessionMessage; index: number }) => {
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
          <View>
            {sendError ? (
              <View style={styles.errorBar}>
                <FormError message={sendError} />
              </View>
            ) : null}
            <View style={styles.inputBar}>
              <TextInput
                style={styles.input}
                value={inputText}
                onChangeText={(text) => {
                  setInputText(text);
                  if (sendError) setSendError(null);
                }}
                placeholder="Cevabını yaz..."
                placeholderTextColor={Colors.textMuted}
                multiline
                editable={!isAITyping}
                maxLength={MAX_MESSAGE_LENGTH}
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

  // Gönderim hatası (raunt ilerlemedi)
  errorBar: {
    paddingHorizontal: Spacing.md,
    paddingTop: Spacing.sm,
    backgroundColor: Colors.surface,
    borderTopWidth: 1,
    borderTopColor: Colors.border,
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
