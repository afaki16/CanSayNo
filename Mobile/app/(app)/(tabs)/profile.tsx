// Profil tab — placeholder. Avatar + isim/email + çıkış. Detaylı profil sonraki sprint'te.

import { router } from 'expo-router';
import { Crown, SignOut, User } from 'phosphor-react-native';
import { StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { Button } from '@/components/ui';
import { Colors, FontSize, FontWeight, Radius, Spacing } from '@/constants/theme';
import { useAuth } from '@/hooks/useAuth';

export default function ProfileScreen() {
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    await logout();
    router.replace('/');
  };

  const fullName = [user?.firstName, user?.lastName].filter(Boolean).join(' ');

  return (
    <SafeAreaView style={styles.safeArea} edges={['top']}>
      <View style={styles.container}>
        <View style={styles.profileHeader}>
          <View style={styles.avatar}>
            <User size={40} color={Colors.primary} weight="fill" />
          </View>
          <Text style={styles.name}>{fullName || 'Kullanıcı'}</Text>
          <Text style={styles.email}>{user?.email ?? ''}</Text>
        </View>

        <Text style={styles.hint}>Detaylı profil yakında</Text>

        <View style={styles.actions}>
          {/* Geçici: paywall modal'ını test etmek için giriş noktası */}
          <Button
            variant="secondary"
            fullWidth
            onPress={() => router.push('/(app)/paywall')}
            leftIcon={<Crown size={20} color={Colors.surface} weight="fill" />}
          >
            Premium&apos;a Yükselt
          </Button>
          <Button
            variant="outline"
            fullWidth
            onPress={handleLogout}
            leftIcon={<SignOut size={20} color={Colors.primary} />}
          >
            Çıkış Yap
          </Button>
        </View>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: Colors.background,
  },
  container: {
    flex: 1,
    padding: Spacing.lg,
  },
  profileHeader: {
    alignItems: 'center',
    paddingVertical: Spacing.xl,
    gap: Spacing.sm,
  },
  avatar: {
    width: 88,
    height: 88,
    borderRadius: Radius.full,
    backgroundColor: Colors.primarySoft,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.sm,
  },
  name: {
    fontSize: FontSize.xl,
    fontWeight: FontWeight.bold,
    color: Colors.text,
  },
  email: {
    fontSize: FontSize.sm,
    color: Colors.textSecondary,
  },
  hint: {
    textAlign: 'center',
    fontSize: FontSize.sm,
    color: Colors.textMuted,
  },
  actions: {
    marginTop: 'auto',
    gap: Spacing.sm,
  },
});
