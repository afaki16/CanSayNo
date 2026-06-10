import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { mdi } from 'vuetify/iconsets/mdi'
import { useAppData } from '../composables/useAppData'

/**
 * Vuetify plugin — light/dark theme entegrasyonu.
 *
 * Tema renkleri bizim CSS variables ile AYNI hex değerlerini kullanıyor
 * (theme.css ile uyum). Vuetify kendi MD3 theme sistemini kullandığı için
 * burada hex statik tanımlı; runtime'da useTheme() ile senkronize edilir.
 *
 * data.json'daki primary gradient'i opsiyonel olarak primary rengi override edebilir.
 */

// Material 3 light theme — theme.css :root ile aynı palette
const LIGHT_THEME_BASE = {
  colors: {
    background: '#faf8ff',
    surface: '#faf8ff',
    'surface-bright': '#faf8ff',
    'surface-light': '#ffffff',
    'surface-variant': '#dae2fd',
    'on-surface-variant': '#3c4a42',
    primary: '#006c49',
    'primary-darken-1': '#005236',
    secondary: '#855300',
    'secondary-darken-1': '#653e00',
    error: '#ba1a1a',
    info: '#3b82f6',
    success: '#006c49',
    warning: '#fea619',
    // Bizim tema sistemindeki ek tonlar
    'surface-container-lowest': '#ffffff',
    'surface-container-low': '#f2f3ff',
    'surface-container': '#eaedff',
    'surface-container-high': '#e2e7ff',
    'surface-container-highest': '#dae2fd',
    outline: '#6c7a71',
    'outline-variant': '#bbcabf'
  }
}

// Material 3 dark theme — theme.css .dark ile aynı palette
const DARK_THEME_BASE = {
  dark: true,
  colors: {
    background: '#0b1326',
    surface: '#0b1326',
    'surface-bright': '#1a2235',
    'surface-light': '#1e293b',
    'surface-variant': '#1e293b',
    'on-surface-variant': '#94a3b8',
    primary: '#4edea3',
    'primary-darken-1': '#10b981',
    secondary: '#fea619',
    'secondary-darken-1': '#855300',
    error: '#ff5252',
    info: '#3b82f6',
    success: '#4edea3',
    warning: '#fea619',
    'surface-container-lowest': '#0b1326',
    'surface-container-low': '#161e31',
    'surface-container': '#1e293b',
    'surface-container-high': '#222c45',
    'surface-container-highest': '#2d3748',
    outline: 'rgba(255, 255, 255, 0.2)',
    'outline-variant': 'rgba(255, 255, 255, 0.1)'
  }
}

export default defineNuxtPlugin(async (nuxtApp) => {
  const { loadAppData, appData } = useAppData()
  await loadAppData()

  // data.json'da gradient primary varsa, primary'yi override et
  // (tenant branding için — opsiyonel)
  const colors = appData.value?.theme?.colors
  let primaryOverride: { light: string; dark: string } | null = null

  if (typeof colors?.primary === 'string' && colors.primary.includes('gradient')) {
    const hexMatches = colors.primary.match(/#[a-fA-F0-9]{6}/g)
    if (hexMatches && hexMatches.length >= 2) {
      // Gradient'in [dark, main, light] formatında olduğunu varsay
      primaryOverride = {
        light: hexMatches[1], // main → light theme primary
        dark: hexMatches[2] ?? hexMatches[1] // light → dark theme primary
      }
    }
  }

  // Light theme — tenant override varsa primary'yi değiştir
  const lightTheme = {
    ...LIGHT_THEME_BASE,
    colors: {
      ...LIGHT_THEME_BASE.colors,
      ...(primaryOverride ? { primary: primaryOverride.light } : {})
    }
  }

  // Dark theme — aynı şekilde
  const darkTheme = {
    ...DARK_THEME_BASE,
    colors: {
      ...DARK_THEME_BASE.colors,
      ...(primaryOverride ? { primary: primaryOverride.dark } : {})
    }
  }

  // Initial theme'i localStorage'dan oku (FOUC önleme)
  // useTheme() composable'ı plugins/theme.client.ts'de init ediliyor
  // ama plugin sırası: vuetify.client.ts daha önce yüklenir, theme.client.ts sonra.
  // Bu yüzden localStorage'ı manuel okuyoruz initial value için.
  let initialTheme: 'light' | 'dark' = 'light'
  try {
    if (typeof localStorage !== 'undefined') {
      const stored = localStorage.getItem('app-theme')
      if (stored === 'dark' || stored === 'light') {
        initialTheme = stored
      }
    }
  } catch {
    // localStorage erişilemiyorsa default 'light'
  }

  const vuetify = createVuetify({
    components,
    directives,
    theme: {
      defaultTheme: initialTheme,
      themes: {
        light: lightTheme,
        dark: darkTheme
      }
    },
    icons: {
      defaultSet: 'mdi',
      sets: { mdi }
    },
    defaults: {
      VBtn: {
        style: 'text-transform: none;',
        variant: 'flat'
      },
      VCard: {
        elevation: 2
      },
      VTextField: {
        variant: 'outlined',
        density: 'comfortable'
      },
      VSelect: {
        variant: 'outlined',
        density: 'comfortable'
      },
      VTextarea: {
        variant: 'outlined',
        density: 'comfortable'
      }
    }
  })

  nuxtApp.vueApp.use(vuetify)

  // useTheme() tema değiştirildiğinde Vuetify'ı senkronize et.
  // Plugin yüklendikten sonra çalışır — DOM hazır.
  if (import.meta.client) {
    // Vuetify instance'ına erişmek için
    const vuetifyInstance = vuetify
    // useTheme composable'ı henüz çalışıyor olmayabilir; bu yüzden
    // localStorage event'leri ve manuel sync için bir helper provide edelim.
    nuxtApp.provide('syncVuetifyTheme', (themeName: 'light' | 'dark') => {
      try {
        vuetifyInstance.theme.global.name.value = themeName
      } catch (error) {
        console.warn('Vuetify theme sync failed:', error)
      }
    })
  }
})
