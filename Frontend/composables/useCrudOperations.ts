// composables/useCrudOperations.ts
import { useToastStore } from '~/stores/toast'

export const useCrudOperations = (config) => {
  const {
    loadItems,
    createItem,
    updateItem,
    deleteItem,
    itemName = 'öğe',
    serverSidePagination = false,
    initialPageSize = 10,
    initialData,
    toastMessages = {
      createSuccess: `${itemName} başarıyla oluşturuldu`,
      updateSuccess: `${itemName} başarıyla güncellendi`,
      deleteSuccess: `${itemName} başarıyla silindi`,
      loadError: `${itemName} yüklenirken hata oluştu`,
      createError: `${itemName} oluşturulurken hata oluştu`,
      updateError: `${itemName} güncellenirken hata oluştu`,
      deleteError: `${itemName} silinirken hata oluştu`
    }
  } = config

  const dialogManager = useDialogManager()
  const toastStore = useToastStore()
  const items = ref([])
  const isLoading = ref(false)
  const isDeleting = ref(false)

  // Pagination state (for server-side)
  const currentPage = ref(1)
  const pageSize = ref(initialPageSize)
  const totalCount = ref(0)
  const searchTerm = ref('')

  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  // Sync from initialData (e.g. useAsyncData for SSR)
  if (initialData) {
    watchEffect(() => {
      const data = typeof initialData === 'function' ? initialData() : initialData
      const resolved = data?.value ?? data
      if (resolved?.data) {
        items.value = resolved.data.items || []
        totalCount.value = resolved.data.totalCount ?? 0
        currentPage.value = resolved.data.pageNumber ?? 1
      }
    })
  }

  // Load items
  const loadItemsData = async (page, size, search) => {
    try {
      isLoading.value = true
      const p = page ?? currentPage.value
      const s = size ?? pageSize.value
      const srch = search ?? searchTerm.value
      const params = serverSidePagination ? [p, s, srch] : []
      const response = await loadItems(...params)

      if (serverSidePagination && response?.data) {
        items.value = response.data.items || []
        totalCount.value = response.data.totalCount ?? 0
        currentPage.value = response.data.pageNumber ?? p
      } else if (Array.isArray(response)) {
        items.value = response
        totalCount.value = response.length
      } else if (response && response.data) {
        items.value = response.data.items || response.data
        totalCount.value = response.data.totalCount ?? (response.data.items || response.data).length
      } else {
        items.value = []
        totalCount.value = 0
      }
    } catch (error) {
      console.error(`Error loading ${itemName}:`, error)
      items.value = []
      totalCount.value = 0
    } finally {
      isLoading.value = false
    }
  }

  const handlePageChange = (page) => {
    currentPage.value = page
    if (serverSidePagination) {
      loadItemsData(page, pageSize.value, searchTerm.value)
    }
  }

  const handlePageSizeChange = (size) => {
    pageSize.value = size
    currentPage.value = 1
    if (serverSidePagination) {
      loadItemsData(1, size, searchTerm.value)
    }
  }

  const handleSearch = (term = '') => {
    searchTerm.value = term
    currentPage.value = 1
    loadItemsData(1, pageSize.value, serverSidePagination ? term : '')
  }

  // Handle submit (create/update)
  const handleSubmit = async (itemData) => {
    try {
      isLoading.value = true
      
      if (dialogManager.isEditMode.value && dialogManager.selectedItem.value) {
        await updateItem(dialogManager.selectedItem.value.id, itemData)
        toastStore.add('success', toastMessages.updateSuccess)
      } else {
        await createItem(itemData)
        toastStore.add('success', toastMessages.createSuccess)
      }
      
      dialogManager.closeCreateDialog()
      await loadItemsData(currentPage.value, pageSize.value, searchTerm.value)
    } catch (error) {
      console.error(`Error submitting ${itemName}:`, error)
    } finally {
      isLoading.value = false
    }
  }

  // Handle delete
  const confirmDelete = async () => {
    if (!dialogManager.itemToDelete.value) return

    try {
      isDeleting.value = true
      await deleteItem(dialogManager.itemToDelete.value.id)
      await loadItemsData(currentPage.value, pageSize.value, searchTerm.value)
      dialogManager.closeDeleteDialog()
      toastStore.add('success', toastMessages.deleteSuccess)
    } catch (error) {
      console.error(`Error deleting ${itemName}:`, error)
    } finally {
      isDeleting.value = false
    }
  }

  // Refresh handler
  const refreshData = () => {
    loadItemsData(currentPage.value, pageSize.value, searchTerm.value)
  }

  return {
    // Data
    items,
    isLoading,
    isDeleting,

    // Pagination (for server-side)
    currentPage,
    pageSize,
    totalCount,
    totalPages,
    searchTerm,

    // Dialog manager
    ...dialogManager,

    // Operations
    loadItemsData,
    refreshData,
    handleSubmit,
    confirmDelete,
    handleSearch,
    handlePageChange,
    handlePageSizeChange
  }
}