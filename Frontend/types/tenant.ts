export interface Tenant {
  id: number
  name: string
  description?: string
  domain?: string
  isActive: boolean
  contactEmail?: string
  contactPhone?: string
  createdDate: string
  userCount?: number
}

export interface CreateTenantRequest {
  name: string
  description?: string
  domain?: string
  isActive: boolean
  contactEmail?: string
  contactPhone?: string
}

export interface UpdateTenantRequest {
  id: number
  name: string
  description?: string
  domain?: string
  isActive: boolean
  contactEmail?: string
  contactPhone?: string
}
