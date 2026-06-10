export const useValidators = () => {
  const { t } = useI18n()

  const validationRules = {
    required: (value: any) => !!value || t('validation.required'),

    email: (value: string) => {
      const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      return pattern.test(value) || t('validation.emailFormat')
    },

    password: (value: string) => {
      if (!value) return t('validation.passwordRequired')
      if (value.length < 6) return t('validation.passwordMinLength', { min: 6 })
      if (!/(?=.*[a-z])/.test(value)) return t('validation.passwordLowercase')
      if (!/(?=.*[A-Z])/.test(value)) return t('validation.passwordUppercase')
      if (!/(?=.*\d)/.test(value)) return t('validation.passwordDigit')
      return true
    },

    confirmPassword: (password: string) => (value: string) => {
      return value === password || t('validation.passwordsDoNotMatch')
    },

    minLength: (min: number) => (value: string) => {
      return value.length >= min || t('validation.minLength', { min })
    },

    maxLength: (max: number) => (value: string) => {
      return value.length <= max || t('validation.maxLength', { max })
    },

    phone: (value: string) => {
      if (!value) return true // Optional field
      const pattern = /^[\+]?[1-9][\d]{0,15}$/
      return pattern.test(value) || t('validation.phoneFormat')
    },

    /** Boşsa geçerli; doluysa e-posta formatı kontrol edilir */
    optionalEmail: (value: string) => {
      if (value === null || value === undefined || String(value).trim() === '') return true
      return validationRules.email(value as string)
    },

    /** Boşsa geçerli; doluysa telefon formatı kontrol edilir */
    optionalPhone: (value: string) => {
      if (value === null || value === undefined || String(value).trim() === '') return true
      return validationRules.phone(value)
    }
  }

  return {
    validationRules
  }
}
