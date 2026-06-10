<template>
  <!-- Breadcrumb -->
  <div class="mb-6">
    <BreadCrumb
      :items="[{ text: 'Ana Sayfa', to: '/' }, { text: 'Tenant\'lar' }]"
    />
  </div>

  <BaseDataTable
    :items="items"
    :columns="tableColumns"
    title="Tenant'lar"
    toolbar-icon="mdi-domain"
    search-placeholder="Tenant ara..."
    add-button-text="Yeni Tenant Ekle"
    :loading="isLoading"
    loading-text="Tenant'lar yükleniyor..."
    empty-title="Tenant bulunamadı"
    :show-add-button="true"
    :show-advanced-filters="false"
    :show-actions="true"
    :show-view-button="true"
    :show-edit-button="true"
    :show-delete-button="true"
    :show-pagination="true"
    :items-per-page="pageSize"
    @add="openCreateDialog"
    @view="openViewDialog"
    @edit="openEditDialog"
    @delete="openDeleteDialog"
    @search="handleSearch"
    @refresh="refreshData"
  >
    <template #cell-name="{ value }">
      <v-chip
        color="primary"
        variant="tonal"
        size="default"
        prepend-icon="mdi-domain"
      >
        {{ value }}
      </v-chip>
    </template>

    <template #cell-isActive="{ value }">
      <v-chip :color="value ? 'success' : 'error'" size="small" variant="tonal">
        {{ value ? "Aktif" : "Pasif" }}
      </v-chip>
    </template>

    <template #cell-userCount="{ value }">
      <v-chip size="small" variant="tonal" color="info">
        {{ value ?? 0 }} kullanıcı
      </v-chip>
    </template>
  </BaseDataTable>

  <!-- Create/Edit Drawer -->
  <ResizableDrawer
    v-model="dialogs.create"
    :title="isEditMode ? 'Tenant Düzenle' : 'Yeni Tenant Ekle'"
    :subtitle="isEditMode ? 'Tenant bilgilerini güncelleyin' : 'Sisteme yeni tenant bilgilerini girin'"
    icon="mdi-domain"
    :default-width="600"
    :min-width="400"
  >
    <TenantForm
      :tenant="selectedItem"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
  </ResizableDrawer>

  <!-- Confirm Delete Dialog -->
  <ConfirmDialog
    v-model="dialogs.delete"
    title="Tenant'ı Sil"
    :message="`'${itemToDelete?.name}' tenant'ını silmek istediğinizden emin misiniz?`"
    type="error"
    confirm-text="Sil"
    :loading="isDeleting"
    @confirm="confirmDelete"
    @cancel="closeDeleteDialog"
  />
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import BaseDataTable from "~/components/UI/BaseDataTable.vue";
import TenantForm from "~/components/Tenants/TenantForm.vue";
import ResizableDrawer from "~/components/UI/ResizableDrawer.vue";
import ConfirmDialog from "~/components/UI/ConfirmDialog.vue";

//#region Page Metadata
definePageMeta({
  title: "Tenant'lar",
  requiresAuth: true,
  middleware: ["permission"],
  roles: ["SuperAdmin"],
});

useHead({
  title: "Tenant'lar",
});
//#endregion

//#region DataTable Columns
const tableColumns = [
  {
    label: "Tenant Adı",
    key: "name",
    sortable: true,
    filterable: true,
    filterType: "text",
    width: "250px",
  },
  {
    label: "Domain",
    key: "domain",
    sortable: true,
    filterable: true,
    filterType: "text",
    width: "200px",
  },
  {
    label: "İletişim E-postası",
    key: "contactEmail",
    sortable: true,
    filterable: true,
    filterType: "text",
    width: "250px",
  },
  {
    label: "Kullanıcı Sayısı",
    key: "userCount",
    sortable: true,
    width: "120px",
  },
  {
    label: "Durum",
    key: "isActive",
    sortable: true,
    filterable: true,
    filterType: "select",
    width: "100px",
  },
];
//#endregion

//#region Composables
const { getTenants, createTenant, updateTenant, deleteTenant } = useTenants();

const {
  items,
  isLoading,
  isDeleting,
  dialogs,
  selectedItem,
  itemToDelete,
  isEditMode,
  pageSize,
  openCreateDialog,
  openViewDialog,
  openEditDialog,
  openDeleteDialog,
  closeCreateDialog,
  closeDeleteDialog,
  handleSubmit,
  confirmDelete,
  handleSearch,
  refreshData,
  loadItemsData,
} = useCrudOperations({
  loadItems: getTenants,
  createItem: createTenant,
  updateItem: updateTenant,
  deleteItem: deleteTenant,
  itemName: "tenant",
  serverSidePagination: false,
  initialPageSize: 10,
});
//#endregion

//#region Lifecycle
onMounted(async () => {
  await loadItemsData();
});
//#endregion
</script>
