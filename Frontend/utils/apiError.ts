/**
 * Extracts human-readable error messages from API / axios error responses.
 * Matches backend shape: { success: false, error: { errors: { field: [...] } } }
 */
export function parseApiErrorMessages(error: any, fallbackMessage = 'An unexpected error occurred'): string[] {
  const data = error?.response?.data ?? error?.data

  if (!data) {
    return [error?.message || fallbackMessage]
  }

  const collected: string[] = []

  if (data.error?.errors && typeof data.error.errors === 'object') {
    for (const field of Object.values(data.error.errors) as any[]) {
      if (Array.isArray(field)) {
        collected.push(...field)
      } else if (typeof field === 'string') {
        collected.push(field)
      }
    }
  }

  if (collected.length === 0 && Array.isArray(data.errors)) {
    collected.push(...data.errors)
  }

  if (collected.length === 0 && typeof data.error === 'string') {
    collected.push(data.error)
  }

  if (collected.length === 0 && data.message) {
    collected.push(data.message)
  }

  if (collected.length === 0) {
    collected.push(fallbackMessage)
  }

  return collected
}
