<template>
  <!-- Giriş sayfasıyla aynı düzen: solda marka sahnesi, sağda koyu zeminde form. -->
  <div class="register-page">
    <div class="lang-switcher-wrapper">
      <LanguageSwitcher />
    </div>

    <div class="left-panel">
      <BrandHero :logo-src="logoSrc" />
    </div>

    <div class="right-panel">
      <div class="right-inner">
        <div class="form-header">
          <img :src="logoSrc" :alt="logoAlt" class="form-header-logo" />
          <h1>{{ t('auth.createAccountTitle') }}</h1>
          <p>{{ t('auth.createAccountSubtitle') }}</p>
        </div>

        <AuthRegisterForm
          ref="registerFormRef"
          :loading="authStore.isLoading"
          submit-button-class="register-submit"
          @submit="handleRegister"
        >
          <template #terms>
            <div class="terms-section">
              <v-checkbox
                v-model="acceptTerms"
                :rules="[v => !!v || t('auth.mustAcceptTerms')]"
                density="compact"
                hide-details
                class="terms-checkbox"
              >
                <template #label>
                  <span class="terms-text">
                    {{ t('auth.agreeTo') }}
                    <a href="/terms" target="_blank" class="terms-link">{{ t('auth.termsOfService') }}</a>
                    {{ t('auth.and') }}
                    <a href="/privacy" target="_blank" class="terms-link">{{ t('auth.privacyPolicy') }}</a>
                  </span>
                </template>
              </v-checkbox>
            </div>
          </template>

          <template #submit-text>
            {{ t('auth.createAccountTitle') }}
          </template>
        </AuthRegisterForm>

        <div class="divider">
          <span>{{ t('auth.or') }}</span>
        </div>

        <div class="login-section">
          <p>{{ t('auth.alreadyHaveAccount') }}</p>
          <v-btn variant="outlined" class="login-btn" @click="$router.push('/')">
            <v-icon start size="18">mdi-login</v-icon>
            {{ t('auth.signIn') }}
          </v-btn>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { RegisterRequest } from '~/types'
import AuthRegisterForm from '~/components/Auth/RegisterForm.vue'

definePageMeta({
  layout: false,
  middleware: 'guest'
})

const { t } = useI18n()
const authStore = useAuthStore()
const auth = useAuth()

const registerFormRef = ref()
const acceptTerms = ref(false)

const { loadAppData, getLogo } = useAppData()
const logoSrc = computed(() => getLogo.value?.src || '/favicon.ico')
const logoAlt = computed(() => getLogo.value?.alt || 'CanSayNo')

const handleRegister = async (formData: RegisterRequest) => {
  if (!acceptTerms.value) return

  try {
    await auth.register(formData)
  } catch (error) {
    console.error('Registration failed:', error)
  }
}

onMounted(async () => {
  await loadAppData()
})

useHead({
  title: 'Hesap Oluştur - CanSayNo',
  meta: [
    { name: 'description', content: 'Create your CanSayNo account' }
  ]
})
</script>

<style scoped>
.register-page {
  --rp-bg: #0a0a0f;
  --rp-border: rgba(255, 255, 255, 0.06);
  /* CanSayNo marka renkleri — koyu zeminde okunsun diye açık ton */
  --rp-accent: #8b9cff;
  --rp-gradient: linear-gradient(135deg, #5b4be0, #2d7ff9);
  --rp-text: #f1f1f4;
  --rp-text-secondary: rgba(255, 255, 255, 0.5);
  --rp-text-muted: rgba(255, 255, 255, 0.3);

  display: flex;
  min-height: 100vh;
  background: var(--rp-bg);
  color: var(--rp-text);
}

.left-panel {
  flex: 1.1;
  position: relative;
  overflow: hidden;
  display: flex;
}

.right-panel {
  flex: 0.9;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 48px 40px;
}

.right-inner {
  width: 100%;
  max-width: 420px;
}

.form-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  margin-bottom: 32px;
}

.form-header-logo {
  max-height: 56px;
  width: auto;
  margin-bottom: 22px;
}

.form-header h1 {
  font-size: 1.7rem;
  font-weight: 700;
  margin-bottom: 6px;
}

.form-header p {
  color: var(--rp-text-secondary);
  font-size: 0.92rem;
}

/* Form alanları: RegisterForm'un saydam beyaz alanları koyu zeminde biraz daha sakin */
.right-inner :deep(.v-field) {
  background: rgba(255, 255, 255, 0.04) !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
}

.right-inner :deep(.v-field--focused) {
  border-color: var(--rp-accent) !important;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.12) !important;
}

.terms-section {
  margin-bottom: 20px;
}

.terms-checkbox :deep(.v-selection-control__input) {
  color: var(--rp-accent) !important;
}

.terms-text {
  font-size: 0.85rem;
  line-height: 1.5;
  color: var(--rp-text-secondary);
}

.terms-link {
  color: var(--rp-accent);
  font-weight: 600;
  text-decoration: none;
}

.terms-link:hover {
  color: #b5c1ff;
}

.right-inner :deep(.register-submit) {
  border-radius: 12px !important;
  height: 50px !important;
  font-weight: 600 !important;
  text-transform: none !important;
  letter-spacing: 0.3px;
  background: var(--rp-gradient) !important;
  color: #fff !important;
  box-shadow: none !important;
  transition: transform 0.3s ease, box-shadow 0.3s ease !important;
}

.right-inner :deep(.register-submit:hover:not(:disabled)) {
  transform: translateY(-2px);
  box-shadow: 0 12px 40px rgba(99, 102, 241, 0.35) !important;
}

.divider {
  display: flex;
  align-items: center;
  gap: 16px;
  margin: 28px 0;
}

.divider span {
  font-size: 0.8rem;
  color: var(--rp-text-muted);
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--rp-border);
}

.login-section {
  text-align: center;
}

.login-section p {
  font-size: 0.88rem;
  color: var(--rp-text-secondary);
  margin-bottom: 14px;
}

.login-btn {
  border: 1.5px solid rgba(255, 255, 255, 0.12) !important;
  border-radius: 12px !important;
  color: var(--rp-text) !important;
  font-weight: 500 !important;
  text-transform: none !important;
}

.login-btn:hover {
  border-color: var(--rp-accent) !important;
  background: rgba(99, 102, 241, 0.06) !important;
}

.lang-switcher-wrapper {
  position: absolute;
  top: 24px;
  right: 24px;
  z-index: 100;
}

@media (max-width: 1024px) {
  .left-panel {
    display: none;
  }
}

@media (max-width: 480px) {
  .right-panel {
    padding: 24px 20px;
  }
  .form-header h1 {
    font-size: 1.4rem;
  }
}
</style>
