# Kickstart Frontend — Yeni Sayfa (CRUD) Oluşturma Rehberi

Bu rehber, projeye yeni bir CRUD sayfası eklerken **hangi dosyaları oluşturman**, **hangi mevcut dosyalara satır eklemen** gerektiğini sırasıyla gösterir. Mevcut **Users** ve **Tenants** sayfaları referans alınmıştır.

Rehber boyunca `Product` örnek entity adı olarak kullanılmıştır. Senin entity'nin adına göre değiştir:
- `Product` → `SeninEntity` (PascalCase, tekil)
- `products` → `urunler` (i18n key'leri için)
- Backend endpoint kalıbı: `/api/products` (controller adının küçük harfli çoğul hali)

---

## Proje Yapısı

```
Frontend/
├── pages/                  → Nuxt file-based routing (sayfalar)
├── composables/            → İş mantığı (API çağrıları, CRUD yönetimi)
├── components/
│   ├── UI/                 → Genel: BaseDataTable, ResizableDrawer, ConfirmDialog
│   └── [Entity]/           → Entity'ye özel: form, detay bileşeni
├── types/                  → TypeScript tip tanımları
├── utils/
│   └── apiEndpoints.ts     → Tüm API endpoint'leri merkezi
├── i18n/locales/           → tr.json + en.json
├── middleware/             → auth.global, permission
└── stores/                 → Pinia stores (auth, toast)
```

---

## Adımlar (Özet)

Yeni entity için **6 yeni dosya oluşturulur** ve **3 mevcut dosyaya satır eklenir**:

| # | Dosya | Aksiyon |
|---|-------|---------|
| 1 | `types/product.ts` | YENİ |
| 2 | `types/index.ts` | SATIR EKLE (export) |
| 3 | `utils/apiEndpoints.ts` | SATIR EKLE (`PRODUCTS` grubu) |
| 4 | `composables/useProducts.ts` | YENİ |
| 5 | `components/Products/ProductForm.vue` | YENİ |
| 6 | `components/Products/ProductDetail.vue` | YENİ (opsiyonel) |
| 7 | `pages/products/index.vue` | YENİ |
| 8 | `composables/useNavigation.ts` | SATIR EKLE (sidebar menüsü) |
| 9 | `i18n/locales/tr.json` ve `en.json` | SATIR EKLE (menü etiketi) |

---

## ADIM 1 — Tip Tanımları (YENİ DOSYA)

**Dosya:** `types/product.ts`

```typescript
export interface Product {
  id: number
  name: string
  description?: string
  price: number
  isActive: boolean
  categoryId: number
  categoryName?: string
  createdDate: string
  tenantId?: number
}

export interface CreateProductRequest {
  name: string
  description?: string
  price: number
  categoryId: number
  /** Sadece SuperAdmin belirtebilir. Belirtilmezse mevcut kullanıcının tenant'ı kullanılır. */
  tenantId?: number
}

export interface UpdateProductRequest {
  id: number
  name: string
  description?: string
  price: number
  isActive: boolean
  categoryId: number
}
```

**ID tipi notu:**
- Backend tarafında entity'lerin Id'si `int`. Frontend tip tanımında `number` kullan.
- API endpoint helper'ları (`GET_BY_ID`, `UPDATE`, `DELETE`) `string` parametre alır — değer JS'de implicit olarak `number`'dan `string`'e dönüşür.

---

## ADIM 2 — types/index.ts'e Export Ekle (SATIR EKLE)

**Dosya:** `types/index.ts`

Mevcut entity export bloklarının yanına ekle:

```typescript
// Product Types
export type {
  Product,
  CreateProductRequest,
  UpdateProductRequest
} from './product'
```

> Eğer Product entity'sinde bir enum varsa (örn. `ProductStatus`), `export { ProductStatus } from './product'` şeklinde ayrı satırla ekle (enum runtime value olduğundan `type` ile export edilmez).

---

## ADIM 3 — API Endpoints (MEVCUT DOSYAYA SATIR EKLE)

**Dosya:** `utils/apiEndpoints.ts`

`API_ENDPOINTS` objesine yeni endpoint grubu ekle (mevcut `USERS`, `TENANTS` bloklarının yanına):

```typescript
// Product management endpoints
PRODUCTS: {
  LIST: '/api/products',
  CREATE: '/api/products',
  GET_BY_ID: (id: string) => `/api/products/${id}`,
  UPDATE: (id: string) => `/api/products/${id}`,
  DELETE: (id: string) => `/api/products/${id}`,
},
```

**Önemli:** Backend `BaseController`'ın route kalıbı `[Route("api/[controller]")]`'dır — bu da `api/products` (controller adının küçük harfli hali) demektir. Backend ile uyuşması zorunlu.

> Mevcut `getPaginatedEndpoint(baseEndpoint, page, pageSize, search?)` helper'ı tüm listeleme endpoint'lerinde kullanılır — yeniden tanımlamana gerek yok. `page`, `pageSize`, `searchTerm` query parametreleri otomatik eklenir.

---

## ADIM 4 — Composable (YENİ DOSYA)

**Dosya:** `composables/useProducts.ts`

`useUsers.ts`'i birebir referans al — frontend bu pattern'i kullanır:

```typescript
import type { Product, CreateProductRequest, UpdateProductRequest } from '~/types'
import { API_ENDPOINTS, getPaginatedEndpoint } from '~/utils/apiEndpoints'
import { useApi } from './useApi'

export const useProducts = () => {
  const api = useApi()

  const getProducts = async (page = 1, pageSize = 10, searchTerm = '') => {
    try {
      const endpoint = getPaginatedEndpoint(API_ENDPOINTS.PRODUCTS.LIST, page, pageSize, searchTerm)
      const response = await api.get<{
        items: Product[]
        totalCount: number
        totalPages: number
        pageNumber: number
      }>(endpoint)
      return response
    } catch (error) {
      console.error('Get products error:', error)
      throw error
    }
  }

  const getProductById = async (id: string) => {
    try {
      const response = await api.get<Product>(API_ENDPOINTS.PRODUCTS.GET_BY_ID(id))
      return response.data
    } catch (error) {
      console.error('Get product error:', error)
      throw error
    }
  }

  const createProduct = async (productData: CreateProductRequest) => {
    try {
      const response = await api.post<Product>(API_ENDPOINTS.PRODUCTS.CREATE, productData)
      if (response.success) {
        return response.data
      }
    } catch (error) {
      console.error('Create product error:', error)
      throw error
    }
  }

  const updateProduct = async (id: string, productData: UpdateProductRequest) => {
    try {
      const response = await api.put<Product>(API_ENDPOINTS.PRODUCTS.UPDATE(id), productData)
      if (response.success) {
        return response.data
      }
    } catch (error) {
      console.error('Update product error:', error)
      throw error
    }
  }

  const deleteProduct = async (id: string) => {
    try {
      await api.delete(API_ENDPOINTS.PRODUCTS.DELETE(id))
      return true
    } catch (error) {
      console.error('Delete product error:', error)
      throw error
    }
  }

  return {
    getProducts,
    getProductById,
    createProduct,
    updateProduct,
    deleteProduct
  }
}
```

**Response yapısı — backend ne döndürür:**

| Endpoint | Backend Response | Frontend `response` (api.get'ten dönen) |
|---|---|---|
| `GET /api/products` (paged) | `{ success, data: { items, totalCount, totalPages, pageNumber } }` | `{ success, data: { items, totalCount, totalPages, pageNumber } }` |
| `GET /api/products/{id}` | `{ success, data: {...} }` | `{ success, data: {...} }` |
| `POST /api/products` | `{ success, data: {...} }` | `{ success, data: {...} }` |
| `PUT /api/products/{id}` | `{ success, data: {...} }` | `{ success, data: {...} }` |
| `DELETE /api/products/{id}` | `{ success, message }` | `{ success, message }` |

> **`getProducts` neden response'u olduğu gibi döndürür?** `useCrudOperations` `serverSidePagination: true` modunda `response.data.items` ve `response.data.totalCount` yapısını bekler. Bu yüzden composable'da `response.data.items` extract etme.

> **`getProductById`/`createProduct`/`updateProduct`** ise `response.data` döndürür çünkü çağıran kod doğrudan entity bekler.

> **FormData (resim yükleme) gerekirse:** `useNuxtApp().$api.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } })` kullan. Bu composable yapısı sadece JSON içindir.

---

## ADIM 5 — Form Bileşeni (YENİ DOSYA)

**Dosya:** `components/Products/ProductForm.vue`

`TenantForm.vue`'yu birebir referans al (en sade form). Karmaşık tablı form için `UserForm.vue` referans:

```vue
<template>
  <v-form ref="formRef" @submit.prevent="handleSubmit">
    <v-container>
      <v-row>
        <v-col cols="12">
          <v-text-field
            v-model="formData.name"
            label="Ürün Adı"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-package-variant"
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
            :rules="[rules.maxLength(500)]"
            class="modern-input"
          />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12" sm="6">
          <v-text-field
            v-model.number="formData.price"
            label="Fiyat"
            type="number"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-currency-try"
            density="comfortable"
            hide-details="auto"
            :rules="[rules.required]"
            class="modern-input"
          />
        </v-col>
        <v-col cols="12" sm="6">
          <v-select
            v-model="formData.categoryId"
            :items="categories"
            item-title="name"
            item-value="id"
            label="Kategori"
            variant="outlined"
            :disabled="loading"
            prepend-inner-icon="mdi-shape"
            density="comfortable"
            hide-details="auto"
            :rules="[rules.required]"
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
        {{ product ? 'Güncelle' : 'Kaydet' }}
      </v-btn>
    </div>
  </v-form>
</template>

<script setup lang="ts">
import type { Product, CreateProductRequest, UpdateProductRequest } from '~/types'
import { ref, reactive, watch, computed } from 'vue'

const props = defineProps<{
  product?: Product | null
  categories: { id: number; name: string }[]
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [data: CreateProductRequest | UpdateProductRequest]
  cancel: []
}>()

const { validationRules: rules } = useValidators()

const formRef = ref()
const formData = reactive<CreateProductRequest>({
  name: '',
  description: '',
  price: 0,
  categoryId: 0 as number,
  isActive: true as any   // CreateProductRequest'te yoksa Update için
})

const isFormValid = computed(() => formRef.value?.isValid ?? false)

watch(
  () => props.product,
  (newVal) => {
    if (newVal) {
      Object.assign(formData, {
        name: newVal.name || '',
        description: newVal.description || '',
        price: newVal.price ?? 0,
        categoryId: newVal.categoryId ?? 0,
        isActive: newVal.isActive ?? true,
      })
    } else {
      Object.assign(formData, {
        name: '',
        description: '',
        price: 0,
        categoryId: 0,
        isActive: true,
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
    price: formData.price,
    categoryId: formData.categoryId,
    isActive: formData.isActive,
  }

  if (props.product) {
    emit('submit', { ...submitData, id: props.product.id } as UpdateProductRequest)
  } else {
    emit('submit', submitData as CreateProductRequest)
  }
}
</script>
```

**`useValidators` ile mevcut kurallar:**

| Kural | Kullanım |
|-------|----------|
| `rules.required` | `:rules="[rules.required]"` |
| `rules.email` / `rules.optionalEmail` | E-posta formatı |
| `rules.phone` / `rules.optionalPhone` | Telefon formatı |
| `rules.minLength(n)` / `rules.maxLength(n)` | `:rules="[rules.minLength(2)]"` |
| `rules.password` | Şifre güçlülüğü |
| `rules.confirmPassword(password)` | Şifre eşleşme |

> Form CSS class'ları (`modern-input`, `btn-gradient-primary`, `btn-gradient-dark`, `form-actions`) `assets/css/global-admin.css` ve `assets/scss/main.scss`'te tanımlıdır — değiştirme.

---

## ADIM 6 — Detay Bileşeni (OPSİYONEL — YENİ DOSYA)

Eğer "Görüntüle" modal'ı istiyorsan `components/Products/ProductDetail.vue` oluştur. `UserDetail.vue`'yu referans al.

Eğer detay modal'ı istemiyorsan:
- `pages/products/index.vue`'da `BaseDataTable`'a `:show-view-button="false"` yaz, `use-view-modal` prop'unu kaldır
- `ResizableDrawer` (view) bloğunu sayfa template'inden çıkar

`Tenants` sayfası bu basit yaklaşımı kullanır (sadece edit/delete, view yok).

---

## ADIM 7 — Sayfa (YENİ DOSYA)

**Dosya:** `pages/products/index.vue`

`users/index.vue` (SSR + server-side pagination + view modal'lı) en kapsamlı örnektir. Senin entity'n için gereksizse `tenants/index.vue` pattern'i kullan (client-side pagination, view modal yok).

**Server-side pagination + view modal (kapsamlı) versiyon — `users/index.vue` pattern'i:**

```vue
<template>
  <!-- Breadcrumb -->
  <div class="mb-6">
    <BreadCrumb :items="[
      { text: 'Ana Sayfa', to: '/' },
      { text: 'Ürünler' }
    ]" />
  </div>

  <BaseDataTable
    :items="items"
    :columns="tableColumns"
    title="Ürünler"
    toolbar-icon="mdi-package-variant"
    search-placeholder="Ürün ara..."
    add-button-text="Yeni Ürün Ekle"
    :loading="isLoading"
    loading-text="Ürünler yükleniyor..."
    empty-title="Ürün bulunamadı"
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
    <!-- Custom hücre render'ları -->
    <template #cell-name="{ value }">
      <v-chip color="primary" variant="tonal" size="default" prepend-icon="mdi-package-variant">
        {{ value }}
      </v-chip>
    </template>

    <template #cell-isActive="{ value }">
      <v-chip :color="value ? 'success' : 'error'" size="small" variant="tonal">
        {{ value ? 'Aktif' : 'Pasif' }}
      </v-chip>
    </template>

    <template #cell-price="{ value }">
      {{ new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value) }}
    </template>
  </BaseDataTable>

  <!-- Create/Edit Drawer -->
  <ResizableDrawer
    v-model="dialogs.create"
    :title="isEditMode ? 'Ürünü Düzenle' : 'Yeni Ürün Ekle'"
    :subtitle="isEditMode ? 'Ürün bilgilerini güncelleyin' : 'Sisteme yeni ürün bilgilerini girin'"
    icon="mdi-package-variant"
    :default-width="600"
    :min-width="400"
  >
    <ProductForm
      :product="selectedItem"
      :categories="categories"
      :loading="isLoading"
      @submit="handleSubmit"
      @cancel="closeCreateDialog"
    />
  </ResizableDrawer>

  <!-- View Detail Drawer (opsiyonel) -->
  <ResizableDrawer
    v-model="dialogs.view"
    title="Ürün Detayları"
    subtitle="Ürün bilgilerini görüntüleyin"
    icon="mdi-package-variant-closed"
    :default-width="800"
    :min-width="600"
  >
    <ProductDetail
      :product="selectedItem"
      @close="closeViewDialog"
      @edit="handleEditFromDetail"
    />
  </ResizableDrawer>

  <!-- Confirm Delete -->
  <ConfirmDialog
    v-model="dialogs.delete"
    title="Ürünü Sil"
    :message="`'${itemToDelete?.name}' ürününü silmek istediğinizden emin misiniz?`"
    type="error"
    confirm-text="Sil"
    :loading="isDeleting"
    @confirm="confirmDelete"
    @cancel="closeDeleteDialog"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import BaseDataTable from '~/components/UI/BaseDataTable.vue'
import ResizableDrawer from '~/components/UI/ResizableDrawer.vue'
import ConfirmDialog from '~/components/UI/ConfirmDialog.vue'
import ProductForm from '~/components/Products/ProductForm.vue'
import ProductDetail from '~/components/Products/ProductDetail.vue'

//#region Page Metadata
definePageMeta({
  title: 'Ürünler',
  requiresAuth: true,
  middleware: ['permission'],
  permissions: ['Products.Read']
})

useHead({
  title: 'Ürünler',
})
//#endregion

//#region DataTable Columns
const tableColumns = [
  {
    label: 'Ürün Adı',
    key: 'name',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '250px'
  },
  {
    label: 'Kategori',
    key: 'categoryName',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '200px'
  },
  {
    label: 'Fiyat',
    key: 'price',
    sortable: true,
    width: '120px'
  },
  {
    label: 'Durum',
    key: 'isActive',
    sortable: true,
    filterable: true,
    filterType: 'select',
    width: '100px'
  }
]
//#endregion

//#region Composables
const { getProducts, createProduct, updateProduct, deleteProduct } = useProducts()

// SSR: ilk sayfa verisi sunucuda yüklenir
const { data: productsData } = await useAsyncData(
  'products-initial',
  () => getProducts(1, 10, ''),
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
  loadItems: getProducts,
  createItem: createProduct,
  updateItem: updateProduct,
  deleteItem: deleteProduct,
  itemName: 'ürün',
  serverSidePagination: true,
  initialPageSize: 10,
  initialData: productsData
})
//#endregion

//#region Additional Data (örn: dropdown'lar için)
const categories = ref<{ id: number; name: string }[]>([])

const loadCategories = async () => {
  // Eğer useCategories composable'ın varsa onu kullan; örnek:
  // const { getCategories } = useCategories()
  // const res = await getCategories(1, 1000, '')
  // categories.value = res?.data?.items || []
}

const handleEditFromDetail = (product: any) => {
  closeViewDialog()
  openEditDialog(product)
}
//#endregion

//#region Lifecycle
onMounted(async () => {
  await loadCategories()
  if (!productsData.value) {
    await loadItemsData(1, 10, '')
  }
})
//#endregion
</script>
```

**Client-side pagination (basit) versiyon — `tenants/index.vue` pattern'i:**

Eğer kayıt sayısı az ve listeleme tek seferde yapılabilir (1000 kayıttan az):
- `useCrudOperations`'da `serverSidePagination: false` yap
- Sayfa template'inden `:server-*` ve `@page-*` event'lerini, `currentPage`/`totalCount` kullanımını çıkar
- `useAsyncData` yapma — sadece `onMounted` içinde `await loadItemsData()` çağır
- `composables/useProducts.ts`'de `getProducts` default `pageSize = 1000` ver

---

### Pagination Modu Hangisi?

| Senaryo | Tercih | Composable Default | Page Setup |
|---------|--------|--------------------|-----|
| Kayıt sayısı 1000+ veya bilinmiyor | **Server-side** | `pageSize=10` | `useAsyncData` + `serverSidePagination: true` |
| Kayıt sayısı az (< 1000) ve sabit | **Client-side** | `pageSize=1000` | `serverSidePagination: false` |

`Users` server-side, `Tenants` client-side kullanır — referans için bu iki sayfaya bak.

---

### Permission Kontrolü — `definePageMeta`

Sayfayı korumak için `definePageMeta` kullanılır. Üç farklı yaklaşım:

```typescript
// 1. Tek permission (auth.global middleware otomatik kontrol eder)
definePageMeta({
  title: 'Ürünler',
  requiresAuth: true,
  permissions: ['Products.Read']
})

// 2. Permission + middleware (daha esnek — middleware dosyası "permission")
definePageMeta({
  title: 'Ürünler',
  requiresAuth: true,
  middleware: ['permission'],
  permissions: ['Products.Read']
})

// 3. Sadece role (Tenants sayfası gibi)
definePageMeta({
  title: 'Kiracılar',
  requiresAuth: true,
  middleware: ['permission'],
  roles: ['SuperAdmin']
})
```

**Önemli:** Permission string'leri **backend `Permissions.cs`'deki PascalCase** ile aynı yazılır: `'Products.Read'`, `'Products.Create'`, vb. Backend permission name'lerini bu şekilde tutar.

`middleware/permission.ts` `to.meta.permission`, `to.meta.permissions`, `to.meta.role`, `to.meta.roles` meta'larını kontrol eder ve uygun değilse 403 fırlatır.

---

## ADIM 8 — Sidebar Menüsüne Ekle (MEVCUT DOSYAYA SATIR EKLE)

**Dosya:** `composables/useNavigation.ts`

Mevcut menü yapısının uygun bir grubuna ekle. Yeni grup oluşturmak yerine mevcut "userManagement" gibi bir grupun altına eklemek tercih edilir, ya da yeni bir grup açabilirsin:

```typescript
// Mevcut "userManagement" grubunun yanına yeni grup olarak:
{
  title: 'sidebar.menu.catalogManagement',
  icon: 'mdi-package-variant',
  children: [
    {
      title: 'sidebar.menu.products',
      icon: 'mdi-package-variant-closed',
      to: '/products',
      permission: 'Products.Read'
    }
  ]
}

// VEYA tek bir item olarak (üst grup yoksa):
{
  title: 'sidebar.menu.products',
  icon: 'mdi-package-variant',
  to: '/products',
  permission: 'Products.Read'
}
```

**`permission` ve `roles` alanları:**
- `permission: 'Products.Read'` — kullanıcının bu permission'ı yoksa menüde görünmez
- `roles: ['SuperAdmin']` — sadece belirtilen rollerdeki kullanıcılar görür

`filterNavigationByPermissions` fonksiyonu menüyü `authStore.hasPermission()` / `authStore.hasRole()` ile filtreler — manuel yapmana gerek yok.

---

## ADIM 9 — i18n Çevirileri (MEVCUT DOSYALARA SATIR EKLE)

**Dosyalar:** `i18n/locales/tr.json` ve `i18n/locales/en.json`

Sidebar menü etiketleri için iki dosyaya da ekle:

```json
// tr.json — "sidebar.menu" altına:
"sidebar": {
  "menu": {
    ...mevcut anahtarlar,
    "catalogManagement": "Katalog Yönetimi",
    "products": "Ürünler"
  }
}
```

```json
// en.json — "sidebar.menu" altına:
"sidebar": {
  "menu": {
    ...mevcut anahtarlar,
    "catalogManagement": "Catalog Management",
    "products": "Products"
  }
}
```

> Sidebar `t('sidebar.menu.products')` ile çağırır. Her iki dile de eklemeyi unutma, aksi halde eksik olan dilde key string olarak görünür.

---

## `useCrudOperations` — Kullanılabilir Değerler

Sayfada kullandığında destructure edebileceğin tüm değerler:

```typescript
const {
  // Data
  items,              // Listedeki tüm itemlar
  isLoading,          // Yükleme durumu
  isDeleting,         // Silme durumu

  // Pagination (server-side)
  currentPage,
  pageSize,
  totalCount,
  totalPages,
  searchTerm,

  // Dialog State (useDialogManager'dan)
  dialogs,            // { create, view, edit, delete } boolean
  selectedItem,       // Üzerinde işlem yapılan item
  itemToDelete,       // Silinecek item
  isEditMode,         // Düzenleme mi?

  // Dialog Açma/Kapatma
  openCreateDialog,
  openViewDialog,
  openEditDialog,
  openDeleteDialog,
  closeCreateDialog,
  closeViewDialog,
  closeDeleteDialog,

  // CRUD İşlemleri
  handleSubmit,       // Form submit
  confirmDelete,      // Silme onayı
  handleSearch,       // Arama
  handlePageChange,   // Sayfa değiştirme
  handlePageSizeChange,
  refreshData,        // Refresh
  loadItemsData       // İlk yükleme/manuel reload
} = useCrudOperations({
  loadItems,          // Composable'dan getProducts
  createItem,         // createProduct
  updateItem,         // updateProduct (id, data)
  deleteItem,         // deleteProduct (id)
  itemName: 'ürün',   // Toast mesajları için
  serverSidePagination: true | false,
  initialPageSize: 10,
  initialData: productsData,    // SSR data
  toastMessages: {              // Opsiyonel — varsayılan mesajlar otomatik üretilir
    createSuccess: 'Ürün başarıyla oluşturuldu',
    updateSuccess: 'Ürün başarıyla güncellendi',
    deleteSuccess: 'Ürün başarıyla silindi',
    loadError: 'Ürünler yüklenirken hata oluştu',
    createError: 'Ürün oluşturulurken hata oluştu',
    updateError: 'Ürün güncellenirken hata oluştu',
    deleteError: 'Ürün silinirken hata oluştu',
  }
})
```

---

## `BaseDataTable` — Ana Prop'lar

`components/UI/BaseDataTable.vue`'nun beklediği prop'lar:

| Prop | Tip | Notlar |
|------|-----|--------|
| `items` | `any[]` | Listelenecek veri |
| `columns` | `Column[]` | `{ label, key, sortable?, filterable?, filterType?, width?, formatter? }` |
| `title` | `string` | Üst başlık |
| `toolbar-icon` | `string` | MDI icon adı |
| `search-placeholder` | `string` | Arama input placeholder'ı |
| `add-button-text` | `string` | Ekleme butonu metni |
| `loading` | `boolean` | İskelet/spinner durumu |
| `empty-title` | `string` | Boşken gösterilecek mesaj |
| `show-add-button` | `boolean` | + butonu |
| `show-actions` | `boolean` | View/Edit/Delete kolonu |
| `show-view-button`, `show-edit-button`, `show-delete-button` | `boolean` | Hangi action butonları |
| `show-pagination` | `boolean` | Pagination bar |
| `server-side-pagination` | `boolean` | Backend pagination mı? |
| `server-total-count` | `number` | Server-side için toplam kayıt |
| `server-current-page` | `number` | Server-side için aktif sayfa |
| `use-view-modal` | `boolean` | View butonu modal mı yoksa route mu? |

**Event'ler:** `@add`, `@view`, `@edit`, `@delete`, `@search`, `@page-change`, `@page-size-change`, `@refresh`.

**Custom hücre render'ı için slot:** `<template #cell-{columnKey}="{ item, value }">`. Örnek: `#cell-status`, `#cell-name`.

---

## Kontrol Listesi (Yeni Sayfa)

### Tipler
- [ ] `types/product.ts` — entity, Create*Request, Update*Request interface'leri
- [ ] `types/index.ts` — yeni tipler export edildi

### API
- [ ] `utils/apiEndpoints.ts` — yeni `PRODUCTS` grubu eklendi (LIST/CREATE/GET_BY_ID/UPDATE/DELETE)

### Composable
- [ ] `composables/useProducts.ts` — get/getById/create/update/delete fonksiyonları

### Bileşenler
- [ ] `components/Products/ProductForm.vue` — form bileşeni (`product?`, `loading?` prop, `submit`/`cancel` emit)
- [ ] `components/Products/ProductDetail.vue` — detay bileşeni (opsiyonel)

### Sayfa
- [ ] `pages/products/index.vue` — `BaseDataTable` + `useCrudOperations` + drawer'lar
- [ ] `definePageMeta` ile permission/role tanımlandı
- [ ] Server-side veya client-side pagination doğru seçildi

### Navigation
- [ ] `composables/useNavigation.ts` — sidebar menüye eklendi
- [ ] Menü item'ında `permission` veya `roles` filtreleme tanımlandı

### i18n
- [ ] `i18n/locales/tr.json` — menü etiketleri eklendi
- [ ] `i18n/locales/en.json` — menü etiketleri eklendi

---

## Sık Yapılan Hatalar

| Hata | Doğrusu |
|------|---------|
| Composable'da `response.value` döndürmek | Bu projede `useApi` Axios `response.data` döndürür — `response.data` veya doğrudan `response` kullan |
| `getProducts`'tan items extract etmek (`return response.data.items`) | `useCrudOperations` server-side modda `response.data.items` yapısını **kendisi** bekler — composable'da olduğu gibi döndür |
| `useCrudOperations`'a `getById` geçmek | `useCrudOperations` `getById` kullanmaz — `selectedItem` zaten `openEditDialog(item)` ile gelir, getById sadece detay sayfaları için |
| `definePageMeta`'da permission'ı küçük harfle yazmak | Backend `Permissions.cs`'de **PascalCase** (`'Products.Read'`) — frontend de aynı şekilde |
| API endpoint path'inde tireli/snake_case kullanmak | Backend `[Route("api/[controller]")]` kullanır — controller adının **küçük harfli çoğul** hali (`/api/products`) |
| Yeni sayfayı menüye eklemeyi unutmak | `useNavigation.ts`'de tanımlı değilse sidebar'da görünmez (route yine çalışır, ama erişim için elden yazılması gerekir) |
| i18n'de menu key'i eklememek | `useNavigation`'daki `title: 'sidebar.menu.products'` çevirisi yoksa raw key gösterilir |
| Form'da `formData` reactivity'sini bozmak | `Object.assign(formData, ...)` ile güncelle, `formData = {...}` ile yeniden atama yapma (reactive bağlantısı kopar) |
| `ResizableDrawer` v-model'i doğru yönetmemek | `v-model="dialogs.create"` — `useCrudOperations` `dialogs` ref'i içinde tutar |
| `ConfirmDialog`'a `itemToDelete?.name` yerine direkt `itemToDelete.name` yazmak | İlk yüklemede `null` olabilir — optional chaining kullan |
| Server-side pagination'da `loadItemsData()`'yı parametresiz çağırmak | Server-side modda `loadItemsData(1, pageSize, '')` parametreleri verilmeli |
| Backend yeni endpoint eklendi ama frontend `apiEndpoints.ts`'i unuttu | Path mismatch — 404 alırsın; her zaman ikisini birlikte güncelle |
| Composable'ı `import` etmeden çağırmak | Nuxt auto-import bu klasörü tarar; `useProducts` direkt yazılabilir — sayfada `import { useProducts }` yazma |
| SSR data ve client-side ilk yükleme'yi ikisi birden çalıştırmak | `onMounted` içinde `if (!productsData.value) await loadItemsData(...)` kontrolü ekle |
