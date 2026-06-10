<template>
  <div :class="['alert-box', `alert-box--${variant}`]" role="alert">
    <!-- Icon box -->
    <div class="alert-icon-box">
      <!-- warning: triangle exclamation -->
      <svg
        v-if="variant === 'warning'"
        width="18"
        height="18"
        viewBox="0 0 24 24"
        fill="currentColor"
        aria-hidden="true"
      >
        <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z" />
      </svg>
      <!-- error: circle exclamation -->
      <svg
        v-else-if="variant === 'error'"
        width="18"
        height="18"
        viewBox="0 0 24 24"
        fill="currentColor"
        aria-hidden="true"
      >
        <path
          d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"
        />
      </svg>
      <!-- success: check circle -->
      <svg
        v-else
        width="18"
        height="18"
        viewBox="0 0 24 24"
        fill="currentColor"
        aria-hidden="true"
      >
        <path
          d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"
        />
      </svg>
    </div>

    <!-- Content -->
    <div class="alert-content">
      <div v-if="title" class="alert-title">{{ title }}</div>
      <div v-if="messageList.length" class="alert-message">
        <template v-if="!isList">{{ messageList[0] }}</template>
        <ul v-else>
          <li v-for="(msg, i) in messageList" :key="i">{{ msg }}</li>
        </ul>
      </div>
      <slot />
    </div>

    <!-- Close button -->
    <button
      v-if="closable"
      type="button"
      class="alert-close"
      :aria-label="closeLabel"
      @click="emit('close')"
    >
      <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
        <path
          d="M18.3 5.71a1 1 0 0 0-1.41 0L12 10.59 7.11 5.7A1 1 0 0 0 5.7 7.11L10.59 12 5.7 16.89a1 1 0 1 0 1.41 1.41L12 13.41l4.89 4.89a1 1 0 0 0 1.41-1.41L13.41 12l4.89-4.89a1 1 0 0 0 0-1.4z"
        />
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    /** Renk varyantı */
    variant?: "warning" | "error" | "success";
    /** Kalın başlık (opsiyonel) */
    title?: string;
    /** Tek mesaj veya birden fazla mesaj (dizi → disc liste) */
    message?: string | string[];
    /** Kapatma (×) butonunu göster */
    closable?: boolean;
    /** Kapatma butonu erişilebilirlik etiketi */
    closeLabel?: string;
  }>(),
  {
    variant: "warning",
    title: "",
    message: () => [],
    closable: true,
    closeLabel: "Kapat",
  },
);

const emit = defineEmits<{ (e: "close"): void }>();

const messageList = computed<string[]>(() => {
  const m = props.message;
  if (Array.isArray(m)) return m.filter(Boolean);
  return m ? [m] : [];
});

const isList = computed(() => messageList.value.length > 1);
</script>

<style scoped>
.alert-box {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px 16px;
  border-radius: 14px;
  border: 1.5px solid;
  margin-bottom: 20px;
}

/* Icon box */
.alert-icon-box {
  flex-shrink: 0;
  width: 34px;
  height: 34px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Content */
.alert-content {
  flex: 1;
  min-width: 0;
}

.alert-title {
  font-size: 13.5px;
  font-weight: 700;
  margin-bottom: 3px;
}

.alert-message {
  font-size: 13px;
  line-height: 1.5;
  opacity: 0.85;
}

.alert-message ul {
  margin: 0;
  padding-left: 1.1rem;
  list-style: disc;
}

.alert-message li + li {
  margin-top: 0.25rem;
}

/* Close button */
.alert-close {
  flex-shrink: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  border: none;
  background: transparent;
  border-radius: 6px;
  color: inherit;
  cursor: pointer;
  opacity: 0.5;
  transition: opacity 0.2s ease, background 0.2s ease;
}

.alert-close:hover {
  opacity: 1;
  background: rgba(0, 0, 0, 0.06);
}

/* ── Variants ── */
.alert-box--warning {
  background: #fffbeb;
  border-color: #fde68a;
  color: #92400e;
}
.alert-box--warning .alert-icon-box {
  background: #fef3c7;
  color: #d97706;
}

.alert-box--error {
  background: #fef2f2;
  border-color: #fecaca;
  color: #991b1b;
}
.alert-box--error .alert-icon-box {
  background: #fee2e2;
  color: #dc2626;
}

.alert-box--success {
  background: #f0fdf4;
  border-color: #bbf7d0;
  color: #166534;
}
.alert-box--success .alert-icon-box {
  background: #dcfce7;
  color: #16a34a;
}
</style>
