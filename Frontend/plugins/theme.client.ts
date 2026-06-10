/**
 * Theme initialization plugin.
 *
 * Uygulama açılışında çalışır, localStorage'dan tema okur,
 * <html>'e 'dark' class'ı ekler. Bu sayede sayfa render edilmeden
 * önce doğru tema uygulanır (Flash of Unstyled Content önlenir).
 */
export default defineNuxtPlugin(() => {
  if (typeof window === 'undefined') return

  // composable'ı çağırmak init'i tetikler
  useTheme()
})
