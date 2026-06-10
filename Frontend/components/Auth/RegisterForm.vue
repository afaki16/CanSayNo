<template>
  <v-form ref="form" v-model="isValid" @submit.prevent="$emit('submit', formData)" :class="formClass">
      <div class="input-group">
        <BaseTextBox
          v-model="formData.firstName"
          :label="t('auth.firstNameLabel')"
          prepend-inner-icon="mdi-account"
          :rules="[rules.required, rules.minLength(2)]"
          type="text"
          required
          min-length="2"
          :disabled="loading"
          :class="inputClass"
        />
      </div>
      <div class="input-group">
        <BaseTextBox
          v-model="formData.lastName"
          :label="t('auth.lastNameLabel')"
          :rules="[rules.required, rules.minLength(2)]"
          type="text"
          required
          min-length="2"
          :disabled="loading"
          :class="inputClass"
        />
      </div>

    <div class="input-group">
      <BaseTextBox
        v-model="formData.email"
        :label="t('auth.email')"
        type="email"
        prepend-inner-icon="mdi-email"
        :rules="[rules.required, rules.email]"
        required
        :disabled="loading"
        :class="inputClass"
      />
    </div>

    <div class="input-group">
      <BaseTextBox
        v-model="formData.phoneNumber"
        :label="t('auth.phoneLabel')"
        type="tel"
        prepend-inner-icon="mdi-phone"
        :rules="[rules.phone]"
        :disabled="loading"
        hint="Format: +1234567890"
        :class="inputClass"
      />
    </div>

    <div class="input-group">
        <BaseTextBox
          v-model="formData.password"
          :label="t('auth.password')"
          type="password"
          prepend-inner-icon="mdi-lock"
          :rules="[rules.password]"
          required
          min-length="8"
          require-uppercase
          require-lowercase
          require-number
          require-special-character
          :disabled="loading"
          :class="inputClass"
        />
    </div>

    <div class="input-group">
      <BaseTextBox
        v-model="formData.confirmPassword"
        :label="t('auth.confirmPasswordLabel')"
        type="password"
        prepend-inner-icon="mdi-lock-check"
        :rules="[rules.required, rules.confirmPassword(formData.password)]"
        required
        :disabled="loading"
        :class="inputClass"
      />
    </div>

    <!-- Password strength indicator -->
    <div class="password-strength mb-4">
      <div class="strength-header">
        <span class="strength-label">{{ t('auth.passwordStrength') }}:</span>
        <span class="strength-text" :class="passwordStrengthColor">
          {{ passwordStrengthText }}
        </span>
      </div>
      <v-progress-linear
        :model-value="passwordStrength"
        :color="passwordStrengthColor"
        height="4"
        rounded
        class="strength-bar"
      />
    </div>

    <slot name="terms" />

    <v-btn
      type="submit"
      :color="submitButtonColor"
      size="large"
      block
      :loading="loading"
      :disabled="!isValid"
      :class="submitButtonClass"
      elevation="8"
    >
      <slot name="submit-text">{{ t('auth.createAccountTitle') }}</slot>
    </v-btn>
  </v-form>
</template>

<script setup lang="ts">
import type { RegisterRequest } from '~/types'
import BaseTextBox from '~/components/UI/BaseTextBox.vue'

// Props & Emits
const props = defineProps<{
  loading?: boolean
  config?: any
  formClass?: string
  inputClass?: string
  submitButtonColor?: string
  submitButtonClass?: string
}>()

const emit = defineEmits<{
  submit: [data: RegisterRequest]
}>()

// Composables
const { t } = useI18n()
const { validationRules: rules } = useValidators()

// Reactive data
const form = ref()
const isValid = ref(false)

const formData = reactive<RegisterRequest>({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  phoneNumber: ''
})

// Password strength computation
const passwordStrength = computed(() => {
  const password = formData.password
  if (!password) return 0
  
  let strength = 0
  
  // Length check
  if (password.length >= 8) strength += 25
  
  // Character variety checks
  if (/[a-z]/.test(password)) strength += 25
  if (/[A-Z]/.test(password)) strength += 25
  if (/\d/.test(password)) strength += 15
  if (/[^A-Za-z0-9]/.test(password)) strength += 10
  
  return Math.min(strength, 100)
})

const passwordStrengthText = computed(() => {
  const strength = passwordStrength.value
  if (strength < 25) return t('auth.strengthWeak')
  if (strength < 50) return t('auth.strengthFair')
  if (strength < 75) return t('auth.strengthGood')
  return t('auth.strengthStrong')
})

const passwordStrengthColor = computed(() => {
  const strength = passwordStrength.value
  if (strength < 25) return 'error'
  if (strength < 50) return 'warning'
  if (strength < 75) return 'info'
  return 'success'
})

// Methods
const validateForm = () => {
  return form.value?.validate()
}

const reset = () => {
  form.value?.reset()
  Object.assign(formData, {
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phoneNumber: ''
  })
}


// Expose methods
defineExpose({
  validateForm,
  reset,
  formData
})
</script>

<style scoped>
.input-row {
  display: flex;
  gap: 15px;
  margin-bottom: 20px;
}

.input-row .input-group {
  flex: 1;
}

.input-group {
  margin-bottom: 20px;
}

.input-group :deep(.v-field) {
  background: rgba(255, 255, 255, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.2) !important;
  border-radius: 12px !important;
}

.input-group :deep(.v-field__input) {
  color: white !important;
}

.input-group :deep(.v-field__label) {
  color: rgba(255, 255, 255, 0.8) !important;
}

.input-group :deep(.v-icon) {
  color: rgba(255, 255, 255, 0.8) !important;
}

.password-strength {
  text-align: left;
}

.strength-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.strength-label {
  color: rgba(255, 255, 255, 0.8);
  font-size: 0.875rem;
}

.strength-text {
  font-size: 0.875rem;
  font-weight: 600;
}

.strength-bar {
  border-radius: 2px;
}

/* Responsive design */
@media (max-width: 768px) {
  .input-row {
    flex-direction: column;
    gap: 0;
  }
}
</style> 