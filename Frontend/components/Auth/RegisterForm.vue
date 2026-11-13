<template>
  <!-- Alan düzeni giriş sayfasıyla (ve Dernify ile) aynı: etiket ÜSTTE, alanın içinde yer tutucu,
       ikonlar marka renginde, hata mesajı alanın altında. -->
  <v-form ref="form" v-model="isValid" :class="formClass" @submit.prevent="$emit('submit', formData)">
    <div class="field-row">
      <div class="field-block">
        <label class="field-label" for="register-first-name">{{ t('auth.firstNameLabel') }}</label>
        <v-text-field
          id="register-first-name"
          v-model="formData.firstName"
          :placeholder="t('auth.firstNamePlaceholder')"
          prepend-inner-icon="mdi-account-outline"
          :rules="[rules.required, rules.minLength(2)]"
          :disabled="loading"
          autocomplete="given-name"
          v-bind="fieldProps"
        />
      </div>
      <div class="field-block">
        <label class="field-label" for="register-last-name">{{ t('auth.lastNameLabel') }}</label>
        <v-text-field
          id="register-last-name"
          v-model="formData.lastName"
          :placeholder="t('auth.lastNamePlaceholder')"
          :rules="[rules.required, rules.minLength(2)]"
          :disabled="loading"
          autocomplete="family-name"
          v-bind="fieldProps"
        />
      </div>
    </div>

    <div class="field-block">
      <label class="field-label" for="register-email">{{ t('auth.email') }}</label>
      <v-text-field
        id="register-email"
        v-model="formData.email"
        type="email"
        :placeholder="t('auth.emailPlaceholder')"
        prepend-inner-icon="mdi-email-outline"
        :rules="[rules.required, rules.email]"
        :disabled="loading"
        autocomplete="email"
        v-bind="fieldProps"
      />
    </div>

    <div class="field-block">
      <label class="field-label" for="register-phone">{{ t('auth.phoneLabel') }}</label>
      <v-text-field
        id="register-phone"
        v-model="formData.phoneNumber"
        type="tel"
        :placeholder="t('auth.phonePlaceholder')"
        prepend-inner-icon="mdi-phone-outline"
        :rules="[rules.phone]"
        :disabled="loading"
        autocomplete="tel"
        v-bind="fieldProps"
      />
    </div>

    <div class="field-block">
      <label class="field-label" for="register-password">{{ t('auth.password') }}</label>
      <v-text-field
        id="register-password"
        v-model="formData.password"
        :type="showPassword ? 'text' : 'password'"
        :placeholder="t('auth.newPasswordPlaceholder')"
        prepend-inner-icon="mdi-lock-outline"
        :append-inner-icon="showPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'"
        :rules="[rules.password]"
        :disabled="loading"
        autocomplete="new-password"
        v-bind="fieldProps"
        @click:append-inner="showPassword = !showPassword"
      />
    </div>

    <div class="field-block">
      <label class="field-label" for="register-confirm-password">{{ t('auth.confirmPasswordLabel') }}</label>
      <v-text-field
        id="register-confirm-password"
        v-model="formData.confirmPassword"
        :type="showConfirmPassword ? 'text' : 'password'"
        :placeholder="t('auth.confirmPasswordPlaceholder')"
        prepend-inner-icon="mdi-lock-check-outline"
        :append-inner-icon="showConfirmPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'"
        :rules="[rules.required, rules.confirmPassword(formData.password)]"
        :disabled="loading"
        autocomplete="new-password"
        v-bind="fieldProps"
        @click:append-inner="showConfirmPassword = !showConfirmPassword"
      />
    </div>

    <!-- Şifre gücü: yalnızca şifre yazılınca görünür -->
    <div v-if="formData.password" class="password-strength">
      <div class="strength-header">
        <span class="strength-label">{{ t('auth.passwordStrength') }}</span>
        <span class="strength-text" :class="`text-${passwordStrengthColor}`">
          {{ passwordStrengthText }}
        </span>
      </div>
      <v-progress-linear
        :model-value="passwordStrength"
        :color="passwordStrengthColor"
        height="4"
        rounded
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
    >
      <slot name="submit-text">{{ t('auth.createAccountTitle') }}</slot>
    </v-btn>
  </v-form>
</template>

<script setup lang="ts">
import type { RegisterRequest } from '~/types'

defineProps<{
  loading?: boolean
  formClass?: string
  submitButtonColor?: string
  submitButtonClass?: string
}>()

defineEmits<{
  submit: [data: RegisterRequest]
}>()

const { t } = useI18n()
const { validationRules: rules } = useValidators()

// Tüm alanlarda ortak Vuetify ayarları (giriş sayfasındakiyle aynı görünüm)
const fieldProps = {
  variant: 'outlined',
  density: 'comfortable',
  hideDetails: 'auto',
  class: 'field-input'
} as const

const form = ref()
const isValid = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)

const formData = reactive<RegisterRequest>({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  phoneNumber: ''
})

const passwordStrength = computed(() => {
  const password = formData.password
  if (!password) return 0

  let strength = 0
  if (password.length >= 8) strength += 25
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

const validateForm = () => form.value?.validate()

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

defineExpose({
  validateForm,
  reset,
  formData
})
</script>

<style scoped>
.field-row {
  display: flex;
  gap: 14px;
}

.field-row .field-block {
  flex: 1;
  min-width: 0;
}

.field-block {
  margin-bottom: 18px;
}

.field-label {
  display: block;
  font-size: 0.82rem;
  font-weight: 600;
  letter-spacing: 0.3px;
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 7px;
}

.field-input :deep(.v-field) {
  background: rgba(255, 255, 255, 0.04) !important;
  border-radius: 12px !important;
  color: #f1f1f4 !important;
}

.field-input :deep(.v-field__outline) {
  --v-field-border-opacity: 0.14;
  color: #fff;
}

.field-input :deep(.v-field--focused .v-field__outline) {
  --v-field-border-opacity: 1;
  color: #8b9cff;
}

.field-input :deep(.v-field--focused) {
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.14);
}

.field-input :deep(.v-field--error .v-field__outline) {
  --v-field-border-opacity: 0.9;
  color: rgb(var(--v-theme-error));
}

.field-input :deep(.v-field__input) {
  color: #f1f1f4 !important;
}

.field-input :deep(.v-field__input::placeholder) {
  color: rgba(255, 255, 255, 0.32);
  opacity: 1;
}

/* İkonlar marka renginde (koyu zeminde açık ton) */
.field-input :deep(.v-field__prepend-inner .v-icon),
.field-input :deep(.v-field__append-inner .v-icon) {
  color: #8b9cff !important;
  opacity: 0.9;
}

.field-input :deep(.v-messages__message) {
  font-size: 0.78rem;
  padding-top: 2px;
}

.password-strength {
  margin: -4px 0 18px;
}

.strength-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
}

.strength-label {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.8rem;
}

.strength-text {
  font-size: 0.8rem;
  font-weight: 600;
}

@media (max-width: 480px) {
  .field-row {
    flex-direction: column;
    gap: 0;
  }
}
</style>
