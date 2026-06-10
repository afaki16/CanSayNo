export const useUtils = () => {
  const formatDate = (date: string | Date, format = 'short') => {
    const d = new Date(date)
    
    if (format === 'short') {
      return d.toLocaleDateString()
    } else if (format === 'long') {
      return d.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })
    } else if (format === 'datetime') {
      return d.toLocaleString()
    }
    
    return d.toLocaleDateString()
  }

  const formatCurrency = (amount: number, currency = 'USD') => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency
    }).format(amount)
  }

  const truncateText = (text: string, length = 50) => {
    if (text.length <= length) return text
    return text.substring(0, length) + '...'
  }

  const capitalizeFirst = (text: string) => {
    if (!text) return text
    return text.charAt(0).toUpperCase() + text.slice(1).toLowerCase()
  }

  const getUserInitials = (firstName: string, lastName: string) => {
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase()
  }

  const getUserStatusColor = (status: number) => {
    switch (status) {
      case 1: return 'success' // Active
      case 2: return 'warning' // Inactive
      case 3: return 'error'   // Banned
      case 4: return 'info'    // PendingVerification
      default: return 'grey'
    }
  }

  const getUserStatusText = (status: number) => {
    switch (status) {
      case 1: return 'Active'
      case 2: return 'Inactive'
      case 3: return 'Banned'
      case 4: return 'Pending Verification'
      default: return 'Unknown'
    }
  }

  const downloadFile = (data: any, filename: string, type = 'application/json') => {
    const blob = new Blob([data], { type })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    link.click()
    window.URL.revokeObjectURL(url)
  }

  const copyToClipboard = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text)
      return true
    } catch (error) {
      console.error('Failed to copy to clipboard:', error)
      return false
    }
  }

  const debounce = <T extends (...args: any[]) => void>(
    func: T,
    delay: number
  ): ((...args: Parameters<T>) => void) => {
    let timeoutId: NodeJS.Timeout
    return (...args: Parameters<T>) => {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func(...args), delay)
    }
  }

  return {
    formatDate,
    formatCurrency,
    truncateText,
    capitalizeFirst,
    getUserInitials,
    getUserStatusColor,
    getUserStatusText,
    downloadFile,
    copyToClipboard,
    debounce
  }
} 