<template>
  <div class="verify-container" :style="{ background: 'var(--theme-gradient-sidebar)' }">
    <div class="verify-card">
      <div v-if="state === 'loading'" class="state-block">
        <v-progress-circular indeterminate color="white" size="56" width="4" />
        <h2 class="title">Email adresiniz doğrulanıyor...</h2>
        <p class="subtitle">Lütfen bekleyin.</p>
      </div>

      <div v-else-if="state === 'success'" class="state-block">
        <v-icon size="72" color="success">mdi-check-circle-outline</v-icon>
        <h2 class="title">Email'iniz doğrulandı</h2>
        <p class="subtitle">Artık giriş yapabilirsiniz.</p>
        <v-btn
          color="white"
          variant="outlined"
          size="large"
          class="action-btn"
          @click="goToLogin"
        >
          Giriş Sayfasına Git
        </v-btn>
      </div>

      <div v-else class="state-block">
        <v-icon size="72" color="error">mdi-alert-circle-outline</v-icon>
        <h2 class="title">Bağlantı geçersiz</h2>
        <p class="subtitle">{{ errorMessage }}</p>

        <div class="actions-row">
          <v-btn
            color="white"
            variant="outlined"
            size="large"
            class="action-btn"
            :loading="resending"
            :disabled="!email"
            @click="onResend"
          >
            Tekrar Gönder
          </v-btn>
          <v-btn
            color="white"
            variant="text"
            size="large"
            class="action-btn"
            @click="goToLogin"
          >
            Giriş Sayfası
          </v-btn>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { parseApiErrorMessages } from '~/utils/apiError'

definePageMeta({
  layout: false
})

const route = useRoute()
const router = useRouter()
const auth = useAuth()
const toast = useToast()

const state = ref<'loading' | 'success' | 'error'>('loading')
const errorMessage = ref('Doğrulama bağlantısı geçersiz veya süresi dolmuş.')
const resending = ref(false)

const email = computed(() => (route.query.email as string) || '')
const token = computed(() => (route.query.token as string) || '')

const goToLogin = () => router.push('/')

const onResend = async () => {
  if (!email.value) return
  resending.value = true
  try {
    await auth.resendVerification(email.value)
    toast.success('Doğrulama bağlantısı tekrar gönderildi. Lütfen email kutunuzu kontrol edin.')
  } catch (error) {
    const messages = parseApiErrorMessages(error)
    toast.error(messages[0] || 'İstek gönderilemedi.')
  } finally {
    resending.value = false
  }
}

onMounted(async () => {
  if (!email.value || !token.value) {
    state.value = 'error'
    errorMessage.value = 'Bağlantı eksik veya hatalı.'
    return
  }

  try {
    const response = await auth.verifyEmail(email.value, token.value)
    if (response?.success || response?.isSuccess) {
      state.value = 'success'
    } else {
      state.value = 'error'
      const messages = parseApiErrorMessages({ data: response })
      errorMessage.value = messages[0] || errorMessage.value
    }
  } catch (error) {
    state.value = 'error'
    const messages = parseApiErrorMessages(error)
    errorMessage.value = messages[0] || errorMessage.value
  }
})

useHead({
  title: 'Email Doğrulama - Kickstart'
})
</script>

<style scoped>
.verify-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.verify-card {
  width: 100%;
  max-width: 460px;
  padding: 48px 40px;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.18);
  border-radius: 20px;
  color: white;
  text-align: center;
  box-shadow: 0 25px 45px rgba(0, 0, 0, 0.25);
}

.state-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.title {
  font-size: 1.75rem;
  font-weight: 700;
  margin: 8px 0 0;
}

.subtitle {
  color: rgba(255, 255, 255, 0.78);
  font-size: 1rem;
  margin: 0;
}

.actions-row {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  justify-content: center;
  margin-top: 8px;
}

.action-btn {
  border-radius: 12px !important;
  font-weight: 600 !important;
  text-transform: none !important;
}
</style>
