// composables/useDialogManager.ts
export const useDialogManager = () => {
  const dialogs = ref({
    create: false,
    view: false,
    edit: false,
    delete: false
  })
  
  const selectedItem = ref(null)
  const itemToDelete = ref(null)
  const isEditMode = ref(false)

  const openDialog = (type, item = null) => {
    if (type === 'delete') {
      itemToDelete.value = item
      dialogs.value.delete = true
      return
    }

    selectedItem.value = item ? { ...item } : null
    
    if (type === 'view') {
      dialogs.value.view = true
      isEditMode.value = false
    } else {
      isEditMode.value = type === 'edit'
      dialogs.value.create = true
    }
  }

  const closeDialog = (type) => {
    dialogs.value[type] = false
    
    if (type === 'delete') {
      itemToDelete.value = null
    } else {
      selectedItem.value = null
      isEditMode.value = false
    }
  }

  const closeAllDialogs = () => {
    Object.keys(dialogs.value).forEach(key => {
      dialogs.value[key] = false
    })
    selectedItem.value = null
    itemToDelete.value = null
    isEditMode.value = false
  }

  // Convenience methods
  const openCreateDialog = () => openDialog('create')
  const openViewDialog = (item) => openDialog('view', item)
  const openEditDialog = (item) => openDialog('edit', item)
  const openDeleteDialog = (item) => openDialog('delete', item)
  
  const closeCreateDialog = () => closeDialog('create')
  const closeViewDialog = () => closeDialog('view')
  const closeDeleteDialog = () => closeDialog('delete')

  return {
    // States
    dialogs,
    selectedItem,
    itemToDelete,
    isEditMode,
    
    // Generic methods
    openDialog,
    closeDialog,
    closeAllDialogs,
    
    // Convenience methods
    openCreateDialog,
    openViewDialog,
    openEditDialog,
    openDeleteDialog,
    closeCreateDialog,
    closeViewDialog,
    closeDeleteDialog
  }
}