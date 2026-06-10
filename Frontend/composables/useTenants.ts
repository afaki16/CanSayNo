import type { Tenant, CreateTenantRequest, UpdateTenantRequest } from '~/types'
import { API_ENDPOINTS, getPaginatedEndpoint } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

export const useTenants = () => {
  const api = useApi()

  const getTenants = async (page = 1, pageSize = 1000, searchTerm = '') => {
    try {
      const endpoint = getPaginatedEndpoint(API_ENDPOINTS.TENANTS.LIST, page, pageSize, searchTerm)
      const response = await api.get<{ success: boolean; data: Tenant[] }>(endpoint)
      return response
    } catch (error) {
      console.error('Get tenants error:', error)
      throw error
    }
  }

  const getTenantById = async (id: string) => {
    try {
      const response = await api.get<Tenant>(API_ENDPOINTS.TENANTS.GET_BY_ID(id))
      return response?.data ?? response
    } catch (error) {
      console.error('Get tenant error:', error)
      throw error
    }
  }

  const createTenant = async (tenantData: CreateTenantRequest) => {
    try {
      const response = await api.post<{ success: boolean; data: Tenant }>(API_ENDPOINTS.TENANTS.CREATE, tenantData)
      if (response?.success) {
        return response.data
      }
    } catch (error) {
      console.error('Create tenant error:', error)
      throw error
    }
  }

  const updateTenant = async (id: string, tenantData: UpdateTenantRequest) => {
    try {
      const response = await api.put<{ success: boolean; data: Tenant }>(API_ENDPOINTS.TENANTS.UPDATE(id), tenantData)
      if (response?.success) {
        return response.data
      }
    } catch (error) {
      console.error('Update tenant error:', error)
      throw error
    }
  }

  const deleteTenant = async (id: string) => {
    try {
      await api.delete(API_ENDPOINTS.TENANTS.DELETE(id))
      return true
    } catch (error) {
      console.error('Delete tenant error:', error)
      throw error
    }
  }

  return {
    getTenants,
    getTenantById,
    createTenant,
    updateTenant,
    deleteTenant,
  }
}
