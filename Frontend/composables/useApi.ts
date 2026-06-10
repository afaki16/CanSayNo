import type { ApiResponse } from '~/types'
import { useToastStore } from '~/stores/toast'
import { parseApiErrorMessages } from '~/utils/apiError'

export interface ApiRequestOptions {
  /** Skip global error toast (e.g. login shows inline alert instead) */
  silent?: boolean
}

export const useApi = () => {
  const { $api } = useNuxtApp()
  const toastStore = useToastStore()

  const handleApiError = (error: any) => {
    const messages = parseApiErrorMessages(error)
    toastStore.add('error', messages)
    throw error
  }

  const get = async <T>(url: string): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.get(url)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  const post = async <T>(url: string, data?: any, options?: ApiRequestOptions): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.post(url, data)
      return response.data
    } catch (error) {
      if (!options?.silent) {
        handleApiError(error)
      }
      throw error
    }
  }

  const put = async <T>(url: string, data?: any): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.put(url, data)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  const del = async <T>(url: string): Promise<ApiResponse<T>> => {
    try {
      const response = await $api.delete(url)
      return response.data
    } catch (error) {
      handleApiError(error)
      throw error
    }
  }

  return {
    get,
    post,
    put,
    delete: del,
    handleApiError
  }
}
