import type { User } from './user'
import type { Role } from './role'
import type { Permission } from './permission'

export interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  permissions: string[]
  roles: string[]
}

export interface UserState {
  users: User[]
  currentUser: User | null
  totalUsers: number
  isLoading: boolean
  searchTerm: string
  currentPage: number
  itemsPerPage: number
}

export interface RoleState {
  roles: Role[]
  currentRole: Role | null
  isLoading: boolean
}

export interface PermissionState {
  permissions: Permission[]
  isLoading: boolean
}
