<template>
  <div
    class="bg-surface-container-lowest border border-outline-variant/20 rounded-xl shadow-sm overflow-hidden flex flex-col h-full"
  >
    <!-- Banner (gradient — gerçek tema rengiyle) -->
    <div class="h-24 relative bg-gradient-to-br from-primary to-primary-container">
      <!-- Subtle pattern overlay (optional decorative) -->
      <div
        class="absolute inset-0 opacity-20"
        style="background-image: radial-gradient(circle at 50% 0%, rgba(255,255,255,0.3) 0%, transparent 60%);"
      ></div>
    </div>

    <!-- Avatar + Info Section -->
    <div class="px-md pb-md pt-12 relative flex flex-col items-center">
      <!-- Avatar — banner'a yarı taşan -->
      <div
        class="absolute -top-10 left-1/2 -translate-x-1/2 w-20 h-20 rounded-full border-4 border-surface-container-lowest overflow-hidden shadow-lg bg-primary/20 flex items-center justify-center"
      >
        <span class="text-headline-md font-headline-md font-bold text-primary">
          {{ userInitials }}
        </span>
      </div>

      <!-- Name + Email -->
      <h4 class="text-headline-md font-headline-md text-on-surface text-center">
        {{ userFullName }}
      </h4>
      <p class="text-body-sm text-on-surface-variant font-medium truncate max-w-full">
        {{ userEmail }}
      </p>

      <!-- Stats Row -->
      <div
        class="flex gap-lg mt-md w-full justify-around border-t border-outline-variant/20 pt-md"
      >
        <div class="text-center">
          <p class="text-body-md font-bold text-on-surface">{{ rolesCount }}</p>
          <p class="text-label-sm text-on-surface-variant">Rol</p>
        </div>
        <div class="text-center">
          <p class="text-body-md font-bold text-on-surface">{{ permissionsCount }}</p>
          <p class="text-label-sm text-on-surface-variant">İzin</p>
        </div>
        <div class="text-center">
          <p class="text-body-md font-bold text-on-surface">{{ activeSessionsCount }}</p>
          <p class="text-label-sm text-on-surface-variant">Oturum</p>
        </div>
      </div>

      <!-- Action Button -->
      <button
        @click="$router.push('/profile')"
        class="w-full mt-md py-2.5 bg-secondary-container text-on-secondary-container font-bold rounded-xl hover:opacity-90 transition-all text-body-sm"
      >
        Profili Düzenle
      </button>
    </div>

    <!-- Roles List (Top Communities yerine) -->
    <div
      v-if="userRoles.length > 0"
      class="mt-auto px-md py-md border-t border-outline-variant/20"
    >
      <div class="flex justify-between items-center mb-md">
        <h5
          class="text-label-sm font-bold text-on-surface-variant uppercase tracking-widest"
        >
          Rollerim
        </h5>
        <NuxtLink
          to="/roles"
          class="text-on-surface-variant hover:text-primary transition-colors"
          title="Tüm rolleri görüntüle"
        >
          <Icon name="mdi:arrow-right" class="w-4 h-4" />
        </NuxtLink>
      </div>

      <div class="space-y-3">
        <div
          v-for="role in displayRoles"
          :key="role.id || role.name"
          class="flex items-center gap-3 group cursor-pointer"
        >
          <!-- Role Icon Badge -->
          <div
            class="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center text-primary flex-shrink-0"
          >
            <Icon
              :name="role.isSystemRole ? 'mdi:shield-crown' : 'mdi:shield-account'"
              class="w-4 h-4"
            />
          </div>
          <div class="flex-1 min-w-0">
            <p
              class="text-body-sm font-semibold text-on-surface group-hover:text-primary transition-colors truncate"
            >
              {{ role.name }}
            </p>
            <p
              v-if="role.description"
              class="text-label-sm text-on-surface-variant truncate"
            >
              {{ role.description }}
            </p>
          </div>
          <span
            v-if="role.isSystemRole"
            class="text-label-sm bg-secondary-container/20 text-on-secondary-container px-2 py-0.5 rounded font-bold"
          >
            Sistem
          </span>
        </div>

        <!-- "Daha fazla" link -->
        <p
          v-if="userRoles.length > displayRoles.length"
          class="text-label-sm text-on-surface-variant text-center pt-1"
        >
          +{{ userRoles.length - displayRoles.length }} daha
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const authStore = useAuthStore()

// MOCK DATA — backend bağlantısı 2B sonrası bir adımda yapılacak.
// Şu an aktif session sayısı için gerçek endpoint yok, mock 3 kullanıyoruz.
const MOCK_ACTIVE_SESSIONS = 3

const userFullName = computed(
  () => authStore.userFullName || 'Kullanıcı'
)

const userEmail = computed(() => authStore.user?.email || '')

const userInitials = computed(() => {
  if (!authStore.user) return '?'
  const first = authStore.user.firstName?.charAt(0) || ''
  const last = authStore.user.lastName?.charAt(0) || ''
  return (first + last).toUpperCase() || '?'
})

// Rol ve izin sayıları auth store'dan
const userRoles = computed(() => authStore.user?.roles || [])
const rolesCount = computed(() => userRoles.value.length)
const permissionsCount = computed(() => authStore.permissions.length)
const activeSessionsCount = computed(() => MOCK_ACTIVE_SESSIONS)

// İlk 3 rolü göster, gerisi "+N daha" ile özetlenir
const displayRoles = computed(() => userRoles.value.slice(0, 3))
</script>
