export interface Permission {
  id: string
  name: string
  displayName?: string
  description?: string
  resource: string
  action?: string
  type?: PermissionType
  fullPermission?: string
  isActive?: boolean
  assignedCount?: number
  createdDate?: string
}

export enum PermissionType {
  Create = 1,
  Read = 2,
  Update = 3,
  Delete = 4,
  Manage = 5
}
