import { ref, computed } from 'vue'

const THEME_STORAGE_KEY = 'app-theme'
type Theme = 'light' | 'dark'

// Module-level state — singleton
const currentTheme = ref<Theme>('light')
let initialized = false

/**
 * Theme composable — light/dark switching, localStorage persist, Vuetify sync.
 *
 * Kullanım:
 *   const { theme, toggleTheme, isDark } = useTheme()
 *
 * Yan etkiler:
 *  - <html>'e 'dark' class'ı ekler/çıkarır (Tailwind dark mode için)
 *  - Vuetify global theme'ini senkronize eder ($syncVuetifyTheme plugin helper'ı)
 *  - localStorage'a 'app-theme' anahtarıyla kaydeder
 */
export const useTheme = () => {
  const initTheme = () => {
    if (initialized || typeof window === 'undefined') return
    initialized = true

    try {
      const stored = localStorage.getItem(THEME_STORAGE_KEY) as Theme | null
      if (stored === 'light' || stored === 'dark') {
        currentTheme.value = stored
      }
    } catch {
      // localStorage erişilemiyorsa default'a düş
    }

    applyTheme(currentTheme.value)
  }

  const applyTheme = (theme: Theme) => {
    if (typeof document === 'undefined') return

    // 1. <html>'e dark class'ı ekle/çıkar (Tailwind dark mode strategy)
    const root = document.documentElement
    if (theme === 'dark') {
      root.classList.add('dark')
    } else {
      root.classList.remove('dark')
    }

    // 2. Vuetify global theme'ini senkronize et
    // $syncVuetifyTheme plugin tarafından provide edilen helper
    try {
      const nuxtApp = useNuxtApp()
      const syncFn = nuxtApp.$syncVuetifyTheme as ((t: Theme) => void) | undefined
      if (typeof syncFn === 'function') {
        syncFn(theme)
      }
    } catch {
      // useNuxtApp henüz hazır değilse (init sırasında) sessizce geç
      // Vuetify plugin kendi defaultTheme'ini localStorage'dan okuyor zaten
    }
  }

  const setTheme = (theme: Theme) => {
    currentTheme.value = theme
    applyTheme(theme)
    try {
      localStorage.setItem(THEME_STORAGE_KEY, theme)
    } catch {
      // sessizce geç
    }
  }

  const toggleTheme = () => {
    setTheme(currentTheme.value === 'dark' ? 'light' : 'dark')
  }

  const isDark = computed(() => currentTheme.value === 'dark')
  const isLight = computed(() => currentTheme.value === 'light')

  if (import.meta.client) {
    initTheme()
  }

  return {
    theme: currentTheme,
    isDark,
    isLight,
    setTheme,
    toggleTheme
  }
}
