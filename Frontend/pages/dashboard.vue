<template>
  <div class="space-y-md">
    <!-- 1. Welcome Header -->
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
        <div
          class="flex items-center gap-2 text-primary bg-primary/10 px-3 py-1.5 rounded-full"
        >
          <Icon name="mdi:calendar-check" class="w-4 h-4" />
          <span class="text-body-sm font-bold">{{ t('dashboard.todayLabel') }} {{ todayDate }}</span>
        </div>
      </div>
    </div>

    <!-- 2. Stats Cards Grid -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-md">
      <StatCard
        v-for="stat in statsCards"
        :key="stat.label"
        :label="stat.label"
        :value="stat.value"
        :trend-percent="stat.trendPercent"
        :trend-subtitle="stat.trendSubtitle"
        :icon="stat.icon"
        :color="stat.color"
        :sparkline="stat.sparkline"
      />
    </div>

    <!-- 3. İki kolonlu içerik alanı: Chart (sol) + Profile Card (sağ) -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-md">
      <!-- Sol: Haftalık Gelir Analizi (mevcut içerik — korunuyor) -->
      <div
        v-if="isAdminOrSuperAdmin"
        class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm overflow-hidden"
      >
        <div class="flex items-center gap-2 px-md py-4 border-b border-outline-variant">
          <Icon name="mdi:chart-line" class="w-5 h-5 text-primary" />
          <h2 class="text-body-md font-bold text-on-surface">
            {{ t('dashboard.weeklyRevenueAnalysis') }}
          </h2>
        </div>
        <div class="p-md">
          <div class="h-[360px]">
            <canvas ref="revenueChart"></canvas>
          </div>
        </div>
      </div>

      <!-- Sağ: User Profile Card (yeni) -->
      <div class="lg:col-span-4">
        <UserProfileCard />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { Chart, registerables } from "chart.js";
import { useAuth } from "~/composables/useAuth";
import { useAuthStore } from "~/stores/auth";
import StatCard from "~/components/Dashboard/StatCard.vue";
import UserProfileCard from "~/components/Dashboard/UserProfileCard.vue";

Chart.register(...registerables);

definePageMeta({
  middleware: ["permission"],
});

const authStore = useAuthStore();
const { hasRole } = useAuth();
const { t, locale } = useI18n();

const isAdminOrSuperAdmin = computed(() => {
  return hasRole("Admin") || hasRole("SuperAdmin");
});

const revenueChart = ref<HTMLCanvasElement>();

// Saate göre selamlama + kullanıcı adı
const greeting = computed(() => {
  const hour = new Date().getHours();
  const period = hour < 12 ? "morning" : hour < 18 ? "afternoon" : "evening";
  const prefix = t(`dashboard.greeting.${period}`);
  const name = authStore.userFullName;
  return name ? `${prefix}, ${name}` : prefix;
});

const todayDate = computed(() => {
  const localeMap: Record<string, string> = {
    tr: "tr-TR",
    en: "en-US",
  };
  const dateLocale = localeMap[locale.value] || "tr-TR";
  return new Date().toLocaleDateString(dateLocale, {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
});

/**
 * MOCK DATA — Aşama 2A için.
 * Backend bağlantısı 2A sonrası bağımsız bir adımda yapılacak.
 *
 * Sparkline değerleri son 7 gün simülasyonu — gerçek API'den günlük sayılar gelecek.
 * Etiketler i18n'den geldiği için computed; locale değişince başlıklar güncellenir.
 */
const statsCards = computed(() => [
  {
    label: t("dashboard.stats.totalUsers"),
    value: 1247,
    trendPercent: 12,
    trendSubtitle: t("dashboard.trendVsLast7Days"),
    icon: "mdi:account-multiple",
    color: "primary" as const,
    sparkline: [1180, 1190, 1205, 1220, 1230, 1240, 1247],
  },
  {
    label: t("dashboard.stats.activeUsers"),
    value: 892,
    trendPercent: 5,
    trendSubtitle: t("dashboard.trendVsLast7Days"),
    icon: "mdi:account-check",
    color: "info" as const,
    sparkline: [840, 850, 865, 870, 880, 885, 892],
  },
  {
    label: t("dashboard.stats.totalRoles"),
    value: 14,
    trendPercent: 0,
    trendSubtitle: t("dashboard.trendNoChange"),
    icon: "mdi:shield-account",
    color: "tertiary" as const,
    sparkline: [14, 14, 14, 14, 14, 14, 14],
  },
  {
    label: t("dashboard.stats.totalPermissions"),
    value: 47,
    trendPercent: 4,
    trendSubtitle: t("dashboard.stats.newPermissions"),
    icon: "mdi:key",
    color: "secondary" as const,
    sparkline: [45, 45, 45, 46, 47, 47, 47],
  },
]);

const initRevenueChart = () => {
  if (!revenueChart.value) return;

  new Chart(revenueChart.value, {
    type: "line",
    data: {
      labels: [
        t("dashboard.days.mon"),
        t("dashboard.days.tue"),
        t("dashboard.days.wed"),
        t("dashboard.days.thu"),
        t("dashboard.days.fri"),
        t("dashboard.days.sat"),
        t("dashboard.days.sun"),
      ],
      datasets: [
        {
          label: t("dashboard.revenueLabel"),
          data: [12500, 19800, 15200, 21300, 18900, 25400, 22100],
          borderColor:
            getComputedStyle(document.documentElement)
              .getPropertyValue("--theme-primary")
              .trim() || "#2563eb",
          backgroundColor: `rgba(${getComputedStyle(document.documentElement).getPropertyValue("--theme-primary-rgb").trim() || "37, 99, 235"}, 0.1)`,
          tension: 0.4,
          fill: true,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false,
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: {
            callback: function (value) {
              return "₺" + value.toLocaleString();
            },
          },
        },
      },
    },
  });
};

onMounted(() => {
  if (isAdminOrSuperAdmin.value) {
    setTimeout(() => {
      initRevenueChart();
    }, 100);
  }
});

useHead({
  title: () => t("dashboard.pageTitle"),
});
</script>
