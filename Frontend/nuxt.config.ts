/**
 * CSP (Content Security Policy) — defense-in-depth XSS koruması.
 *
 * NOT: Nuxt SPA modunda (`ssr: false`) state hydration için inline script
 * kullanır, bu yüzden 'unsafe-inline' script-src'te ZORUNLU. Bunu kaldıramayız.
 *
 * Bunu kompanse etmek için diğer direktifleri sıkıştırıyoruz — bir saldırgan
 * inline script enjekte etse bile:
 *  - connect-src kısıtlamasıyla veri exfiltrate edemez
 *  - frame-ancestors ile clickjacking yapılamaz
 *  - form-action ile form hijack edilemez
 *
 * API_BASE_URL environment variable'ı production'da API domain'ini belirler.
 * Eğer env yoksa development localhost'a düşer.
 *
 * Yeni bir 3rd party CDN/script eklemek gerektiğinde aşağıdaki listelere ekle:
 *  - Yeni script CDN'i → script-src
 *  - Yeni font CDN'i → font-src
 *  - Yeni image domain'i → img-src
 *  - Yeni API/WebSocket → connect-src
 */
function buildCsp(apiBase: string): string {
  // API URL'den sadece origin (protocol + host + port) çıkar.
  // Örnek: https://localhost:44333/api/foo → https://localhost:44333
  let apiOrigin = "";
  try {
    const url = new URL(apiBase);
    apiOrigin = url.origin;
  } catch {
    // Eğer apiBase geçersizse, connect-src sadece 'self' olur — defensive fallback
    apiOrigin = "";
  }

  const directives = [
    // Default: yalnızca aynı origin
    "default-src 'self'",

    // Script: SPA hydration için unsafe-inline zorunlu (Nuxt limitation)
    // unsafe-eval Vue runtime için bazı durumlarda gerekli olabilir; eklemiyoruz,
    // sorun çıkarsa eklenebilir.
    "script-src 'self' 'unsafe-inline'",

    // Style: Vuetify/Vue scoped style için unsafe-inline zorunlu
    // Google Fonts CSS dosyaları fonts.googleapis.com'dan geliyor
    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",

    // Image: kendi domain, data URI (inline SVG/base64),
    // ve unsplash (login arka plan carousel'ı için data.json'da tanımlı)
    "img-src 'self' data: https://images.unsplash.com",

    // Font: kendi domain, data URI (MDI icon font),
    // Google Fonts woff/woff2 dosyaları fonts.gstatic.com'dan
    "font-src 'self' data: https://fonts.gstatic.com",

    // Connect (XHR/fetch/WebSocket): kendi origin + backend API origin + iconify icon API
    // api.iconify.design Nuxt Icon modülünün icon SVG'lerini çektiği resmi API.
    apiOrigin
      ? `connect-src 'self' ${apiOrigin} https://api.iconify.design`
      : "connect-src 'self' https://api.iconify.design",

    // Clickjacking koruması — bu sayfa hiçbir iframe içinde gömülemez
    "frame-ancestors 'none'",

    // Form action — formlar sadece kendi origin'e POST edebilir
    // (saldırgan inline form enjekte etse bile başka domain'e veri gönderemez)
    "form-action 'self'",

    // <base> tag enjeksiyonu engellenir
    "base-uri 'none'",

    // Eski plugin'ler (Flash, Java applet vb.) tamamen yasak
    "object-src 'none'",
  ];

  return directives.join("; ");
}

export default defineNuxtConfig({
  devtools: { enabled: false },
  compatibilityDate: "2025-07-26",

  // TypeScript configuration - geçici olarak kapatın
  typescript: {
    strict: false, // ✅ Production için önerilen - true
    typeCheck: false, // ✅ Build sırasında kontrol -true
  },

  // CSS Framework
  css: [
    "~/assets/css/theme.css",
    "vuetify/lib/styles/main.sass",
    "@mdi/font/css/materialdesignicons.min.css",
    "~/assets/scss/main.scss",
    "~/assets/css/main.css",
    "~/assets/css/global-admin.css",
    "flag-icons/css/flag-icons.min.css",
  ],

  build: {
    transpile: ["vuetify"],
  },

  modules: [
    "@pinia/nuxt",
    "@vueuse/nuxt",
    "@nuxtjs/google-fonts",
    "@nuxtjs/tailwindcss",
    "@nuxt/icon",
    "@nuxtjs/i18n",
  ],

  googleFonts: {
    families: {
      Inter: [300, 400, 500, 600, 700],
      "Plus+Jakarta+Sans": [600, 700, 800],
      "Material+Icons": true,
    },
  },

  i18n: {
    locales: [
      { code: "tr", name: "Türkçe", file: "tr.json" },
      { code: "en", name: "English", file: "en.json" },
    ],
    defaultLocale: "tr",
    strategy: "no_prefix",
    lazy: true,
    langDir: "locales/",
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: "i18n_redirected",
      redirectOn: "root",
      alwaysRedirect: false,
    },
  },

  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE_URL || "https://localhost:44333",
      appName: "Kickstart",
      appVersion: "1.0.0",
      defaultLayout: process.env.NUXT_PUBLIC_DEFAULT_LAYOUT || "default",
    },
  },

  app: {
    head: {
      title: "Kickstart",
      meta: [
        { charset: "utf-8" },
        { name: "viewport", content: "width=device-width, initial-scale=1" },
      ],
      link: [
        { rel: "icon", type: "image/x-icon", href: "/favicon.ico" },
      ],
    },
  },

  ssr: false,

  // Security headers — Nuxt/Nitro tarafından her route'a uygulanır.
  // routeRules '/**' pattern'i mevcut ve gelecekteki tüm sayfaları kapsar.
  nitro: {
    routeRules: {
      "/**": {
        headers: {
          "Content-Security-Policy": buildCsp(
            process.env.API_BASE_URL || "https://localhost:44333",
          ),
          // Clickjacking koruması (CSP frame-ancestors zaten kapsıyor,
          // ama eski tarayıcılar için defense-in-depth)
          "X-Frame-Options": "DENY",
          // MIME-type sniffing engellenir
          "X-Content-Type-Options": "nosniff",
          // Referer leak'ı sınırla
          "Referrer-Policy": "strict-origin-when-cross-origin",
          // Hassas browser API'lerini kapatır
          "Permissions-Policy":
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
            "magnetometer=(), microphone=(), payment=(), usb=()",
        },
      },
    },
  },

  vite: {
    define: {
      "process.env.DEBUG": false,
    },
    css: {
      preprocessorOptions: {
        scss: {
          additionalData: '@use "~/assets/scss/variables.scss" as *;',
        },
      },
    },
    ssr: {
      noExternal: ["vuetify"],
    },
    // HMR overlay'i kapatın
    server: {
      hmr: {
        overlay: false,
      },
    },
  },

  imports: {
    dirs: ["composables/**", "stores/**", "utils/**"],
  },

  components: {
    dirs: [
      "~/components",
      "~/components/UI",
      "~/components/Auth",
      "~/components/Users",
      "~/components/Roles",
      "~/components/Tenants",
    ],
  },
});
