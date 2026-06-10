<template>
  <div
    class="bg-surface-container-lowest border border-outline-variant/20 rounded-xl p-md shadow-sm hover:shadow-md transition-all"
  >
    <div class="flex justify-between items-start">
      <!-- Sol: Label + Rakam + Trend -->
      <div class="flex-1 min-w-0">
        <p class="text-label-sm text-on-surface-variant font-bold uppercase tracking-wider mb-2">
          {{ label }}
        </p>
        <h3 class="text-headline-md font-headline-md text-on-surface">
          {{ formattedValue }}
        </h3>
        <p
          v-if="trendPercent !== undefined"
          class="flex items-center gap-1 font-bold text-body-sm mt-2"
          :class="trendColorClass"
        >
          <Icon
            :name="trendIcon"
            class="w-4 h-4"
          />
          {{ trendPrefix }}{{ Math.abs(trendPercent) }}%
          <span class="font-normal text-on-surface-variant">{{ trendSubtitle }}</span>
        </p>
      </div>

      <!-- Sağ: İkon (büyük) -->
      <div
        class="w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0 ml-3"
        :class="iconBgClass"
      >
        <Icon
          :name="icon"
          class="w-6 h-6"
          :class="iconTextClass"
        />
      </div>
    </div>

    <!-- Sparkline (alt) -->
    <div v-if="sparkline && sparkline.length > 0" class="mt-md h-12">
      <svg
        class="w-full h-full"
        :viewBox="`0 0 ${sparklineWidth} 40`"
        preserveAspectRatio="none"
      >
        <defs>
          <linearGradient :id="gradientId" x1="0%" x2="0%" y1="0%" y2="100%">
            <stop offset="0%" :stop-color="sparklineColor" stop-opacity="0.3" />
            <stop offset="100%" :stop-color="sparklineColor" stop-opacity="0" />
          </linearGradient>
        </defs>
        <!-- Filled area -->
        <path :d="sparklineAreaPath" :fill="`url(#${gradientId})`" />
        <!-- Line -->
        <path
          :d="sparklinePath"
          fill="none"
          :stroke="sparklineColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        />
      </svg>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  label: string
  value: number | string
  trendPercent?: number
  trendSubtitle?: string
  icon: string
  color?: 'primary' | 'secondary' | 'tertiary' | 'info'
  sparkline?: number[]
}

const props = withDefaults(defineProps<Props>(), {
  trendPercent: undefined,
  trendSubtitle: 'vs geçen hafta',
  color: 'primary',
  sparkline: () => []
})

const gradientId = `grad-${Math.random().toString(36).slice(2, 11)}`
const sparklineWidth = 100

// Sparkline rengi — SVG stroke için CSS variable üzerinden RGB
const sparklineColor = computed(() => {
  const colorMap: Record<string, string> = {
    primary: 'rgb(var(--color-primary))',
    secondary: 'rgb(var(--color-secondary-container))',
    tertiary: 'rgb(var(--color-tertiary))',
    info: '#3b82f6'
  }
  return colorMap[props.color] ?? colorMap.primary
})

// İkon arka plan rengi — STANDART TAILWIND OPACITY (artık çalışıyor!)
const iconBgClass = computed(() => {
  const map: Record<string, string> = {
    primary: 'bg-primary/10',
    secondary: 'bg-secondary-container/20',
    tertiary: 'bg-tertiary/10',
    info: 'bg-blue-500/10'
  }
  return map[props.color] ?? map.primary
})

const iconTextClass = computed(() => {
  const map: Record<string, string> = {
    primary: 'text-primary',
    secondary: 'text-secondary-container',
    tertiary: 'text-tertiary',
    info: 'text-blue-500'
  }
  return map[props.color] ?? map.primary
})

const trendColorClass = computed(() => {
  if (props.trendPercent === undefined || props.trendPercent === 0) {
    return 'text-on-surface-variant'
  }
  return props.trendPercent > 0 ? 'text-primary' : 'text-error'
})

const trendIcon = computed(() => {
  if (props.trendPercent === undefined || props.trendPercent === 0) {
    return 'mdi:minus'
  }
  return props.trendPercent > 0 ? 'mdi:trending-up' : 'mdi:trending-down'
})

const trendPrefix = computed(() => {
  if (props.trendPercent === undefined || props.trendPercent === 0) return ''
  return props.trendPercent > 0 ? '+' : '-'
})

const formattedValue = computed(() => {
  if (typeof props.value === 'number') {
    return props.value.toLocaleString('tr-TR')
  }
  return props.value
})

const sparklinePath = computed(() => {
  if (!props.sparkline || props.sparkline.length === 0) return ''
  return buildSparklinePath(props.sparkline, sparklineWidth, 40, false)
})

const sparklineAreaPath = computed(() => {
  if (!props.sparkline || props.sparkline.length === 0) return ''
  return buildSparklinePath(props.sparkline, sparklineWidth, 40, true)
})

function buildSparklinePath(data: number[], width: number, height: number, fillArea: boolean): string {
  if (data.length === 0) return ''
  if (data.length === 1) {
    const y = height / 2
    return fillArea ? `M 0 ${y} L ${width} ${y} L ${width} ${height} L 0 ${height} Z` : `M 0 ${y} L ${width} ${y}`
  }

  const min = Math.min(...data)
  const max = Math.max(...data)
  const range = max - min || 1
  const padding = 4

  const points = data.map((value, i) => {
    const x = (i / (data.length - 1)) * width
    const normalized = (value - min) / range
    const y = padding + (1 - normalized) * (height - padding * 2)
    return [x, y] as const
  })

  let path = `M ${points[0][0]} ${points[0][1]}`
  for (let i = 1; i < points.length; i++) {
    const [x1, y1] = points[i - 1]
    const [x2, y2] = points[i]
    const cpx1 = x1 + (x2 - x1) / 2
    const cpx2 = x1 + (x2 - x1) / 2
    path += ` C ${cpx1} ${y1}, ${cpx2} ${y2}, ${x2} ${y2}`
  }

  if (fillArea) {
    path += ` L ${width} ${height} L 0 ${height} Z`
  }

  return path
}
</script>
