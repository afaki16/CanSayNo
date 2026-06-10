<template>
  <v-form ref="formRef" @submit.prevent="handleSubmit">
    <v-container>
      <v-row>
        <v-col cols="12">
          <v-text-field
            v-model="formData.name"
            label="Tenant Adı"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-domain"
            density="comfortable"
            hide-details="auto"
            :rules="[rules.required, rules.minLength(2)]"
            class="modern-input"
          />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <v-text-field
            v-model="formData.domain"
            label="Domain"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-web"
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

      <v-row>
        <v-col cols="12" sm="6">
          <v-text-field
            v-model="formData.contactEmail"
            label="İletişim E-postası"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-email"
            density="comfortable"
            hide-details="auto"
            type="email"
            :rules="[rules.optionalEmail]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field
            v-model="formData.contactPhone"
            label="İletişim Telefonu"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-phone"
            density="comfortable"
            hide-details="auto"
            type="tel"
            :rules="[rules.optionalPhone]"
            class="modern-input"
          />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <v-switch
            v-model="formData.isActive"
            label="Aktif"
            color="primary"
            :disabled="loading"
            hide-details
          />
        </v-col>
      </v-row>
    </v-container>

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
        type="submit"
        class="btn-gradient-primary"
      >
        {{ tenant ? 'Güncelle' : 'Kaydet' }}
      </v-btn>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import type { Tenant, CreateTenantRequest, UpdateTenantRequest } from '~/types'
import { ref, reactive, watch, computed } from 'vue'

const props = defineProps<{
  tenant?: Tenant | null
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: CreateTenantRequest | UpdateTenantRequest]
  cancel: []
}>()

const { validationRules: rules } = useValidators()

const formRef = ref()
const formData = reactive<CreateTenantRequest>({
  name: '',
  description: '',
  domain: '',
  isActive: true,
  contactEmail: '',
  contactPhone: '',
})

const isFormValid = computed(() => formRef.value?.isValid ?? false)

watch(
  () => props.tenant,
  (newVal) => {
    if (newVal) {
      Object.assign(formData, {
        name: newVal.name || '',
        description: newVal.description || '',
        domain: newVal.domain || '',
        isActive: newVal.isActive ?? true,
        contactEmail: newVal.contactEmail || '',
        contactPhone: newVal.contactPhone || '',
      })
    } else {
      Object.assign(formData, {
        name: '',
        description: '',
        domain: '',
        isActive: true,
        contactEmail: '',
        contactPhone: '',
      })
    }
  },
  { immediate: true }
)

const handleSubmit = async () => {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  const submitData = {
    name: formData.name,
    description: formData.description || undefined,
    domain: formData.domain || undefined,
    isActive: formData.isActive,
    contactEmail: formData.contactEmail || undefined,
    contactPhone: formData.contactPhone || undefined,
  }

  if (props.tenant) {
    emit('submit', { ...submitData, id: props.tenant.id } as UpdateTenantRequest)
  } else {
    emit('submit', submitData as CreateTenantRequest)
  }
}
</script>
