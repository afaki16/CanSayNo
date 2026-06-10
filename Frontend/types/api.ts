export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  error?: string
  errors?: string[]
}

export interface ValidationError {
  field: string
  message: string
}

export interface ApiError {
  message: string
  errors?: ValidationError[]
  statusCode?: number
}
