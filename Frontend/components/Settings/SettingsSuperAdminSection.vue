<template>
  <div class="superadmin-settings">

    <!-- Başlık -->
    <div class="section-header mb-6">
      <div class="d-flex align-center ga-3">
        <v-avatar color="deep-purple" variant="tonal" size="44" rounded="lg">
          <v-icon>mdi-shield-crown-outline</v-icon>
        </v-avatar>
        <div>
          <h2 class="section-title">Sistem Ayarları</h2>
          <p class="section-desc">Tüm tenant'ları kapsayan global oturum ve kullanıcı yönetimi</p>
        </div>
      </div>
    </div>

    <!-- Stat Kartlar + Tenant Filtresi -->
    <v-row class="mb-5" dense>
      <v-col cols="12" sm="6" md="4">
        <v-card class="stat-card" rounded="xl" elevation="0" border>
          <v-card-text class="d-flex align-center ga-4 pa-5">
            <v-avatar color="deep-purple" variant="tonal" size="52" rounded="lg">
              <v-icon size="26">mdi-account-group</v-icon>
            </v-avatar>
            <div>
              <div class="stat-value">
                <v-progress-circular v-if="loadingCount" indeterminate size="20" width="2" />
                <template v-else>{{ activeUserCount }}</template>
              </div>
              <div class="stat-label">
                {{ selectedTenantId ? 'Tenant Aktif Kullanıcı' : 'Toplam Aktif Kullanıcı' }}
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col v-if="tenants.length" cols="12" sm="6" md="4">
        <v-card rounded="xl" elevation="0" border height="100%">
          <v-card-text class="d-flex align-center pa-4" style="height:100%">
            <v-select
              v-model="selectedTenantId"
              :items="tenantOptions"
              item-title="label"
              item-value="value"
              label="Tenant Filtresi"
              clearable
              hide-details
              density="comfortable"
              prepend-inner-icon="mdi-domain"
              @update:model-value="onTenantChange"
            />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Çevrimiçi Kullanıcılar -->
    <v-card rounded="xl" elevation="0" border class="mb-5">
      <v-card-title class="d-flex align-center flex-wrap ga-2 pa-5">
        <div class="d-flex align-center ga-2">
          <v-icon color="deep-purple">mdi-account-multiple-check-outline</v-icon>
          <span class="table-title">Şu An Çevrimiçi</span>
          <v-chip v-if="selectedTenantId" size="x-small" color="deep-purple" variant="tonal">
            {{ selectedTenantLabel }}
          </v-chip>
        </div>
        <v-spacer />
        <v-btn
          size="small"
          variant="text"
          prepend-icon="mdi-refresh"
          :loading="loadingActiveUsers"
          @click="loadActiveUsers"
        >
          Yenile
        </v-btn>
      </v-card-title>
      <v-divider />
      <v-card-text class="pa-0">
        <p class="text-body-2 text-medium-emphasis px-5 pt-4 pb-2">
          En az bir geçerli oturumu olan kullanıcılar.
          {{ selectedTenantId ? 'Seçili tenant filtrelenmiş.' : 'Tüm tenant\'lar gösteriliyor.' }}
        </p>
        <v-progress-linear v-if="loadingActiveUsers" indeterminate color="deep-purple" />
        <v-table v-if="!loadingActiveUsers && activeUsers.length" density="comfortable" class="online-table">
          <thead>
            <tr>
              <th>Kullanıcı</th>
              <th>Tenant</th>
              <th>Oturum</th>
              <th>Son Aktivite</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in activeUsers" :key="row.userId + '-' + (row.tenantId ?? 'x')">
              <td>
                <div class="d-flex align-center ga-3 py-1">
                  <v-avatar color="deep-purple" variant="tonal" size="32" class="text-caption font-weight-bold">
                    {{ initials(row.fullName) }}
                  </v-avatar>
                  <div>
                    <div class="font-weight-medium">{{ row.fullName }}</div>
                    <div class="text-caption text-medium-emphasis">{{ row.email }}</div>
                  </div>
                </div>
              </td>
              <td>
                <v-chip size="small" variant="tonal" color="grey">
                  <v-icon start size="12">mdi-domain</v-icon>
                  {{ row.tenantName || '—' }}
                </v-chip>
              </td>
              <td>
                <v-chip size="small" color="success" variant="tonal">
                  <v-icon start size="12">mdi-circle</v-icon>
                  {{ row.activeSessionCount }} oturum
                </v-chip>
              </td>
              <td class="text-caption text-medium-emphasis">{{ formatActivity(row.lastActivityAt) }}</td>
            </tr>
          </tbody>
        </v-table>
        <div v-if="!loadingActiveUsers && activeUsers.length === 0" class="empty-table pa-8 text-center">
          <v-icon size="40" color="grey-lighten-1">mdi-account-off-outline</v-icon>
          <p class="text-body-2 text-medium-emphasis mt-2">Şu anda çevrimiçi kullanıcı yok.</p>
        </div>
      </v-card-text>
    </v-card>

    <!-- Uzaktan Kullanıcı Çıkışı -->
    <v-card rounded="xl" elevation="0" border>
      <v-card-title class="pa-5 d-flex align-center ga-2">
        <v-icon color="error">mdi-account-off-outline</v-icon>
        <span class="table-title">Uzaktan Kullanıcı Çıkışı</span>
      </v-card-title>
      <v-divider />
      <v-card-text class="pa-5">
        <p class="text-body-2 text-medium-emphasis mb-4">
          Herhangi bir kullanıcıyı tüm cihazlarından çıkış yaptırabilirsiniz.
        </p>
        <div class="d-flex align-start ga-3 flex-wrap">
          <v-autocomplete
            v-model="selectedUser"
            :items="users"
            item-title="fullName"
            item-value="id"
            label="Kullanıcı seçin"
            placeholder="İsim veya e-posta ile ara..."
            :loading="loadingUsers"
            clearable
            hide-details
            density="comfortable"
            style="min-width: 280px; flex: 1"
          >
            <template #item="{ props, item }">
              <v-list-item v-bind="props">
                <template #prepend>
                  <v-avatar color="deep-purple" variant="tonal" size="32" class="text-caption font-weight-bold mr-2">
                    {{ initials(item.raw.fullName) }}
                  </v-avatar>
                </template>
                <template #title>{{ item.raw.fullName }}</template>
                <template #subtitle>{{ item.raw.email }}</template>
              </v-list-item>
            </template>
          </v-autocomplete>
          <v-btn
            color="error"
            variant="tonal"
            :disabled="!selectedUser"
            :loading="revoking"
            prepend-icon="mdi-logout"
            @click="revokeSessions"
          >
            Çıkış Yaptır
          </v-btn>
        </div>
      </v-card-text>
    </v-card>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { dateTimeFormat } from '~/utils/datesFormat'
import type { ActiveUserSnapshot } from '~/composables/useSettings'

const { getActiveUserCount, getActiveUsersSnapshot, getRevokableUsers, revokeUserSessions } = useSettings()
const { getTenants } = useTenants()

const activeUserCount = ref<number>(0)
const loadingCount = ref(false)
const tenants = ref<Array<{ id: number; name: string }>>([])
const selectedTenantId = ref<number | null>(null)
const users = ref<Array<{ id: number; fullName: string; email: string }>>([])
const loadingUsers = ref(false)
const selectedUser = ref<number | null>(null)
const revoking = ref(false)
const activeUsers = ref<ActiveUserSnapshot[]>([])
const loadingActiveUsers = ref(false)

const initials = (name: string) =>
  name?.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase() ?? '?'

const formatActivity = (iso: string | null) => iso ? dateTimeFormat(iso) : '—'

const tenantOptions = computed(() => [
  { label: 'Tüm tenant\'lar', value: null },
  ...tenants.value.map(t => ({ label: t.name, value: t.id }))
])

const selectedTenantLabel = computed(() =>
  tenants.value.find(t => t.id === selectedTenantId.value)?.name ?? ''
)

const onTenantChange = () => {
  Promise.all([loadActiveCount(), loadUsers(), loadActiveUsers()])
}

const loadActiveCount = async () => {
  loadingCount.value = true
  try { activeUserCount.value = await getActiveUserCount(selectedTenantId.value ?? undefined) }
  catch { activeUserCount.value = 0 }
  finally { loadingCount.value = false }
}

const loadTenants = async () => {
  try {
    const res = await getTenants(1, 100, '')
    const items = res?.data ?? []
    const list = Array.isArray(items) ? items : (items?.items ?? [])
    tenants.value = list.map((t: any) => ({ id: t.id, name: t.name }))
  } catch {
    tenants.value = []
  }
}

const loadUsers = async () => {
  loadingUsers.value = true
  try { users.value = await getRevokableUsers(selectedTenantId.value ?? undefined) }
  catch { users.value = [] }
  finally { loadingUsers.value = false }
}

const loadActiveUsers = async () => {
  loadingActiveUsers.value = true
  try { activeUsers.value = await getActiveUsersSnapshot(selectedTenantId.value ?? undefined) }
  catch { activeUsers.value = [] }
  finally { loadingActiveUsers.value = false }
}

const revokeSessions = async () => {
  if (!selectedUser.value) return
  revoking.value = true
  try {
    await revokeUserSessions(selectedUser.value, 'SuperAdmin tarafından çıkış yaptırıldı')
    selectedUser.value = null
    await Promise.all([loadActiveCount(), loadUsers(), loadActiveUsers()])
  } finally {
    revoking.value = false
  }
}

onMounted(async () => {
  await loadTenants()
  await Promise.all([loadActiveCount(), loadUsers(), loadActiveUsers()])
})
</script>

<style scoped>
.superadmin-settings {
  max-width: 960px;
}

.section-title {
  font-size: 1.15rem;
  font-weight: 700;
  color: rgb(var(--v-theme-on-surface));
  margin: 0;
}

.section-desc {
  font-size: 0.85rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin: 0;
}

.stat-card {
  transition: box-shadow 0.2s;
}
.stat-card:hover {
  box-shadow: 0 4px 20px rgba(0,0,0,0.15) !important;
}

.stat-value {
  font-size: 1.8rem;
  font-weight: 700;
  color: rgb(var(--v-theme-on-surface));
  line-height: 1.2;
}

.stat-label {
  font-size: 0.8rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin-top: 2px;
}

.table-title {
  font-size: 0.95rem;
  font-weight: 600;
}

.online-table :deep(th) {
  font-weight: 600;
  font-size: 0.78rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: rgb(var(--v-theme-on-surface-variant)) !important;
  background: rgb(var(--v-theme-surface-variant));
}

.online-table :deep(tr:hover td) {
  background: rgba(var(--v-theme-deep-purple), 0.08);
}

.empty-table {
  min-height: 120px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
</style>
