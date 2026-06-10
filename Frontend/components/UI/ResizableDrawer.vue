<template>
  <Teleport to="body">
    <Transition name="drawer">
      <div v-if="modelValue" class="drawer-overlay">
        <div 
          class="drawer-container" 
          :style="{ width: isFullscreen ? '100vw' : drawerWidth + 'px' }"
          :class="{ 'fullscreen': isFullscreen, 'resizing': isResizingActive }"
          @click.stop
        >
          <!-- Resize Handle with Dots -->
          <div 
            v-if="!isFullscreen"
            class="resize-handle"
            @mousedown="startResize"
          >
            <div class="resize-dots">
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
            </div>
          </div>

          <!-- DEFAULT VARIANT -->
          <template v-if="variant === 'default'">
            <div class="drawer-header">
              <h2 class="drawer-title">
                <v-icon v-if="icon" size="24" class="mr-2">{{ icon }}</v-icon>
                {{ title }}
              </h2>
              <div class="drawer-actions">
                <v-btn
                  :icon="isFullscreen ? 'mdi-fullscreen-exit' : 'mdi-fullscreen'"
                  variant="text"
                  size="small"
                  @click="toggleFullscreen"
                  class="mr-2"
                ></v-btn>
                <v-btn
                  icon="mdi-close"
                  variant="text"
                  size="small"
                  @click="$emit('update:modelValue', false)"
                ></v-btn>
              </div>
            </div>

            <v-divider></v-divider>
          </template>

          <!-- MODERN VARIANT -->
          <template v-else>
            <div class="drawer-header-modern">
              <!-- Aurora gradient glow (dekoratif) -->
              <div class="drawer-header-modern__glow" aria-hidden="true"></div>

              <div class="drawer-header-modern__content">
                <!-- Sol: Avatar/Icon Badge -->
                <div v-if="icon" class="drawer-header-modern__avatar">
                  <div class="drawer-header-modern__avatar-ring" aria-hidden="true"></div>
                  <v-icon :icon="icon" size="28" />
                </div>

                <!-- Orta: Title + Subtitle -->
                <div class="drawer-header-modern__text">
                  <h2 class="drawer-header-modern__title">{{ title }}</h2>
                  <p v-if="subtitle" class="drawer-header-modern__subtitle">{{ subtitle }}</p>
                </div>

                <!-- Sağ: Actions -->
                <div class="drawer-header-modern__actions">
                  <button
                    class="drawer-header-modern__action-btn"
                    :title="isFullscreen ? 'Tam ekranı kapat' : 'Tam ekran'"
                    @click="toggleFullscreen"
                  >
                    <v-icon :icon="isFullscreen ? 'mdi-fullscreen-exit' : 'mdi-fullscreen'" size="18" />
                  </button>
                  <button
                    class="drawer-header-modern__action-btn"
                    title="Kapat"
                    @click="$emit('update:modelValue', false)"
                  >
                    <v-icon icon="mdi-close" size="18" />
                  </button>
                </div>
              </div>
            </div>
          </template>

          <div class="drawer-content">
            <slot></slot>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  title: {
    type: String,
    required: true
  },
  icon: {
    type: String,
    default: ''
  },
  defaultWidth: {
    type: Number,
    default: 600
  },
  minWidth: {
    type: Number,
    default: 400
  },
  subtitle: {
    type: String,
    default: ''
  },
  variant: {
    type: String,
    default: 'modern'
  }
})

const emit = defineEmits(['update:modelValue'])

const drawerWidth = ref(props.defaultWidth)
const isFullscreen = ref(false)
const previousWidth = ref(props.defaultWidth)
const isResizingActive = ref(false)
const maxDrawerWidth = typeof window !== 'undefined' ? window.innerWidth * 0.9 : 1200

let isResizing = false
let startX = 0
let startWidth = 0

const toggleFullscreen = () => {
  if (!isFullscreen.value) {
    previousWidth.value = drawerWidth.value
    isFullscreen.value = true
  } else {
    isFullscreen.value = false
    drawerWidth.value = previousWidth.value
  }
}

const startResize = (e) => {
  isResizing = true
  isResizingActive.value = true
  startX = e.clientX
  startWidth = drawerWidth.value
  document.body.style.cursor = 'ew-resize'
  document.body.style.userSelect = 'none'
  
  document.addEventListener('mousemove', handleResize)
  document.addEventListener('mouseup', stopResize)
}

const handleResize = (e) => {
  if (!isResizing) return
  
  const diff = startX - e.clientX
  let newWidth = startWidth + diff
  
  newWidth = Math.max(props.minWidth, Math.min(newWidth, maxDrawerWidth))
  drawerWidth.value = newWidth
}

const stopResize = () => {
  isResizing = false
  isResizingActive.value = false
  document.body.style.cursor = ''
  document.body.style.userSelect = ''
  
  document.removeEventListener('mousemove', handleResize)
  document.removeEventListener('mouseup', stopResize)
}

const handleKeyDown = (e) => {
  if ((e.key === 'Escape' || e.keyCode === 27) && props.modelValue) {
    e.preventDefault()
    if (isFullscreen.value) {
      toggleFullscreen()
    } else {
      emit('update:modelValue', false)
    }
  }
}

watch(() => props.modelValue, (newValue) => {
  if (!newValue) {
    isFullscreen.value = false
  }
})

onMounted(() => {
  if (import.meta.client) {
    document.addEventListener('keydown', handleKeyDown)
  }
})

onUnmounted(() => {
  if (import.meta.client) {
    document.removeEventListener('mousemove', handleResize)
    document.removeEventListener('mouseup', stopResize)
    document.removeEventListener('keydown', handleKeyDown)
  }
})
</script>

<style scoped>
.drawer-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 9999;
  display: flex;
  justify-content: flex-end;
}

.drawer-container {
  max-width: 90vw;
  min-width: 400px;
  height: 100vh;
  background: rgb(var(--color-surface-container-lowest));
  color: rgb(var(--color-on-surface));
  box-shadow: -4px 0 16px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  position: relative;
  transition: width 0.3s ease;
}

.drawer-container.resizing {
  transition: none;
}

.drawer-container.fullscreen {
  max-width: 100vw;
  min-width: 100vw;
  box-shadow: none;
}

.resize-handle {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 10px;
  cursor: ew-resize;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s ease;
}

.resize-handle:hover {
  background: rgb(var(--color-on-surface) / 0.04);
}

.resize-dots {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 3px;
  padding: 2px;
}

.dot {
  width: 3px;
  height: 3px;
  background: rgb(var(--color-on-surface-variant) / 0.6);
  border-radius: 50%;
  transition: all 0.2s ease;
}

.resize-handle:hover .dot {
  background: rgb(var(--color-on-surface-variant));
  transform: scale(1.2);
}

.resize-handle:active .dot {
  background: rgb(var(--color-primary));
}

.drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  background: rgb(var(--color-surface-container-low));
  color: rgb(var(--color-on-surface));
  flex-shrink: 0;
}

.drawer-title {
  display: flex;
  align-items: center;
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0;
  color: rgb(var(--color-on-surface));
}

.drawer-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.drawer-content {
  flex: 1;
  padding: 24px;
  overflow-y: auto;
}

.drawer-content::-webkit-scrollbar {
  width: 8px;
}

.drawer-content::-webkit-scrollbar-track {
  background: rgb(var(--color-surface-container-low));
}

.drawer-content::-webkit-scrollbar-thumb {
  background: rgb(var(--color-on-surface-variant) / 0.4);
  border-radius: 4px;
}

.drawer-content::-webkit-scrollbar-thumb:hover {
  background: rgb(var(--color-on-surface-variant) / 0.6);
}

.drawer-enter-active,
.drawer-leave-active {
  transition: opacity 0.3s ease;
}

.drawer-enter-active .drawer-container,
.drawer-leave-active .drawer-container {
  transition: transform 0.3s ease;
}

.drawer-enter-from,
.drawer-leave-to {
  opacity: 0;
}

.drawer-enter-from .drawer-container {
  transform: translateX(100%);
}

.drawer-leave-to .drawer-container {
  transform: translateX(100%);
}

/* ==========================================
   MODERN VARIANT — Drawer Header
   ========================================== */
.drawer-header-modern {
  position: relative;
  padding: 24px 28px 22px;
  background: rgb(var(--color-surface-container-low));
  border-bottom: 1px solid rgb(var(--color-outline-variant) / 0.4);
  overflow: hidden;
  flex-shrink: 0;
}

/* Aurora glow — sol üstte yumuşak ışıma */
.drawer-header-modern__glow {
  position: absolute;
  top: -40%;
  left: -10%;
  width: 60%;
  height: 180%;
  background: radial-gradient(
    ellipse at center,
    rgb(var(--color-primary) / 0.18) 0%,
    rgb(var(--color-primary) / 0.08) 35%,
    transparent 70%
  );
  filter: blur(40px);
  pointer-events: none;
  z-index: 0;
}

.drawer-header-modern__content {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: 16px;
}

/* Avatar (icon badge) */
.drawer-header-modern__avatar {
  position: relative;
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgb(var(--color-primary) / 0.15);
  color: rgb(var(--color-primary));
  box-shadow: 0 0 24px rgb(var(--color-primary) / 0.25);
}

/* Avatar dış halka */
.drawer-header-modern__avatar-ring {
  position: absolute;
  inset: -4px;
  border-radius: 50%;
  border: 1px solid rgb(var(--color-primary) / 0.35);
  pointer-events: none;
}

/* Text bloğu */
.drawer-header-modern__text {
  flex: 1;
  min-width: 0;
}

.drawer-header-modern__title {
  font-size: 1.25rem;
  font-weight: 700;
  color: rgb(var(--color-on-surface));
  line-height: 1.2;
  margin: 0 0 4px 0;
  letter-spacing: -0.01em;
}

.drawer-header-modern__subtitle {
  font-size: 0.875rem;
  color: rgb(var(--color-on-surface-variant));
  margin: 0;
  line-height: 1.3;
}

/* Actions */
.drawer-header-modern__actions {
  display: flex;
  gap: 8px;
  flex-shrink: 0;
}

.drawer-header-modern__action-btn {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgb(var(--color-surface-container-high) / 0.6);
  color: rgb(var(--color-on-surface-variant));
  border: 1px solid rgb(var(--color-outline-variant) / 0.3);
  cursor: pointer;
  transition: all 0.2s ease;
}

.drawer-header-modern__action-btn:hover {
  background: rgb(var(--color-surface-container-highest));
  color: rgb(var(--color-on-surface));
  border-color: rgb(var(--color-outline-variant) / 0.6);
}

/* Dark mode ince ayarları */
.dark .drawer-header-modern__glow {
  background: radial-gradient(
    ellipse at center,
    rgb(var(--color-primary) / 0.28) 0%,
    rgb(var(--color-primary) / 0.12) 35%,
    transparent 70%
  );
}

.dark .drawer-header-modern__avatar {
  box-shadow: 0 0 32px rgb(var(--color-primary) / 0.4);
}
</style>
