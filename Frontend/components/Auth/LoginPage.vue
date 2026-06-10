<template>
  <div class="login-page">
    <div class="lang-switcher-wrapper">
      <LanguageSwitcher />
    </div>
    <!-- LEFT: Image Carousel -->
    <div class="left-panel">
      <div
        v-for="(image, index) in backgroundImages"
        :key="index"
        :class="['carousel-slide', { active: currentImageIndex === index }]"
        :style="{ backgroundImage: `url(${image})` }"
      />
      <div class="left-overlay" />
      <div class="left-content">
        <div class="brand-badge">
          <v-icon size="16" color="white">mdi-lightning-bolt</v-icon>
          <span>{{ t("auth.brandBadge") }}</span>
        </div>
        <h2 class="left-title">
          {{ t("auth.heroTitle") }}
        </h2>
        <p class="left-subtitle">
          {{ t("auth.heroSubtitle") }}
        </p>
        <div class="carousel-dots">
          <div
            v-for="(_, index) in backgroundImages"
            :key="index"
            :class="['dot', { active: currentImageIndex === index }]"
            @click="goToSlide(index)"
          />
        </div>
      </div>
    </div>

    <!-- RIGHT: Login Form -->
    <div class="right-panel">
      <div class="right-inner">
        <div class="form-header">
          <div class="form-header-logo">
            <img :src="logoSrc" :alt="logoAlt" class="form-header-logo-img" />
          </div>
          <h1>{{ t("auth.welcomeBack") }}</h1>
          <p>
            {{ t("auth.signInSubtitle") }}
          </p>
        </div>

        <!-- Session Expired Alert -->
        <AlertBox
          v-if="route.query.expired && !sessionExpiredDismissed"
          variant="warning"
          :message="t('auth.sessionExpired')"
          @close="sessionExpiredDismissed = true"
        />

        <!-- Login API errors (from /api/auth/login) -->
        <AlertBox
          v-if="loginApiErrors.length > 0 && !emailNotConfirmed"
          variant="error"
          :message="loginApiErrors"
          @close="loginApiErrors = []"
        />

        <!-- Email not confirmed: show inline resend block -->
        <AlertBox
          v-if="emailNotConfirmed"
          :variant="resendAlertType"
          :message="resendAlertMessage"
          @close="onCloseEmailNotConfirmed"
        >
          <v-btn
            size="small"
            variant="outlined"
            :loading="resending"
            :disabled="resendButtonDisabled"
            class="mt-2"
            @click="onResendVerification"
          >
            <v-icon start size="16">{{ resendButtonIcon }}</v-icon>
            {{ resendButtonText }}
          </v-btn>
        </AlertBox>

        <v-form
          ref="loginForm"
          v-model="isFormValid"
          @submit.prevent="handleLogin"
        >
          <div class="input-group">
            <label>{{ t("auth.email") }}</label>
            <v-text-field
              v-model="form.email"
              type="email"
              :placeholder="t('auth.emailPlaceholder')"
              prepend-inner-icon="mdi-email-outline"
              :rules="[validationRules.required, validationRules.email]"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              autofocus
              class="custom-input"
            />
          </div>

          <div class="input-group">
            <label>{{ t("auth.password") }}</label>
            <v-text-field
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              :placeholder="t('auth.passwordPlaceholder')"
              prepend-inner-icon="mdi-lock-outline"
              :append-inner-icon="
                showPassword ? 'mdi-eye-off-outline' : 'mdi-eye-outline'
              "
              :rules="[validationRules.required]"
              variant="outlined"
              density="comfortable"
              hide-details="auto"
              class="custom-input"
              @click:append-inner="showPassword = !showPassword"
            />
          </div>

          <div
            class="options-row"
            :class="{ 'options-row--no-forgot': !showForgotPassword }"
          >
            <v-checkbox
              v-model="form.rememberMe"
              :label="t('auth.rememberMe')"
              density="compact"
              hide-details
              class="remember-checkbox"
            />
            <a
              v-if="showForgotPassword"
              class="forgot-link"
              @click.prevent="$router.push('/auth/forgot-password')"
            >
              {{ t("auth.forgotPassword") }}
            </a>
          </div>

          <v-btn
            type="submit"
            size="large"
            block
            :loading="authStore.isLoading"
            :disabled="!isFormValid"
            class="login-btn"
          >
            {{ t("auth.login") }}
          </v-btn>
        </v-form>

        <!-- Register Section (controlled by showRegister prop) -->
        <template v-if="showRegister">
          <div class="divider">
            <span>{{ t("auth.or") }}</span>
          </div>

          <div class="register-section">
            <p>
              {{ t("auth.noAccount") }}
            </p>
            <v-btn
              variant="outlined"
              class="register-btn"
              @click="$router.push('/auth/register')"
            >
              <v-icon start size="18">mdi-account-plus-outline</v-icon>
              {{ t("auth.createAccount") }}
            </v-btn>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { LoginRequest } from "~/types";
import { parseApiErrorMessages } from "~/utils/apiError";
import { ErrorCode } from "~/utils/errorCodes";

const props = withDefaults(
  defineProps<{
    showRegister?: boolean;
    showForgotPassword?: boolean;
  }>(),
  {
    showRegister: true, // false: register gizli, true yapınca açılır
    showForgotPassword: true, // false: forgot password linki gizli, true: gösterilir
  },
);

const { t } = useI18n();
const route = useRoute();
const authStore = useAuthStore();
const { validationRules } = useValidators();
const { getBackgroundImages, getLoginConfig, getLogo } = useAppData();

const logoSrc = computed(() => getLogo.value?.src || "/favicon.ico");
const logoAlt = computed(() => getLogo.value?.alt || "Logo");

const loginForm = ref();
const isFormValid = ref(false);
const showPassword = ref(false);
const loginApiErrors = ref<string[]>([]);
const emailNotConfirmed = ref(false);
const sessionExpiredDismissed = ref(false);
const resending = ref(false);
const currentImageIndex = ref(0);

// Resend verification UI state
const resendState = ref<"idle" | "sent" | "error">("idle");
const resendCooldown = ref(0);
let resendTimer: ReturnType<typeof setInterval> | null = null;
let resendErrorTimeout: ReturnType<typeof setTimeout> | null = null;

const clearResendTimers = () => {
  if (resendTimer) {
    clearInterval(resendTimer);
    resendTimer = null;
  }
  if (resendErrorTimeout) {
    clearTimeout(resendErrorTimeout);
    resendErrorTimeout = null;
  }
};

const resendAlertType = computed<"warning" | "success" | "error">(() => {
  if (resendState.value === "sent") return "success";
  if (resendState.value === "error") return "error";
  return "warning";
});

const resendAlertMessage = computed(() => {
  if (resendState.value === "sent") {
    return t("auth.resendEmailSent");
  }
  if (resendState.value === "error") {
    return t("auth.resendEmailError");
  }
  return loginApiErrors.value[0] || t("auth.emailNotConfirmed");
});

const resendButtonDisabled = computed(
  () => resendState.value === "sent" && resendCooldown.value > 0,
);

const resendButtonText = computed(() => {
  if (resendState.value === "sent" && resendCooldown.value > 0) {
    return `${t("auth.resendButtonSent")} (${resendCooldown.value}s)`;
  }
  return t("auth.resendButton");
});

const resendButtonIcon = computed(() =>
  resendState.value === "sent" && resendCooldown.value > 0
    ? "mdi-check-circle-outline"
    : "mdi-email-sync-outline",
);

const onCloseEmailNotConfirmed = () => {
  emailNotConfirmed.value = false;
  loginApiErrors.value = [];
  clearResendTimers();
  resendState.value = "idle";
  resendCooldown.value = 0;
};

const backgroundImages = computed(() => getBackgroundImages.value || []);
const loginConfig = computed(() => getLoginConfig.value);

const form = reactive<LoginRequest>({
  email: "",
  password: "",
  rememberMe: false,
  deviceId: "",
  deviceName: "",
});

// ── Carousel Logic ──
let imageInterval: ReturnType<typeof setInterval>;
let isPaused = false;

const goToSlide = (index: number) => {
  currentImageIndex.value = index;
};

const nextSlide = () => {
  if (!isPaused && backgroundImages.value.length > 0) {
    currentImageIndex.value =
      (currentImageIndex.value + 1) % backgroundImages.value.length;
  }
};

// ── Login Handler ──
const handleLogin = async () => {
  if (!isFormValid.value) return;
  loginApiErrors.value = [];
  emailNotConfirmed.value = false;
  clearResendTimers();
  resendState.value = "idle";
  resendCooldown.value = 0;
  try {
    await useAuth().login(form);
  } catch (error: any) {
    console.error("Login failed:", error);
    loginApiErrors.value = parseApiErrorMessages(error);
    const code = error?.response?.data?.error?.code ?? error?.data?.error?.code;
    if (code === ErrorCode.EmailNotConfirmed) {
      emailNotConfirmed.value = true;
    }
  }
};

const onResendVerification = async () => {
  if (!form.email) return;
  if (resendState.value === "sent" && resendCooldown.value > 0) return;
  resending.value = true;
  try {
    await useAuth().resendVerification(form.email);
    clearResendTimers();
    resendState.value = "sent";
    resendCooldown.value = 60;
    resendTimer = setInterval(() => {
      resendCooldown.value -= 1;
      if (resendCooldown.value <= 0) {
        resendCooldown.value = 0;
        clearResendTimers();
        // Alert yeşil kalsın (kullanıcı yeni mail bekliyor); sadece buton tekrar aktif olsun.
      }
    }, 1000);
  } catch {
    clearResendTimers();
    resendState.value = "error";
    resendCooldown.value = 0;
    resendErrorTimeout = setTimeout(() => {
      resendState.value = "idle";
      resendErrorTimeout = null;
    }, 5000);
  } finally {
    resending.value = false;
  }
};

// ── Lifecycle ──
onMounted(() => {
  // Pre-fill email from query
  const registeredEmail = route.query.email as string;
  if (registeredEmail) form.email = registeredEmail;

  // NOT: deviceId ve deviceName artık useAuth.login() içinde otomatik üretiliyor
  // (cookie-based, localStorage'a yazmıyor). Burada manuel set etmeye gerek yok.
  // Eski localStorage-based deviceId kodu güvenlik nedeniyle kaldırıldı.

  // Start carousel
  imageInterval = setInterval(
    nextSlide,
    loginConfig.value?.rotationInterval || 3000,
  );
});

onUnmounted(() => {
  if (imageInterval) clearInterval(imageInterval);
  clearResendTimers();
});
</script>

<style scoped>
/* ── Variables ── */
.login-page {
  --lp-bg: #0a0a0f;
  --lp-card-border: rgba(255, 255, 255, 0.06);
  --lp-input-bg: rgba(255, 255, 255, 0.04);
  --lp-input-border: rgba(255, 255, 255, 0.1);
  --lp-accent: #818cf8;
  --lp-accent-glow: rgba(129, 140, 248, 0.25);
  --lp-gradient: linear-gradient(135deg, #6366f1, #8b5cf6, #a855f7);
  --lp-text: #f1f1f4;
  --lp-text-secondary: rgba(255, 255, 255, 0.5);
  --lp-text-muted: rgba(255, 255, 255, 0.3);

  display: flex;
  min-height: 100vh;
  background: var(--lp-bg);
  color: var(--lp-text);
  font-family: inherit;
}

/* ── LEFT PANEL ── */
.left-panel {
  flex: 1.1;
  position: relative;
  overflow: hidden;
  display: flex;
  align-items: flex-end;
}

.carousel-slide {
  position: absolute;
  inset: 0;
  z-index: 0;
  background-size: cover;
  background-position: center;
  opacity: 0;
  /* Sadece crossfade — zoom/scale yok (önceki “geri küçülme” hissi kalkar) */
  transition: opacity 1s cubic-bezier(0.4, 0, 0.2, 1);
  transform: translateZ(0);
  backface-visibility: hidden;
}

.carousel-slide.active {
  z-index: 1;
  opacity: 1;
}

.left-overlay {
  position: absolute;
  inset: 0;
  background:
    linear-gradient(to right, transparent 60%, var(--lp-bg) 100%),
    linear-gradient(to top, rgba(10, 10, 15, 0.85) 0%, transparent 50%),
    linear-gradient(to bottom, rgba(10, 10, 15, 0.3) 0%, transparent 30%);
  z-index: 2;
}

.left-content {
  position: relative;
  z-index: 3;
  padding: 48px;
  width: 100%;
}

.brand-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 100px;
  padding: 8px 18px;
  font-size: 0.8rem;
  font-weight: 500;
  letter-spacing: 0.5px;
  margin-bottom: 24px;
}

.left-title {
  font-size: 2.4rem;
  font-weight: 700;
  line-height: 1.15;
  color: white;
  margin-bottom: 14px;
  max-width: 440px;
}

.left-subtitle {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.55);
  line-height: 1.6;
  max-width: 400px;
}

.carousel-dots {
  display: flex;
  gap: 8px;
  margin-top: 28px;
}

.dot {
  width: 32px;
  height: 3px;
  border-radius: 3px;
  background: rgba(255, 255, 255, 0.2);
  transition: all 0.5s ease;
  cursor: pointer;
}

.dot.active {
  width: 48px;
  background: var(--lp-accent);
  box-shadow: 0 0 12px var(--lp-accent-glow);
}

/* ── RIGHT PANEL ── */
.right-panel {
  flex: 0.9;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  position: relative;
  z-index: 5;
}

.right-panel::before {
  content: "";
  position: absolute;
  top: -200px;
  right: -200px;
  width: 500px;
  height: 500px;
  background: radial-gradient(
    circle,
    rgba(99, 102, 241, 0.06),
    transparent 70%
  );
  pointer-events: none;
}

.right-inner {
  width: 100%;
  max-width: 400px;
  animation: fadeUp 0.7s 0.2s cubic-bezier(0.22, 1, 0.36, 1) both;
}

@keyframes fadeUp {
  from {
    opacity: 0;
    transform: translateY(24px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Form Header */
.form-header {
  margin-bottom: 36px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.form-header-logo {
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.form-header-logo-img {
  max-height: 56px;
  width: auto;
  max-width: min(220px, 100%);
  height: auto;
  object-fit: contain;
  display: block;
}

.form-header h1 {
  font-size: 1.7rem;
  font-weight: 700;
  margin-bottom: 6px;
  width: 100%;
}

.form-header p {
  color: var(--lp-text-secondary);
  font-size: 0.92rem;
  width: 100%;
}

/* Input Groups */
.input-group {
  margin-bottom: 20px;
}

.input-group label {
  display: block;
  font-size: 0.82rem;
  font-weight: 500;
  color: var(--lp-text-secondary);
  margin-bottom: 8px;
  letter-spacing: 0.3px;
}

.custom-input :deep(.v-field) {
  background: var(--lp-input-bg) !important;
  border-color: var(--lp-input-border) !important;
  border-radius: 12px !important;
  color: var(--lp-text) !important;
}

.custom-input :deep(.v-field--focused) {
  border-color: var(--lp-accent) !important;
  box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1) !important;
}

.custom-input :deep(.v-field__input),
.custom-input :deep(.v-label),
.custom-input :deep(.v-icon) {
  color: rgba(255, 255, 255, 0.6) !important;
}

.custom-input :deep(.v-field__input) {
  color: var(--lp-text) !important;
}

/* Options Row */
.options-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 28px;
}

.options-row--no-forgot {
  justify-content: flex-start;
}

.remember-checkbox :deep(.v-label) {
  font-size: 0.85rem;
  color: var(--lp-text-secondary) !important;
}

.remember-checkbox :deep(.v-selection-control__input) {
  color: var(--lp-accent) !important;
}

.forgot-link {
  font-size: 0.85rem;
  color: var(--lp-accent);
  text-decoration: none;
  font-weight: 500;
  cursor: pointer;
  transition: color 0.2s;
}

.forgot-link:hover {
  color: #a5b4fc;
}

/* Login Button */
.login-btn {
  border-radius: 12px !important;
  height: 50px !important;
  font-weight: 600 !important;
  text-transform: none !important;
  background: var(--lp-gradient) !important;
  color: white !important;
  letter-spacing: 0.3px;
  transition: all 0.3s ease !important;
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-2px) !important;
  box-shadow: 0 12px 40px rgba(99, 102, 241, 0.35) !important;
}

/* Divider */
.divider {
  display: flex;
  align-items: center;
  gap: 16px;
  margin: 28px 0;
}

.divider span {
  font-size: 0.8rem;
  color: var(--lp-text-muted);
  white-space: nowrap;
}

.divider::before,
.divider::after {
  content: "";
  flex: 1;
  height: 1px;
  background: var(--lp-card-border);
}

/* Register Section */
.register-section {
  text-align: center;
}

.register-section p {
  font-size: 0.88rem;
  color: var(--lp-text-secondary);
  margin-bottom: 14px;
}

.register-btn {
  border: 1.5px solid rgba(255, 255, 255, 0.12) !important;
  border-radius: 12px !important;
  color: var(--lp-text) !important;
  font-weight: 500 !important;
  text-transform: none !important;
  transition: all 0.3s ease !important;
}

.register-btn:hover {
  border-color: var(--lp-accent) !important;
  background: rgba(99, 102, 241, 0.06) !important;
  transform: translateY(-1px) !important;
}

/* ── Responsive ── */
@media (max-width: 1024px) {
  .left-panel {
    display: none;
  }
  .right-panel {
    flex: 1;
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

.lang-switcher-wrapper {
  position: absolute;
  top: 24px;
  right: 24px;
  z-index: 100;
}
</style>
