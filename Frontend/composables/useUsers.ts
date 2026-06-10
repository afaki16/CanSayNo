import type { User, CreateUserRequest, UpdateUserRequest } from '~/types'
import { API_ENDPOINTS, getPaginatedEndpoint } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

export const useUsers = () => {
  const api = useApi()

  const getUsers = async (page = 1, pageSize = 10, searchTerm = '') => {
    try {
      const endpoint = getPaginatedEndpoint(API_ENDPOINTS.USERS.LIST, page, pageSize, searchTerm)
      const response = await api.get<{ items: User[]; totalCount: number; totalPages: number; pageNumber: number }>(endpoint)
      return response
    } catch (error) {
      console.error('Get users error:', error)
      throw error
    }
  }

  const getUserById = async (id: string) => {
    try {
      const response = await api.get<User>(API_ENDPOINTS.USERS.GET_BY_ID(id))
      return response.data
    } catch (error) {
      console.error('Get user error:', error)
      throw error
    }
  }

  const createUser = async (userData: CreateUserRequest) => {
    try {
      const response = await api.post<User>(API_ENDPOINTS.USERS.CREATE, userData)
      if (response.success) {
        return response.data
      }
    } catch (error) {
      console.error('Create user error:', error)
      throw error
    }
  }

  const updateUser = async (id: string, userData: UpdateUserRequest) => {
    try {
      const response = await api.put<User>(API_ENDPOINTS.USERS.UPDATE(id), userData)
      if (response.success) {
        return response.data
      }
    } catch (error) {
      console.error('Update user error:', error)
      throw error
    }
  }

  const deleteUser = async (id: string) => {
    try {
      await api.delete(API_ENDPOINTS.USERS.DELETE(id))
      return true
    } catch (error) {
      console.error('Delete user error:', error)
      throw error
    }
  }

  return {
    getUsers,
    getUserById,
    createUser,
    updateUser,
    deleteUser
  }
} 