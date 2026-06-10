<template>
  <!-- Breadcrumb -->
    <div class="mb-6">
      <BreadCrumb :items="[
        { text: 'Ana Sayfa', to: '/' },
        { text: 'Roller' }
      ]" />
    </div>

     <!-- DataTable -->
     <BaseDataTable
      :items="items"
      :columns="tableColumns"
      title="Roller"
      toolbar-icon="mdi-shield-account"
      search-placeholder="Rol ara..."
      add-button-text="Yeni Rol Ekle"
      :loading="isLoading"
      loading-text="Roller yükleniyor..."
      empty-title="Roller bulunamadı"
      :show-add-button="true"
      :show-advanced-filters="false"
      :show-actions="true"
      :show-view-button="true"
      :show-edit-button="true"
      :show-delete-button="true"
      :show-pagination="true"
      :items-per-page="pageSize"
      :server-side-pagination="true"
      :server-total-count="totalCount"
      :server-current-page="currentPage"
      use-view-modal
      @add="openCreateDialog"
      @view="openViewDialog"
      @edit="openEditDialog"
      @delete="openDeleteDialog"
      @search="handleSearch"
      @page-change="handlePageChange"
      @page-size-change="handlePageSizeChange"
      @refresh="refreshData"
    >
       <!-- For RoleName -->
    <template #cell-name="{ value }">
  <v-chip
    color="primary"
    variant="tonal"
    size="default"
    prepend-icon="mdi-shield-account"
  >
    {{ value }}
  </v-chip>
</template>

    <!-- For isSystemRole -->
  <template #cell-isSystemRole="{ value }">
  <v-chip
    :color="value == true ? 'warning' : 'success '"
    size="small"
    variant="tonal"
  >
    {{ value == true ? 'Sistem' : 'Özel' }}
  </v-chip>
</template>

     </BaseDataTable>

    <!-- Create/Edit Role Drawer -->
    <ResizableDrawer
      v-model="dialogs.create"
      :title="isEditMode ? 'Rolü Düzenle' : 'Yeni Rol Ekle'"
      :subtitle="isEditMode ? 'Rol bilgilerini ve yetkilerini düzenleyin' : 'Yeni rol ve yetkilerini tanımlayın'"
      icon="mdi-shield-account"
      :default-width="600"
      :min-width="400"
    >
      <RoleForm
        :role="selectedItem"
        :permissions="permissions"
        :loading="isLoading"
        @submit="handleSubmit"
        @cancel="closeCreateDialog"
      />
    </ResizableDrawer>

    <!-- Confirm Delete Dialog -->
    <ConfirmDialog
      v-model="dialogs.delete"
      title="Rolü Sil"
      :message="`'${itemToDelete?.name}' rolünü silmek istediğinizden emin misiniz?`"
      type="error"
      confirm-text="Sil"
      :loading="isDeleting"
      @confirm="confirmDelete"
      @cancel="closeDeleteDialog"
    />
  
</template>

<script setup>
import { ref, onMounted } from 'vue'
import RoleForm from '~/components/Roles/RoleForm.vue'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import ResizableDrawer from '~/components/UI/ResizableDrawer.vue'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'

//#region Page Metadata
definePageMeta({
  title: 'Roller',
  requiresAuth: true,
  permissions: ['roles.read']
})

useHead({
  title: 'Roller',
})
//#endregion

//#region DataTable Columns
const tableColumns = [
  {
    label: 'Rol Adı',
    key: 'name',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '300px',
  },
  {
    label: 'Rol Tipi',
    key: 'isSystemRole',
    sortable: true,
    filterable: false,
    filterType: 'select',
    width: '300px'
  },
  {
    label: 'Rol Açıklaması',
    key: 'description',
    sortable: true,
    filterable: false,
    width: '300px'
  }
]
//#endregion

//#region Composables
const { getRoles, createRole, updateRole, deleteRole } = useRoles()
const { getPermissions } = usePermissions()

// SSR: İlk sayfa verisi sunucuda yüklenir
const { data: rolesData } = await useAsyncData(
  'roles-initial',
  () => getRoles(1, 10, ''),
  { server: true }
)

const {
  items,
  isLoading,
  isDeleting,
  dialogs,
  selectedItem,
  itemToDelete,
  isEditMode,
  currentPage,
  pageSize,
  totalCount,
  openCreateDialog,
  openViewDialog,
  openEditDialog,
  openDeleteDialog,
  closeCreateDialog,
  closeDeleteDialog,
  handleSubmit,
  confirmDelete,
  handleSearch,
  handlePageChange,
  handlePageSizeChange,
  refreshData,
  loadItemsData
} = useCrudOperations({
  loadItems: getRoles,
  createItem: createRole,
  updateItem: updateRole,
  deleteItem: deleteRole,
  itemName: 'rol',
  serverSidePagination: true,
  initialPageSize: 10,
  initialData: rolesData
})

//#endregion

//#region Additional Data
const permissions = ref([])

const loadPermissions = async () => {
  try {
    const response = await getPermissions()
    permissions.value = response || []
  } catch (error) {
    console.error('Error loading permissions:', error)
    permissions.value = []
  }
}
//#endregion

//#region Lifecycle
onMounted(async () => {
  await loadPermissions()
  if (!rolesData.value) {
    await loadItemsData(1, 10, '')
  }
})
//#endregion
</script>
