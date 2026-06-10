<template>
  <div class="modern-data-table">
    <!-- Header Section -->
    <div class="table-header glassmorphism-header">
      <div class="header-content">
        <!-- Title & Description -->
        <div class="header-left animate-fade-in">
          <div class="title-section">
            <div class="title-wrapper">
              <div class="title-icon modern-icon">
                <v-icon :icon="toolbarIcon" size="28" />
              </div>
              <div class="title-content">
                <h2 class="page-title modern-title">{{ title }}</h2>
                <p class="page-description modern-description">{{ description }}</p>
              </div>
            </div>
          </div>
        </div>
        <!-- Search -->
        <div class="header-right-search">
          <div class="search-input-wrapper">
            <svg class="search-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              type="text"
              v-model="searchQuery"
              :placeholder="searchPlaceholder"
              class="search-input"
              @input="handleSearch"
            />
            <button
              v-if="searchQuery"
              @click="clearSearch"
              class="clear-search-btn"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
        <!-- Actions Section -->
        <div class="header-right">
          <!-- Advanced Filters Toggle -->
          <button
            v-if="showAdvancedFilters"
            @click="toggleAdvancedFilters"
            class="filter-toggle-btn"
            :class="{ 'active': showFilters }"
          >
            <svg class="btn-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 2v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
            </svg>
            Filtreler
            <span v-if="activeFiltersCount" class="filter-count">{{ activeFiltersCount }}</span>
          </button>

          <!-- Refresh Button -->
          <button
            @click="handleRefresh"
            class="refresh-btn icon-only-btn"
            :disabled="refreshing"
            :title="refreshing ? 'Yeniliyor...' : 'Tabloyu Yenile'"
          >
            <svg 
              class="btn-icon" 
              :class="{ 'animate-spin': refreshing }" 
              fill="none" 
              viewBox="0 0 24 24" 
              stroke="currentColor"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </button>

          <!-- Export Button -->
          <button
            v-if="showExportButton"
            @click="$emit('export')"
            class="export-btn"
          >
            <svg class="btn-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            Dışa Aktar
          </button>

          <!-- Add Button -->
          <button
            v-if="canCreate"
            @click="$emit('add')"
            class="add-button"
          >
            <svg class="btn-icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            {{ addButtonText }}
          </button>
        </div>
      </div>

      <!-- Advanced Filters Panel -->
      <div v-if="showAdvancedFilters && showFilters" class="filters-panel">
        <div class="filters-header">
          <h3 class="filters-title">Gelişmiş Filtreler</h3>
          <button @click="clearAllFilters" class="clear-filters-btn">
            Tümünü Temizle
          </button>
        </div>
        
        <div class="filters-grid">
          <div
            v-for="column in filterableColumns"
            :key="column.key"
            class="filter-item"
          >
            <!-- Text Filter -->
            <v-text-field
              :label="column.label"
              v-if="column.filterType === 'text' || !column.filterType"
              v-model="columnFilters[column.key]"
              :placeholder="`${column.label} filtrele...`"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              class="filter-input compact-filter"
              @update:model-value="debouncedApplyFilters"
            />
            
            <!-- Select Filter -->
            <v-select
              :label="column.label"
              v-else-if="column.filterType === 'select'"
              v-model="columnFilters[column.key]"
              :items="getColumnSelectOptions(column.key)"
              item-title="title"
              item-value="value"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              placeholder="Tümü"
              class="filter-select compact-filter"
              @update:model-value="applyFilters"
            />
            
            <!-- Single Date Filter -->
            <div v-else-if="column.filterType === 'date' || column.filterType === 'single-date'" class="date-filter-wrapper">
              <v-text-field
                :label="column.label"
                v-model="columnFilters[column.key]"
                type="date"
                variant="outlined"
                density="compact"
                hide-details
                clearable
                class="professional-date-filter"
                @update:model-value="debouncedApplyFilters"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
 
    <!-- Table Container -->
    <div class="table-container">
      <!-- Loading State - Skeleton -->
      <div v-if="loading" class="skeleton-loading-state">
        <div class="skeleton-table">
          <div class="skeleton-header">
            <div class="skeleton-header-row">
              <div 
                v-for="column in columns" 
                :key="column.key" 
                class="skeleton-header-cell"
              ></div>
            </div>
          </div>
          <div class="skeleton-body">
            <div 
              v-for="row in 8" 
              :key="row" 
              class="skeleton-row"
              :style="{ animationDelay: `${row * 0.02}s` }"
            >
              <div 
                v-for="column in columns" 
                :key="column.key" 
                class="skeleton-cell"
              ></div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="(props.serverSidePagination ? props.items.length : filteredItems.length) === 0" class="empty-state">
        <div class="empty-icon">
          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h3 class="empty-title">{{ emptyTitle }}</h3>
        <p class="empty-description">{{ emptyDescription }}</p>
      </div>

      <!-- Data Table -->
      <div v-else class="table-wrapper modern-scroll-container">
        <!-- Horizontal Scroll Indicators -->
        <div class="scroll-indicators">
          <div 
            class="scroll-indicator left" 
            :class="{ 'visible': canScrollLeft }"
            @click="scrollHorizontal('left')"
          >
            <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
            </svg>
          </div>
          <div 
            class="scroll-indicator right" 
            :class="{ 'visible': canScrollRight }"
            @click="scrollHorizontal('right')"
          >
            <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </div>
        </div>
        
        <div 
          class="table-responsive infinite-scroll-table" 
          ref="tableContainer"
          @scroll="handleScroll"
        >
          <table class="data-table">
            <thead class="table-header-section">
              <tr class="table-header-row">
                <th
                  v-for="column in columns"
                  :key="column.key"
                  class="table-header-cell"
                  :class="[
                    column.align || 'left',
                    { 'sortable': column.sortable !== false },
                    { 'sorted': sortBy === column.key }
                  ]"
                  :style="{ width: column.width }"
                  @click="column.sortable !== false ? handleSort(column.key) : null"
                >
                  <div class="header-cell-content">
                    <span class="header-text">{{ column.label }}</span>
                    <div v-if="column.sortable !== false" class="sort-indicators">
                      <svg
                        class="sort-icon sort-asc"
                        :class="{ 'active': sortBy === column.key && sortOrder === 'asc' }"
                        fill="none" viewBox="0 0 24 24" stroke="currentColor"
                      >
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
                      </svg>
                      <svg
                        class="sort-icon sort-desc"
                        :class="{ 'active': sortBy === column.key && sortOrder === 'desc' }"
                        fill="none" viewBox="0 0 24 24" stroke="currentColor"
                      >
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                      </svg>
                    </div>
                  </div>
                </th>
                <th v-if="showActions" class="table-header-cell actions-header">
                  <span class="header-text">İşlemler</span>
                </th>
              </tr>
            </thead>
            <tbody class="table-body">
              <tr
                v-for="(item, index) in paginatedItems"
                :key="item.id || index"
                class="table-row modern-table-row"
                @click="$emit('row-click', item)"
              >
                <td
                  v-for="column in columns"
                  :key="column.key"
                  class="table-cell"
                  :class="column.align || 'left'"
                >
                  <slot :name="`cell-${column.key}`" :item="item" :value="getNestedValue(item, column.key)">
                    <div class="cell-content">
                      <span class="cell-text" :class="column.cellClass">
                        {{ formatCellValue(getNestedValue(item, column.key), column) }}
                      </span>
                    </div>
                  </slot>
                </td>
                <td v-if="showActions" class="table-cell actions-cell">
                  <div class="action-buttons">
                    <slot name="actions" :item="item">
                      <div class="actions-inline">
                        <button
                          v-if="canRead"
                          @click.stop="handleViewClick(item)"
                          class="action-button view-button"
                          title="Görüntüle"
                        >
                          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                        </button>
                        <button
                          v-if="canUpdate"
                          @click="$emit('edit', item)"
                          class="action-button edit-button"
                          title="Düzenle"
                        >
                          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </button>
                        <button
                          v-if="canDelete"
                          @click="$emit('delete', item)"
                          class="action-button delete-button"
                          title="Sil"
                        >
                          <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                        </button>
                      </div>
                    </slot>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="pagination">
          <div class="pagination-info">
            <div class="results-info">
              <span class="results-text">Sayfa / Sonuç</span>
              <select v-model="itemsPerPageLocal" @change="changeItemsPerPage" class="items-per-page-select">
                <option :value="5">5</option>
                <option :value="10">10</option>
                <option :value="25">25</option>
                <option :value="50">50</option>
                <option :value="100">100</option>
              </select>
              <span class="total-count">{{ (effectiveCurrentPage - 1) * itemsPerPageLocal + 1 }}-{{ Math.min(effectiveCurrentPage * itemsPerPageLocal, totalItems) }} / {{ totalItems }}</span>
            </div>
          </div>
          <div class="pagination-controls">
            <button
              @click="goToPage(1)"
              :disabled="effectiveCurrentPage === 1"
              class="pagination-button"
              title="İlk sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
              </svg>
            </button>
            
            <button
              @click="goToPage(effectiveCurrentPage - 1)"
              :disabled="effectiveCurrentPage === 1"
              class="pagination-button"
              title="Önceki sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            
            <button
              v-for="page in visiblePages"
              :key="page"
              @click="goToPage(page)"
              class="pagination-button page-button"
              :class="{ 'active': page === effectiveCurrentPage }"
            >
              {{ page }}
            </button>
            
            <button
              @click="goToPage(effectiveCurrentPage + 1)"
              :disabled="effectiveCurrentPage === totalPages"
              class="pagination-button"
              title="Sonraki sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
            
            <button
              @click="goToPage(totalPages)"
              :disabled="effectiveCurrentPage === totalPages"
              class="pagination-button"
              title="Son sayfa"
            >
              <svg fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 5l7 7-7 7M5 5l7 7-7 7" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { getCurrentInstance, onMounted, nextTick } from 'vue'

const props = defineProps({
  items: {
    type: Array,
    default: () => []
  },
  columns: {
    type: Array,
    required: true
  },
  title: {
    type: String,
    default: 'Veri Tablosu'
  },
  description: {
    type: String,
    default: ''
  },
  toolbarIcon: {
    type: String,
    default: 'mdi-table'
  },
  searchPlaceholder: {
    type: String,
    default: 'Tabloda ara...'
  },
  addButtonText: {
    type: String,
    default: 'Yeni Ekle'
  },
  loading: {
    type: Boolean,
    default: false
  },
  loadingText: {
    type: String,
    default: 'Veriler yükleniyor...'
  },
  emptyTitle: {
    type: String,
    default: 'Veri bulunamadı'
  },
  emptyDescription: {
    type: String,
    default: 'Henüz hiç veri eklenmemiş veya arama kriterlerinize uygun veri bulunamadı.'
  },
  showAddButton: {
    type: Boolean,
    default: true
  },
  showExportButton: {
    type: Boolean,
    default: false
  },
  showAdvancedFilters: {
    type: Boolean,
    default: false
  },
  showActions: {
    type: Boolean,
    default: true
  },
  showViewButton: {
    type: Boolean,
    default: false
  },
  showEditButton: {
    type: Boolean,
    default: false
  },
  showDeleteButton: {
    type: Boolean,
    default: false
  },
  showPagination: {
    type: Boolean,
    default: true
  },
  useViewModal: {
    type: Boolean,
    default: false
  },
  disablePermissions: {
    type: Boolean,
    default: false
  },
  itemsPerPage: {
    type: Number,
    default: 10
  },
  serverSidePagination: {
    type: Boolean,
    default: false
  },
  serverTotalCount: {
    type: Number,
    default: 0
  },
  serverCurrentPage: {
    type: Number,
    default: 1
  }
})

const emit = defineEmits([
  'add', 'view', 'edit', 'delete', 'export', 'search', 'sort', 'row-click', 'filter', 'refresh', 'table-refresh', 'load-more', 'page-change', 'page-size-change'
])

onMounted(() => {
  nextTick(() => {
    if (tableContainer.value) {
      updateScrollIndicators(tableContainer.value)
      
      const resizeObserver = new ResizeObserver(() => {
        if (tableContainer.value) {
          updateScrollIndicators(tableContainer.value)
        }
      })
      
      resizeObserver.observe(tableContainer.value)
    }
  })
})

const { hasPermission } = useAuth()
const route = useRoute()
const router = useRouter()

const getResourceFromRoute = () => {
  const path = route.path
  const segments = path.split('/').filter(Boolean)
  return segments[0] || 'unknown'
}

const getPermissionName = (action) => {
  const resource = getResourceFromRoute()
  const resourceName = resource.charAt(0).toUpperCase() + resource.slice(1)
  return `${resourceName}.${action}`
}

const canCreate = computed(() => {
  if (!props.showAddButton) return false
  if (props.disablePermissions) return true
  const permission = getPermissionName('Create')
  return hasPermission(permission)
})

const canRead = computed(() => {
  if (!props.showViewButton) return false
  if (props.disablePermissions) return true
  const permission = getPermissionName('Read')
  return hasPermission(permission)
})

const canUpdate = computed(() => {
  if (!props.showEditButton) return false
  if (props.disablePermissions) return true
  const permission = getPermissionName('Update')
  return hasPermission(permission)
})

const canDelete = computed(() => {
  if (!props.showDeleteButton) return false
  if (props.disablePermissions) return true
  const permission = getPermissionName('Delete')
  return hasPermission(permission)
})

// Reactive data
const searchQuery = ref('')
const sortBy = ref('')
const sortOrder = ref('asc')
const currentPage = ref(1)
const itemsPerPageLocal = ref(props.itemsPerPage)
const showFilters = ref(false)
const columnFilters = ref({})
const datePickerMenus = ref({})
const refreshing = ref(false)

// Scroll related
const tableContainer = ref(null)
const canScrollLeft = ref(false)
const canScrollRight = ref(false) 
const isLoadingMore = ref(false)
const hasReachedEnd = ref(false)

const filterableColumns = computed(() => {
  return props.columns.filter(col => col.filterable !== false)
})

const activeFiltersCount = computed(() => {
  return Object.values(columnFilters.value).filter(val => val && val.toString().trim()).length
})

const filteredItems = computed(() => {
  let filtered = [...props.items]
  
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(item => {
      return props.columns.some(column => {
        const value = getNestedValue(item, column.key)
        return value && value.toString().toLowerCase().includes(query)
      })
    })
  }
  
  Object.entries(columnFilters.value).forEach(([key, value]) => {
    if (value && value.toString().trim()) {
      if (key.endsWith('_start') || key.endsWith('_end')) {
        const baseKey = key.replace('_start', '').replace('_end', '')
        const startDate = columnFilters.value[baseKey + '_start']
        const endDate = columnFilters.value[baseKey + '_end']
        
        if (startDate || endDate) {
          filtered = filtered.filter(item => {
            const itemDate = new Date(getNestedValue(item, baseKey))
            if (startDate && itemDate < new Date(startDate)) return false
            if (endDate && itemDate > new Date(endDate)) return false
            return true
          })
        }
      } else {
        const column = props.columns.find(col => col.key === key)
        if (column && column.filterType === 'select') {
          filtered = filtered.filter(item => {
            const itemValue = getNestedValue(item, key)
            return itemValue === value
          })
        } else {
          filtered = filtered.filter(item => {
            const itemValue = getNestedValue(item, key)
            return itemValue && itemValue.toString().toLowerCase().includes(value.toLowerCase())
          })
        }
      }
    }
  })
  
  if (sortBy.value) {
    filtered.sort((a, b) => {
      const aValue = getNestedValue(a, sortBy.value)
      const bValue = getNestedValue(b, sortBy.value)
      
      if (aValue === bValue) return 0
      
      const comparison = aValue < bValue ? -1 : 1
      return sortOrder.value === 'asc' ? comparison : -comparison
    })
  }
  
  return filtered
})

const totalItems = computed(() =>
  props.serverSidePagination ? props.serverTotalCount : filteredItems.value.length
)
const totalPages = computed(() => Math.ceil(totalItems.value / itemsPerPageLocal.value))

const effectiveCurrentPage = computed(() =>
  props.serverSidePagination ? props.serverCurrentPage : currentPage.value
)

const paginatedItems = computed(() => {
  if (props.serverSidePagination) {
    return props.items
  }
  const start = (currentPage.value - 1) * itemsPerPageLocal.value
  const end = start + itemsPerPageLocal.value
  return filteredItems.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages = []
  const maxVisible = 5
  const page = effectiveCurrentPage.value
  let start = Math.max(1, page - Math.floor(maxVisible / 2))
  let end = Math.min(totalPages.value, start + maxVisible - 1)

  if (end - start + 1 < maxVisible) {
    start = Math.max(1, end - maxVisible + 1)
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  return pages
})

const getNestedValue = (obj, path) => {
  return path.split('.').reduce((current, key) => {
    return current && current[key] !== undefined ? current[key] : ''
  }, obj)
}

const formatCellValue = (value, column) => {
  if (column.formatter && typeof column.formatter === 'function') {
    return column.formatter(value)
  }
  return value || '-'
}

const getColumnSelectOptions = (columnKey) => {
  const values = props.items
    .map(item => getNestedValue(item, columnKey))
    .filter(val => val !== null && val !== undefined && val !== '')
  const uniqueValues = [...new Set(values)]
  
  return uniqueValues.map(value => {
    let displayText = value.toString()
    
    if (value === 0 || value === '0') {
      displayText = 'Pasif'
    } else if (value === 1 || value === '1') {
      displayText = 'Aktif'
    }
    
    return {
      title: displayText,
      value: value
    }
  })
}

let filterTimeout = null
const debouncedApplyFilters = () => {
  if (filterTimeout) {
    clearTimeout(filterTimeout)
  }
  filterTimeout = setTimeout(() => {
    applyFilters()
  }, 500)
}

let searchDebounceTimeout = null
const handleSearch = () => {
  if (props.serverSidePagination) {
    if (searchDebounceTimeout) clearTimeout(searchDebounceTimeout)
    searchDebounceTimeout = setTimeout(() => {
      emit('search', searchQuery.value)
    }, 400)
  } else {
    currentPage.value = 1
  }
}

const clearSearch = () => {
  searchQuery.value = ''
  if (props.serverSidePagination) {
    if (searchDebounceTimeout) clearTimeout(searchDebounceTimeout)
    emit('search', '')
  } else {
    currentPage.value = 1
  }
}

const handleRefresh = async () => {
  refreshing.value = true
  try {
    emit('refresh')
    emit('table-refresh')
    await new Promise(resolve => setTimeout(resolve, 500))
  } finally {
    refreshing.value = false
  }
}

const handleScroll = (event) => {
  const container = event.target
  updateScrollIndicators(container)
  
  const scrollThreshold = 100
  const isNearBottom = container.scrollHeight - container.scrollTop <= container.clientHeight + scrollThreshold
  
  if (isNearBottom && !isLoadingMore.value && !hasReachedEnd.value) {
    loadMoreData()
  }
}

const updateScrollIndicators = (container) => {
  if (!container) return
  
  const scrollLeft = container.scrollLeft
  const scrollWidth = container.scrollWidth
  const clientWidth = container.clientWidth
  const hasHorizontalScroll = scrollWidth > clientWidth
  
  if (!hasHorizontalScroll) {
    canScrollLeft.value = false
    canScrollRight.value = false
    return
  }
  
  const leftThreshold = 10
  const rightThreshold = 50
  
  canScrollLeft.value = scrollLeft > leftThreshold
  canScrollRight.value = scrollLeft < (scrollWidth - clientWidth - rightThreshold)
}

const scrollHorizontal = (direction) => {
  if (!tableContainer.value) return
  
  const scrollAmount = 200
  const currentScroll = tableContainer.value.scrollLeft
  const newScroll = direction === 'left' 
    ? currentScroll - scrollAmount 
    : currentScroll + scrollAmount
  
  tableContainer.value.scrollTo({
    left: newScroll,
    behavior: 'smooth'
  })
}

const loadMoreData = async () => {
  if (isLoadingMore.value) return
  isLoadingMore.value = true
  
  try {
    emit('load-more')
    await new Promise(resolve => setTimeout(resolve, 1000))
  } catch (error) {
    console.error('Load more error:', error)
  } finally {
    isLoadingMore.value = false
  }
}

const handleSort = (key) => {
  if (sortBy.value === key) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = key
    sortOrder.value = 'asc'
  }
  
  currentPage.value = 1
  emit('sort', { key, order: sortOrder.value })
}

const toggleAdvancedFilters = () => {
  showFilters.value = !showFilters.value
}

const applyFilters = () => {
  currentPage.value = 1
  
  const processedFilters = { ...columnFilters.value }
  
  props.columns.forEach(column => {
    if (column.filterType === 'select' && processedFilters[column.key]) {
      const selectOptions = getColumnSelectOptions(column.key)
      const selectedOption = selectOptions.find(option => option.title === processedFilters[column.key])
      if (selectedOption) {
        processedFilters[column.key] = selectedOption.value
      }
    }
  })
  
  emit('filter', processedFilters)
}

const clearAllFilters = () => {
  columnFilters.value = {}
  datePickerMenus.value = {}
  applyFilters()
}

const goToPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    if (props.serverSidePagination) {
      emit('page-change', page)
    } else {
      currentPage.value = page
    }
  }
}

const changeItemsPerPage = () => {
  if (props.serverSidePagination) {
    emit('page-size-change', itemsPerPageLocal.value)
  } else {
    currentPage.value = 1
  }
}

const handleViewClick = (item) => {
  if (props.useViewModal) {
    emit('view', item)
  } else {
    const resource = getResourceFromRoute()
    const detailPath = `/${resource}/${item.id}`
    router.push(detailPath)
  }
}

watch(() => props.items, () => {
  currentPage.value = 1
})

watch(() => props.itemsPerPage, (newValue) => {
  itemsPerPageLocal.value = newValue
})
</script>

<style scoped>
.modern-data-table {
  @apply bg-surface-container-lowest rounded-2xl shadow-xl border border-outline-variant/30 overflow-hidden;
}

/* Header Section */
.table-header {
  @apply border-b border-outline-variant/30;
  background: linear-gradient(
    135deg,
    rgb(var(--color-surface-container-low)) 0%,
    rgb(var(--color-surface-container)) 100%
  );
}

.header-content {
  @apply px-8 py-6 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6;
}

.header-left {
  @apply flex-1;
}

.title-section {
  @apply flex items-start gap-4;
}

.title-wrapper {
  @apply flex items-center gap-4;
}

.title-icon {
  @apply w-12 h-12 rounded-xl flex items-center justify-center;
  background: var(--theme-gradient);
  color: white;
  box-shadow: 0 8px 16px rgb(var(--color-primary) / 0.3);
}

.title-content {
  @apply flex flex-col;
}

.page-title {
  @apply text-2xl font-bold text-on-surface mb-1;
}

.page-description {
  @apply text-sm text-on-surface-variant;
}

.header-right {
  @apply flex flex-wrap items-center gap-3;
}

.header-right-search {
  @apply flex flex-wrap items-center gap-9 pl-4;
}

/* Buttons */
.filter-toggle-btn,
.refresh-btn,
.export-btn,
.add-button {
  @apply inline-flex items-center gap-2 px-4 py-2.5 text-sm font-medium rounded-xl transition-all duration-200;
}

.filter-toggle-btn {
  @apply bg-surface-container-lowest border border-outline-variant/40 text-on-surface hover:bg-surface-container-low hover:border-outline-variant relative;
}

.filter-toggle-btn.active {
  background: rgb(var(--color-primary) / 0.08);
  border-color: rgb(var(--color-primary) / 0.3);
  color: rgb(var(--color-primary));
}

.filter-count {
  @apply absolute -top-2 -right-2 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center;
  background: rgb(var(--color-primary));
}

.refresh-btn {
  background: var(--theme-gradient);
  border: 1px solid rgb(var(--color-primary));
  color: white;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgb(var(--color-primary) / 0.2);
}

.refresh-btn:hover {
  background: var(--theme-gradient-hover);
  border-color: rgb(var(--color-primary));
  box-shadow: 0 4px 8px rgb(var(--color-primary) / 0.3);
  transform: translateY(-1px);
}

.refresh-btn:disabled {
  @apply opacity-60 cursor-not-allowed;
}

.refresh-btn .animate-spin {
  animation: spin 1s linear infinite;
}

.icon-only-btn {
  width: 40px;
  height: 40px;
  min-width: 40px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.icon-only-btn .btn-icon {
  width: 18px;
  height: 18px;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.export-btn {
  @apply bg-surface-container border border-outline-variant/40 text-on-surface hover:bg-surface-container-high;
}

.add-button {
  background: var(--theme-gradient);
  border: 1px solid rgb(var(--color-primary));
  color: white;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgb(var(--color-primary) / 0.2);
}

.add-button:hover {
  background: var(--theme-gradient-hover);
  border-color: rgb(var(--color-primary));
  box-shadow: 0 4px 8px rgb(var(--color-primary) / 0.3);
  transform: translateY(-1px);
}

.btn-icon {
  @apply w-4 h-4;
}

/* Search */
.search-input-wrapper {
  @apply relative;
}

.search-icon {
  @apply absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-on-surface-variant/60;
}

.search-input {
  @apply w-full pl-10 pr-10 py-2.5 rounded-xl focus:ring-2 bg-surface-container-lowest transition-colors placeholder-on-surface-variant/60 text-on-surface;
  border: 2px solid rgb(var(--color-outline-variant)) !important;
  --tw-ring-color: rgb(var(--color-primary));
}

.search-input:focus {
  border-color: rgb(var(--color-primary)) !important;
}

.clear-search-btn {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-on-surface-variant/60 hover:text-on-surface-variant transition-colors;
}

/* Filters Panel */
.filters-panel {
  @apply border-t border-outline-variant/30 bg-surface-container-low p-6;
}

.filters-header {
  @apply flex items-center justify-between mb-4;
}

.filters-title {
  @apply text-lg font-semibold text-on-surface;
}

.clear-filters-btn {
  @apply text-sm font-medium transition-colors;
  color: rgb(var(--color-primary));
}

.clear-filters-btn:hover {
  color: rgb(var(--color-primary));
}

.filters-grid {
  @apply grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4;
}

.filter-item {
  @apply flex flex-col mb-4;
}

.filter-input :deep(.v-field),
.filter-select :deep(.v-field) {
  @apply bg-surface-container-lowest border border-outline-variant/40 rounded-lg transition-colors;
}

.compact-filter :deep(.v-field) {
  min-height: 40px !important;
}

.compact-filter :deep(.v-field__input) {
  padding-top: 6px !important;
  padding-bottom: 6px !important;
  font-size: 14px !important;
}

.compact-filter :deep(.v-field__prepend-inner),
.compact-filter :deep(.v-field__append-inner) {
  padding-top: 4px !important;
}

/* Date Filter */
.date-filter-wrapper {
  position: relative;
}

.professional-date-filter {
  border-radius: 8px;
  transition: all 0.3s ease;
}

.professional-date-filter .v-field {
  background: rgb(var(--color-surface-container-lowest));
  border: 1px solid rgb(var(--color-outline-variant) / 0.4);
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.professional-date-filter .v-field:hover {
  border-color: rgb(var(--color-primary));
  box-shadow: 0 2px 6px rgb(var(--color-primary) / 0.15);
}

.professional-date-filter .v-field--focused {
  border-color: rgb(var(--color-primary)) !important;
  box-shadow: 0 0 0 3px rgb(var(--color-primary) / 0.1) !important;
}

.professional-date-filter .v-field__input {
  color: rgb(var(--color-on-surface));
  font-weight: 500;
}

/* Empty State */
.empty-state {
  @apply flex flex-col items-center justify-center py-16 px-6;
}

.empty-icon {
  @apply w-16 h-16 text-on-surface-variant/40 mb-4;
}

.empty-title {
  @apply text-xl font-semibold text-on-surface mb-2;
}

.empty-description {
  @apply text-on-surface-variant text-center mb-6 max-w-md;
}

/* Table */
.table-wrapper {
  @apply flex flex-col;
}

.table-responsive {
  @apply overflow-x-auto;
}

.data-table {
  @apply min-w-full;
}

.table-header-section {
  background: linear-gradient(
    to right,
    rgb(var(--color-surface-container-low)),
    rgb(var(--color-surface-container))
  );
}

.table-header-row {
  @apply border-b border-outline-variant/40;
}

.table-header-cell {
  @apply px-6 py-4 text-left font-semibold text-on-surface whitespace-nowrap;
}

.table-header-cell.center { @apply text-center; }
.table-header-cell.right { @apply text-right; }
.table-header-cell.sortable { @apply cursor-pointer hover:bg-surface-container transition-colors select-none; }
.table-header-cell.sorted {
  background: rgb(var(--color-primary) / 0.06);
  color: rgb(var(--color-primary));
}

.header-cell-content {
  @apply flex items-center gap-2;
}

.header-text {
  @apply font-semibold;
}

.sort-indicators {
  @apply flex flex-col;
}

.sort-icon {
  @apply w-3 h-3 text-on-surface-variant/60 transition-colors;
}

.sort-icon.active {
  color: rgb(var(--color-primary));
}

.sort-asc {
  @apply mb-0.5;
}

.actions-header {
  @apply text-center;
}

/* Table Body */
.table-body {
  @apply divide-y divide-outline-variant/30;
}

.table-row {
  @apply hover:bg-surface-container-low transition-colors cursor-pointer;
}

.table-cell {
  @apply px-6 py-4 whitespace-nowrap;
}

.table-cell.center { @apply text-center; }
.table-cell.right { @apply text-right; }

.cell-content {
  @apply flex items-center;
  transition: none;
}

.cell-text {
  @apply text-on-surface text-sm;
}

/* Actions */
.actions-cell {
  @apply text-center;
}

.action-buttons {
  @apply flex justify-center;
}

.actions-inline {
  @apply flex items-center gap-2;
}

.action-button {
  @apply p-2 rounded-lg transition-all duration-200 hover:scale-105;
}

.action-button svg {
  @apply w-4 h-4;
}

.view-button {
  color: rgb(var(--color-primary));
}
.view-button:hover {
  background: rgb(var(--color-primary) / 0.06);
  color: rgb(var(--color-primary));
}
.edit-button { @apply text-secondary hover:bg-secondary-container/20 hover:text-secondary-container; }
.delete-button { @apply text-error hover:bg-error/10 hover:text-error; }

/* Pagination */
.pagination {
  @apply flex flex-col sm:flex-row sm:items-center sm:justify-between px-6 py-4 border-t border-outline-variant/30 bg-surface-container-lowest;
}

.pagination-info {
  @apply mb-4 sm:mb-0;
}

.results-info {
  @apply flex items-center gap-3;
}

.results-text {
  @apply text-sm text-on-surface;
}

.items-per-page-select {
  @apply px-3 py-2 border border-outline-variant/40 rounded-md text-sm bg-surface-container-lowest text-on-surface font-medium focus:ring-2 transition-colors;
  --tw-ring-color: rgb(var(--color-primary));
}

.items-per-page-select:focus {
  border-color: rgb(var(--color-primary));
}

.total-count {
  @apply text-sm text-on-surface-variant font-medium;
}

.pagination-controls {
  @apply flex items-center gap-1;
}

.pagination-button {
  @apply px-3 py-2 text-sm font-medium text-on-surface-variant bg-surface-container-lowest border border-outline-variant/40 rounded-lg hover:bg-surface-container-low focus:outline-none focus:ring-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed;
  --tw-ring-color: rgb(var(--color-primary));
}

.pagination-button.page-button {
  @apply w-10 h-10 flex items-center justify-center;
}

.pagination-button.active {
  color: white;
  background: rgb(var(--color-primary));
  border-color: rgb(var(--color-primary));
}

.pagination-button.active:hover {
  background: rgb(var(--color-primary));
  border-color: rgb(var(--color-primary));
}

.pagination-button svg {
  @apply w-4 h-4;
}

/* Glassmorphism Header */
.glassmorphism-header {
  background: rgb(var(--color-surface-container-low) / 0.85);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgb(var(--color-outline-variant) / 0.3);
  box-shadow:
    0 8px 32px rgba(0, 0, 0, 0.1),
    inset 0 1px 0 rgb(var(--color-on-surface) / 0.05);
  border-radius: 16px;
  position: relative;
  overflow: hidden;
}

.glassmorphism-header::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, 
    rgba(var(--theme-primary-dark-rgb), 0.3) 0%, 
    rgba(var(--theme-primary-rgb), 0.3) 100%);
}

.animate-fade-in {
  animation: fadeInUp 0.6s ease-out;
}

@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.modern-icon {
  background: var(--theme-gradient);
  border-radius: 12px;
  padding: 8px;
  color: white;
  box-shadow: 0 4px 12px rgb(var(--color-primary) / 0.3);
  transition: all 0.3s ease;
}

.modern-icon:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgb(var(--color-primary) / 0.4);
}

.modern-title {
  font-weight: 700;
  font-size: 1.5rem;
  color: rgb(var(--color-on-surface));
  margin: 0;
  line-height: 1.2;
}

.modern-description {
  color: rgb(var(--color-on-surface-variant));
  font-weight: 500;
  margin: 4px 0 0 0;
  font-size: 0.875rem;
}

/* Skeleton Loading */
.skeleton-loading-state {
  padding: 24px;
  animation: fadeIn 0.3s ease-in;
}

.skeleton-table {
  border: 1px solid rgb(var(--color-outline-variant) / 0.3);
  border-radius: 12px;
  overflow: hidden;
  background: rgb(var(--color-surface-container-lowest));
}

.skeleton-header {
  background: rgb(var(--color-surface-container-low));
  border-bottom: 1px solid rgb(var(--color-outline-variant) / 0.3);
}

.skeleton-header-row {
  display: flex;
  padding: 16px;
  gap: 16px;
}

.skeleton-header-cell {
  height: 20px;
  background: linear-gradient(
    90deg,
    rgb(var(--color-surface-container)) 25%,
    rgb(var(--color-surface-container-low)) 50%,
    rgb(var(--color-surface-container)) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 4px;
  flex: 1;
}

.skeleton-body {
  padding: 0;
}

.skeleton-row {
  display: flex;
  padding: 16px;
  gap: 16px;
  border-bottom: 1px solid rgb(var(--color-outline-variant) / 0.2);
  animation: skeletonFadeIn 0.2s ease-out both;
}

.skeleton-row:last-child {
  border-bottom: none;
}

.skeleton-cell {
  height: 16px;
  background: linear-gradient(
    90deg,
    rgb(var(--color-surface-container) / 0.5) 25%,
    rgb(var(--color-surface-container-low)) 50%,
    rgb(var(--color-surface-container) / 0.5) 75%
  );
  background-size: 200% 100%;
  animation: shimmer 2.5s infinite;
  border-radius: 4px;
  flex: 1;
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

@keyframes skeletonFadeIn {
  from { opacity: 0; transform: translateY(3px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* Modern Table Rows */
.modern-table-row {
  transition: all 0.2s ease-in-out;
  position: relative;
  cursor: pointer;
}

.modern-table-row:hover {
  background: linear-gradient(135deg, 
    rgba(var(--theme-primary-dark-rgb), 0.02) 0%, 
    rgba(var(--theme-primary-rgb), 0.02) 100%);
  box-shadow: 
    0 2px 6px rgb(var(--color-primary) / 0.05),
    0 1px 2px rgba(0, 0, 0, 0.03);
}

.action-buttons .actions-inline button {
  transition: all 0.2s ease;
  border-radius: 8px;
}

.action-buttons .actions-inline button:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}

/* Scroll Features */
.modern-scroll-container {
  position: relative;
}

.scroll-indicators {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  z-index: 10;
}

.scroll-indicator {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 44px;
  height: 44px;
  background: linear-gradient(135deg, 
    rgba(255, 255, 255, 0.3) 0%, 
    rgba(var(--theme-primary-rgb), 0.9) 40%, 
    rgba(var(--theme-primary-dark-rgb), 0.9) 100%);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  pointer-events: all;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  opacity: 0;
  backdrop-filter: blur(15px);
  box-shadow: 0 4px 16px rgb(var(--color-primary) / 0.3);
  border: 2px solid rgba(255, 255, 255, 0.5);
  z-index: 15;
}

.scroll-indicator.visible {
  opacity: 0.9;
}

.scroll-indicator.left { left: 8px; }
.scroll-indicator.right { right: 8px; }

.scroll-indicator:hover {
  opacity: 1;
  transform: translateY(-50%) scale(1.05);
  background: linear-gradient(135deg, 
    rgba(255, 255, 255, 0.4) 0%, 
    rgba(var(--theme-primary-rgb), 1) 35%, 
    rgba(var(--theme-primary-dark-rgb), 1) 100%);
  box-shadow: 0 6px 24px rgb(var(--color-primary) / 0.6);
  border-color: rgba(255, 255, 255, 0.8);
}

.scroll-indicator:active {
  transform: translateY(-50%) scale(0.95);
}

.scroll-indicator svg {
  width: 16px;
  height: 16px;
  stroke-width: 2;
}

.infinite-scroll-table {
  position: relative;
  overflow: auto;
  scroll-behavior: smooth;
}

/* Custom Scrollbar */
.infinite-scroll-table::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

.infinite-scroll-table::-webkit-scrollbar-track {
  background: rgb(var(--color-surface-container-low));
  border-radius: 4px;
}

.infinite-scroll-table::-webkit-scrollbar-thumb {
  background: var(--theme-gradient);
  border-radius: 4px;
  transition: all 0.3s ease;
}

.infinite-scroll-table::-webkit-scrollbar-thumb:hover {
  background: var(--theme-gradient-hover);
}

/* Responsive */
@media (max-width: 1024px) {
  .header-content { @apply flex-col items-stretch; }
  .header-right { @apply flex-col sm:flex-row; }
  .filters-grid { @apply grid-cols-1 sm:grid-cols-2; }
}

@media (max-width: 768px) {
  .modern-data-table { @apply rounded-xl; }
  .header-content { @apply px-4 py-4; }
  .filters-panel { @apply p-4; }
  .table-header-cell, .table-cell { @apply px-3 py-3; }
  .pagination { @apply px-4 py-3 flex-col items-stretch; }
  .pagination-controls { @apply justify-center; }
  .results-info { @apply flex-col sm:flex-row items-start sm:items-center text-center sm:text-left; }
}

@media (max-width: 640px) {
  .title-wrapper { @apply flex-col items-start; }
  .header-right { @apply flex-col; }
  .filters-grid { @apply grid-cols-1; }
}
</style>
