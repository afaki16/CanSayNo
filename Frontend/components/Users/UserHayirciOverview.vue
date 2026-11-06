<template>
  <div>
    <div v-if="loading" class="d-flex justify-center py-8">
      <v-progress-circular indeterminate color="primary" />
    </div>

    <v-alert v-else-if="error" type="warning" variant="tonal" density="comfortable">
      Hayırcı bilgileri yüklenemedi.
      <template #append>
        <v-btn size="small" variant="text" @click="load">Tekrar dene</v-btn>
      </template>
    </v-alert>

    <template v-else-if="overview">
      <!-- Abonelik -->
      <div class="hy-section">
        <h3 class="hy-title"><v-icon size="20" class="mr-2">mdi-crown</v-icon>Abonelik</h3>
        <div class="hy-grid">
          <div class="hy-item">
            <div class="hy-label">Durum</div>
            <v-chip :color="overview.isPremium ? 'success' : 'default'" size="small" variant="tonal">
              {{ overview.isPremium ? 'Premium' : 'Ücretsiz' }}
            </v-chip>
          </div>
          <template v-if="overview.subscription">
            <div class="hy-item">
              <div class="hy-label">Ürün</div>
              <div class="hy-value">{{ overview.subscription.productId || '—' }}</div>
            </div>
            <div class="hy-item">
              <div class="hy-label">Mağaza durumu</div>
              <div class="hy-value">{{ SUBSCRIPTION_STATUS_LABELS[overview.subscription.status] }}</div>
            </div>
            <div class="hy-item">
              <div class="hy-label">Bitiş / yenileme</div>
              <div class="hy-value">{{ formatDate(overview.subscription.expiresAt) }}</div>
            </div>
          </template>
        </div>
      </div>

      <!-- İlerleme -->
      <div class="hy-section">
        <h3 class="hy-title"><v-icon size="20" class="mr-2">mdi-chart-box</v-icon>İlerleme</h3>
        <div class="hy-grid">
          <div class="hy-item">
            <div class="hy-label">Seviye / XP</div>
            <div class="hy-value">{{ overview.level }} · {{ overview.totalPoints }} XP</div>
          </div>
          <div class="hy-item">
            <div class="hy-label">Seri (şu an / en uzun)</div>
            <div class="hy-value">{{ overview.currentStreak }} / {{ overview.longestStreak }} gün</div>
          </div>
          <div class="hy-item">
            <div class="hy-label">Pratik (başlatılan / değerlendirilen)</div>
            <div class="hy-value">{{ overview.totalSessions }} / {{ overview.evaluatedSessions }}</div>
          </div>
          <div class="hy-item">
            <div class="hy-label">Ortalama puan</div>
            <div class="hy-value">{{ overview.averageScore ?? '—' }}</div>
          </div>
          <div class="hy-item">
            <div class="hy-label">Toplam LLM token</div>
            <div class="hy-value">{{ overview.totalTokens.toLocaleString('tr-TR') }}</div>
          </div>
          <div class="hy-item">
            <div class="hy-label">Son pratik</div>
            <div class="hy-value">{{ formatDate(overview.lastPracticeAt) }}</div>
          </div>
        </div>
        <div v-if="overview.badges.length" class="d-flex flex-wrap ga-2 mt-3">
          <v-chip v-for="b in overview.badges" :key="b" size="small" variant="tonal" color="amber-darken-2" prepend-icon="mdi-medal">
            {{ BADGE_LABELS[b] ?? b }}
          </v-chip>
        </div>
      </div>

      <!-- Son pratikler -->
      <div class="hy-section">
        <h3 class="hy-title"><v-icon size="20" class="mr-2">mdi-history</v-icon>Son pratikler</h3>
        <p v-if="!overview.recentSessions.length" class="text-medium-emphasis text-body-2">Henüz pratik yok.</p>
        <v-table v-else density="compact">
          <thead>
            <tr>
              <th>Senaryo</th>
              <th>Durum</th>
              <th>Puan</th>
              <th>Token</th>
              <th>Başlangıç</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in overview.recentSessions" :key="s.id">
              <td>
                <v-chip size="x-small" variant="tonal" :color="contextOf(s.contextId).color" class="mr-2">
                  {{ contextOf(s.contextId).title }}
                </v-chip>
                {{ s.scenarioTitle }}
              </td>
              <td>{{ SESSION_STATUS_LABELS[s.status] }}</td>
              <td>{{ s.totalScore ?? '—' }}</td>
              <td>{{ s.tokensUsed.toLocaleString('tr-TR') }}</td>
              <td>{{ formatDate(s.startedAt) }}</td>
            </tr>
          </tbody>
        </v-table>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { AdminUserOverview } from '~/types'
import { BADGE_LABELS, SESSION_STATUS_LABELS, SUBSCRIPTION_STATUS_LABELS, contextOf } from '~/utils/hayirciLabels'

const props = defineProps<{ userId?: number | null }>()

const { getUserOverview } = useHayirciAdmin()
const overview = ref<AdminUserOverview | null>(null)
const loading = ref(false)
const error = ref(false)

const formatDate = (iso: string | null) =>
  iso ? new Date(iso).toLocaleString('tr-TR', { dateStyle: 'medium', timeStyle: 'short' }) : '—'

const load = async () => {
  if (!props.userId) return
  loading.value = true
  error.value = false
  try {
    overview.value = (await getUserOverview(props.userId)) ?? null
  } catch {
    error.value = true
  } finally {
    loading.value = false
  }
}

watch(() => props.userId, () => {
  overview.value = null
  void load()
}, { immediate: true })
</script>

<style scoped>
.hy-section {
  margin-bottom: 24px;
}

.hy-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 12px;
  display: flex;
  align-items: center;
  color: rgb(var(--v-theme-on-surface));
}

.hy-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 12px;
}

.hy-item {
  padding: 12px 14px;
  background: rgb(var(--v-theme-surface-variant));
  border: 1px solid rgb(var(--v-theme-outline-variant));
  border-radius: 8px;
}

.hy-label {
  font-size: 0.72rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: rgb(var(--v-theme-on-surface-variant));
  margin-bottom: 4px;
  font-weight: 500;
}

.hy-value {
  font-size: 0.95rem;
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
}
</style>
