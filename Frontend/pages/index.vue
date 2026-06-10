<template>
  <SplashScreen2 v-if="showSplash" @complete="onSplashComplete" />

  <template v-else-if="!isLoading">
    <component
      :is="loginComponent"
      :show-register="false"
    />
  </template>

  <div v-else class="login-loading">
    <v-progress-circular indeterminate color="primary" size="48" />
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false,
  middleware: 'guest'
})

const showSplash = ref(true) // Eğer açılışta screen açılmasın istiyorsak false çek
const onSplashComplete = () => { showSplash.value = false }

const { loadAppData, isLoading, appData } = useAppData()

const loginPage = computed(() => appData?.value?.tenant?.loginPage ?? 'default')

const loginComponents: Record<string, ReturnType<typeof defineAsyncComponent>> = {
  default: defineAsyncComponent(() => import('~/components/Auth/LoginPage.vue'))
}

const loginComponent = computed(() => {
  const page = loginPage.value || 'default'
  return loginComponents[page] ?? loginComponents.default
})

onMounted(async () => {
  await loadAppData()
})

useHead({
  title: 'Kickstart - Authentication System',
  meta: [
    { name: 'description', content: 'Sign in to your Kickstart account' }
  ]
})
</script>

<style scoped>
.login-loading {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--theme-gradient-sidebar, #14532d);
}
</style>
