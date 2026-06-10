<template>
  <div class="user-detail-container">
    <!-- Header with Avatar -->
    <div class="detail-header">
      <v-avatar size="80" color="primary" class="detail-avatar">
        <v-img 
          v-if="user?.profileImageUrl" 
          :src="user.profileImageUrl" 
          alt="Profile"
        />
        <span v-else class="text-h4">
          {{ getInitials(user?.firstName, user?.lastName) }}
        </span>
      </v-avatar>
      <div class="header-info">
        <h2 class="user-name">{{ user?.firstName }} {{ user?.lastName }}</h2>
        <div class="user-meta">
          <v-chip
            :color="user?.status === 1 ? 'success' : 'error'"
            size="small"
            variant="tonal"
            prepend-icon="mdi-circle"
          >
            {{ user?.status === 1 ? 'Aktif' : 'Pasif' }}
          </v-chip>
          <v-chip
            v-if="user?.emailConfirmed"
            color="info"
            size="small"
            variant="tonal"
            prepend-icon="mdi-email-check"
          >
            E-posta Doğrulandı
          </v-chip>
        </div>
      </div>
    </div>

    <v-divider class="my-6" />

    <!-- Tabs -->
    <div class="tabs-container">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        :class="['tab-item', { active: currentTab === tab.value }]"
        @click="currentTab = tab.value"
      >
        <div class="tab-icon-wrapper">
          <v-icon :icon="tab.icon" size="22" />
        </div>
        <span class="tab-label">{{ tab.label }}</span>
      </button>
    </div>

    <v-window v-model="currentTab" class="mt-6">
      <!-- Tab 1: Genel Bilgiler -->
      <v-window-item value="general">
        <div class="info-section">
          <h3 class="section-title">
            <v-icon size="20" class="mr-2">mdi-account-details</v-icon>
            Kişisel Bilgiler
          </h3>
          <div class="info-grid">
            <div class="info-item">
              <div class="info-label">
                <v-icon size="18" color="primary">mdi-account</v-icon>
                Ad Soyad
              </div>
              <div class="info-value">{{ user?.firstName }} {{ user?.lastName }}</div>
            </div>

            <div class="info-item">
              <div class="info-label">
                <v-icon size="18" color="primary">mdi-email</v-icon>
                E-posta
              </div>
              <div class="info-value">{{ user?.email || '-' }}</div>
            </div>

            <div class="info-item">
              <div class="info-label">
                <v-icon size="18" color="primary">mdi-phone</v-icon>
                Telefon
              </div>
              <div class="info-value">{{ user?.phoneNumber || '-' }}</div>
            </div>
          </div>
        </div>

        <v-divider class="my-6" />

        <div class="info-section">
          <h3 class="section-title">
            <v-icon size="20" class="mr-2">mdi-clock-outline</v-icon>
            Hesap Bilgileri
          </h3>
          <div class="info-grid">
            <div class="info-item">
              <div class="info-label">
                <v-icon size="18" color="primary">mdi-calendar-plus</v-icon>
                Kayıt Tarihi
              </div>
              <div class="info-value">{{ formatDateTime(user?.createdDate) }}</div>
            </div>

            <div class="info-item">
              <div class="info-label">
                <v-icon size="18" color="primary">mdi-login</v-icon>
                Son Giriş
              </div>
              <div class="info-value">{{ formatDateTime(user?.lastLoginDate) }}</div>
            </div>
          </div>
        </div>
      </v-window-item>

      <!-- Tab 2: Roller ve Yetkiler -->
      <v-window-item value="roles">
        <div class="info-section">
          <div class="section-header">
            <h3 class="section-title">
              <v-icon size="20" class="mr-2">mdi-shield-account</v-icon>
              Atanmış Roller
            </h3>
            <v-chip size="small" color="primary" variant="tonal">
              {{ user?.roles?.length || 0 }} Rol
            </v-chip>
          </div>

          <div v-if="user?.roles && user.roles.length > 0" class="roles-list">
            <div v-for="role in user.roles" :key="role.id" class="role-card">
              <div class="role-card-header">
                <div class="role-card-icon">
                  <v-icon size="24" color="primary">mdi-shield-star</v-icon>
                </div>
                <div class="role-card-info">
                  <h4 class="role-card-name">{{ role.name }}</h4>
                  <p v-if="role.description" class="role-card-description">
                    {{ role.description }}
                  </p>
                </div>
                <v-chip
                  v-if="role.isSystemRole"
                  size="small"
                  color="orange"
                  variant="tonal"
                >
                  Sistem Rolü
                </v-chip>
              </div>
            </div>
          </div>

          <div v-else class="empty-state">
            <v-icon size="64" color="grey-400">mdi-shield-off</v-icon>
            <h3 class="empty-title">Rol Atanmamış</h3>
            <p class="empty-text">Bu kullanıcıya henüz rol atanmamış</p>
          </div>
        </div>
      </v-window-item>

      <!-- Tab 3: Güvenlik -->
      <v-window-item value="security">
        <div class="info-section">
          <h3 class="section-title">
            <v-icon size="20" class="mr-2">mdi-security</v-icon>
            Güvenlik Bilgileri
          </h3>
          <div class="security-grid">
            <div class="security-card">
              <div class="security-icon" :class="user?.emailConfirmed ? 'success' : 'warning'">
                <v-icon size="32">{{ user?.emailConfirmed ? 'mdi-email-check' : 'mdi-email-alert' }}</v-icon>
              </div>
              <div class="security-info">
                <h4 class="security-title">E-posta Doğrulama</h4>
                <p class="security-status">
                  {{ user?.emailConfirmed ? 'Doğrulanmış' : 'Doğrulanmamış' }}
                </p>
              </div>
            </div>

            <div class="security-card">
              <div class="security-icon" :class="user?.phoneConfirmed ? 'success' : 'warning'">
                <v-icon size="32">{{ user?.phoneConfirmed ? 'mdi-phone-check' : 'mdi-phone-alert' }}</v-icon>
              </div>
              <div class="security-info">
                <h4 class="security-title">Telefon Doğrulama</h4>
                <p class="security-status">
                  {{ user?.phoneConfirmed ? 'Doğrulanmış' : 'Doğrulanmamış' }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </v-window-item>
    </v-window>

    <!-- Footer Actions -->
    <div class="detail-footer">
      <v-btn
        variant="outlined"
        size="large"
        @click="$emit('close')"
        class="btn-gradient-dark"
      >
        Kapat
      </v-btn>
      <v-btn
        size="large"
        @click="$emit('edit', user)"
        class="btn-gradient-primary"
        prepend-icon="mdi-pencil"
      >
        Düzenle
      </v-btn>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { User } from '~/types'
import { dateTimeFormatLong } from '~/utils/datesFormat'

defineProps<{
  user?: User | null
}>()

defineEmits<{
  close: []
  edit: [user: User]
}>()

const currentTab = ref('general')

const tabs = [
  { value: 'general', label: 'Genel Bilgiler', icon: 'mdi-information' },
  { value: 'roles', label: 'Roller', icon: 'mdi-shield-account' },
  { value: 'security', label: 'Güvenlik', icon: 'mdi-security' }
]

const getInitials = (firstName?: string, lastName?: string) => {
  if (!firstName && !lastName) return '?'
  const first = firstName?.charAt(0) || ''
  const last = lastName?.charAt(0) || ''
  return (first + last).toUpperCase()
}

const formatDateTime = (date?: string | null) => {
  if (!date) return '-'
  return dateTimeFormatLong(date)
}
</script>

<style scoped>
.user-detail-container {
  padding: 24px;
}

.detail-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 24px;
}

.detail-avatar {
  border: 3px solid rgb(var(--v-theme-outline-variant));
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.header-info {
  flex: 1;
}

.user-name {
  font-size: 1.5rem;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
  margin: 0 0 8px 0;
}

.user-meta {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.info-section {
  margin-bottom: 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.section-title {
  font-size: 1rem;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
  margin: 0 0 16px 0;
  display: flex;
  align-items: center;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 12px;
}

.info-item {
  padding: 14px 16px;
  background: rgb(var(--v-theme-surface-variant));
  border-radius: 8px;
  border: 1px solid rgb(var(--v-theme-outline-variant));
  transition: all 0.2s ease;
}

.info-item:hover {
  border-color: rgb(var(--v-theme-outline));
  background: rgb(var(--v-theme-surface));
}

.info-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.75rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin-bottom: 6px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-value {
  font-size: 0.9375rem;
  color: rgb(var(--v-theme-on-surface));
  font-weight: 500;
}

/* Roles Section */
.roles-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.role-card {
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgb(var(--v-theme-outline-variant));
  border-radius: 8px;
  padding: 14px 16px;
  transition: all 0.2s ease;
}

.role-card:hover {
  border-color: rgb(var(--v-theme-outline));
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.role-card-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.role-card-icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(var(--v-theme-primary), 0.1);
  border-radius: 8px;
  flex-shrink: 0;
}

.role-card-info {
  flex: 1;
  min-width: 0;
}

.role-card-name {
  font-size: 0.9375rem;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
  margin: 0 0 4px 0;
}

.role-card-description {
  font-size: 0.8125rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin: 0;
  line-height: 1.4;
}

/* Security Section */
.security-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 12px;
  margin-bottom: 20px;
}

.security-card {
  display: flex;
  gap: 12px;
  padding: 14px 16px;
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgb(var(--v-theme-outline-variant));
  border-radius: 8px;
  transition: all 0.2s ease;
}

.security-card:hover {
  border-color: rgb(var(--v-theme-outline));
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.security-icon {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  flex-shrink: 0;
}

.security-icon.success {
  background: #ecfdf5;
  color: #10b981;
}

.security-icon.warning {
  background: #fff7ed;
  color: #f59e0b;
}

.security-icon.default {
  background: rgb(var(--v-theme-surface-variant));
  color: rgb(var(--v-theme-on-surface-variant));
}

.security-info {
  flex: 1;
}

.security-title {
  font-size: 0.8125rem;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
  margin: 0 0 4px 0;
}

.security-status {
  font-size: 0.8125rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin: 0;
  font-weight: 500;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 48px 24px;
}

.empty-title {
  font-size: 1rem;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface-variant));
  margin-top: 12px;
}

.empty-text {
  font-size: 0.875rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin-top: 6px;
  opacity: 0.7;
}

/* Footer */
.detail-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid rgb(var(--v-theme-outline-variant));
}

@media (max-width: 768px) {
  .user-detail-container {
    padding: 16px;
  }

  .detail-header {
    flex-direction: column;
    text-align: center;
  }

  .user-name {
    font-size: 1.25rem;
  }

  .info-grid,
  .security-grid {
    grid-template-columns: 1fr;
  }

  .detail-footer {
    flex-direction: column;
  }

  .detail-footer .v-btn {
    width: 100%;
  }
}
</style>
