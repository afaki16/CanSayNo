// Pratik sonu — skor halkası, kazanılan XP/seri, 5 eksenli beceri kırılımı,
// iyi yapılanlar, gelişim önerileri, koç notu ve "şöyle de diyebilirdin" örneği.
//
// Değerlendirme POST /api/Sessions/{id}/evaluate'ten gelir (sessionId param'ı).
// Backend idempotent: ekran yeniden açıldığında LLM'e gitmeden aynı sonuç döner.
// "Tekrar dene" yeni bir oturum açar (günlük limit burada da geçerli).

import { LinearGradient } from 'expo-linear-gradient';
import { router, useLocalSearchParams } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import {
  ArrowUpRight,
  ChatCircleText,
  CheckCircle,
  Fire,
  Lightbulb,
  Star,
  ThumbsUp,
} from 'phosphor-react-native';
import { ActivityIndicator, ScrollView, StyleSheet, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import Svg, { Circle } from 'react-native-svg';

import { BrandMark, Button, ErrorState, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { useEvaluateSession } from '@/hooks/queries/useSessions';
import { useStartPractice } from '@/hooks/useStartPractice';
import { toEvaluateErrorMessage } from '@/lib/api';
import { EVALUATION_AXES, EVALUATION_AXIS_LABELS } from '@/lib/constants/evaluation';

const RING_SIZE = 140;
const RING_STROKE = 11;
const RING_RADIUS = 62;
const RING_CIRCUMFERENCE = 2 * Math.PI * RING_RADIUS;

function headlineFor(score: number): string {
  if (score >= 80) return 'Harika iş çıkardın! 🎉';
  if (score >= 60) return 'İyi gidiyorsun! 💪';
  return 'Güzel bir başlangıç! 🌱';
}

function axisColor(value: number): string {
  if (value >= 80) return Colors.success;
  if (value >= 60) return Colors.warning;
  return Colors.error;
}

export default function ResultScreen() {
  const { scenarioId, sessionId } = useLocalSearchParams<{
    scenarioId: string;
    sessionId?: string;
  }>();
  const insets = useSafeAreaInsets();
  const { startPractice, isStarting } = useStartPractice();
  const { data: session, isPending, isError, error, refetch, isFetching } =
    useEvaluateSession(sessionId);

  const retry = () => startPractice(scenarioId, { replace: true });
  const backToScenarios = () => router.replace('/(app)/(tabs)/scenarios');

  if (!sessionId) {
    return (
      <View style={[styles.root, styles.centered, { paddingTop: insets.top }]}>
        <Text style={styles.loadingTitle}>Pratik bulunamadı</Text>
        <Button variant="primary" onPress={backToScenarios}>
          Senaryolara dön
        </Button>
      </View>
    );
  }

  if (isPending) {
    return (
      <View style={[styles.root, styles.centered, { paddingTop: insets.top }]}>
        <ActivityIndicator size="large" color={Colors.primary} />
        <Text style={styles.loadingTitle}>Değerlendirmen hazırlanıyor</Text>
        <Text style={styles.loadingText}>
          Cevaplarını beş açıdan inceliyoruz, birkaç saniye sürebilir.
        </Text>
      </View>
    );
  }

  const evaluation = session?.evaluation;
  if (isError || !evaluation) {
    return (
      <View style={[styles.root, { paddingTop: insets.top }]}>
        <View style={styles.errorBody}>
          <ErrorState
            message={toEvaluateErrorMessage(error)}
            onRetry={() => refetch()}
            isRetrying={isFetching}
          />
        </View>
        <View style={[styles.errorFooter, { paddingBottom: insets.bottom + Spacing.md }]}>
          <Button variant="outline" size="lg" fullWidth onPress={backToScenarios}>
            Senaryolara dön
          </Button>
        </View>
      </View>
    );
  }

  const dashOffset = RING_CIRCUMFERENCE * (1 - evaluation.totalScore / 100);

  return (
    <View style={styles.root}>
      <StatusBar style="light" />
      <ScrollView showsVerticalScrollIndicator={false}>
        {/* Kutlama header'ı */}
        <LinearGradient
          colors={[Colors.primary, Colors.primaryDark]}
          start={{ x: 0.3, y: 0 }}
          end={{ x: 0.7, y: 1 }}
          style={[styles.hero, { paddingTop: insets.top + 24 }]}
        >
          <View style={[styles.confetti, styles.confettiA]} />
          <View style={[styles.confetti, styles.confettiB]} />
          <View style={[styles.confetti, styles.confettiC]} />

          <Text style={styles.heroLabel}>PRATİK TAMAMLANDI</Text>

          <View style={styles.ring}>
            <Svg
              width={RING_SIZE}
              height={RING_SIZE}
              style={{ transform: [{ rotate: '-90deg' }] }}
            >
              <Circle
                cx={RING_SIZE / 2}
                cy={RING_SIZE / 2}
                r={RING_RADIUS}
                fill="none"
                stroke="rgba(255,255,255,0.25)"
                strokeWidth={RING_STROKE}
              />
              <Circle
                cx={RING_SIZE / 2}
                cy={RING_SIZE / 2}
                r={RING_RADIUS}
                fill="none"
                stroke="#FFFFFF"
                strokeWidth={RING_STROKE}
                strokeLinecap="round"
                strokeDasharray={RING_CIRCUMFERENCE}
                strokeDashoffset={dashOffset}
              />
            </Svg>
            <View style={styles.ringCenter}>
              <Text style={styles.score}>{evaluation.totalScore}</Text>
              <Text style={styles.scoreUnit}>/ 100 puan</Text>
            </View>
          </View>

          <Text style={styles.headline}>{headlineFor(evaluation.totalScore)}</Text>

          <View style={styles.pillRow}>
            <View style={styles.pill}>
              <Star size={14} color="#FFC93C" weight="fill" />
              <Text style={styles.pillText}>+{evaluation.xpEarned} XP</Text>
            </View>
            <View style={styles.pill}>
              <Fire size={14} color={Colors.secondary} weight="fill" />
              <Text style={styles.pillText}>Seri: {evaluation.streakAfter} gün</Text>
            </View>
          </View>
        </LinearGradient>

        <View style={[styles.body, { paddingBottom: insets.bottom + 24 }]}>
          {/* Beceri kırılımı */}
          <View style={styles.card}>
            <Text style={styles.cardTitle}>Beceri kırılımı</Text>
            <View style={styles.axisList}>
              {EVALUATION_AXES.map((axis) => {
                const value = evaluation.scores[axis] ?? 0;
                return (
                  <View key={axis}>
                    <View style={styles.axisHeader}>
                      <Text style={styles.axisLabel}>
                        {EVALUATION_AXIS_LABELS[axis]}
                      </Text>
                      <Text style={styles.axisValue}>{value}</Text>
                    </View>
                    <View style={styles.axisTrack}>
                      <View
                        style={[
                          styles.axisFill,
                          { width: `${value}%`, backgroundColor: axisColor(value) },
                        ]}
                      />
                    </View>
                  </View>
                );
              })}
            </View>
          </View>

          {/* İyi yaptıkların */}
          {evaluation.strengths.length > 0 ? (
            <View style={[styles.listCard, { backgroundColor: Colors.successSoft }]}>
              <View style={styles.listHeader}>
                <ThumbsUp size={17} color={Colors.success} weight="fill" />
                <Text style={[styles.listTitle, { color: '#0E7A52' }]}>
                  İyi yaptıkların
                </Text>
              </View>
              {evaluation.strengths.map((item) => (
                <View key={item} style={styles.listItem}>
                  <CheckCircle size={18} color={Colors.success} weight="fill" />
                  <Text style={styles.listItemText}>{item}</Text>
                </View>
              ))}
            </View>
          ) : null}

          {/* Bir dahakine dene */}
          {evaluation.improvements.length > 0 ? (
            <View style={[styles.listCard, { backgroundColor: Colors.tipSoft }]}>
              <View style={styles.listHeader}>
                <ArrowUpRight size={17} color={Colors.secondary} weight="fill" />
                <Text style={[styles.listTitle, { color: '#B85427' }]}>
                  Bir dahakine dene
                </Text>
              </View>
              {evaluation.improvements.map((item) => (
                <View key={item} style={styles.listItem}>
                  <Lightbulb size={18} color={Colors.secondary} weight="fill" />
                  <Text style={styles.listItemText}>{item}</Text>
                </View>
              ))}
            </View>
          ) : null}

          {/* Koç notu */}
          <View style={styles.coachCard}>
            <View style={styles.coachHeader}>
              <BrandMark size={26} />
              <Text style={styles.coachTitle}>Koç notu</Text>
            </View>
            <Text style={styles.coachText}>{evaluation.coachNote}</Text>
          </View>

          {/* Şöyle de diyebilirdin */}
          {evaluation.idealResponseExample ? (
            <View style={styles.coachCard}>
              <View style={styles.coachHeader}>
                <ChatCircleText size={22} color={Colors.primary} weight="fill" />
                <Text style={styles.coachTitle}>Şöyle de diyebilirdin</Text>
              </View>
              <Text style={styles.coachText}>“{evaluation.idealResponseExample}”</Text>
            </View>
          ) : null}

          {/* Aksiyonlar */}
          <View style={styles.ctaRow}>
            <View style={styles.ctaSecondary}>
              <Button
                variant="outline"
                size="md"
                fullWidth
                loading={isStarting}
                onPress={retry}
              >
                Tekrar dene
              </Button>
            </View>
            <View style={styles.ctaPrimary}>
              <Button variant="primary" size="md" fullWidth onPress={backToScenarios}>
                Senaryolara dön
              </Button>
            </View>
          </View>
        </View>
      </ScrollView>
      {/* Durum çubuğu şeridi: kaydırınca içerik saat/ikonların altına girmesin.
          Rengi hero gradyanının üst rengiyle aynı, en üstteyken fark edilmez. */}
      <View style={[styles.statusBarScrim, { height: insets.top }]} pointerEvents="none" />
    </View>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  statusBarScrim: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    backgroundColor: Colors.primary,
  },
  centered: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: Spacing.lg,
    gap: Spacing.md,
  },
  errorBody: {
    flex: 1,
  },
  errorFooter: {
    paddingHorizontal: Spacing.lg,
  },
  loadingTitle: {
    marginTop: Spacing.sm,
    fontSize: FontSize.lg,
    fontWeight: FontWeight.bold,
    color: Colors.text,
    textAlign: 'center',
  },
  loadingText: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
    textAlign: 'center',
  },

  // Header
  hero: {
    alignItems: 'center',
    paddingHorizontal: Spacing.lg,
    paddingBottom: 30,
    overflow: 'hidden',
  },
  confetti: {
    position: 'absolute',
  },
  confettiA: {
    top: 40,
    left: 30,
    width: 10,
    height: 10,
    borderRadius: 3,
    backgroundColor: '#FFC93C',
    transform: [{ rotate: '20deg' }],
  },
  confettiB: {
    top: 80,
    right: 36,
    width: 8,
    height: 8,
    borderRadius: 4,
    backgroundColor: Colors.secondary,
  },
  confettiC: {
    top: 130,
    left: 50,
    width: 7,
    height: 7,
    borderRadius: 2,
    backgroundColor: '#7BE0B0',
    transform: [{ rotate: '45deg' }],
  },
  heroLabel: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.bold,
    letterSpacing: 0.56,
    color: 'rgba(255,255,255,0.85)',
  },
  ring: {
    width: RING_SIZE,
    height: RING_SIZE,
    marginTop: 18,
    marginBottom: 14,
  },
  ringCenter: {
    ...StyleSheet.absoluteFillObject,
    alignItems: 'center',
    justifyContent: 'center',
  },
  score: {
    fontSize: 44,
    lineHeight: 48,
    fontWeight: FontWeight.black,
    color: '#FFFFFF',
  },
  scoreUnit: {
    fontSize: FontSize.xs,
    fontWeight: FontWeight.semibold,
    color: 'rgba(255,255,255,0.8)',
  },
  headline: {
    fontSize: FontSize.xl,
    fontWeight: FontWeight.black,
    letterSpacing: -0.44,
    color: '#FFFFFF',
  },
  pillRow: {
    flexDirection: 'row',
    gap: Spacing.sm,
    marginTop: Spacing.md,
  },
  pill: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    paddingHorizontal: 13,
    paddingVertical: 7,
    borderRadius: Radius.full,
    backgroundColor: 'rgba(255,255,255,0.18)',
  },
  pillText: {
    fontSize: 13,
    fontWeight: FontWeight.extrabold,
    color: '#FFFFFF',
  },

  // Gövde
  body: {
    paddingHorizontal: 20,
    paddingTop: 20,
    gap: 14,
  },
  card: {
    padding: 18,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: Colors.divider,
    backgroundColor: Colors.surface,
  },
  cardTitle: {
    fontSize: 15,
    fontWeight: FontWeight.extrabold,
    letterSpacing: -0.15,
    color: Colors.text,
    marginBottom: Spacing.md,
  },
  axisList: {
    gap: 14,
  },
  axisHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 6,
  },
  axisLabel: {
    fontSize: 13,
    fontWeight: FontWeight.semibold,
    color: Colors.textSecondary,
  },
  axisValue: {
    fontSize: 13,
    fontWeight: FontWeight.extrabold,
    color: Colors.text,
  },
  axisTrack: {
    height: 8,
    borderRadius: Radius.full,
    backgroundColor: Colors.track,
    overflow: 'hidden',
  },
  axisFill: {
    height: '100%',
    borderRadius: Radius.full,
  },
  listCard: {
    padding: 18,
    borderRadius: 18,
    gap: 10,
  },
  listHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 7,
    marginBottom: 2,
  },
  listTitle: {
    fontSize: FontSize.sm,
    fontWeight: FontWeight.extrabold,
  },
  listItem: {
    flexDirection: 'row',
    gap: 9,
  },
  listItemText: {
    flex: 1,
    fontSize: FontSize.sm,
    lineHeight: 20,
    color: Colors.text,
  },
  coachCard: {
    padding: 18,
    borderRadius: 18,
    backgroundColor: Colors.ink,
  },
  coachHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.sm,
    marginBottom: 10,
  },
  coachTitle: {
    fontSize: 13,
    fontWeight: FontWeight.extrabold,
    color: '#FFFFFF',
  },
  coachText: {
    fontSize: FontSize.sm,
    lineHeight: 22,
    color: 'rgba(255,255,255,0.85)',
  },
  ctaRow: {
    flexDirection: 'row',
    gap: 10,
    marginTop: Spacing.xs,
  },
  ctaSecondary: {
    flex: 1,
  },
  ctaPrimary: {
    flex: 1.4,
  },
});
