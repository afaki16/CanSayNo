// API Endpoints configuration for Swagger API
export const API_ENDPOINTS = {
  // Authentication endpoints
  AUTH: {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    LOGOUT: '/api/auth/logout',
    REFRESH_TOKEN: '/api/auth/refresh-token',
    ME: '/api/auth/me',
    FORGOT_PASSWORD: '/api/auth/forgot-password',
    RESET_PASSWORD: '/api/auth/reset-password',
    VERIFY_EMAIL: '/api/auth/verify-email',
    RESEND_VERIFICATION: '/api/auth/resend-verification',
    SESSIONS: '/api/auth/sessions',
    LOGOUT_ALL: '/api/auth/logout-all',
    LOGOUT_DEVICE: (deviceId: string) => `/api/auth/logout-device/${deviceId}`,
    REVOKE_SESSION_BY_ID: (sessionId: number) => `/api/auth/sessions/${sessionId}/revoke`,
    CHANGE_PASSWORD: '/api/auth/change-password'
  },

  // User management endpoints
  USERS: {
    LIST: '/api/users',
    CREATE: '/api/users',
    GET_BY_ID: (id: string) => `/api/users/${id}`,
    UPDATE: (id: string) => `/api/users/${id}`,
    DELETE: (id: string) => `/api/users/${id}`,
    CHANGE_PASSWORD: (id: string) => `/api/users/${id}/change-password`,
    UPDATE_PROFILE: '/api/users/profile',
    UPLOAD_AVATAR: '/api/users/avatar'
  },

  // Tenant management endpoints
  TENANTS: {
    LIST: '/api/tenants',
    CREATE: '/api/tenants',
    GET_BY_ID: (id: string) => `/api/tenants/${id}`,
    UPDATE: (id: string) => `/api/tenants/${id}`,
    DELETE: (id: string) => `/api/tenants/${id}`,
  },

  // Role management endpoints
  ROLES: {
    LIST: '/api/roles',
    CREATE: '/api/roles',
    GET_BY_ID: (id: string) => `/api/roles/${id}`,
    UPDATE: (id: string) => `/api/roles/${id}`,
    DELETE: (id: string) => `/api/roles/${id}`,
    ASSIGN_PERMISSIONS: (id: string) => `/api/roles/${id}/permissions`
  },

  // Permission management endpoints
  PERMISSIONS: {
    LIST: '/api/permissions',
    GET_BY_ID: (id: string) => `/api/permissions/${id}`,
    CREATE: '/api/permissions',
    UPDATE: (id: string) => `/api/permissions/${id}`,
    DELETE: (id: string) => `/api/permissions/${id}`
  },

  // Admin endpoints (Admin/SuperAdmin only)
  ADMIN: {
    ACTIVE_USERS_COUNT: '/api/admin/active-users-count',
    ACTIVE_USERS: '/api/admin/active-users',
    REVOKABLE_USERS: '/api/admin/revokable-users',
    REVOKE_USER_SESSIONS: (userId: number) => `/api/admin/users/${userId}/revoke-sessions`
  },

  // Dashboard endpoints
  DASHBOARD: {
    STATS: '/api/dashboard/stats',
    RECENT_ACTIVITIES: '/api/dashboard/recent-activities',
    USER_GROWTH: '/api/dashboard/user-growth'
  },

  // File upload endpoints
  FILES: {
    UPLOAD: '/api/files/upload',
    DELETE: (id: string) => `/api/files/${id}`
  }
}

// Helper function to build full API URLs
export const buildApiUrl = (endpoint: string): string => {
  const config = useRuntimeConfig()
  return `${config.public.apiBase}${endpoint}`
}

// Helper function to get paginated endpoint
export const getPaginatedEndpoint = (baseEndpoint: string, page: number = 1, pageSize: number = 10, search?: string): string => {
  const params = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString()
  })
  
  if (search) {
    params.append('searchTerm', search)
  }
  
  return `${baseEndpoint}?${params.toString()}`
} 