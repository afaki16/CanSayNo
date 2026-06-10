<template>
  <div class="profile-page">
    <div class="mb-6">
      <BreadCrumb
        :items="[{ text: 'Ana Sayfa', to: '/dashboard' }, { text: 'Profilim' }]"
      />
    </div>

    <!-- ═══════════════ HERO ═══════════════ -->
    <v-card class="hero-card mb-6" rounded="xl" elevation="0" border>
      <!-- Gradient banner with decorative blobs -->
      <div class="hero-banner">
        <div class="blob blob-1" />
        <div class="blob blob-2" />
        <div class="blob blob-3" />
      </div>

      <v-card-text class="hero-body pt-0 px-6 pb-0">
        <div class="hero-top">
          <!-- Avatar -->
          <div class="avatar-wrap">
            <v-avatar size="108" color="primary" class="profile-avatar">
              <span class="avatar-initials">{{ initials }}</span>
            </v-avatar>
            <span class="online-dot" />
          </div>

          <!-- Name & email -->
          <div class="hero-meta">
            <h1 class="profile-name">{{ user?.fullName }}</h1>
            <p class="profile-email mt-1">
              <v-icon size="13" class="me-1">mdi-email-outline</v-icon>
              {{ user?.email }}
            </p>
          </div>

          <!-- Status chip -->
          <v-chip
            class="hero-status ml-auto"
            :color="statusColor"
            variant="tonal"
            size="small"
            :prepend-icon="
              statusColor === 'success'
                ? 'mdi-check-circle-outline'
                : 'mdi-alert-circle-outline'
            "
          >
            {{ statusLabel }}
          </v-chip>
        </div>
      </v-card-text>

      <!-- Stats strip -->
      <div class="stats-strip">
        <div class="stat-pill">
          <v-icon size="16" color="primary">mdi-monitor-multiple</v-icon>
          <div>
            <div class="stat-val">
              {{ loadingSessions ? "—" : activeSessions.length }}
            </div>
            <div class="stat-lbl">Aktif Oturum</div>
          </div>
        </div>
        <div class="stat-sep" />
        <div class="stat-pill">
          <v-icon size="16" color="primary"
            >mdi-calendar-account-outline</v-icon
          >
          <div>
            <div class="stat-val">{{ memberDays }}</div>
            <div class="stat-lbl">Gün Üye</div>
          </div>
        </div>
        <div class="stat-sep" />
        <div class="stat-pill">
          <v-icon size="16" color="primary">mdi-login-variant</v-icon>
          <div>
            <div class="stat-val">
              {{ user?.lastLoginDate ? lastLoginShort : "—" }}
            </div>
            <div class="stat-lbl">Son Giriş</div>
          </div>
        </div>
      </div>
    </v-card>

    <!-- ═══════════════ MAIN ═══════════════ -->
    <v-row>
      <!-- ── Left: Account Info ── -->
      <v-col cols="12" md="4">
        <v-card rounded="xl" elevation="0" border height="100%">
          <v-card-title class="section-title pa-5">
            <v-avatar
              color="primary"
              variant="tonal"
              size="32"
              rounded="lg"
              class="mr-3"
            >
              <v-icon size="16">mdi-account-circle-outline</v-icon>
            </v-avatar>
            Hesap Bilgileri
          </v-card-title>
          <v-divider />

          <v-card-text class="pa-4">
            <div class="info-grid">
              <div class="info-tile">
                <span class="tile-lbl">Ad</span>
                <span class="tile-val">{{ user?.firstName || "—" }}</span>
              </div>

              <div class="info-tile">
                <span class="tile-lbl">Soyad</span>
                <span class="tile-val">{{ user?.lastName || "—" }}</span>
              </div>

              <div class="info-tile info-tile--wide">
                <span class="tile-lbl">E-posta</span>
                <div class="d-flex align-center ga-1">
                  <span class="tile-val">{{ user?.email }}</span>
                  <v-tooltip
                    :text="user?.emailConfirmed ? 'Doğrulandı' : 'Doğrulanmadı'"
                    location="top"
                  >
                    <template #activator="{ props }">
                      <v-icon
                        v-bind="props"
                        size="14"
                        :color="user?.emailConfirmed ? 'success' : 'warning'"
                      >
                        {{
                          user?.emailConfirmed
                            ? "mdi-check-circle"
                            : "mdi-alert-circle-outline"
                        }}
                      </v-icon>
                    </template>
                  </v-tooltip>
                </div>
              </div>

              <div v-if="user?.phoneNumber" class="info-tile info-tile--wide">
                <span class="tile-lbl">Telefon</span>
                <div class="d-flex align-center ga-1">
                  <span class="tile-val">{{ user.phoneNumber }}</span>
                  <v-tooltip
                    :text="user?.phoneConfirmed ? 'Doğrulandı' : 'Doğrulanmadı'"
                    location="top"
                  >
                    <template #activator="{ props }">
                      <v-icon
                        v-bind="props"
                        size="14"
                        :color="user?.phoneConfirmed ? 'success' : 'warning'"
                      >
                        {{
                          user?.phoneConfirmed
                            ? "mdi-check-circle"
                            : "mdi-alert-circle-outline"
                        }}
                      </v-icon>
                    </template>
                  </v-tooltip>
                </div>
              </div>

              <div class="info-tile">
                <span class="tile-lbl">Kayıt Tarihi</span>
                <span class="tile-val">{{
                  user?.createdDate ? dateFormat(user.createdDate) : "—"
                }}</span>
              </div>

              <div class="info-tile">
                <span class="tile-lbl">Son Giriş</span>
                <span class="tile-val tile-val--sm">{{
                  user?.lastLoginDate ? dateTimeFormat(user.lastLoginDate) : "—"
                }}</span>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <!-- ── Right: Sessions ── -->
      <v-col cols="12" md="8">
        <v-card rounded="xl" elevation="0" border>
          <v-card-title class="section-title d-flex align-center pa-5">
            <v-avatar
              color="primary"
              variant="tonal"
              size="32"
              rounded="lg"
              class="mr-3"
            >
              <v-icon size="16">mdi-monitor-multiple</v-icon>
            </v-avatar>
            <span>Oturumlarım</span>
            <v-chip
              v-if="!loadingSessions && activeSessions.length"
              size="x-small"
              color="primary"
              variant="tonal"
              class="ml-2"
            >
              {{ activeSessions.length }} aktif
            </v-chip>
            <v-spacer />
            <v-btn
              size="small"
              variant="text"
              color="primary"
              prepend-icon="mdi-refresh"
              :loading="loadingSessions"
              @click="loadSessions"
            >
              Yenile
            </v-btn>
          </v-card-title>
          <v-divider />

          <v-card-text class="pa-0">
            <v-progress-linear
              v-if="loadingSessions"
              indeterminate
              color="primary"
            />

            <!-- Empty -->
            <div
              v-if="!loadingSessions && !sessions.length"
              class="empty-state pa-10"
            >
              <v-avatar color="grey-lighten-3" size="64" class="mb-4">
                <v-icon size="30" color="grey">mdi-monitor-off</v-icon>
              </v-avatar>
              <p class="text-body-2 text-medium-emphasis">
                Aktif oturum bulunamadı.
              </p>
            </div>

            <!-- Active sessions -->
            <div
              v-if="!loadingSessions && activeSessions.length"
              class="pa-4 d-flex flex-column ga-2"
            >
              <div
                v-for="session in activeSessions"
                :key="session.id"
                class="session-card"
                :class="{ 'session-card--current': session.isCurrentSession }"
              >
                <div
                  class="session-accent"
                  :class="{
                    'session-accent--current': session.isCurrentSession,
                  }"
                />

                <v-avatar
                  :color="session.isCurrentSession ? 'primary' : 'grey'"
                  variant="tonal"
                  size="44"
                  rounded="lg"
                  class="flex-shrink-0"
                >
                  <v-icon size="22">{{
                    deviceIcon(session.deviceType)
                  }}</v-icon>
                </v-avatar>

                <div class="flex-1 min-w-0">
                  <div class="d-flex align-center ga-2 flex-wrap">
                    <span class="session-name">{{ session.deviceName }}</span>
                    <v-chip
                      v-if="session.isCurrentSession"
                      size="x-small"
                      color="primary"
                      variant="flat"
                      rounded
                    >
                      Bu cihaz
                    </v-chip>
                    <v-chip
                      size="x-small"
                      color="success"
                      variant="tonal"
                      rounded
                    >
                      <v-icon start size="9">mdi-circle</v-icon>
                      Aktif
                    </v-chip>
                  </div>
                  <div class="session-meta mt-1">
                    <span>
                      <v-icon size="11">mdi-ip-network-outline</v-icon>
                      {{ session.ipAddress }}
                    </span>
                    <span class="mx-1 opacity-30">·</span>
                    <span>
                      <v-icon size="11">mdi-calendar-outline</v-icon>
                      {{ formatDate(session.createdDate) }}
                    </span>
                    <span class="mx-1 opacity-30">·</span>
                    <span class="text-success font-weight-medium">
                      <v-icon size="11" color="success"
                        >mdi-clock-outline</v-icon
                      >
                      {{ remainingLabel(session) }} kaldı
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Inactive (collapsible) -->
            <div
              v-if="!loadingSessions && inactiveSessions.length"
              class="px-4 pb-4"
            >
              <v-divider class="mb-3" />
              <div
                class="history-toggle d-flex align-center ga-2 mb-3"
                @click="showHistory = !showHistory"
              >
                <v-icon size="16" color="grey">{{
                  showHistory ? "mdi-chevron-up" : "mdi-chevron-down"
                }}</v-icon>
                <span class="text-caption text-medium-emphasis">
                  Geçmiş oturumlar ({{ inactiveSessions.length }})
                </span>
              </div>

              <div v-if="showHistory" class="d-flex flex-column ga-2">
                <div
                  v-for="session in inactiveSessions"
                  :key="session.id"
                  class="session-card session-card--inactive"
                >
                  <div class="session-accent session-accent--error" />
                  <v-avatar
                    color="grey"
                    variant="tonal"
                    size="44"
                    rounded="lg"
                    class="flex-shrink-0"
                  >
                    <v-icon size="22" color="grey">{{
                      deviceIcon(session.deviceType)
                    }}</v-icon>
                  </v-avatar>
                  <div class="flex-1 min-w-0">
                    <span class="session-name text-medium-emphasis">{{
                      session.deviceName
                    }}</span>
                    <div class="session-meta mt-1">
                      <span>
                        <v-icon size="11">mdi-ip-network-outline</v-icon>
                        {{ session.ipAddress }}
                      </span>
                      <span class="mx-1 opacity-30">·</span>
                      <span>
                        <v-icon size="11">mdi-calendar-outline</v-icon>
                        {{ formatDate(session.createdDate) }}
                      </span>
                      <span class="mx-1 opacity-30">·</span>
                      <span class="text-error">Sona erdi</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- ═══════════════ CHANGE PASSWORD ═══════════════ -->
    <v-row class="mt-0">
      <v-col cols="12" md="6">
        <v-card rounded="xl" elevation="0" border>
          <v-card-title class="section-title pa-5">
            <v-avatar
              color="primary"
              variant="tonal"
              size="32"
              rounded="lg"
              class="mr-3"
            >
              <v-icon size="16">mdi-lock-reset</v-icon>
            </v-avatar>
            Şifre Değiştir
          </v-card-title>
          <v-divider />

          <v-card-text class="pa-5">
            <v-form
              ref="passwordFormRef"
              @submit.prevent="submitChangePassword"
            >
              <v-text-field
                v-model="currentPassword"
                label="Mevcut Şifre"
                :type="showCurrent ? 'text' : 'password'"
                :append-inner-icon="showCurrent ? 'mdi-eye-off' : 'mdi-eye'"
                :rules="[validationRules.required]"
                variant="outlined"
                density="comfortable"
                class="mb-3"
                @click:append-inner="showCurrent = !showCurrent"
              />
              <v-text-field
                v-model="newPassword"
                label="Yeni Şifre"
                :type="showNew ? 'text' : 'password'"
                :append-inner-icon="showNew ? 'mdi-eye-off' : 'mdi-eye'"
                :rules="[validationRules.password]"
                variant="outlined"
                density="comfortable"
                class="mb-3"
                @click:append-inner="showNew = !showNew"
              />
              <v-text-field
                v-model="confirmNewPassword"
                label="Yeni Şifre Tekrar"
                :type="showConfirm ? 'text' : 'password'"
                :append-inner-icon="showConfirm ? 'mdi-eye-off' : 'mdi-eye'"
                :rules="[validationRules.confirmPassword(newPassword)]"
                variant="outlined"
                density="comfortable"
                class="mb-4"
                @click:append-inner="showConfirm = !showConfirm"
              />
              <v-btn
                type="submit"
                color="primary"
                :loading="isChangingPassword"
                block
              >
                Şifreyi Güncelle
              </v-btn>
            </v-form>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import type { Session } from "~/types";
import { dateFormat, dateTimeFormat } from "~/utils/datesFormat";
import BreadCrumb from "~/components/BreadCrumb.vue";
import { useValidators } from "~/composables/useValidators";

definePageMeta({
  title: "Profilim",
  requiresAuth: true,
});

useHead({ title: "Profilim - Kickstart" });

const authStore = useAuthStore();
const { getUserSessions, changePassword } = useAuth();
const { validationRules } = useValidators();

const user = computed(() => authStore.user);

const initials = computed(() => {
  const u = authStore.user;
  if (!u) return "?";
  return `${u.firstName?.charAt(0) ?? ""}${u.lastName?.charAt(0) ?? ""}`.toUpperCase();
});

const statusColor = computed(() => {
  switch (authStore.user?.status) {
    case 1:
      return "success";
    case 2:
      return "warning";
    case 3:
      return "error";
    default:
      return "grey";
  }
});

const statusLabel = computed(() => {
  switch (authStore.user?.status) {
    case 1:
      return "Aktif";
    case 2:
      return "Pasif";
    case 3:
      return "Yasaklı";
    default:
      return "Bilinmiyor";
  }
});

const memberDays = computed(() => {
  const d = authStore.user?.createdDate;
  if (!d) return "—";
  return Math.floor((Date.now() - new Date(d).getTime()) / 86_400_000);
});

const lastLoginShort = computed(() => {
  const d = authStore.user?.lastLoginDate;
  if (!d) return "—";
  const date = new Date(d);
  const today = new Date();
  if (date.toDateString() === today.toDateString()) return "Bugün";
  const diff = Math.floor((today.getTime() - date.getTime()) / 86_400_000);
  if (diff === 1) return "Dün";
  if (diff < 7) return `${diff} gün önce`;
  return dateFormat(d);
});

// Sessions
const sessions = ref<Session[]>([]);
const loadingSessions = ref(false);
const showHistory = ref(false);

const activeSessions = computed(() =>
  sessions.value.filter((s: Session) => s.isActive),
);
const inactiveSessions = computed(() =>
  sessions.value.filter((s: Session) => !s.isActive),
);

const loadSessions = async () => {
  loadingSessions.value = true;
  try {
    sessions.value = await getUserSessions();
  } catch {
    sessions.value = [];
  } finally {
    loadingSessions.value = false;
  }
};

const deviceIcon = (type: string) => {
  switch (type?.toLowerCase()) {
    case "mobile":
      return "mdi-cellphone";
    case "tablet":
      return "mdi-tablet";
    default:
      return "mdi-monitor";
  }
};

const formatDate = (iso: string) => dateTimeFormat(iso);

const remainingLabel = (session: Session) => {
  const raw = session.remainingTime?.toString() ?? "";
  if (!raw || raw.startsWith("-")) return "—";
  let days = 0;
  let timePart = raw;
  if (raw.includes(".")) {
    const dot = raw.indexOf(".");
    days = parseInt(raw.slice(0, dot) ?? "0");
    timePart = raw.slice(dot + 1);
  }
  const [h, m] = timePart.split(":").map(Number);
  if (days >= 1) return `${days} gün`;
  if (h >= 1) return `${h} saat`;
  if (m >= 1) return `${m} dakika`;
  return "< 1 dakika";
};

// Change password
const passwordFormRef = ref();
const currentPassword = ref("");
const newPassword = ref("");
const confirmNewPassword = ref("");
const showCurrent = ref(false);
const showNew = ref(false);
const showConfirm = ref(false);
const isChangingPassword = ref(false);

const submitChangePassword = async () => {
  const { valid } = await passwordFormRef.value.validate();
  if (!valid) return;
  isChangingPassword.value = true;
  try {
    await changePassword(currentPassword.value, newPassword.value);
    passwordFormRef.value.reset();
    currentPassword.value = "";
    newPassword.value = "";
    confirmNewPassword.value = "";
  } finally {
    isChangingPassword.value = false;
  }
};

onMounted(loadSessions);
</script>

<style scoped>
.profile-page {
  max-width: 1080px;
  padding: 24px;
}

/* ══════════════════════════════════════════
   HERO
══════════════════════════════════════════ */
.hero-card {
  overflow: visible;
}

.hero-banner {
  height: 140px;
  border-radius: 12px 12px 0 0;
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 45%, #a855f7 100%);
  position: relative;
  overflow: hidden;
}

.blob {
  position: absolute;
  border-radius: 50%;
  opacity: 0.18;
  filter: blur(2px);
  background: white;
}
.blob-1 {
  width: 160px;
  height: 160px;
  top: -60px;
  left: -40px;
}
.blob-2 {
  width: 100px;
  height: 100px;
  top: 20px;
  right: 80px;
  opacity: 0.12;
}
.blob-3 {
  width: 200px;
  height: 200px;
  bottom: -100px;
  right: -60px;
  opacity: 0.08;
}

.hero-body {
  overflow: visible;
}

.hero-top {
  display: flex;
  align-items: flex-end;
  gap: 18px;
  margin-top: -54px;
  flex-wrap: wrap;
  padding-bottom: 16px;
}

.avatar-wrap {
  position: relative;
  flex-shrink: 0;
}

.profile-avatar {
  border: 4px solid white !important;
  box-shadow: 0 8px 24px rgba(79, 70, 229, 0.3) !important;
}

.avatar-initials {
  font-size: 2.2rem;
  font-weight: 800;
  color: white;
  letter-spacing: -1px;
}

.online-dot {
  position: absolute;
  bottom: 6px;
  right: 6px;
  width: 16px;
  height: 16px;
  background: #22c55e;
  border: 3px solid white;
  border-radius: 50%;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.25);
}

.hero-meta {
  padding-bottom: 4px;
}

.profile-name {
  font-size: 1.5rem;
  font-weight: 800;
  color: #111827;
  line-height: 1.15;
  letter-spacing: -0.4px;
}

.profile-email {
  font-size: 0.82rem;
  color: #6b7280;
  display: flex;
  align-items: center;
  margin: 0;
}

.hero-status {
  margin-bottom: 4px;
}

/* Stats strip */
.stats-strip {
  display: flex;
  align-items: center;
  gap: 0;
  border-top: 1px solid rgba(0, 0, 0, 0.07);
  background: #fafafa;
  border-radius: 0 0 12px 12px;
  overflow: hidden;
}

.stat-pill {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 20px;
}

.stat-sep {
  width: 1px;
  height: 36px;
  background: rgba(0, 0, 0, 0.08);
  flex-shrink: 0;
}

.stat-val {
  font-size: 1.25rem;
  font-weight: 700;
  color: #111827;
  line-height: 1;
}

.stat-lbl {
  font-size: 0.7rem;
  color: #9ca3af;
  margin-top: 2px;
}

/* ══════════════════════════════════════════
   SECTION TITLES
══════════════════════════════════════════ */
.section-title {
  font-size: 0.88rem !important;
  font-weight: 700 !important;
  color: #111827 !important;
  display: flex;
  align-items: center;
}

/* ══════════════════════════════════════════
   INFO GRID
══════════════════════════════════════════ */
.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}

.info-tile {
  background: #f9fafb;
  border: 1px solid #f3f4f6;
  border-radius: 10px;
  padding: 10px 14px;
  display: flex;
  flex-direction: column;
  gap: 3px;
  transition: border-color 0.15s;
}

.info-tile:hover {
  border-color: rgba(79, 70, 229, 0.25);
}

.info-tile--wide {
  grid-column: span 2;
}

.tile-lbl {
  font-size: 0.68rem;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  font-weight: 600;
}

.tile-val {
  font-size: 0.875rem;
  font-weight: 600;
  color: #111827;
  word-break: break-all;
}

.tile-val--sm {
  font-size: 0.8rem;
}

/* ══════════════════════════════════════════
   SESSION CARDS
══════════════════════════════════════════ */
.session-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 12px 14px;
  border-radius: 12px;
  border: 1px solid #f3f4f6;
  background: #fafafa;
  position: relative;
  overflow: hidden;
  transition:
    border-color 0.15s,
    box-shadow 0.15s;
}

.session-card:hover {
  border-color: rgba(79, 70, 229, 0.2);
  box-shadow: 0 2px 10px rgba(79, 70, 229, 0.06);
}

.session-card--current {
  background: rgba(79, 70, 229, 0.03);
  border-color: rgba(79, 70, 229, 0.2);
}

.session-card--inactive {
  opacity: 0.6;
}

.session-accent {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 3px;
  background: #e5e7eb;
  border-radius: 3px 0 0 3px;
}

.session-accent--current {
  background: #4f46e5;
}
.session-accent--error {
  background: #ef4444;
}

.session-name {
  font-size: 0.875rem;
  font-weight: 600;
  color: #111827;
}

.session-meta {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 0.72rem;
  color: rgba(0, 0, 0, 0.45);
  flex-wrap: wrap;
}

.history-toggle {
  cursor: pointer;
  user-select: none;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 160px;
}

.min-w-0 {
  min-width: 0;
}
.flex-1 {
  flex: 1;
}
</style>
