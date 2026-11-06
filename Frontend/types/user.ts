export interface User {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  phoneNumber?: string
  status: UserStatus
  lastLoginDate?: string
  emailConfirmed: boolean
  phoneConfirmed: boolean
  profileImageUrl?: string
  createdDate: string
  tenantId?: number
  roles: any[]
  permissions: any[]
}

export interface CreateUserRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  phoneNumber?: string
  status: UserStatus
  roleIds: string[]
  /** Sadece SuperAdmin belirtebilir. Belirtilmezse mevcut kullanıcının tenant'ı kullanılır. */
  tenantId?: number
}

export interface UpdateUserRequest {
  id: string
  firstName: string
  lastName: string
  email: string
  phoneNumber?: string
  status: UserStatus
  profileImageUrl?: string
  roleIds: string[]
}

// Backend Domain/Common/Enums/UserStatus ile aynı değerler. API JSON'da enum'ı camelCase
// metin olarak gönderir ("active"); sayıya çevirmek için utils/userStatus.ts.
export enum UserStatus {
  Inactive = 0,
  Active = 1,
  Banned = 2,
  PendingVerification = 3
}
