export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()
  
  // Get required permission from route meta
  const requiredPermission = to.meta.permission as string
  const requiredPermissions = to.meta.permissions as string[]
  const requiredRole = to.meta.role as string
  const requiredRoles = to.meta.roles as string[]

  // Check single permission
  if (requiredPermission && !authStore.hasPermission(requiredPermission)) {
    throw createError({
      statusCode: 403,
      statusMessage: 'Access Denied: Insufficient permissions'
    })
  }

  // Check multiple permissions (user must have ALL)
  if (requiredPermissions && !authStore.hasAllPermissions(requiredPermissions)) {
    throw createError({
      statusCode: 403,
      statusMessage: 'Access Denied: Insufficient permissions'
    })
  }

  // Check single role
  if (requiredRole && !authStore.hasRole(requiredRole)) {
    throw createError({
      statusCode: 403,
      statusMessage: 'Access Denied: Insufficient role'
    })
  }

  // Check multiple roles (user must have at least one)
  if (requiredRoles && !requiredRoles.some(role => authStore.hasRole(role))) {
    throw createError({
      statusCode: 403,
      statusMessage: 'Access Denied: Insufficient role'
    })
  }
}) 