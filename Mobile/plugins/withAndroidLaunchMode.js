// Expo config plugin: Android ana Activity'nin launchMode'unu singleTop yapar.
// RevenueCat: varsayılan singleTask, kullanıcı ödeme doğrulaması (banka/SMS) için
// uygulamadan çıkıp döndüğünde satın almayı iptal edebiliyor. singleTop deep link'leri
// (cansayno://) bozmaz — açık Activity'ye onNewIntent ile gelir.
const { withAndroidManifest } = require('expo/config-plugins');

module.exports = function withAndroidLaunchMode(config) {
  return withAndroidManifest(config, (cfg) => {
    const app = cfg.modResults.manifest.application?.[0];
    const main = app?.activity?.find((a) => a.$['android:name'] === '.MainActivity');
    if (main) {
      main.$['android:launchMode'] = 'singleTop';
    }
    return cfg;
  });
};
