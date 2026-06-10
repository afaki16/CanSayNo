<template>
    <!-- Breadcrumb -->
    <div class="mb-6">
      <BreadCrumb :items="[
        { text: 'Ana Sayfa', to: '/' },
        { text: 'Kullanıcılar' }
      ]" />
    </div>

  <BaseDataTable
      :items="items"
      :columns="tableColumns"
      title="Kullanıcılar"
      toolbar-icon="mdi-account"
      search-placeholder="Kullanıcı ara..."
      add-button-text="Yeni Kullanıcı Ekle"
      :loading="isLoading"
      loading-text="Kullanıcılar yükleniyor..."
      empty-title="Kullanıcı bulunamadı"
      :show-add-button="true"
      :show-advanced-filters="true"
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
    <!-- For FullName -->
 <template #cell-fullName="{ item, value }">
  <v-chip
    color="primary"
    variant="tonal"
    size="default"
    prepend-icon="mdi-account"
  >
    {{ value }}
    <span v-if="item.username" class="ml-1 opacity-75 text-xs">
      (@{{ item.username }})
    </span>
  </v-chip>
</template>

<!-- For Status -->
  <template #cell-status="{ value }">
  <v-chip
    :color="value == 1 ? 'success' : 'error'"
    size="small"
    variant="tonal"
  >
    {{ value == 1 ? 'Aktif' : 'Pasif' }}
  </v-chip>
</template>
  </BaseDataTable>

  <!-- Create/Edit Drawer -->
  <ResizableDrawer
    v-model="dialogs.create"
    :title="isEditMode ? 'Kullanıcıyı Düzenle' : 'Yeni Kullanıcı Ekle'"
    :subtitle="isEditMode ? 'Kullanıcı bilgilerini düzenleyin' : 'Sisteme yeni kullanıcı bilgilerini girin'"
    icon="mdi-account"
    :default-width="600"
    :min-width="400"
  >
    <UserForm
      :user="selectedItem"
      :roles="roles"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
  </ResizableDrawer>

  <!-- View Detail Drawer -->
  <ResizableDrawer
    v-model="dialogs.view"
    title="Kullanıcı Detayları"
    :subtitle="'Kullanıcı bilgilerini görüntüleyin'"
    icon="mdi-account-details"
    :default-width="800"
    :min-width="600"
  >
    <UserDetail
      :user="selectedItem"
      @close="closeViewDialog"
      @edit="handleEditFromDetail"
    />
  </ResizableDrawer>

  <!-- Confirm Delete Dialog -->
  <ConfirmDialog
    v-model="dialogs.delete"
    title="Kullanıcıyı Sil"
    :message="`'${itemToDelete?.fullName || itemToDelete?.email}' kullanıcısını silmek istediğinizden emin misiniz?`"
    type="error"
    confirm-text="Sil"
    :loading="isDeleting"
    @confirm="confirmDelete"
    @cancel="closeDeleteDialog"
  />

</template>

<script setup>
import { ref, onMounted } from 'vue'
import { dateTimeFormatLong } from '~/utils/datesFormat.ts'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import ResizableDrawer from '~/components/UI/ResizableDrawer.vue'
import UserForm from '~/components/Users/UserForm.vue'
import UserDetail from '~/components/Users/UserDetail.vue'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'

//#region Page Metadata
definePageMeta({
  title: 'Kullanıcılar',
  requiresAuth: true,
  permissions: ['users.read']
})

useHead({
  title: 'Kullanıcılar',
})
//#endregion

//#region DataTable Columns
const authStore = useAuthStore()
const tableColumns = computed(() => {
  const base = [
    {
      label: 'Ad Soyad',
      key: 'fullName',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '300px'
    },
    {
      label: 'E-posta',
      key: 'email',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '300px'
    },
    {
      label: 'Durum',
      key: 'status',
      sortable: true,
      filterable: true,
      filterType: 'select',
      width: '300px'
    },
    {
      label: 'Son Giriş Tarihi',
      key: 'lastLoginDate',
      sortable: true,
      filterable: false,
      width: '300px',
      formatter: dateTimeFormatLong
    }
  ]
  if (authStore.hasRole('SuperAdmin')) {
    base.splice(2, 0, {
      label: 'Tenant ID',
      key: 'tenantId',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '100px'
    })
  }
  return base
})
//#endregion

//#region Composables
const { getUsers, createUser, updateUser, deleteUser } = useUsers()
const { getRoles } = useRoles()

// SSR: İlk sayfa verisi sunucuda yüklenir
const { data: usersData } = await useAsyncData(
  'users-initial',
  () => getUsers(1, 10, ''),
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
  closeViewDialog,
  closeDeleteDialog,
  handleSubmit,
  confirmDelete,
  handleSearch,
  handlePageChange,
  handlePageSizeChange,
  refreshData,
  loadItemsData
} = useCrudOperations({
  loadItems: getUsers,
  createItem: createUser,
  updateItem: updateUser,
  deleteItem: deleteUser,
  itemName: 'kullanıcı',
  serverSidePagination: true,
  initialPageSize: 10,
  initialData: usersData
})

//#endregion

//#region Additional Data
const roles = ref([])

const loadRoles = async () => {
  try {
    const response = await getRoles(1, 1000, '')
    roles.value = response?.data?.items || []
  } catch (error) {
    console.error('Error loading roles:', error)
    roles.value = []
  }
}

const handleEditFromDetail = (user) => {
  closeViewDialog()
  openEditDialog(user)
}
//#endregion

//#region Lifecycle
onMounted(async () => {
  await loadRoles()
  // SSR verisi yoksa (client-side navigation) ilk yükleme
  if (!usersData.value) {
    await loadItemsData(1, 10, '')
  }
})
//#endregion
</script>
