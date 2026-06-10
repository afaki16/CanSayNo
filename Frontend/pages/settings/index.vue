<template>
  <div class="settings-container">
    <!-- Breadcrumb -->
    <div class="mb-6">
      <BreadCrumb
        :items="[{ text: 'Ana Sayfa', to: '/dashboard' }, { text: 'Ayarlar' }]"
      />
    </div>

    <!-- Page Header -->
    <div class="settings-header mb-8">
      <h1 class="settings-title">Ayarlar</h1>
      <p class="settings-subtitle">Sistem ve tenant ayarlarını yönetin</p>
    </div>

    <!-- Tabs: Admin / SuperAdmin (sadece her iki rol varsa göster) -->
    <v-card class="settings-card" elevation="2">
      <template v-if="hasMultipleTabs">
        <v-tabs
          v-model="activeTab"
          bg-color="transparent"
          color="primary"
          grow
          class="settings-tabs"
        >
          <v-tab value="admin" prepend-icon="mdi-shield-account-outline">
            Admin Ayarları
          </v-tab>
          <v-tab value="superadmin" prepend-icon="mdi-shield-crown-outline">
            SuperAdmin Ayarları
          </v-tab>
        </v-tabs>
        <v-divider />
      </template>

      <v-window v-model="activeTab" class="settings-window">
        <!-- Admin Tab Content -->
        <v-window-item value="admin" v-if="showAdminTab">
          <div class="pa-6">
            <SettingsAdminSection />
          </div>
        </v-window-item>

        <!-- SuperAdmin Tab Content -->
        <v-window-item value="superadmin" v-if="showSuperAdminTab">
          <div class="pa-6">
            <SettingsSuperAdminSection />
          </div>
        </v-window-item>
      </v-window>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import BreadCrumb from "~/components/BreadCrumb.vue";
import SettingsAdminSection from "~/components/Settings/SettingsAdminSection.vue";
import SettingsSuperAdminSection from "~/components/Settings/SettingsSuperAdminSection.vue";

//#region Page Metadata
definePageMeta({
  title: "Ayarlar",
  requiresAuth: true,
  middleware: ["permission"],
  roles: ["Admin", "SuperAdmin"],
});

useHead({
  title: "Ayarlar - Kickstart",
});
//#endregion

//#region Auth & Tabs
const authStore = useAuthStore();

// Tenant Ayarları: sadece Admin rolü (SuperAdmin ayrı sekme; yoksa yanlışlıkla tenant-scope UI görünür)
const showAdminTab = computed(() => authStore.hasRole("Admin"));
const showSuperAdminTab = computed(() => authStore.hasRole("SuperAdmin"));

// Hem Admin hem SuperAdmin atanmış kullanıcıda iki sekme
const hasMultipleTabs = computed(
  () => showAdminTab.value && showSuperAdminTab.value,
);

const activeTab = ref<"admin" | "superadmin">("admin");

watch(
  [showAdminTab, showSuperAdminTab],
  ([admin, superAdmin]) => {
    if (superAdmin) activeTab.value = "superadmin";
    else if (admin) activeTab.value = "admin";
  },
  { immediate: true },
);
//#endregion
</script>

<style scoped>
.settings-container {
  padding: 24px;
  background: rgb(var(--v-theme-background));
  min-height: 100vh;
}

.settings-header {
  background: rgb(var(--v-theme-surface));
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.settings-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: rgb(var(--v-theme-on-surface));
  margin: 0;
}

.settings-subtitle {
  font-size: 0.95rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin-top: 8px;
}

.settings-card {
  border-radius: 16px;
  overflow: hidden;
}

.settings-tabs :deep(.v-tab) {
  text-transform: none;
  font-weight: 600;
}

.settings-window {
  min-height: 300px;
}
</style>
