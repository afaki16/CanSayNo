export type Nullable<T> = T | null
export type Optional<T> = T | undefined
export type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P]
}

export interface DashboardStats {
  totalUsers: number
  activeUsers: number
  totalRoles: number
  totalPermissions: number
  recentLogins: number
}

export interface MenuItem {
  id: string
  title: string
  icon: string
  path?: string
  children?: MenuItem[]
  permission?: string
  badge?: string | number
  color?: string
}

export interface UserPreferences {
  theme: 'light' | 'dark' | 'auto'
  language: string
  notifications: boolean
  compactMode: boolean
  sidebarCollapsed: boolean
}
