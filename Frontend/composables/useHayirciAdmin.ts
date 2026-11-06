import type {
  AdminDashboard,
  AdminScenario,
  AdminScenarioUpsert,
  AdminUserOverview,
  CreateAdminScenarioRequest,
} from '~/types'
import { API_ENDPOINTS } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

// Hayırcı admin uçları. Senaryo listesi küçük (≈50) — istemci tarafı sayfalama:
// getScenarios yanıtı olduğu gibi döner, useCrudOperations `response.data` dizisini kullanır.
// Hatalar useApi'de global toast ile gösterilir.
export const useHayirciAdmin = () => {
  const api = useApi()

  const getScenarios = async () =>
    api.get<AdminScenario[]>(API_ENDPOINTS.HAYIRCI_ADMIN.SCENARIOS)

  const createScenario = async ({ id, ...body }: CreateAdminScenarioRequest) => {
    const response = await api.post<AdminScenario>(API_ENDPOINTS.HAYIRCI_ADMIN.SCENARIO(id), body)
    return response.data
  }

  const updateScenario = async (id: string, body: AdminScenarioUpsert) => {
    const response = await api.put<AdminScenario>(API_ENDPOINTS.HAYIRCI_ADMIN.SCENARIO(id), body)
    return response.data
  }

  const setScenarioActive = async (id: string, isActive: boolean) => {
    const response = await api.put<AdminScenario>(API_ENDPOINTS.HAYIRCI_ADMIN.SCENARIO_ACTIVE(id), { isActive })
    return response.data
  }

  const getUserOverview = async (userId: number) => {
    const response = await api.get<AdminUserOverview>(API_ENDPOINTS.HAYIRCI_ADMIN.USER_OVERVIEW(userId))
    return response.data
  }

  const getDashboard = async () => {
    const response = await api.get<AdminDashboard>(API_ENDPOINTS.HAYIRCI_ADMIN.DASHBOARD)
    return response.data
  }

  return {
    getScenarios,
    createScenario,
    updateScenario,
    setScenarioActive,
    getUserOverview,
    getDashboard,
  }
}
