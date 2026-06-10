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
      <!-- Tab 1: Kişisel Bilgiler -->
      <v-window-item value="personal">
        <v-container>
          <v-row>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.firstName"
                label="Ad"
                variant="outlined"
                :disabled="loading"
                prepend-inner-icon="mdi-account"
                density="comfortable"
                hide-details="auto"
                :rules="[rules.required, rules.minLength(2)]"
                class="modern-input"
              />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.lastName"
                label="Soyad"
                variant="outlined"
                :disabled="loading"
                prepend-inner-icon="mdi-account"
                density="comfortable"
                hide-details="auto"
                :rules="[rules.required, rules.minLength(2)]"
                class="modern-input"
              />
            </v-col>
          </v-row>

          <v-row>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.email"
                label="E-posta"
                variant="outlined"
                :disabled="loading || !!user"
                prepend-inner-icon="mdi-email"
                density="comfortable"
                hide-details="auto"
                type="email"
                :rules="[rules.required, rules.email]"
                class="modern-input"
              />
            </v-col>
            <v-col cols="12" sm="6">
              <v-text-field
                v-model="formData.phoneNumber"
                label="Telefon Numarası"
                variant="outlined"
                :disabled="loading"
                prepend-inner-icon="mdi-phone"
                density="comfortable"
                hide-details="auto"
                type="tel"
                :rules="[rules.required, rules.phone]"
                class="modern-input"
              />
            </v-col>
          <!-- Tenant seçici: Sadece SuperAdmin yeni kullanıcı oluştururken -->
          <v-col v-if="!user && canAccessAllTenants" cols="12" sm="6">
            <v-select
              v-model="formData.tenantId"
              :items="tenants"
              item-title="name"
              item-value="id"
              label="Tenant"
              variant="outlined"
              :disabled="loading"
              prepend-inner-icon="mdi-domain"
              density="comfortable"
              hide-details="auto"
              clearable
              class="modern-input"
            />
          </v-col>
        </v-row>

        <!-- Status (Sadece edit modunda) -->
        <v-row v-if="user">
          <v-col cols="12">
            <v-switch
              v-model="formData.status"
              :true-value="1"
              :false-value="0"
              color="primary"
              :disabled="loading"
              hide-details
              class="status-switch"
            >
              <template #label>
                <div class="status-label">
                  <span class="status-text">Kullanıcı Durumu</span>
                  <v-chip
                    :color="formData.status === 1 ? 'success' : 'error'"
                    size="small"
                    variant="tonal"
                  >
                    {{ formData.status === 1 ? 'Aktif' : 'Pasif' }}
                  </v-chip>
                </div>
              </template>
            </v-switch>
            <p class="status-description mt-2">
              {{ formData.status === 1 
                  ? 'Kullanıcı sisteme giriş yapabilir ve tüm yetkilerini kullanabilir.' 
                  : 'Kullanıcı sisteme giriş yapamaz ve hesabı devre dışıdır.' 
              }}
            </p>
          </v-col>
        </v-row>
      </v-container>
    </v-window-item>

    <!-- Tab 2: Şifre (Sadece yeni kullanıcı) -->
    <v-window-item v-if="!user" value="password">
      <v-container>
        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="formData.password"
              label="Şifre"
              variant="outlined"
              :disabled="loading"
              :type="showPassword ? 'text' : 'password'"
              prepend-inner-icon="mdi-lock"
              :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showPassword = !showPassword"
              density="comfortable"
              hide-details="auto"
              :rules="[rules.password]"
              class="modern-input"
            />
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="formData.confirmPassword"
              label="Şifre Tekrar"
              variant="outlined"
              :disabled="loading"
              :type="showConfirmPassword ? 'text' : 'password'"
              prepend-inner-icon="mdi-lock-check"
              :append-inner-icon="showConfirmPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showConfirmPassword = !showConfirmPassword"
              density="comfortable"
              hide-details="auto"
              :rules="[rules.confirmPassword(formData.password)]"
              class="modern-input"
            />
          </v-col>
        </v-row>
      </v-container>
    </v-window-item>

    <!-- Tab 3: Roller -->
    <v-window-item value="roles">
      <v-container>
        <v-row v-if="roles && roles.length">
          <v-col cols="12">
            <v-text-field
              v-model="searchQuery"
              prepend-inner-icon="mdi-magnify"
              variant="outlined"
              density="compact"
              hide-details
              clearable
              class="mb-4 modern-input"
            />

            <div class="quick-actions mb-4">
              <v-btn
                variant="outlined"
                size="small"
                @click="selectAllRoles"
                :disabled="loading"
                prepend-icon="mdi-check-all"
              >
                Tümünü Seç
              </v-btn>
              <v-btn
                variant="outlined"
                size="small"
                @click="clearAllRoles"
                :disabled="loading"
                prepend-icon="mdi-close-box"
              >
                Temizle
              </v-btn>
              <v-chip 
                v-if="formData.roleIds.length" 
                size="small" 
                color="primary" 
                variant="tonal"
              >
                {{ formData.roleIds.length }} seçili
              </v-chip>
            </div>

            <div class="roles-grid">
              <div
                v-for="role in filteredRoles"
                :key="role.id"
                class="role-item"
                :class="{ 'selected': formData.roleIds.includes(String(role.id)) }"
              >
                <v-checkbox
                  v-model="formData.roleIds"
                  :value="String(role.id)"
                  :disabled="loading"
                  color="primary"
                  hide-details
                  class="role-checkbox"
                />
                <div class="role-info">
                  <div class="role-header">
                    <h4 class="role-name">{{ role.name }}</h4>
                    <v-chip
                      v-if="role.isSystemRole"
                      size="x-small"
                      color="orange"
                      variant="tonal"
                    >
                      Sistem
                    </v-chip>
                  </div>
                  <p v-if="role.description" class="role-description">
                    {{ role.description }}
                  </p>
                </div>
              </div>
            </div>
          </v-col>
        </v-row>

        <v-row v-else>
          <v-col cols="12">
            <div class="empty-state">
              <v-icon size="64" color="grey-400">mdi-shield-off</v-icon>
              <h3 class="empty-title">Rol Bulunamadı</h3>
              <p class="empty-text">Atanabilir rol bulunmuyor</p>
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
        {{ user ? 'Güncelle' : 'Kaydet' }}
      </v-btn>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import type { User, Role, CreateUserRequest, UpdateUserRequest } from '~/types'
import { computed, reactive, ref, watch, watchEffect, onMounted } from 'vue'

const { validationRules: rules } = useValidators()

const props = defineProps<{
  user?: User | null
  roles: Role[]
  loading?: boolean
}>()

const authStore = useAuthStore()
const { getTenants } = useTenants()
const tenants = ref<{ id: number; name: string }[]>([])
const canAccessAllTenants = computed(() => authStore.hasRole('SuperAdmin'))

const emit = defineEmits<{
  submit: [data: CreateUserRequest | UpdateUserRequest]
  cancel: []
  'update:selected-roles': [roleIds: string[]]
}>()

const formRef = ref()
const currentTab = ref('personal')
const searchQuery = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)

const isFormValid = computed(() => formRef.value?.isValid ?? false)

const tabs = computed(() => {
  const baseTabs = [
    { value: 'personal', label: 'Kişisel Bilgiler', icon: 'mdi-account' }
  ]
  
  if (!props.user) {
    baseTabs.push({ value: 'password', label: 'Şifre', icon: 'mdi-lock' })
  }
  
  baseTabs.push({ value: 'roles', label: 'Roller', icon: 'mdi-shield-account' })
  
  return baseTabs
})

const formData = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  password: '',
  confirmPassword: '',
  status: 1,
  roleIds: [] as string[],
  tenantId: null as number | null
})

onMounted(async () => {
  if (canAccessAllTenants.value && !props.user) {
    try {
      const res = await getTenants(1, 100, '')
      tenants.value = (res?.data?.items ?? res?.data ?? []) as { id: number; name: string }[]
    } catch {
      tenants.value = []
    }
  }
})

const filteredRoles = computed(() => {
  if (!searchQuery.value) return props.roles
  
  return props.roles.filter(role =>
    role.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    (role.description && role.description.toLowerCase().includes(searchQuery.value.toLowerCase()))
  )
})

const selectAllRoles = () => {
  formData.roleIds = props.roles.map(r => String(r.id))
}

const clearAllRoles = () => {
  formData.roleIds = []
}

const handleSubmit = async () => {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  const submitData: any = {
    firstName: formData.firstName,
    lastName: formData.lastName,
    email: formData.email,
    phoneNumber: formData.phoneNumber,
    status: formData.status,
    profileImageUrl: props.user?.profileImageUrl || "",
    roleIds: formData.roleIds.map(id => Number(id))
  }
  
  if (props.user) {
    submitData.id = Number(props.user.id)
  } else {
    submitData.password = formData.password
    if (formData.tenantId != null) {
      submitData.tenantId = formData.tenantId
    }
  }
  
  emit('submit', submitData)
}

watch(() => formData.roleIds, (newIds) => {
  emit('update:selected-roles', newIds)
}, { deep: true })

watchEffect(() => {
  if (props.user) {
    Object.assign(formData, {
      firstName: props.user.firstName,
      lastName: props.user.lastName,
      email: props.user.email,
      phoneNumber: props.user.phoneNumber || '',
      status: props.user.status ?? 1,
      roleIds: props.user.roles?.map(r => String(r.id)) || [],
      password: '',
      confirmPassword: ''
    })
  } else {
    Object.assign(formData, {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      status: 1,
      roleIds: [],
      tenantId: null,
      password: '',
      confirmPassword: ''
    })
  }
})
</script>

<style scoped>
.status-switch {
  margin-bottom: 8px;
}

.status-label {
  display: flex;
  align-items: center;
  gap: 12px;
}

.status-text {
  font-weight: 500;
  color: #1e293b;
}

.status-description {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
  line-height: 1.5;
}

.quick-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.roles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

.role-item {
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px;
  display: flex;
  gap: 12px;
  transition: all 0.2s ease;
  cursor: pointer;
  background: white;
}

.role-item:hover {
  border-color: #cbd5e1;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.role-item.selected {
  border-color: rgb(var(--color-primary));
  background: #f0f7ff;
  box-shadow: 0 4px 12px rgb(var(--color-primary) / 0.1);
}

.role-checkbox {
  margin: 0 !important;
  flex-shrink: 0;
}

.role-checkbox :deep(.v-selection-control) {
  min-height: auto !important;
}

.role-info {
  flex: 1;
  min-width: 0;
}

.role-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.role-name {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0;
}

.role-description {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
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

.empty-text {
  font-size: 0.875rem;
  color: #64748b;
  margin-top: 8px;
}

@media (max-width: 768px) {
  .roles-grid {
    grid-template-columns: 1fr;
  }
}
</style>
