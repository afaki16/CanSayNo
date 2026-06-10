/**
 * Guest middleware — sadece login olmamış kullanıcılar erişebilir.
 * Login/register sayfalarında kullanılır.
 */
export default defineNuxtRouteMiddleware(async () => {
  const authStore = useAuthStore()

  // Henüz initialize olmamışsa silent refresh dene
  if (!authStore.isAuthenticated) {
    await authStore.initializeAuth()
  }

  if (authStore.isAuthenticated) {
    return navigateTo('/dashboard')
  }
})
