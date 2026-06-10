<template>
  <v-card variant="outlined" class="sessions-card">
    <!-- Header -->
    <v-card-title class="d-flex align-center flex-wrap ga-2 pa-5">
      <div class="d-flex align-center ga-2">
        <v-icon color="primary">mdi-monitor-multiple</v-icon>
        <span class="sessions-title">Aktif Oturumlarım</span>
        <v-chip v-if="activeSessions.length" size="x-small" color="primary" variant="tonal">
          {{ activeSessions.length }}
        </v-chip>
      </div>
      <v-spacer />
      <div class="d-flex align-center ga-2">
        <v-btn size="small" variant="text" prepend-icon="mdi-refresh" :loading="loading" @click="loadSessions">
          Yenile
        </v-btn>
        <v-btn
          v-if="activeSessions.length > 0"
          size="small"
          variant="tonal"
          color="error"
          prepend-icon="mdi-logout-variant"
          :loading="loggingOutAll"
          @click="confirmLogoutAll = true"
        >
          Tümünden Çık
        </v-btn>
      </div>
    </v-card-title>

    <v-divider />

    <v-card-text class="pa-0">
      <v-progress-linear v-if="loading" indeterminate color="primary" />

      <!-- Boş durum -->
      <div v-if="!loading && activeSessions.length === 0" class="empty-state pa-8 text-center">
        <v-icon size="48" color="grey-lighten-1" class="mb-3">mdi-monitor-off</v-icon>
        <p class="text-body-2 text-medium-emphasis">Aktif oturum bulunamadı.</p>
      </div>

      <!-- Aktif oturumlar -->
      <div v-if="!loading && activeSessions.length > 0" class="px-3 pt-3">
        <div
          v-for="session in activeSessions"
          :key="session.id"
          class="session-row"
          :class="{ 'current-session': session.isCurrentSession }"
        >
          <div class="session-left">
            <v-avatar :color="session.isCurrentSession ? 'primary' : 'grey'" variant="tonal" size="40" rounded="lg">
              <v-icon size="20">{{ deviceIcon(session.deviceType) }}</v-icon>
            </v-avatar>
            <div class="session-info">
              <div class="d-flex align-center ga-2 flex-wrap">
                <span class="session-device">{{ session.deviceName }}</span>
                <v-chip v-if="session.isCurrentSession" size="x-small" color="primary" variant="flat">Bu cihaz</v-chip>
              </div>
              <div class="session-meta">
                <v-icon size="11">mdi-ip-network-outline</v-icon>
                {{ session.ipAddress }}
                <span class="dot">·</span>
                <v-icon size="11">mdi-calendar-outline</v-icon>
                {{ formatDate(session.createdDate) }}
                <span class="dot">·</span>
                <v-icon size="11" color="success">mdi-clock-outline</v-icon>
                <span class="text-success">{{ remainingLabel(session) }} kaldı</span>
              </div>
            </div>
          </div>

        </div>
      </div>

      <!-- Geçmiş oturumlar (collapsible) -->
      <div v-if="!loading && inactiveSessions.length > 0" class="px-3 pb-3">
        <v-divider class="my-3" />
        <div
          class="history-toggle d-flex align-center ga-1 cursor-pointer mb-2"
          @click="showHistory = !showHistory"
        >
          <v-icon size="16" color="grey">{{ showHistory ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          <span class="text-caption text-medium-emphasis">
            Geçmiş oturumlar ({{ inactiveSessions.length }})
          </span>
        </div>

        <div v-if="showHistory">
          <div
            v-for="session in inactiveSessions"
            :key="session.id"
            class="session-row session-row--inactive"
          >
            <div class="session-left">
              <v-avatar color="grey" variant="tonal" size="40" rounded="lg">
                <v-icon size="20" color="grey">{{ deviceIcon(session.deviceType) }}</v-icon>
              </v-avatar>
              <div class="session-info">
                <span class="session-device text-medium-emphasis">{{ session.deviceName }}</span>
                <div class="session-meta">
                  <v-icon size="11">mdi-ip-network-outline</v-icon>
                  {{ session.ipAddress }}
                  <span class="dot">·</span>
                  <v-icon size="11">mdi-calendar-outline</v-icon>
                  {{ formatDate(session.createdDate) }}
                  <span class="dot">·</span>
                  <span class="text-error">Sona erdi</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!loading && (activeSessions.length > 0 || inactiveSessions.length > 0)" class="pb-3" />
    </v-card-text>

    <!-- Confirm Dialog -->
    <v-dialog v-model="confirmLogoutAll" max-width="400">
      <v-card rounded="xl">
        <v-card-title class="pt-5 px-5 d-flex align-center ga-2">
          <v-icon color="error">mdi-alert-circle-outline</v-icon>
          Tüm cihazlardan çıkış
        </v-card-title>
        <v-card-text class="px-5 text-body-2">
          Tüm cihazlardan çıkış yapılacak ve mevcut oturumunuz da sonlandırılacak. Devam etmek istiyor musunuz?
        </v-card-text>
        <v-card-actions class="px-5 pb-5">
          <v-spacer />
          <v-btn variant="text" @click="confirmLogoutAll = false">İptal</v-btn>
          <v-btn color="error" variant="tonal" :loading="loggingOutAll" @click="doLogoutAll">Evet, çıkış yap</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Session } from '~/types'
import { dateTimeFormat } from '~/utils/datesFormat'

const { getUserSessions, logoutAllDevices } = useAuth()

const sessions      = ref<Session[]>([])
const loading       = ref(false)
const loggingOutAll = ref(false)
const confirmLogoutAll = ref(false)
const showHistory = ref(false)

const activeSessions   = computed(() => sessions.value.filter((s: Session) => s.isActive))
const inactiveSessions = computed(() => sessions.value.filter((s: Session) => !s.isActive))

const loadSessions = async () => {
  loading.value = true
  try {
    sessions.value = await getUserSessions()
  } catch {
    sessions.value = []
  } finally {
    loading.value = false
  }
}

const deviceIcon = (type: string) => {
  switch (type?.toLowerCase()) {
    case 'mobile': return 'mdi-cellphone'
    case 'tablet': return 'mdi-tablet'
    default:       return 'mdi-monitor'
  }
}

const formatDate = (iso: string) => dateTimeFormat(iso)

const remainingLabel = (session: Session) => {
  const raw = session.remainingTime?.toString() ?? ''
  if (!raw || raw.startsWith('-')) return '—'

  let days = 0
  let timePart = raw
  if (raw.includes('.')) {
    const dotIdx = raw.indexOf('.')
    days = parseInt(raw.slice(0, dotIdx) ?? '0')
    timePart = raw.slice(dotIdx + 1)
  }
  const [h, m] = timePart.split(':').map(Number)
  if (days >= 1) return `${days} gün`
  if (h >= 1)    return `${h} saat`
  if (m >= 1)    return `${m} dakika`
  return '< 1 dakika'
}

const doLogoutAll = async () => {
  loggingOutAll.value = true
  try {
    await logoutAllDevices()
  } finally {
    loggingOutAll.value = false
    confirmLogoutAll.value = false
  }
}

onMounted(loadSessions)
</script>

<style scoped>
.sessions-card {
  border-radius: 16px;
  overflow: hidden;
}

.sessions-title {
  font-size: 0.95rem;
  font-weight: 600;
}

/* Satır */
.session-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 8px;
  border-radius: 10px;
  margin-bottom: 4px;
  transition: background 0.15s;
}
.session-row:hover {
  background: rgba(var(--v-theme-primary), 0.04);
}
.session-row--inactive {
  opacity: 0.55;
}
.current-session {
  background: rgba(var(--v-theme-primary), 0.05);
  border: 1px solid rgba(var(--v-theme-primary), 0.2);
}

.session-left {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.session-info {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.session-device {
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.session-meta {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.75rem;
  color: rgba(0, 0, 0, 0.45);
}

.dot {
  color: rgba(0, 0, 0, 0.2);
  margin: 0 1px;
}

.history-toggle {
  user-select: none;
}

.empty-state {
  min-height: 120px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
</style>
