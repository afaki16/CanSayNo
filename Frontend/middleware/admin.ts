export default defineNuxtRouteMiddleware(() => {
  const authStore = useAuthStore()
  
  const adminRoles = ['Admin', 'SuperAdmin']
  const hasAdminRole = adminRoles.some(role => authStore.hasRole(role))
  
  if (!hasAdminRole) {
    throw createError({
      statusCode: 403,
      statusMessage: 'Access Denied: Admin access required'
    })
  }
}) 