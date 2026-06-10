<template>
  <div class="flex h-screen w-full bg-background font-body-md text-on-surface antialiased overflow-hidden">
    <!-- Sidebar Navigation -->
    <aside
      class="flex flex-col h-full border-r border-outline-variant bg-surface py-base z-30 transition-all duration-300"
      :class="isSidebarOpen ? 'w-64' : 'w-20'"
    >
      <!-- Logo + Brand + Toggle -->
      <div class="px-md py-md mb-md flex items-center" :class="isSidebarOpen ? 'justify-between' : 'justify-center'">
        <div v-if="isSidebarOpen" class="flex items-center gap-3 min-w-0">
          <img
            v-if="appData?.app?.logo?.src"
            :src="appData.app.logo.src"
            :alt="appData?.app?.logo?.alt || 'Logo'"
            class="object-contain flex-shrink-0 h-8 w-8"
          />
          <Icon
            v-else
            name="mdi:cloud"
            class="text-primary"
            style="font-size: 32px"
          />
          <h1 class="text-headline-md font-headline-md font-bold text-on-surface truncate">
            {{ appData?.app?.brand?.text || 'Kickstart' }}
          </h1>
        </div>
        <button
          @click="toggleSidebar"
          class="flex items-center justify-center w-10 h-10 rounded-lg text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high transition-all"
          :title="isSidebarOpen ? 'Daralt' : 'Genişlet'"
        >
          <Icon name="mdi:menu" class="w-6 h-6" />
        </button>
      </div>

      <!-- Search (opsiyonel — sadece sidebar açıkken görünür) -->
      <div v-if="isSidebarOpen" class="px-md mb-base">
        <div class="relative">
          <Icon name="mdi:magnify" class="absolute left-3 top-1/2 -translate-y-1/2 text-on-surface-variant w-4 h-4" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Ara..."
            class="w-full bg-surface-container-low border border-outline-variant/30 rounded-xl py-2 pl-10 pr-4 text-body-sm text-on-surface focus:ring-2 focus:ring-primary focus:border-transparent transition-all placeholder-on-surface-variant/50 outline-none"
          />
        </div>
      </div>

      <!-- Navigation Menu -->
      <nav :key="authStore.user?.id || 'guest'" class="flex-1 overflow-y-auto space-y-1 mt-md scrollbar-thin">
        <template v-for="item in filteredMenus" :key="item.title">
          <!-- Top-level item (no children) -->
          <NuxtLink
            v-if="!item.children && item.to"
            :to="item.to"
            class="flex items-center gap-3 mx-4 px-4 py-3 rounded-xl transition-all"
            :class="[
              isActive(item.to)
                ? 'sidebar-active-pill'
                : 'text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high',
              !isSidebarOpen && 'justify-center px-0'
            ]"
            :title="!isSidebarOpen ? $t(item.title) : undefined"
          >
            <Icon :name="item.icon" class="w-5 h-5 flex-shrink-0" />
            <span v-if="isSidebarOpen" class="font-body-md font-semibold whitespace-nowrap">{{ $t(item.title) }}</span>
          </NuxtLink>

          <!-- Group with children -->
          <div v-else-if="item.children" class="pt-2">
            <p
              v-if="isSidebarOpen"
              class="px-md py-xs text-label-sm uppercase text-on-surface-variant/40 font-bold tracking-widest"
            >
              {{ $t(item.title) }}
            </p>
            <div v-else class="border-t border-outline-variant/30 my-2 mx-4"></div>

            <NuxtLink
              v-for="child in item.children"
              :key="child.title"
              :to="child.to"
              class="flex items-center gap-3 mx-4 px-4 py-3 rounded-xl transition-all"
              :class="[
                isActive(child.to)
                  ? 'sidebar-active-pill'
                  : 'text-on-surface-variant hover:text-on-surface hover:bg-surface-container-high',
                !isSidebarOpen && 'justify-center px-0'
              ]"
              :title="!isSidebarOpen ? $t(child.title) : undefined"
            >
              <Icon :name="child.icon" class="w-5 h-5 flex-shrink-0" />
              <span v-if="isSidebarOpen" class="font-body-md whitespace-nowrap">{{ $t(child.title) }}</span>
            </NuxtLink>
          </div>
        </template>
      </nav>

      <!-- Sidebar Footer: User Dropdown + Logout (sabit) -->
      <div class="px-4 py-4 mt-auto border-t border-outline-variant relative">
        <!-- User Card — Dropdown Trigger -->
        <div class="relative">
          <button
            @click="toggleUserDropdown"
            class="w-full flex items-center gap-3 rounded-xl hover:bg-surface-container-high transition-all"
            :class="isSidebarOpen ? 'px-2 py-2' : 'justify-center p-2'"
            :title="!isSidebarOpen ? userInfo.name : undefined"
          >
            <!-- Avatar -->
            <div class="w-10 h-10 rounded-full bg-primary/20 flex items-center justify-center flex-shrink-0">
              <Icon name="mdi:account" class="w-5 h-5 text-primary" />
            </div>

            <!-- User Info (sadece açık sidebar'da) -->
            <div v-if="isSidebarOpen" class="flex-1 min-w-0 text-left">
              <p class="text-body-sm font-bold text-on-surface truncate">{{ userInfo.name }}</p>
              <p class="text-label-sm text-on-surface-variant truncate">{{ userInfo.email }}</p>
            </div>

            <!-- Caret (sadece açık sidebar'da) -->
            <Icon
              v-if="isSidebarOpen"
              name="mdi:chevron-up"
              class="w-4 h-4 text-on-surface-variant flex-shrink-0 transition-transform"
              :class="{ 'rotate-180': showUserDropdown }"
            />
          </button>

          <!-- Dropdown Menu — yukarı açılır -->
          <Transition
            enter-active-class="transition-all duration-200 ease-out"
            enter-from-class="opacity-0 translate-y-2"
            enter-to-class="opacity-100 translate-y-0"
            leave-active-class="transition-all duration-150 ease-in"
            leave-from-class="opacity-100 translate-y-0"
            leave-to-class="opacity-0 translate-y-2"
          >
            <div
              v-if="showUserDropdown"
              class="user-dropdown-menu absolute bottom-full mb-2 bg-surface-container-low border border-outline-variant/30 rounded-xl shadow-xl z-50 overflow-hidden"
              :class="isSidebarOpen ? 'left-0 right-0' : 'left-full ml-2 w-48'"
            >
              <NuxtLink
                to="/profile"
                @click="closeUserDropdown"
                class="flex items-center gap-3 px-4 py-3 text-on-surface hover:bg-surface-container-high transition-colors"
              >
                <Icon name="mdi:account-circle" class="w-5 h-5 text-on-surface-variant" />
                <span class="text-body-sm font-medium">{{ $t('sidebar.menu.profile') }}</span>
              </NuxtLink>

              <NuxtLink
                to="/settings"
                @click="closeUserDropdown"
                class="flex items-center gap-3 px-4 py-3 text-on-surface hover:bg-surface-container-high transition-colors border-t border-outline-variant/20"
              >
                <Icon name="mdi:cog" class="w-5 h-5 text-on-surface-variant" />
                <span class="text-body-sm font-medium">{{ $t('sidebar.menu.settings') }}</span>
              </NuxtLink>
            </div>
          </Transition>
        </div>

        <!-- Logout — Sabit, dropdown'dan bağımsız -->
        <button
          @click="handleLogout"
          class="w-full flex items-center gap-3 mt-2 px-4 py-2 rounded-xl text-error/80 hover:text-error hover:bg-error/10 transition-colors"
          :class="!isSidebarOpen && 'justify-center px-0'"
        >
          <Icon name="mdi:logout" class="w-5 h-5 flex-shrink-0" />
          <span v-if="isSidebarOpen" class="font-body-md">{{ $t('sidebar.actions.logout') }}</span>
        </button>
      </div>
    </aside>

    <!-- Main Content Area -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <!-- Top Bar -->
      <header class="flex items-center justify-end gap-3 px-margin-desktop py-md border-b border-outline-variant bg-surface flex-shrink-0">
        <!-- Language Switcher (mevcut component) -->
        <LanguageSwitcher v-if="showLanguageSwitcher" />

        <!-- Theme Toggle -->
        <button
          @click="toggleTheme"
          class="w-10 h-10 rounded-xl bg-surface-container-low hover:bg-surface-container-high text-on-surface-variant hover:text-on-surface transition-all flex items-center justify-center"
          :title="isDark ? 'Light moda geç' : 'Dark moda geç'"
        >
          <Icon
            :name="isDark ? 'mdi:weather-sunny' : 'mdi:weather-night'"
            class="w-5 h-5"
          />
        </button>
      </header>

      <!-- Page Content -->
      <main class="flex-1 overflow-y-auto px-margin-desktop py-md">
        <slot />
      </main>
    </div>

    <AppToast />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { storeToRefs } from 'pinia'
import { navigationItems, filterNavigationByPermissions } from '~/composables/useNavigation'
import { useAuth } from '~/composables/useAuth'
import { useAppData } from '~/composables/useAppData'
import { useAuthStore } from '~/stores/auth'
import { useTheme } from '~/composables/useTheme'
import AppToast from '~/components/UI/AppToast.vue'

// Auth state
const authStore = useAuthStore()
const { permissions, roles } = storeToRefs(authStore)
const authUtils = useAuth()
const router = useRouter()
const route = useRoute()

// App data (logo, brand)
const { loadAppData, appData } = useAppData()

// Theme
const { isDark, toggleTheme } = useTheme()

// Sidebar state
const isSidebarOpen = ref(true)
const searchQuery = ref('')

// Language switcher görünürlüğü — opsiyonel, app config'inden okunabilir
const showLanguageSwitcher = computed(() => true)

// User info
const userInfo = computed(() => ({
  name: authStore.userFullName || 'Kullanıcı',
  email: authStore.user?.email || ''
}))

// Yetkiye göre menüleri filtrele
const visibleMenus = computed(() =>
  filterNavigationByPermissions(
    navigationItems,
    (p) => permissions.value.includes(p),
    (r) => roles.value.includes(r)
  )
)

// Search ile menüleri filtrele
const filteredMenus = computed(() => {
  if (!searchQuery.value.trim()) return visibleMenus.value

  const query = searchQuery.value.toLowerCase()
  const filterChildren = (items: typeof visibleMenus.value): typeof visibleMenus.value => {
    return items
      .map((item) => {
        const titleMatch = item.title.toLowerCase().includes(query)
        if (item.children) {
          const matchedChildren = item.children.filter((c) =>
            c.title.toLowerCase().includes(query)
          )
          if (titleMatch || matchedChildren.length > 0) {
            return { ...item, children: matchedChildren.length > 0 ? matchedChildren : item.children }
          }
          return null
        }
        return titleMatch ? item : null
      })
      .filter((i): i is NonNullable<typeof i> => i !== null)
  }
  return filterChildren(visibleMenus.value)
})

// Aktif route kontrolü
const isActive = (to: string | undefined) => {
  if (!to) return false
  return route.path === to || route.path.startsWith(to + '/')
}

// Sidebar toggle
const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

// User dropdown state
const showUserDropdown = ref(false)

const toggleUserDropdown = () => {
  showUserDropdown.value = !showUserDropdown.value
}

const closeUserDropdown = () => {
  showUserDropdown.value = false
}

/**
 * Dropdown dışına tıklayınca kapansın.
 * Named function — onUnmounted'da temizlenebilir.
 */
const handleDocumentClickForUserDropdown = (e: MouseEvent) => {
  const target = e.target as HTMLElement | null
  if (!target) return
  // Dropdown menüsünün veya trigger butonunun içine tıklanmadıysa kapat
  if (!target.closest('.user-dropdown-menu') && !target.closest('button')) {
    showUserDropdown.value = false
  }
}

// Logout
const handleLogout = async () => {
  try {
    await authUtils.logout()
  } catch (error) {
    console.error('Çıkış yapılırken bir hata oluştu:', error)
  }
}

onMounted(async () => {
  await loadAppData()

  // Dropdown dışı tıklama listener
  if (import.meta.client) {
    document.addEventListener('click', handleDocumentClickForUserDropdown)
  }
})

onUnmounted(() => {
  if (import.meta.client) {
    document.removeEventListener('click', handleDocumentClickForUserDropdown)
  }
})
</script>

<style scoped>
/* Active menu item — turuncu gradient pill */
.sidebar-active-pill {
  background: linear-gradient(90deg, #fea619, #ff8c00);
  color: #ffffff !important;
  box-shadow: 0 4px 15px rgba(254, 166, 25, 0.3);
}

.sidebar-active-pill :deep(.iconify),
.sidebar-active-pill span {
  color: #ffffff !important;
}

/* Custom scrollbar */
.scrollbar-thin::-webkit-scrollbar {
  width: 6px;
}
.scrollbar-thin::-webkit-scrollbar-track {
  background: transparent;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
}
.scrollbar-thin::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.2);
}
</style>
