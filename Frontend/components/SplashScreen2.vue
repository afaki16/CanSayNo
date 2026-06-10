<template>
  <Transition name="splash-out">
    <div v-if="showSplash" class="splash">
      <!-- Layered background -->
      <div class="splash-bg">
        <div class="grid-floor" />
        <div class="radial-pulse" />
        <div class="noise-overlay" />
      </div>

      <!-- Concentric rings -->
      <div class="rings-container">
        <div v-for="i in 5" :key="'ring'+i" class="ring" :style="getRingStyle(i)" />
      </div>

      <!-- Orbiting dots -->
      <div class="orbit-track">
        <div v-for="i in 8" :key="'orb'+i" class="orbit-dot" :style="getOrbitStyle(i)" />
      </div>

      <!-- Center logo -->
      <div class="splash-center" :class="{ 'revealed': splashReady }">
        <!-- Hexagonal glow behind logo -->
        <svg class="hex-glow" viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg">
          <polygon
            points="100,10 178,55 178,145 100,190 22,145 22,55"
            fill="none"
            stroke="url(#hexGrad)"
            stroke-width="1.5"
            class="hex-path"
          />
          <polygon
            points="100,30 163,65 163,135 100,170 37,135 37,65"
            fill="none"
            stroke="url(#hexGrad2)"
            stroke-width="0.8"
            class="hex-path-inner"
          />
          <defs>
            <linearGradient id="hexGrad" x1="0" y1="0" x2="1" y2="1">
              <stop offset="0%" stop-color="#00e5ff" stop-opacity="0.8" />
              <stop offset="50%" stop-color="#7c4dff" stop-opacity="0.6" />
              <stop offset="100%" stop-color="#00e5ff" stop-opacity="0.8" />
            </linearGradient>
            <linearGradient id="hexGrad2" x1="1" y1="0" x2="0" y2="1">
              <stop offset="0%" stop-color="#7c4dff" stop-opacity="0.4" />
              <stop offset="100%" stop-color="#00e5ff" stop-opacity="0.4" />
            </linearGradient>
          </defs>
        </svg>

        <!-- Holographic scan line -->
        <div class="holo-scan" />

        <!-- Logo -->
        <img
          :src="appData?.app?.logo?.src || '/images/logo.png'"
          :alt="appData?.app?.logo?.alt || 'Logo'"
          class="splash-logo"
        />
      </div>

      <!-- Floating particles -->
      <div class="particles-field">
        <div
          v-for="i in 50"
          :key="'p'+i"
          class="mote"
          :style="getMoteStyle(i)"
        />
      </div>

      <!-- Corner accents -->
      <svg class="corner-accent top-left" viewBox="0 0 120 120">
        <path d="M0,60 Q0,0 60,0" fill="none" stroke="rgba(0,229,255,0.15)" stroke-width="1" />
        <path d="M0,40 Q0,0 40,0" fill="none" stroke="rgba(0,229,255,0.08)" stroke-width="1" />
      </svg>
      <svg class="corner-accent bottom-right" viewBox="0 0 120 120">
        <path d="M120,60 Q120,120 60,120" fill="none" stroke="rgba(124,77,255,0.15)" stroke-width="1" />
        <path d="M120,80 Q120,120 80,120" fill="none" stroke="rgba(124,77,255,0.08)" stroke-width="1" />
      </svg>
    </div>
  </Transition>
</template>

<script setup lang="ts">
const { appData } = useAppData()

const showSplash = ref(true)
const splashReady = ref(false)

const emit = defineEmits(['complete'])

const getRingStyle = (index: number) => {
  const size = 180 + index * 90
  const delay = index * 0.15
  const duration = 2.5 + index * 0.4
  return {
    width: `${size}px`,
    height: `${size}px`,
    animationDelay: `${delay}s`,
    animationDuration: `${duration}s`,
    opacity: `${0.25 - index * 0.035}`
  }
}

const getOrbitStyle = (index: number) => {
  const angle = (index / 8) * 360
  const delay = index * 0.2
  return {
    '--start-angle': `${angle}deg`,
    animationDelay: `${delay}s`
  } as Record<string, string>
}

const getMoteStyle = (index: number) => {
  const x = Math.random() * 100
  const y = Math.random() * 100
  const size = Math.random() * 3 + 1
  const delay = Math.random() * 3
  const duration = 3 + Math.random() * 4
  const drift = (Math.random() - 0.5) * 80
  const isCyan = Math.random() > 0.5
  return {
    left: `${x}%`,
    top: `${y}%`,
    width: `${size}px`,
    height: `${size}px`,
    '--drift': `${drift}px`,
    '--mote-color': isCyan ? 'rgba(0,229,255,0.7)' : 'rgba(124,77,255,0.6)',
    animationDelay: `${delay}s`,
    animationDuration: `${duration}s`
  } as Record<string, string>
}

onMounted(() => {
  nextTick(() => { splashReady.value = true })

  setTimeout(() => {
    showSplash.value = false
    emit('complete')
  }, 2000)
})
</script>

<style scoped>
.splash {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #060611;
  overflow: hidden;
}

/* ── Background layers ── */
.splash-bg {
  position: absolute;
  inset: 0;
  z-index: 1;
}

.grid-floor {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(0, 229, 255, 0.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 229, 255, 0.03) 1px, transparent 1px);
  background-size: 60px 60px;
  mask-image: radial-gradient(ellipse at center, black 20%, transparent 70%);
  -webkit-mask-image: radial-gradient(ellipse at center, black 20%, transparent 70%);
  animation: grid-drift 20s linear infinite;
}

@keyframes grid-drift {
  0% { transform: perspective(500px) rotateX(25deg) translateY(0); }
  100% { transform: perspective(500px) rotateX(25deg) translateY(60px); }
}

.radial-pulse {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 600px;
  height: 600px;
  transform: translate(-50%, -50%);
  border-radius: 50%;
  background: radial-gradient(
    circle,
    rgba(124, 77, 255, 0.08) 0%,
    rgba(0, 229, 255, 0.04) 40%,
    transparent 70%
  );
  animation: radial-breathe 3s ease-in-out infinite alternate;
}

@keyframes radial-breathe {
  0% { transform: translate(-50%, -50%) scale(0.85); opacity: 0.6; }
  100% { transform: translate(-50%, -50%) scale(1.15); opacity: 1; }
}

.noise-overlay {
  position: absolute;
  inset: 0;
  opacity: 0.04;
  background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
  background-size: 128px 128px;
  pointer-events: none;
}

/* ── Concentric rings ── */
.rings-container {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 2;
}

.ring {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  border-radius: 50%;
  border: 1px solid rgba(0, 229, 255, 0.12);
  animation: ring-expand ease-out infinite;
}

@keyframes ring-expand {
  0% { transform: translate(-50%, -50%) scale(0.6); opacity: 0; }
  30% { opacity: 1; }
  100% { transform: translate(-50%, -50%) scale(1.4); opacity: 0; }
}

/* ── Orbiting dots ── */
.orbit-track {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 280px;
  height: 280px;
  transform: translate(-50%, -50%);
  z-index: 3;
}

.orbit-dot {
  position: absolute;
  width: 4px;
  height: 4px;
  background: #00e5ff;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(0, 229, 255, 0.6);
  top: 50%;
  left: 50%;
  transform-origin: 0 0;
  animation: orbit-spin 4s linear infinite;
}

@keyframes orbit-spin {
  0% { transform: rotate(var(--start-angle)) translateX(140px) scale(1); }
  50% { transform: rotate(calc(var(--start-angle) + 180deg)) translateX(140px) scale(0.5); }
  100% { transform: rotate(calc(var(--start-angle) + 360deg)) translateX(140px) scale(1); }
}

/* ── Center logo block ── */
.splash-center {
  position: relative;
  z-index: 10;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transform: scale(0.7);
  transition: all 0.9s cubic-bezier(0.22, 1, 0.36, 1);
}

.splash-center.revealed {
  opacity: 1;
  transform: scale(1);
}

.hex-glow {
  position: absolute;
  width: 320px;
  height: 320px;
  z-index: 1;
}

.hex-path {
  stroke-dasharray: 800;
  stroke-dashoffset: 800;
  animation: hex-draw 1.8s 0.3s cubic-bezier(0.4, 0, 0.2, 1) forwards,
             hex-rotate 12s 2s linear infinite;
  transform-origin: center;
}

.hex-path-inner {
  stroke-dasharray: 600;
  stroke-dashoffset: 600;
  animation: hex-draw-inner 1.6s 0.6s cubic-bezier(0.4, 0, 0.2, 1) forwards,
             hex-rotate-reverse 16s 2s linear infinite;
  transform-origin: center;
}

@keyframes hex-draw {
  to { stroke-dashoffset: 0; }
}

@keyframes hex-draw-inner {
  to { stroke-dashoffset: 0; }
}

@keyframes hex-rotate {
  to { transform: rotate(360deg); }
}

@keyframes hex-rotate-reverse {
  to { transform: rotate(-360deg); }
}

/* Holographic scan */
.holo-scan {
  position: absolute;
  inset: -20%;
  z-index: 15;
  pointer-events: none;
  background: linear-gradient(
    180deg,
    transparent 0%,
    rgba(0, 229, 255, 0.06) 48%,
    rgba(0, 229, 255, 0.25) 50%,
    rgba(0, 229, 255, 0.06) 52%,
    transparent 100%
  );
  background-size: 100% 40px;
  animation: holo-sweep 2.5s ease-in-out infinite;
  mix-blend-mode: screen;
}

@keyframes holo-sweep {
  0% { transform: translateY(-100%); }
  100% { transform: translateY(100%); }
}

/* Logo */
.splash-logo {
  position: relative;
  z-index: 10;
  height: 200px;
  width: auto;
  object-fit: contain;
  filter: drop-shadow(0 0 40px rgba(0, 229, 255, 0.4))
          drop-shadow(0 0 80px rgba(124, 77, 255, 0.2));
  animation: logo-hover 3s ease-in-out infinite alternate;
}

@keyframes logo-hover {
  0% { transform: translateY(6px) scale(1); }
  100% { transform: translateY(-6px) scale(1.02); }
}

/* ── Floating motes ── */
.particles-field {
  position: absolute;
  inset: 0;
  z-index: 4;
  pointer-events: none;
}

.mote {
  position: absolute;
  border-radius: 50%;
  background: var(--mote-color, rgba(0,229,255,0.6));
  box-shadow: 0 0 6px var(--mote-color, rgba(0,229,255,0.4));
  animation: mote-float linear infinite;
}

@keyframes mote-float {
  0% {
    transform: translateY(0) translateX(0) scale(0);
    opacity: 0;
  }
  15% {
    opacity: 1;
    transform: translateY(-20px) translateX(calc(var(--drift) * 0.2)) scale(1);
  }
  85% {
    opacity: 0.6;
  }
  100% {
    transform: translateY(-120px) translateX(var(--drift)) scale(0);
    opacity: 0;
  }
}

/* ── Corner accents ── */
.corner-accent {
  position: absolute;
  width: 120px;
  height: 120px;
  z-index: 5;
}

.top-left {
  top: 24px;
  left: 24px;
}

.bottom-right {
  bottom: 24px;
  right: 24px;
}

/* ── Exit transition ── */
.splash-out-leave-active {
  transition: all 0.8s cubic-bezier(0.7, 0, 0.3, 1);
}

.splash-out-leave-to {
  opacity: 0;
  transform: scale(1.12);
  filter: blur(12px);
}
</style>