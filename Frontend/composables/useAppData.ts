import { computed, readonly, watch } from 'vue'

function hexToRgb(hex: string): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex)
  if (!result) return '0, 0, 0'
  return `${parseInt(result[1], 16)}, ${parseInt(result[2], 16)}, ${parseInt(result[3], 16)}`
}

function parseGradientColors(gradient: string): { dark: string; main: string; light: string } {
  const hexMatches = gradient.match(/#[a-fA-F0-9]{6}/g)
  if (!hexMatches || hexMatches.length < 2) {
    return { dark: '#4338ca', main: '#2563eb', light: '#3b82f6' }
  }
  if (hexMatches.length === 2) {
    return { dark: hexMatches[0], main: hexMatches[1], light: hexMatches[1] }
  }
  return { dark: hexMatches[0], main: hexMatches[1], light: hexMatches[2] }
}

interface AppData {
  tenant?: {
    loginPage?: string
  }
  app: {
    logo: {
      src: string
      alt: string
    }
    brand: {
      text: string
    }
  }
  theme: {
    colors: {
      primary: string
    }
  }
  login: {
    backgroundImages: string[]
    rotationInterval: number
  }
  register: {
    backgroundImages: string[]
    rotationInterval: number
    overlay: {
      color: string
    }
    card: {
      background: string
      backdropFilter: string
      borderRadius: string
      border: string
      shadow: string
    }
  }
}

export const useAppData = () => {
  // SSR-safe shared state — Nuxt'ın useState'i sayesinde her request için izole
  const appData = useState<AppData | null>('app-data', () => null)
  const isLoading = useState<boolean>('app-data-loading', () => false)
  const error = useState<string | null>('app-data-error', () => null)

  const loadAppData = async (forceReload = false) => {
    if (appData.value && !forceReload) {
      return appData.value
    }

    isLoading.value = true
    error.value = null

    try {
      const response = await fetch('/data.json')
      if (!response.ok) {
        throw new Error('Failed to load app data')
      }

      const data = (await response.json()) as AppData
      appData.value = data
      return appData.value
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error'
      console.error('Error loading app data:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  const injectThemeCSSVariables = () => {
    // SSR'da hiçbir şey yapma — sadece client'ta DOM'a dokun
    if (!import.meta.client || !appData.value) return

    const colors = appData.value.theme?.colors
    if (!colors) return

    const root = document.documentElement
    const p = parseGradientColors(colors.primary)

    // Sadece gradient'ler inject edilir. Düz renkler theme.css'ten gelir (--color-*).
    root.style.setProperty('--theme-gradient', `linear-gradient(135deg, ${p.dark} 0%, ${p.main} 100%)`)
    root.style.setProperty('--theme-gradient-hover', `linear-gradient(135deg, ${p.dark} 0%, ${p.light} 100%)`)
    root.style.setProperty('--theme-gradient-sidebar', colors.primary)
  }

  const getAppInfo = computed(() => appData.value?.app || null)
  const getTheme = computed(() => appData.value?.theme || null)
  const getLoginConfig = computed(() => appData.value?.login || null)
  const getRegisterConfig = computed(() => appData.value?.register || null)

  const getLogo = computed(() => appData.value?.app.logo || null)
  const getBrandText = computed(() => appData.value?.app.brand.text || 'Kickstart')
  const getBackgroundImages = computed(
    () => appData.value?.login.backgroundImages || appData.value?.register.backgroundImages || []
  )
  const getThemeColors = computed(() => appData.value?.theme.colors || null)

  // Sadece client'ta watch et — SSR'da dokuma
  if (import.meta.client) {
    watch(appData, () => {
      injectThemeCSSVariables()
    }, { immediate: true })
  }

  return {
    appData: readonly(appData),
    isLoading: readonly(isLoading),
    error: readonly(error),
    loadAppData,
    getAppInfo,
    getTheme,
    getLoginConfig,
    getRegisterConfig,
    getLogo,
    getBrandText,
    getBackgroundImages,
    getThemeColors
  }
}
