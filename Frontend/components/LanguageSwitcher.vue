<template>
  <v-menu offset="8" location="bottom end">
    <template #activator="{ props }">
      <button
        v-bind="props"
        class="lang-switcher-btn"
        type="button"
        :aria-label="t('common.changeLanguage')"
      >
        <span :class="`fi fi-${currentFlag}`" class="lang-flag"></span>
        <span class="lang-code">{{ locale.toUpperCase() }}</span>
        <v-icon size="x-small" class="lang-chevron">mdi-chevron-down</v-icon>
      </button>
    </template>

    <v-list density="compact" class="lang-list" min-width="160">
      <v-list-item
        v-for="loc in availableLocales"
        :key="loc.code"
        :active="loc.code === locale"
        @click="switchLocale(loc.code)"
      >
        <template #prepend>
          <span :class="`fi fi-${getFlagCode(loc.code)}`" class="lang-flag-list"></span>
        </template>
        <v-list-item-title class="lang-list-title">{{ loc.name }}</v-list-item-title>
        <template #append>
          <v-icon v-if="loc.code === locale" size="small" color="success">mdi-check</v-icon>
        </template>
      </v-list-item>
    </v-list>
  </v-menu>
</template>

<script setup lang="ts">
const { locale, locales, setLocale, t } = useI18n()

const availableLocales = computed(() => {
  return (locales.value as Array<{ code: string; name: string }>)
})

const getFlagCode = (code: string) => {
  if (code === 'en') return 'gb'
  return code
}

const currentFlag = computed(() => getFlagCode(locale.value))

const switchLocale = async (code: string) => {
  await setLocale(code as any)
}
</script>

<style scoped>
.lang-switcher-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: inherit;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.lang-switcher-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  border-color: rgba(255, 255, 255, 0.3);
}

.lang-flag {
  width: 20px;
  height: 15px;
  border-radius: 2px;
  display: inline-block;
}

.lang-flag-list {
  width: 24px;
  height: 18px;
  border-radius: 2px;
  display: inline-block;
  margin-right: 8px;
}

.lang-code {
  letter-spacing: 0.5px;
}

.lang-chevron {
  opacity: 0.7;
}

.lang-list {
  border-radius: 12px;
  padding: 6px;
}

.lang-list-title {
  font-size: 0.875rem;
  font-weight: 500;
}
</style>
