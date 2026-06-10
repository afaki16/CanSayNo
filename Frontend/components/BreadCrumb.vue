<template>
  <nav class="flex" aria-label="Breadcrumb">
    <ol class="inline-flex items-center space-x-1 md:space-x-3">
      <li v-for="(item, index) in items" :key="index" class="inline-flex items-center">
        <div v-if="index !== 0" class="flex items-center">
          <svg class="w-3 h-3 text-gray-400 mx-1" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 6 10">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m1 9 4-4-4-4"/>
          </svg>
        </div>
        <NuxtLink
          v-if="item.to"
          :to="item.to"
          class="inline-flex items-center text-sm font-medium text-gray-700 hover:text-blue-600 dark:text-gray-400 dark:hover:text-blue-500 breadcrumb-link"
        >
          <component
            v-if="item.icon"
            :is="item.icon"
            class="w-4 h-4 mr-2"
          />
          {{ item.text }}
        </NuxtLink>
        <span
          v-else
          class="text-sm font-medium text-gray-500 dark:text-gray-400"
        >
          {{ item.text }}
        </span>
      </li>
    </ol>
  </nav>
</template>

<script setup lang="ts">
interface BreadcrumbItem {
  text: string;
  to?: string;
  icon?: string;
}

defineProps<{
  items: BreadcrumbItem[];
}>();
</script>

<style scoped>
.breadcrumb-item {
  @apply text-sm font-medium;
}

.breadcrumb-item:not(:last-child)::after {
  content: '/';
  @apply mx-2 text-gray-400;
}

.breadcrumb-link:hover {
  color: rgb(var(--color-primary)) !important;
}
</style> 