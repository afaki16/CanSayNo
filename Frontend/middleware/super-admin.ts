export default defineNuxtRouteMiddleware(() => {
  const authStore = useAuthStore()
  
  if (!authStore.hasRole('SuperAdmin')) {
    throw createError({
      statusCode: 403,
      statusMessage: 'Access Denied: Super Admin access required'
    })
  }
}) 