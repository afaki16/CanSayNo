/**
 * Global auth middleware.
 *
 * Çalışma akışı:
 *  1) ssr: false olduğu için sadece client-side çalışır
 *  2) İlk yüklemede authStore.initializeAuth() refresh-token cookie ile
 *     yeni access token almayı dener. Başarılıysa user authenticated olur.
 *  3) Public route'lar için authenticated kullanıcılar dashboard'a yönlendirilir
 *  4) Protected route'larda authenticated olmayan kullanıcılar login'e yönlendirilir
 */
export default defineNuxtRouteMiddleware(async (to) => {
  // ssr: false olduğu için import.meta.server zaten çalışmaz, ama defensive olsun
  if (import.meta.server) return

  const authStore = useAuthStore()

  // İlk yüklemede / sayfa refresh sonrası silent refresh dene
  // (sadece henüz initialize olmamışsa)
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }

  // verify-email her zaman erişilebilir (email link tıklamasıyla gelinir)
  if (to.path === '/auth/verify-email') return

  const publicRoutes = ['/auth/register', '/auth/check-email', '/']

  if (publicRoutes.includes(to.path)) {
    // Authenticated kullanıcı login sayfasına gelirse dashboard'a at
    if (authStore.isAuthenticated && to.path.startsWith('/auth/')) {
      return navigateTo('/dashboard')
    }
    return
  }

  // Korumalı route — authenticate olmamışsa login'e
  if (!authStore.isAuthenticated) {
    return navigateTo('/')
  }

  // Token expired ise silent refresh dene
  if (authStore.isTokenExpired) {
    const refreshed = await authStore.silentRefresh()
    if (!refreshed) {
      authStore.clearAuth()
      return navigateTo('/?expired=true')
    }
  }
})
