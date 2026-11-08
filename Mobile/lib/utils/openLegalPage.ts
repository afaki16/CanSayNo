// Yasal sayfayı uygulama içi tarayıcıda açar (Android: Custom Tabs, iOS: SFSafariViewController);
// kullanıcı uygulamadan çıkmış olmaz. Tarayıcı açılamazsa sistem tarayıcısına düşer.

import * as WebBrowser from 'expo-web-browser';
import { Linking } from 'react-native';

import { Colors } from '@/constants/theme';
import { LEGAL_URLS, type LegalPage } from '@/lib/constants/legal';

export async function openLegalPage(page: LegalPage): Promise<void> {
  const url = LEGAL_URLS[page];
  try {
    await WebBrowser.openBrowserAsync(url, {
      toolbarColor: Colors.background,
      controlsColor: Colors.primary,
      enableBarCollapsing: true,
    });
  } catch {
    await Linking.openURL(url).catch(() => undefined);
  }
}
