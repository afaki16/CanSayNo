<template>
  <div class="mb-6">
    <BreadCrumb :items="[{ text: 'Ana Sayfa', to: '/' }, { text: 'Senaryolar' }]" />
  </div>

  <BaseDataTable
    :items="items"
    :columns="tableColumns"
    title="Senaryolar"
    toolbar-icon="mdi-script-text"
    search-placeholder="Senaryo ara..."
    add-button-text="Yeni Senaryo"
    :loading="isLoading"
    loading-text="Senaryolar yükleniyor..."
    empty-title="Senaryo bulunamadı"
    :show-add-button="canCreate"
    :show-advanced-filters="true"
    :show-actions="canUpdate"
    :show-view-button="false"
    :show-edit-button="canUpdate"
    :show-delete-button="false"
    :show-pagination="true"
    :items-per-page="pageSize"
    @add="openCreateDialog"
    @edit="openEditDialog"
    @search="handleSearch"
    @refresh="refreshData"
  >
    <template #cell-title="{ item }">
      <div class="py-2">
        <div class="font-weight-medium">{{ item.title }}</div>
        <div class="text-caption text-medium-emphasis">{{ item.id }}</div>
      </div>
    </template>

    <template #cell-language="{ value }">
      <v-chip size="small" variant="tonal" :color="languageOf(value).color">
        {{ value.toUpperCase() }}
      </v-chip>
    </template>

    <template #cell-contextId="{ value }">
      <v-chip size="small" variant="tonal" :color="contextOf(value).color">
        {{ contextOf(value).title }}
      </v-chip>
    </template>

    <template #cell-difficulty="{ value }">
      <v-rating :model-value="value" length="5" size="x-small" density="compact" readonly color="amber" />
    </template>

    <template #cell-isFree="{ value }">
      <v-chip size="small" variant="tonal" :color="value ? 'success' : 'warning'">
        {{ value ? 'Ücretsiz' : 'Premium' }}
      </v-chip>
    </template>

    <template #cell-isActive="{ item }">
      <v-switch
        :model-value="item.isActive"
        color="success"
        density="compact"
        hide-details
        inset
        :disabled="!canUpdate || togglingId === item.id"
        :loading="togglingId === item.id"
        @update:model-value="(v: boolean | null) => toggleActive(item, !!v)"
      />
    </template>

    <template #cell-usage="{ item }">
      <div class="text-body-2">
        {{ item.usage.sessions }} oturum
        <span v-if="item.usage.averageScore !== null" class="text-medium-emphasis">
          · ort. {{ item.usage.averageScore }}
        </span>
      </div>
    </template>
  </BaseDataTable>

  <ResizableDrawer
    v-model="dialogs.create"
    :title="isEditMode ? 'Senaryoyu Düzenle' : 'Yeni Senaryo'"
    :subtitle="isEditMode ? selectedItem?.id : 'Yeni senaryo pasif oluşturulur; hazır olunca aktif et'"
    icon="mdi-script-text"
    :default-width="760"
    :min-width="480"
  >
    <ScenarioForm
      :scenario="selectedItem"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
  </ResizableDrawer>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import ResizableDrawer from '~/components/UI/ResizableDrawer.vue'
import ScenarioForm from '~/components/Scenarios/ScenarioForm.vue'
import type { AdminScenario } from '~/types'
import { useAuthStore } from '~/stores/auth'
import { useToastStore } from '~/stores/toast'
import { contextOf, languageOf } from '~/utils/hayirciLabels'

definePageMeta({
  title: 'Senaryolar',
  requiresAuth: true,
  middleware: ['permission'],
  permissions: ['Scenarios.Read'],
})

useHead({ title: 'Senaryolar' })

const authStore = useAuthStore()
const toastStore = useToastStore()
const canCreate = computed(() => authStore.hasPermission('Scenarios.Create'))
const canUpdate = computed(() => authStore.hasPermission('Scenarios.Update'))

const tableColumns = [
  { label: 'Başlık', key: 'title', sortable: true, filterable: true, filterType: 'text', width: '320px' },
  { label: 'Dil', key: 'language', sortable: true, filterable: true, filterType: 'select', width: '80px' },
  { label: 'Bağlam', key: 'contextId', sortable: true, filterable: true, filterType: 'select', width: '170px' },
  { label: 'Zorluk', key: 'difficulty', sortable: true, width: '130px' },
  { label: 'Erişim', key: 'isFree', sortable: true, filterable: true, filterType: 'select', width: '110px' },
  { label: 'Aktif', key: 'isActive', sortable: true, filterable: true, filterType: 'select', width: '90px' },
  { label: 'Kullanım', key: 'usage', sortable: false, width: '170px' },
]

const { getScenarios, createScenario, updateScenario, setScenarioActive } = useHayirciAdmin()

const {
  items,
  isLoading,
  dialogs,
  selectedItem,
  isEditMode,
  pageSize,
  openCreateDialog,
  openEditDialog,
  closeCreateDialog,
  handleSubmit,
  handleSearch,
  refreshData,
  loadItemsData,
} = useCrudOperations({
  loadItems: getScenarios,
  createItem: createScenario,
  updateItem: updateScenario,
  // Silme yok: geçmiş oturumlar senaryoya bağlı. Kaldırmak için pasif yap.
  deleteItem: async () => false,
  itemName: 'senaryo',
  serverSidePagination: false,
  initialPageSize: 25,
  toastMessages: {
    createSuccess: 'Senaryo oluşturuldu',
    updateSuccess: 'Senaryo güncellendi',
    loadError: 'Senaryolar yüklenirken hata oluştu',
    createError: 'Senaryo oluşturulamadı',
    updateError: 'Senaryo güncellenemedi',
  },
})

const togglingId = ref<string | null>(null)

const toggleActive = async (item: AdminScenario, isActive: boolean) => {
  togglingId.value = item.id
  try {
    const updated = await setScenarioActive(item.id, isActive)
    if (updated) {
      item.isActive = updated.isActive
      toastStore.add('success', isActive ? 'Senaryo aktif — mobilde görünüyor' : 'Senaryo pasif — mobilde gizlendi')
    }
  } catch {
    // Hata useApi'de toast ile gösterildi; anahtar eski değerinde kalır.
  } finally {
    togglingId.value = null
  }
}

onMounted(async () => {
  await loadItemsData()
})
</script>
