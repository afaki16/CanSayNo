// lib/api barrel. Bu modülü import etmek response interceptor'ı da kayıt eder
// (aşağıdaki side-effect import). Uygulama API katmanına buradan erişmeli.

import './refreshInterceptor'; // Side effect: 401 → refresh response interceptor'ı kayıt eder.

export {
  apiClient,
  ApiError,
  API_ERROR_CODE,
  parseApiErrorBody,
  type ApiResponse,
  type ApiErrorPayload,
  type ApiErrorCode,
} from './client';
export {
  AUTH_ENDPOINTS,
  SCENARIO_ENDPOINTS,
  SESSION_ENDPOINTS,
  STATS_ENDPOINTS,
  isPath,
} from './endpoints';
export { queryClient } from './queryClient';
export {
  apiGet,
  apiPost,
  apiPostVoid,
  apiPutVoid,
  type ApiQueryOptions,
  type ApiMutationOptions,
} from './hooks';
export { authService } from './services/authService';
export { scenarioService } from './services/scenarioService';
export { sessionService } from './services/sessionService';
export { statsService } from './services/statsService';
export {
  toStartSessionError,
  toSessionErrorMessage,
  toSendMessageErrorMessage,
  toEvaluateErrorMessage,
  type StartSessionError,
  type StartSessionErrorKind,
} from './services/sessionErrors';
export {
  toForgotPasswordErrorMessage,
  toLoginErrorMessage,
  toProfileErrorMessage,
  toRegisterErrorMessage,
  toResetPasswordErrorMessage,
} from './services/authErrors';
export {
  toScenarioErrorMessage,
  isNotFoundError,
} from './services/scenarioErrors';
export { toStoredUser } from './mappers/userMapper';
