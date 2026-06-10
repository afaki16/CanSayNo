import type { ToastType } from '~/stores/toast'
import { parseApiErrorMessages } from '~/utils/apiError'

interface ToastOptions {
  title?: string
  duration?: number
}

export const useToast = () => {
  const store = useToastStore()

  const success = (messages: string | string[], options?: ToastOptions) =>
    store.add('success', messages, options?.title, options?.duration)

  const error = (messages: string | string[], options?: ToastOptions) =>
    store.add('error', messages, options?.title, options?.duration)

  const warning = (messages: string | string[], options?: ToastOptions) =>
    store.add('warning', messages, options?.title, options?.duration)

  const info = (messages: string | string[], options?: ToastOptions) =>
    store.add('info', messages, options?.title, options?.duration)

  /**
   * Parses various API error response formats and shows an error toast.
   *
   * Supported shapes:
   *  - { error: { errors: { fieldName: [...msgs] } } }
   *  - { errors: [...msgs] }
   *  - { error: "msg" }
   *  - { message: "msg" }
   */
  const showApiError = (error: any, fallbackMessage = 'An unexpected error occurred') => {
    store.add('error', parseApiErrorMessages(error, fallbackMessage))
  }

  return {
    toasts: computed(() => store.items),
    success,
    error,
    warning,
    info,
    remove: store.remove,
    clearAll: store.clear,
    showApiError,
  }
}
