import type { User } from './user'
import type { Role } from './role'
import type { Permission } from './permission'

// API Types
export type {
  ApiResponse,
  ValidationError,
  ApiError
} from './api'

// Auth Types
export type {
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
  Session
} from './auth'

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

// User Types
export type {
  User,
  CreateUserRequest,
  UpdateUserRequest
} from './user'

export { UserStatus } from './user'

export interface UserWithRelations extends Omit<User, 'roles' | 'permissions'> {
  roles: Role[]
  permissions: Permission[]
}

// Role Types
export type {
  Role,
  CreateRoleRequest,
  UpdateRoleRequest
} from './role'

// Tenant Types
export type {
  Tenant,
  CreateTenantRequest,
  UpdateTenantRequest
} from './tenant'

// Permission Types
export type { Permission } from './permission'
export { PermissionType } from './permission'

// UI Types
export type {
  NavigationItem,
  BreadcrumbItem,
  TableHeader,
  DataTableOptions,
  FormField
} from './ui'

// Store Types
export type {
  AuthState,
  UserState,
  RoleState,
  PermissionState
} from './store'

// Component Props
export type {
  BaseDialogProps,
  ConfirmDialogProps,
  UserDialogProps,
  RoleDialogProps
} from './component'

// Config Types
export type { AppConfig } from './config'

// Common Types
export type {
  Nullable,
  Optional,
  DeepPartial,
  DashboardStats,
  MenuItem,
  UserPreferences
} from './common'
