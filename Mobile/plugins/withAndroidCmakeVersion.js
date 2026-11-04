// Expo config plugin: Android native build'i daha yeni bir CMake'e sabitler.
// AGP varsayılanı CMake 3.22.1 (ninja 1.10) Windows'ta 260 karakter yol sınırını
// aşamıyor; react-native-keyboard-controller'ın C++ obje yolları ~290 karakter.
// 3.31.x ile gelen ninja 1.12 uzun yolları destekler — Windows'ta
// LongPathsEnabled=1 (kayıt defteri) de açık olmalı. AGP sürümü ilk build'de indirir.
const { withAppBuildGradle } = require('expo/config-plugins');

const CMAKE_VERSION = '3.31.6';
const MARKER = '// withAndroidCmakeVersion';

module.exports = function withAndroidCmakeVersion(config) {
  return withAppBuildGradle(config, (cfg) => {
    if (cfg.modResults.contents.includes(MARKER)) {
      return cfg;
    }
    cfg.modResults.contents = cfg.modResults.contents.replace(
      /\nandroid\s*\{\n/,
      `\nandroid {\n    ${MARKER}\n    externalNativeBuild {\n        cmake {\n            version "${CMAKE_VERSION}"\n        }\n    }\n`,
    );
    return cfg;
  });
};
