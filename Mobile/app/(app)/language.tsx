// Dil seçimi — Profil'deki ayarlar kartından açılır. Seçim cihazda saklanır
// (setAppLanguage); kök layout sorguları yeni dilde yeniden çeker, hatırlatmalar
// useReminderSync tarafından yeni dilde yeniden kurulur.

import { router } from 'expo-router';
import { ArrowLeft, Check } from 'phosphor-react-native';
import { useTranslation } from 'react-i18next';
import { Pressable, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { IconButton, Text } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { APP_LANGUAGES, setAppLanguage, type AppLanguage } from '@/lib/i18n';

export default function LanguageScreen() {
  const { t, i18n } = useTranslation();

  const onSelect = (language: AppLanguage) => {
    if (language !== i18n.language) {
      void setAppLanguage(language);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.topBar}>
          <IconButton
            icon={ArrowLeft}
            bordered
            onPress={() => router.back()}
            accessibilityLabel={t('common.back')}
          />
        </View>

        <View style={styles.header}>
          <Text style={styles.title}>{t('common.language.title')}</Text>
          <Text style={styles.subtitle}>{t('common.language.subtitle')}</Text>
        </View>

        <View style={styles.card}>
          {APP_LANGUAGES.map((language, index) => {
            const selected = language === i18n.language;
            return (
              <Pressable
                key={language}
                onPress={() => onSelect(language)}
                accessibilityRole="radio"
                accessibilityState={{ selected }}
                style={({ pressed }) => [
                  styles.row,
                  index < APP_LANGUAGES.length - 1 && styles.rowDivider,
                  pressed && styles.pressed,
                ]}
              >
                <Text style={[styles.rowTitle, selected && styles.rowTitleSelected]}>
                  {t(`common.language.${language}`)}
                </Text>
                {selected ? <Check size={20} color={Colors.primary} weight="bold" /> : null}
              </Pressable>
            );
          })}
        </View>

        <Text style={styles.note}>{t('common.language.note')}</Text>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: 28,
    paddingTop: Spacing.md,
    paddingBottom: Spacing.xl,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  header: {
    marginTop: 26,
    marginBottom: 30,
  },
  title: {
    fontSize: 30,
    fontWeight: FontWeight.black,
    letterSpacing: -0.9,
    color: Colors.text,
  },
  subtitle: {
    marginTop: Spacing.sm,
    fontSize: FontSize.base,
    color: Colors.textSecondary,
  },
  card: {
    borderRadius: Radius.lg,
    backgroundColor: Colors.surface,
    borderWidth: 1,
    borderColor: Colors.border,
    overflow: 'hidden',
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 15,
    paddingHorizontal: Spacing.md,
  },
  rowDivider: {
    borderBottomWidth: 1,
    borderBottomColor: Colors.chip,
  },
  pressed: {
    backgroundColor: Colors.chip,
  },
  rowTitle: {
    flex: 1,
    fontSize: FontSize.base,
    fontWeight: FontWeight.semibold,
    color: Colors.text,
  },
  rowTitleSelected: {
    fontWeight: FontWeight.bold,
  },
  note: {
    marginTop: Spacing.md,
    fontSize: FontSize.sm,
    color: Colors.textMuted,
  },
});
