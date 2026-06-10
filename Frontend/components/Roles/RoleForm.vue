<template>
  <!-- Modern Tab Navigation -->
  <div class="tabs-container modern-tabs">
    <button
      v-for="tab in tabs"
      :key="tab.value"
      :class="['tab-item', { active: currentTab === tab.value }]"
      @click="currentTab = tab.value"
      :disabled="loading"
    >
      <div class="tab-icon-wrapper">
        <v-icon :icon="tab.icon" size="22" />
      </div>
      <span class="tab-label">{{ tab.label }}</span>
    </button>
  </div>

  <v-form ref="formRef">
    <v-window v-model="currentTab" class="mt-6">
      <!-- Tab 1: Temel Bilgiler -->
      <v-window-item value="basic">
        <v-container>
          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model="formData.name"
                label="Rol Adı"
                variant="outlined"
                :disabled="loading || (role && role.isSystemRole)"
                prepend-inner-icon="mdi-shield-account"
                density="comfortable"
                hide-details="auto"
                :rules="[rules.required, rules.minLength(2)]"
                class="modern-input"
              />
            </v-col>
          </v-row>

          <v-row>
            <v-col cols="12">
              <v-textarea
                v-model="formData.description"
                label="Açıklama"
                variant="outlined"
                :disabled="loading"
                rows="3"
                auto-grow
                prepend-inner-icon="mdi-text"
                density="comfortable"
                hide-details="auto"
                :rules="[rules.maxLength(2000)]"
                class="modern-input"
              />
            </v-col>
          </v-row>
        </v-container>
      </v-window-item>

    <!-- Tab 2: İzinler -->
    <v-window-item value="permissions">
      <v-container fluid>
        <v-row v-if="groupedPermissions && Object.keys(groupedPermissions).length">
          <v-col cols="12">
            <v-text-field
              v-model="searchQuery"
              prepend-inner-icon="mdi-magnify"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              class="mb-3 modern-input"
            />

            <div class="quick-actions mb-3">
              <v-btn
                variant="outlined"
                size="small"
                @click="selectAllPermissions"
                :disabled="loading"
                prepend-icon="mdi-check-all"
              >
                Tümünü Seç
              </v-btn>
              <v-btn
                variant="outlined"
                size="small"
                @click="clearAllPermissions"
                :disabled="loading"
                prepend-icon="mdi-close-box"
              >
                Temizle
              </v-btn>
              <v-chip 
                v-if="formData.permissionIds.length" 
                size="small" 
                color="primary" 
                variant="tonal"
              >
                {{ formData.permissionIds.length }} seçili
              </v-chip>
            </div>

            <div class="permissions-simple-tree">
              <div
                v-for="(perms, resource) in filteredPermissions"
                :key="resource"
                class="resource-group"
              >
                <div 
                  class="resource-row"
                  @click="toggleResource(resource)"
                >
                  <v-icon 
                    class="chevron-icon"
                    :class="{ 'expanded': expandedResources.includes(resource) }"
                    size="18"
                  >
                    mdi-chevron-right
                  </v-icon>
                  <v-checkbox
                    :model-value="isResourceFullySelected(resource)"
                    :indeterminate="isResourcePartiallySelected(resource)"
                    @update:model-value="toggleResourcePermissions(resource, $event)"
                    @click.stop
                    color="primary"
                    hide-details
                    density="compact"
                    class="resource-group-checkbox"
                  />
                  <span class="resource-title">{{ resource }} ({{ perms.length }})</span>
                </div>

                <div 
                  v-show="expandedResources.includes(resource)"
                  class="permissions-simple-container"
                >
                  <div
                    v-for="permission in perms"
                    :key="permission.id"
                    class="permission-simple-item"
                  >
                    <v-checkbox
                      v-model="formData.permissionIds"
                      :value="String(permission.id)"
                      :disabled="loading"
                      color="primary"
                      hide-details
                      density="compact"
                      class="permission-simple-checkbox"
                      :label="permission.name"
                    />
                  </div>
                </div>
              </div>
            </div>
          </v-col>
        </v-row>

        <v-row v-else>
          <v-col cols="12">
            <div class="empty-state">
              <v-icon size="64" color="grey-400">mdi-key-off</v-icon>
              <h3 class="empty-title">İzin Bulunamadı</h3>
              <p class="empty-text">Atanabilir izin bulunmuyor</p>
            </div>
          </v-col>
        </v-row>
      </v-container>
    </v-window-item>
    </v-window>

    <!-- Modern Actions -->
    <div class="form-actions">
      <v-btn
        variant="outlined"
        size="large"
        @click="$emit('cancel')"
        :disabled="loading"
        class="btn-gradient-dark"
      >
        İptal
      </v-btn>
      
      <v-btn
        size="large"
        :loading="loading"
        :disabled="loading || !isFormValid"
        @click="handleSubmit"
        class="btn-gradient-primary"
      >
        {{ role ? 'Güncelle' : 'Kaydet' }}
      </v-btn>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import type { Role, Permission, CreateRoleRequest, UpdateRoleRequest } from '~/types'
import { onMounted, watchEffect, computed, reactive, ref } from 'vue'

const { validationRules: rules } = useValidators()

const props = defineProps<{
  role?: Role | null
  permissions: Permission[]
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: CreateRoleRequest | UpdateRoleRequest]
  cancel: []
}>()

const formRef = ref()
const currentTab = ref('basic')
const expandedResources = ref<string[]>([])
const searchQuery = ref('')

const isFormValid = computed(() => formRef.value?.isValid ?? false)

const tabs = [
  { value: 'basic', label: 'Temel Bilgiler', icon: 'mdi-shield-account' },
  { value: 'permissions', label: 'İzinler', icon: 'mdi-key-variant' }
]

const formData = reactive<CreateRoleRequest>({
  name: '',
  description: '',
  permissionIds: []
})

const groupedPermissions = computed(() => {
  return props.permissions.reduce((groups, permission) => {
    const resource = permission.resource
    if (!groups[resource]) {
      groups[resource] = []
    }
    groups[resource].push(permission)
    return groups
  }, {} as Record<string, Permission[]>)
})

const filteredPermissions = computed(() => {
  if (!searchQuery.value) return groupedPermissions.value
  
  const filtered: Record<string, Permission[]> = {}
  Object.entries(groupedPermissions.value).forEach(([resource, permissions]) => {
    const matchingPermissions = permissions.filter(permission =>
      permission.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      resource.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
    if (matchingPermissions.length > 0) {
      filtered[resource] = matchingPermissions
    }
  })
  return filtered
})

const isResourceFullySelected = (resource: string): boolean => {
  const resourcePermissions = groupedPermissions.value[resource] || []
  return resourcePermissions.length > 0 && 
    resourcePermissions.every(p => formData.permissionIds.includes(String(p.id)))
}

const isResourcePartiallySelected = (resource: string): boolean => {
  const resourcePermissions = groupedPermissions.value[resource] || []
  const selectedCount = resourcePermissions.filter(p => 
    formData.permissionIds.includes(String(p.id))
  ).length
  
  return selectedCount > 0 && selectedCount < resourcePermissions.length
}

const toggleResource = (resource: string) => {
  const index = expandedResources.value.indexOf(resource)
  if (index > -1) {
    expandedResources.value.splice(index, 1)
  } else {
    expandedResources.value.push(resource)
  }
}

const toggleResourcePermissions = (resource: string, selected: boolean) => {
  const resourcePermissions = groupedPermissions.value[resource] || []
  
  if (selected) {
    resourcePermissions.forEach(permission => {
      const permissionId = String(permission.id)
      if (!formData.permissionIds.includes(permissionId)) {
        formData.permissionIds.push(permissionId)
      }
    })
  } else {
    formData.permissionIds = formData.permissionIds.filter(id =>
      !resourcePermissions.some(p => String(p.id) === id)
    )
  }
}

const selectAllPermissions = () => {
  formData.permissionIds = props.permissions.map(p => String(p.id))
}

const clearAllPermissions = () => {
  formData.permissionIds = []
}

const handleSubmit = async () => {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  const submitData = {
    name: formData.name,
    description: formData.description,
    permissionIds: formData.permissionIds
  }
  
  if (props.role) {
    emit('submit', { ...submitData, id: props.role.id } as UpdateRoleRequest)
  } else {
    emit('submit', submitData as CreateRoleRequest)
  }
}

watchEffect(() => {
  if (props.role) {
    Object.assign(formData, {
      name: props.role.name,
      description: props.role.description || '',
      permissionIds: props.role.permissions?.map(p => String(p.id)) || []
    })
  } else {
    Object.assign(formData, {
      name: '',
      description: '',
      permissionIds: []
    })
  }
})

onMounted(() => {
  if (Object.keys(groupedPermissions.value).length > 0) {
    expandedResources.value = [Object.keys(groupedPermissions.value)[0]]
  }
})
</script>

<style scoped>
.quick-actions {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}

.permissions-simple-tree {
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  max-height: 450px;
  overflow-y: auto;
}

.resource-group {
  border-bottom: 1px solid #f1f5f9;
}

.resource-row {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 12px;
  cursor: pointer;
  user-select: none;
  transition: background-color 0.15s ease;
}

.resource-row:hover {
  background: #f8fafc;
}

.chevron-icon {
  color: #64748b;
  transition: transform 0.2s ease;
}

.chevron-icon.expanded {
  transform: rotate(90deg);
}

.resource-group-checkbox {
  margin: 0 !important;
}

.resource-group-checkbox :deep(.v-selection-control) {
  min-height: auto !important;
}

.resource-title {
  font-weight: 500;
  color: #1e293b;
  font-size: 0.875rem;
  flex: 1;
}

.permissions-simple-container {
  padding: 6px 12px 12px 36px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
}

.permission-simple-item {
  padding: 1px 0;
}

.permission-simple-checkbox :deep(.v-label) {
  font-size: 0.8125rem !important;
  color: #374151 !important;
  line-height: 1.4;
}

.empty-state {
  text-align: center;
  padding: 48px 24px;
}

.empty-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #374151;
  margin-top: 16px;
}

</style>
