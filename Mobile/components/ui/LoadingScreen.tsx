// Tam ekran yükleme göstergesi. App açılışta rehydrate sürerken gösterilir.

import { ActivityIndicator, StyleSheet, View } from 'react-native';

import { Colors } from '@/constants/theme';

export function LoadingScreen() {
  return (
    <View style={styles.container}>
      <ActivityIndicator
        size="large"
        color={Colors.primary}
        accessibilityLabel="Yükleniyor"
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.background,
  },
});
