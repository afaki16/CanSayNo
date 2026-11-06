<template>
  <div class="space-y-md">
    <!-- 1. Karşılama -->
    <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-md shadow-sm">
      <div class="flex items-center justify-between flex-wrap gap-md">
        <div>
          <h1 class="text-headline-md font-headline-md text-on-surface">
            {{ greeting }}
          </h1>
          <p class="text-body-md text-on-surface-variant mt-1">
            {{ t('dashboard.subtitle') }}
          </p>
        </div>
        <div class="flex items-center gap-2 text-primary bg-primary/10 px-3 py-1.5 rounded-full">
          <Icon name="mdi:calendar-check" class="w-4 h-4" />
          <span class="text-body-sm font-bold">{{ t('dashboard.todayLabel') }} {{ todayDate }}</span>
        </div>
      </div>
    </div>

    <template v-if="canReadDashboard">
      <div
        v-if="loadFailed"
        class="bg-surface-container-lowest border border-outline-variant rounded-xl p-md text-body-md text-on-surface-variant"
      >
        {{ t('dashboard.loadError') }}
      </div>

      <!-- 2. Gösterge kartları (gerçek veri: GET /api/admin/dashboard) -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-md">
        <StatCard
          v-for="stat in statsCards"
          :key="stat.label"
          :label="stat.label"
          :value="stat.value"
          :caption="stat.caption"
          :icon="stat.icon"
          :color="stat.color"
          :sparkline="stat.sparkline"
        />
      </div>
    </template>

    <!-- 3. Grafik (sol) + profil (sağ) -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-md">
      <div
        v-if="canReadDashboard"
        class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm overflow-hidden"
      >
        <div class="flex items-center gap-2 px-md py-4 border-b border-outline-variant">
          <Icon name="mdi:chart-line" class="w-5 h-5 text-primary" />
          <h2 class="text-body-md font-bold text-on-surface">{{ t('dashboard.activityChart') }}</h2>
        </div>
        <div class="p-md">
          <div class="h-[320px]">
            <canvas ref="activityChart"></canvas>
          </div>
        </div>
      </div>
      <div :class="canReadDashboard ? 'lg:col-span-4' : 'lg:col-span-12'">
        <UserProfileCard />
      </div>
    </div>

    <!-- 4. LLM kullanımı + senaryo sıralamaları -->
    <div v-if="canReadDashboard && dashboard" class="grid grid-cols-1 lg:grid-cols-3 gap-md">
      <div class="bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm p-md">
        <div class="flex items-center gap-2 mb-4">
          <Icon name="mdi:robot-outline" class="w-5 h-5 text-primary" />
          <h2 class="text-body-md font-bold text-on-surface">{{ t('dashboard.llmTitle') }}</h2>
        </div>
        <div class="grid grid-cols-2 gap-md">
          <div>
            <div class="text-body-sm text-on-surface-variant">{{ t('dashboard.tokens') }}</div>
            <div class="text-headline-sm font-bold text-on-surface">
              {{ dashboard.tokensLast30Days.toLocaleString(numberLocale) }}
            </div>
          </div>
          <div>
            <div class="text-body-sm text-on-surface-variant">{{ t('dashboard.estimatedCost') }}</div>
            <div class="text-headline-sm font-bold text-on-surface">
              ${{ dashboard.estimatedCostUsdLast30Days.toLocaleString(numberLocale, { minimumFractionDigits: 2 }) }}
            </div>
          </div>
        </div>
        <p class="text-body-sm text-on-surface-variant mt-4">
          {{ t('dashboard.costNote', { rate: dashboard.usdPerMillionTokens }) }}
        </p>
      </div>

      <div
        v-for="list in rankLists"
        :key="list.title"
        class="bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm p-md"
      >
        <div class="flex items-center gap-2 mb-1">
          <Icon :name="list.icon" class="w-5 h-5 text-primary" />
          <h2 class="text-body-md font-bold text-on-surface">{{ list.title }}</h2>
        </div>
        <p v-if="list.note" class="text-body-sm text-on-surface-variant mb-3">{{ list.note }}</p>
        <p v-if="!list.items.length" class="text-body-sm text-on-surface-variant mt-3">{{ t('dashboard.noData') }}</p>
        <ul v-else class="mt-3 space-y-2">
          <li v-for="item in list.items" :key="item.id" class="flex items-center justify-between gap-2">
            <div class="min-w-0">
              <div class="text-body-md text-on-surface truncate">{{ item.title }}</div>
              <div class="text-body-sm" :style="{ color: contextOf(item.contextId).color }">
                {{ contextOf(item.contextId).title }}
              </div>
            </div>
            <div class="text-body-sm text-on-surface-variant whitespace-nowrap text-right">
              {{ t('dashboard.sessionsCount', { n: item.sessions }) }}
              <template v-if="item.averageScore !== null">
                · {{ t('dashboard.avgScore', { n: item.averageScore }) }}
              </template>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import { Chart, registerables } from 'chart.js'
import { useAuthStore } from '~/stores/auth'
import StatCard from '~/components/Dashboard/StatCard.vue'
import UserProfileCard from '~/components/Dashboard/UserProfileCard.vue'
import type { AdminDashboard } from '~/types'
import { contextOf } from '~/utils/hayirciLabels'

Chart.register(...registerables)

definePageMeta({
  middleware: ['permission'],
})

const authStore = useAuthStore()
const { t, locale } = useI18n()
const { getDashboard } = useHayirciAdmin()

const canReadDashboard = computed(() => authStore.hasPermission('Dashboard.Read'))
const dashboard = ref<AdminDashboard | null>(null)
const loadFailed = ref(false)
const activityChart = ref<HTMLCanvasElement>()
let chartInstance: Chart | null = null

const numberLocale = computed(() => (locale.value === 'en' ? 'en-US' : 'tr-TR'))

const greeting = computed(() => {
  const hour = new Date().getHours()
  const period = hour < 12 ? 'morning' : hour < 18 ? 'afternoon' : 'evening'
  const prefix = t(`dashboard.greeting.${period}`)
  const name = authStore.userFullName
  return name ? `${prefix}, ${name}` : prefix
})

const todayDate = computed(() =>
  new Date().toLocaleDateString(numberLocale.value, { day: 'numeric', month: 'long', year: 'numeric' }),
)

const series = (pick: (p: AdminDashboard['daily'][number]) => number) =>
  dashboard.value?.daily.map(pick) ?? []

const statsCards = computed(() => {
  const d = dashboard.value
  return [
    {
      label: t('dashboard.stats.appUsers'),
      caption: t('dashboard.stats.newUsers', { n: d?.newUsersLast7Days ?? 0 }),
      value: d?.appUsers ?? '—',
      icon: 'mdi:account-multiple',
      color: 'primary' as const,
      sparkline: [] as number[],
    },
    {
      label: t('dashboard.stats.activeToday'),
      caption: t('dashboard.stats.activeWeek', { n: d?.activeUsersLast7Days ?? 0 }),
      value: d?.activeUsersToday ?? '—',
      icon: 'mdi:account-check',
      color: 'info' as const,
      sparkline: series((p) => p.activeUsers),
    },
    {
      label: t('dashboard.stats.premium'),
      caption: t('dashboard.stats.premiumRate', { n: d?.premiumRatePercent ?? 0 }),
      value: d?.premiumUsers ?? '—',
      icon: 'mdi:crown',
      color: 'tertiary' as const,
      sparkline: [] as number[],
    },
    {
      label: t('dashboard.stats.sessionsToday'),
      caption: t('dashboard.stats.evaluatedToday', { n: d?.evaluatedToday ?? 0 }),
      value: d?.sessionsToday ?? '—',
      icon: 'mdi:chat-processing',
      color: 'secondary' as const,
      sparkline: series((p) => p.sessions),
    },
  ]
})

const rankLists = computed(() => [
  {
    title: t('dashboard.topScenarios'),
    icon: 'mdi:fire',
    note: '',
    items: dashboard.value?.topScenarios ?? [],
  },
  {
    title: t('dashboard.lowestScenarios'),
    icon: 'mdi:alert-circle-outline',
    note: t('dashboard.lowestNote'),
    items: dashboard.value?.lowestScoringScenarios ?? [],
  },
])

const cssVar = (name: string, fallback: string) =>
  getComputedStyle(document.documentElement).getPropertyValue(name).trim() || fallback

const renderChart = () => {
  if (!activityChart.value || !dashboard.value) return
  chartInstance?.destroy()
  const primary = cssVar('--theme-primary', '#2563eb')
  const primaryRgb = cssVar('--theme-primary-rgb', '37, 99, 235')
  const labels = dashboard.value.daily.map((p) =>
    new Date(`${p.date}T00:00:00`).toLocaleDateString(numberLocale.value, { day: 'numeric', month: 'short' }),
  )
  chartInstance = new Chart(activityChart.value, {
    type: 'line',
    data: {
      labels,
      datasets: [
        {
          label: t('dashboard.sessionsLabel'),
          data: series((p) => p.sessions),
          borderColor: primary,
          backgroundColor: `rgba(${primaryRgb}, 0.1)`,
          tension: 0.35,
          fill: true,
        },
        {
          label: t('dashboard.activeUsersLabel'),
          data: series((p) => p.activeUsers),
          borderColor: '#F25287',
          backgroundColor: 'rgba(242, 82, 135, 0.08)',
          tension: 0.35,
          fill: false,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: true, position: 'bottom' } },
      scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
    },
  })
}

onMounted(async () => {
  if (!canReadDashboard.value) return
  try {
    dashboard.value = (await getDashboard()) ?? null
    await nextTick()
    renderChart()
  } catch {
    loadFailed.value = true
  }
})

onBeforeUnmount(() => {
  chartInstance?.destroy()
  chartInstance = null
})

useHead({
  title: () => t('dashboard.pageTitle'),
})
</script>
