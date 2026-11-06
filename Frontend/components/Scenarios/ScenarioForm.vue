<template>
  <v-form ref="formRef" @submit.prevent="handleSubmit">
    <v-container>
      <!-- Temel bilgiler -->
      <div class="form-section-title">
        <v-icon size="18" class="mr-2">mdi-information-outline</v-icon>Temel bilgiler
      </div>
      <v-row>
        <v-col cols="12" sm="6">
          <v-select
            v-model="formData.contextId"
            :items="CONTEXTS"
            item-title="title"
            item-value="value"
            label="Bağlam"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading || isEdit"
            :hint="isEdit ? 'Bağlam sonradan değiştirilemez' : undefined"
            persistent-hint
            prepend-inner-icon="mdi-shape"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="6">
          <v-text-field
            v-model="formData.id"
            label="ID"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading || isEdit"
            :rules="isEdit ? [] : [rules.required, idRule]"
            :hint="isEdit ? undefined : `Örn. ${idExample}`"
            persistent-hint
            prepend-inner-icon="mdi-identifier"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model="formData.title"
            label="Başlık"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            counter="80"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(80)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model="formData.shortDescription"
            label="Kısa açıklama (listede görünür)"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            counter="160"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(160)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <v-select
            v-model="formData.difficulty"
            :items="difficultyItems"
            label="Zorluk"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <v-text-field
            v-model.number="formData.estimatedDurationMinutes"
            label="Süre (dk)"
            type="number"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            :rules="[rangeRule(1, 30)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <v-text-field
            v-model.number="formData.sortOrder"
            label="Sıra"
            type="number"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            :rules="[rangeRule(0, 10000)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-combobox
            v-model="formData.tags"
            label="Etiketler (en fazla 8)"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            multiple
            chips
            closable-chips
            :disabled="loading"
            :rules="[(v: string[]) => (v?.length ?? 0) <= 8 || 'En fazla 8 etiket']"
            class="modern-input"
          />
        </v-col>
        <v-col cols="6">
          <v-switch v-model="formData.isFree" label="Ücretsiz" color="primary" hide-details :disabled="loading" />
        </v-col>
        <v-col cols="6">
          <v-switch v-model="formData.isActive" label="Aktif (mobilde görünür)" color="success" hide-details :disabled="loading" />
        </v-col>
      </v-row>

      <!-- Sahne -->
      <div class="form-section-title mt-6">
        <v-icon size="18" class="mr-2">mdi-movie-open-outline</v-icon>Sahne
      </div>
      <v-row>
        <v-col cols="12">
          <v-text-field
            v-model="formData.scene.setting"
            label="Ortam (detay ekranında 'Sahne')"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            counter="200"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(200)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-textarea
            v-model="formData.scene.situation"
            label="Durum (yalnızca karakter prompt'una girer)"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            rows="3"
            auto-grow
            counter="1000"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(1000)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model="formData.scene.userGoal"
            label="Kullanıcının hedefi"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            counter="200"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(200)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model="formData.scene.hint"
            label="İpucu (opsiyonel, kullanıcıya görünür)"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            counter="200"
            :disabled="loading"
            :rules="[rules.maxLength(200)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-textarea
            v-model="formData.scene.openingMessage"
            label="Açılış mesajı (karakterin ilk mesajı)"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            rows="3"
            auto-grow
            counter="600"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(600)]"
            class="modern-input"
          />
        </v-col>
      </v-row>

      <!-- Karakter -->
      <div class="form-section-title mt-6">
        <v-icon size="18" class="mr-2">mdi-account-voice</v-icon>Karakter
      </div>
      <v-row>
        <v-col cols="12" sm="8">
          <v-text-field
            v-model="formData.character.name"
            label="Ad"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(40)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="4">
          <v-text-field
            v-model.number="formData.character.age"
            label="Yaş"
            type="number"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            :rules="[rangeRule(5, 100)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model="formData.character.role"
            label="Rol"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(120)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-text-field
            v-model="formData.character.relationship"
            label="Kullanıcıyla ilişkisi"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            :rules="[rules.required, rules.maxLength(200)]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12">
          <v-select
            v-model="formData.character.archetype"
            :items="ARCHETYPES"
            item-title="title"
            item-value="value"
            label="Arketip"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            :disabled="loading"
            class="modern-input"
          />
        </v-col>
      </v-row>

      <!-- Baskı taktikleri -->
      <div class="form-section-title mt-6">
        <v-icon size="18" class="mr-2">mdi-chess-knight</v-icon>Baskı taktikleri
      </div>
      <p class="form-section-note">
        Her raunt için 1–3 taktik. Yalnızca karakter prompt'una girer, kullanıcıya hiçbir zaman gösterilmez.
      </p>
      <v-row>
        <v-col v-for="round in rounds" :key="round.key" cols="12">
          <v-select
            v-model="formData.pressureTactics[round.key]"
            :items="TACTICS"
            item-title="title"
            item-value="value"
            :label="round.label"
            variant="outlined"
            density="comfortable"
            hide-details="auto"
            multiple
            chips
            closable-chips
            :disabled="loading"
            :rules="[tacticRule]"
            class="modern-input"
          />
        </v-col>
      </v-row>
    </v-container>

    <div class="form-actions">
      <v-btn variant="outlined" size="large" class="btn-gradient-dark" :disabled="loading" @click="$emit('cancel')">
        İptal
      </v-btn>
      <v-btn size="large" type="submit" class="btn-gradient-primary" :loading="loading" :disabled="loading">
        {{ isEdit ? 'Güncelle' : 'Kaydet' }}
      </v-btn>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import type { AdminScenario, AdminScenarioUpsert, CreateAdminScenarioRequest, PressureTactic } from '~/types'
import { ARCHETYPES, CONTEXTS, contextOf, DIFFICULTY_LABELS, TACTICS } from '~/utils/hayirciLabels'

const props = defineProps<{
  scenario?: AdminScenario | null
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: CreateAdminScenarioRequest | AdminScenarioUpsert]
  cancel: []
}>()

const { validationRules: rules } = useValidators()

const formRef = ref()
const isEdit = computed(() => !!props.scenario)

const difficultyItems = Object.entries(DIFFICULTY_LABELS).map(([value, title]) => ({ value: Number(value), title }))
const rounds = [
  { key: 'roundOne' as const, label: '1. raunt (yumuşak giriş)' },
  { key: 'roundTwo' as const, label: '2. raunt (duygusal baskı / aciliyet)' },
  { key: 'roundThree' as const, label: '3. raunt (son koz)' },
]

const ID_PATTERN = /^(is|aile|arkadas|romantik|satici|komsu|okul|dijital)_[0-9]{3}_[a-z0-9_]{2,60}$/
const idExample = computed(() => `${contextOf(formData.contextId).idPrefix}_011_yeni_konu`)
const idRule = (v: string) => {
  if (!ID_PATTERN.test(v ?? '')) return 'Biçim: <bağlam>_<3 hane>_<konu>; küçük harf, rakam ve _'
  if (!v.startsWith(`${contextOf(formData.contextId).idPrefix}_`)) return 'ID öneki bağlamla aynı olmalı'
  return true
}
const rangeRule = (min: number, max: number) => (v: number) =>
  (Number.isInteger(Number(v)) && v >= min && v <= max) || `${min}–${max} arası olmalı`
const tacticRule = (v: PressureTactic[]) => ((v?.length ?? 0) >= 1 && v.length <= 3) || '1–3 taktik seç'

const emptyForm = (): CreateAdminScenarioRequest => ({
  id: '',
  title: '',
  shortDescription: '',
  contextId: 'is',
  difficulty: 2,
  estimatedDurationMinutes: 5,
  isFree: true,
  isActive: false,
  sortOrder: 100,
  tags: [],
  scene: { setting: '', situation: '', userGoal: '', hint: '', openingMessage: '' },
  character: { name: '', age: 30, role: '', relationship: '', archetype: 'masumRicaci' },
  pressureTactics: { roundOne: [], roundTwo: [], roundThree: [] },
})

const formData = reactive<CreateAdminScenarioRequest>(emptyForm())

watch(
  () => props.scenario,
  (s) => {
    const base = emptyForm()
    if (s) {
      // Derin kopya: formdaki düzenleme tablodaki satırı değiştirmesin.
      Object.assign(formData, {
        ...base,
        id: s.id,
        title: s.title,
        shortDescription: s.shortDescription,
        contextId: s.contextId,
        difficulty: s.difficulty,
        estimatedDurationMinutes: s.estimatedDurationMinutes,
        isFree: s.isFree,
        isActive: s.isActive,
        sortOrder: s.sortOrder,
        tags: [...s.tags],
        scene: { ...s.scene, hint: s.scene.hint ?? '' },
        character: { ...s.character },
        pressureTactics: {
          roundOne: [...s.pressureTactics.roundOne],
          roundTwo: [...s.pressureTactics.roundTwo],
          roundThree: [...s.pressureTactics.roundThree],
        },
      })
    } else {
      Object.assign(formData, base)
    }
  },
  { immediate: true },
)

const handleSubmit = async () => {
  const { valid } = await formRef.value?.validate()
  if (!valid) return

  const body: AdminScenarioUpsert = {
    title: formData.title,
    shortDescription: formData.shortDescription,
    contextId: formData.contextId,
    difficulty: formData.difficulty,
    estimatedDurationMinutes: formData.estimatedDurationMinutes,
    isFree: formData.isFree,
    isActive: formData.isActive,
    sortOrder: formData.sortOrder,
    tags: formData.tags,
    scene: { ...formData.scene, hint: formData.scene.hint?.trim() ? formData.scene.hint : null },
    character: { ...formData.character },
    pressureTactics: { ...formData.pressureTactics },
  }
  emit('submit', isEdit.value ? body : { ...body, id: formData.id.trim() })
}
</script>

<style scoped>
.form-section-title {
  display: flex;
  align-items: center;
  font-size: 0.95rem;
  font-weight: 600;
  margin-bottom: 12px;
  color: rgb(var(--v-theme-on-surface));
}

.form-section-note {
  font-size: 0.8rem;
  color: rgb(var(--v-theme-on-surface-variant));
  margin: -4px 0 12px;
}
</style>
