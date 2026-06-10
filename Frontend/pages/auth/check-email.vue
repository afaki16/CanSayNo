<template>
  <div class="check-container" :style="{ background: 'var(--theme-gradient-sidebar)' }">
    <div class="check-card">
      <v-icon size="80" color="white" class="email-icon">mdi-email-fast-outline</v-icon>

      <h1 class="title">Email kutunuzu kontrol edin</h1>
      <p v-if="email" class="subtitle">
        <strong>{{ email }}</strong> adresine bir doğrulama bağlantısı gönderdik.
      </p>
      <p v-else class="subtitle">
        Email adresinize bir doğrulama bağlantısı gönderdik.
      </p>

      <p class="hint">
        Email gelmediyse <strong>spam</strong> klasörünü kontrol etmeyi unutmayın.
      </p>

      <div class="actions">
        <v-btn
          color="white"
          variant="outlined"
          size="large"
          class="action-btn"
          :loading="resending"
          :disabled="!email"
          @click="onResend"
        >
          <v-icon start>mdi-email-sync-outline</v-icon>
          Tekrar Gönder
        </v-btn>

        <v-btn
          variant="text"
          color="white"
          class="link-btn"
          @click="goToLogin"
        >
          Giriş sayfasına dön
        </v-btn>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { parseApiErrorMessages } from '~/utils/apiError'

definePageMeta({
  layout: false,
  middleware: 'guest'
})

const route = useRoute()
const router = useRouter()
const auth = useAuth()
const toast = useToast()

const email = computed(() => (route.query.email as string) || '')
const resending = ref(false)

const goToLogin = () => router.push('/')

const onResend = async () => {
  if (!email.value) return
  resending.value = true
  try {
    await auth.resendVerification(email.value)
    toast.success('Doğrulama bağlantısı tekrar gönderildi.')
  } catch (error) {
    const messages = parseApiErrorMessages(error)
    toast.error(messages[0] || 'İstek gönderilemedi.')
  } finally {
    resending.value = false
  }
}

useHead({
  title: 'Email Doğrulama - Kickstart'
})
</script>

<style scoped>
.check-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.check-card {
  width: 100%;
  max-width: 480px;
  padding: 56px 40px;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.18);
  border-radius: 20px;
  color: white;
  text-align: center;
  box-shadow: 0 25px 45px rgba(0, 0, 0, 0.25);
}

.email-icon {
  margin-bottom: 16px;
  animation: float 3s ease-in-out infinite;
}

.title {
  font-size: 1.85rem;
  font-weight: 700;
  margin: 0 0 12px;
}

.subtitle {
  color: rgba(255, 255, 255, 0.85);
  font-size: 1.05rem;
  margin: 0 0 8px;
}

.hint {
  color: rgba(255, 255, 255, 0.65);
  font-size: 0.95rem;
  margin: 16px 0 28px;
}

.actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
  align-items: center;
}

.action-btn {
  border-radius: 12px !important;
  font-weight: 600 !important;
  text-transform: none !important;
  min-width: 200px;
}

.link-btn {
  text-transform: none !important;
  font-weight: 500 !important;
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-8px); }
}
</style>
