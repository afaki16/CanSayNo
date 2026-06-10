import type { Permission } from './permission'

export interface Role {
  id: string
  name: string
  description?: string
  isSystemRole: boolean
  createdDate: string
  permissions: Permission[]
}

export interface CreateRoleRequest {
  name: string
  description?: string
  permissionIds: string[]
}

export interface UpdateRoleRequest {
  id: string
  name: string
  description?: string
  permissionIds: string[]
}
