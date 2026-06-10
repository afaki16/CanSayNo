<template>
  <div>
    <NuxtRouteAnnouncer />
    <NuxtLayout :name="layoutName">
      <NuxtPage />
    </NuxtLayout>
    <AppToast />
  </div>
</template>

<script setup lang="ts">
import AppToast from '~/components/UI/AppToast.vue'

const config = useRuntimeConfig()
const route = useRoute()

const layoutName = computed(() => {
  if (route.meta.layout === false) return false
  if (typeof route.meta.layout === 'string') return route.meta.layout
  return (config.public.defaultLayout as string) || 'default'
})
</script>
