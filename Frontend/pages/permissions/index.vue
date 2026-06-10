<template>
    <!-- Breadcrumb -->
    <div class="mb-6">
      <BreadCrumb :items="[
        { text: 'Ana Sayfa', to: '/' },
        { text: 'İzinler' }
      ]" />
    </div>

    <BaseDataTable
      :items="permissions"
      :columns="tableColumns"
      title="İzinler"
      toolbar-icon="mdi-key-variant"
      search-placeholder="İzin ara..."
      :loading="isLoading"
      loading-text="İzinler yükleniyor..."
      empty-title="İzin bulunamadı"
      empty-description="Henüz hiç izin tanımlanmamış."
      :show-add-button="false"
      :show-advanced-filters="true"
      :show-actions="false"
      :show-pagination="true"
      :items-per-page="10"
      @search="handleSearch"
    >
      <!-- Custom cell renderers -->
      <template #cell-resource="{ item, value }">
        <div class="resource-cell">
          <div class="resource-badge" :class="getResourceColorClass(value)">
            <span class="resource-name">{{ formatResourceName(value) }}</span>
          </div>
        </div>
      </template>

      <template #cell-description="{ item, value }">
        <div class="description-cell">
          <p class="description-text" :title="value">
            {{ value || 'Açıklama bulunmuyor' }}
          </p>
        </div>
      </template>
    </BaseDataTable>
  
</template>

<script setup>
import { ref, onMounted } from 'vue'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'

// Page metadata - Önemli !!!
definePageMeta({
  title: 'İzin Yönetimi',
  requiresAuth: true,
  permissions: ['permissions.read']
})

useHead({
  title: 'İzin Yönetimi',
})

// Composables
const { getPermissions } = usePermissions()

// Reactive data
const permissions = ref([])
const isLoading = ref(false)

// Table columns for BaseDataTable
const tableColumns = [
  { 
    key: 'name',
    label: 'İzin Adı', 
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '300px'
  },
  { 
    key: 'resource',
    label: 'Kaynak', 
    sortable: true,
    filterable: true,
    filterType: 'select',
    width: '300px'
  },
  { 
    key: 'description',
    label: 'Açıklama',
    sortable: false,
    filterable: true,
    filterType: 'text',
    width: '300px'
  }
]

// Methods
const loadPermissions = async () => {
  try {
    isLoading.value = true
    const response = await getPermissions()
    
    // Handle different response formats
    if (Array.isArray(response)) {
      permissions.value = response
    } else if (response && response.data) {
      permissions.value = response.data.items || response.data
    } else {
      permissions.value = []
    }
  } catch (error) {
    console.error('Error loading permissions:', error)
    permissions.value = []
  } finally {
    isLoading.value = false
  }
}

// Formatting methods
const formatResourceName = (resource) => {
  const resourceNames = {
    'users': 'Kullanıcılar',
    'roles': 'Roller',
    'permissions': 'İzinler',
  }
  return resourceNames[resource?.toLowerCase()] || resource
}

// Style methods
const getResourceColorClass = (resource) => {
  const colorMap = {
    'users': 'resource-blue',
    'roles': 'resource-purple',
    'permissions': 'resource-yellow',
  }
  return colorMap[resource?.toLowerCase()] || 'resource-green'
}

const handleSearch = (query) => {
  // Additional search logic can be implemented here
}

// Load initial data
onMounted(async () => {
  await loadPermissions()
})

</script>

<style scoped>
/* Resource Cell */
.resource-cell {
  @apply flex items-center;
}

.resource-badge {
  @apply inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-medium;
}

/* Resource Colors */
.resource-blue { @apply bg-blue-100 text-blue-800; }
.resource-purple { @apply bg-purple-100 text-purple-800; }
.resource-green { @apply bg-green-100 text-green-800; }
.resource-yellow { @apply bg-yellow-100 text-yellow-800; }

/* Description Cell */
.description-cell {
  @apply max-w-xs;
}

.description-text {
  @apply text-sm text-gray-600 truncate;
}

/* Responsive */
@media (max-width: 768px) {
  .resource-badge {
    @apply px-2 py-0.5;
  }
}
</style>