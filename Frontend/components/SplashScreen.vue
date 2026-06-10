<template>
  <Transition name="splash-fade">
    <div v-if="showSplash" class="splash-screen">
      <div class="splash-bg-gears">
         <v-icon class="bg-gear bg-gear-1" size="500">mdi-cog-outline</v-icon>
         <v-icon class="bg-gear bg-gear-2" size="600">mdi-cog-outline</v-icon>
      </div>

      <div class="splash-content" :class="{ 'splash-animate': splashReady }">
        <div class="splash-logo-wrapper modern-scan-effect">
          <v-icon class="splash-logo-pulse-gear" icon="mdi-cog"></v-icon>
          
          <img
            :src="appData?.app?.logo?.src || '/images/logo.png'"
            :alt="appData?.app?.logo?.alt || 'Logo'"
            class="splash-logo-img"
          />
        </div>
      </div>
      
      <div class="splash-particles-container">
        <div v-for="i in 40" :key="'sp'+i" class="splash-particle" :style="getSplashParticleStyle(i)"></div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const { appData } = useAppData()

const showSplash = ref(true)
const splashReady = ref(false)

const emit = defineEmits(['complete'])

const getSplashParticleStyle = (index: number) => {
  const angle = (index / 40) * Math.PI * 2
  const radius = 150 + Math.random() * 250
  const size = Math.random() * 5 + 3
  const delay = Math.random() * 1
  const duration = 1.2 + Math.random() * 1.5
  const tx = Math.cos(angle) * radius
  const ty = Math.sin(angle) * radius
  return {
    '--tx': `${tx}px`,
    '--ty': `${ty}px`,
    width: `${size}px`,
    height: `${size}px`,
    animationDelay: `${delay}s`,
    animationDuration: `${duration}s`
  } as Record<string, string>
}

onMounted(() => {
  nextTick(() => { splashReady.value = true })

  setTimeout(() => { 
    showSplash.value = false;
    emit('complete');
  }, 2000);
})
</script>

<style scoped>
.splash-screen {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #09071d 0%, #1a163f 50%, #13112c 100%);
  overflow: hidden;
}

.splash-bg-gears {
  position: absolute;
  inset: 0;
  overflow: hidden;
  opacity: 0.03;
  pointer-events: none;
  z-index: 1;
}

.bg-gear-1 {
  position: absolute;
  top: -20%;
  left: -20%;
  animation: rotate-heavy-cw 60s linear infinite;
}

.bg-gear-2 {
  position: absolute;
  bottom: -25%;
  right: -25%;
  animation: rotate-heavy-ccw 80s linear infinite;
}

.splash-content {
  position: relative;
  z-index: 10;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transform: scale(0.8) translateY(20px);
  transition: all 0.8s cubic-bezier(0.22, 1, 0.36, 1);
}

.splash-content.splash-animate {
  opacity: 1;
  transform: scale(1) translateY(0);
}

.splash-logo-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

.modern-scan-effect::after {
  content: '';
  position: absolute;
  top: 0;
  left: -150%;
  width: 100%;
  height: 100%;
  background: linear-gradient(
    90deg,
    transparent 0%,
    rgba(255, 255, 255, 0.1) 30%,
    rgba(102, 126, 234, 0.8) 50%,
    rgba(255, 255, 255, 0.1) 70%,
    transparent 100%
  );
  transform: skewX(-25deg);
  animation: scanning-beam 3s ease-in-out infinite;
  pointer-events: none;
  mix-blend-mode: overlay;
  z-index: 20;
}

@keyframes scanning-beam {
  0% { left: -150%; opacity: 0; }
  10% { opacity: 1; }
  90% { opacity: 1; }
  100% { left: 150%; opacity: 0; }
}

.splash-logo-pulse-gear {
  position: absolute;
  font-size: 700px !important;
  color: rgba(102, 126, 234, 0.25);
  z-index: 1;
  animation: logo-pulse-heavy 2.5s ease-in-out infinite alternate, rotate-heavy-cw 40s linear infinite;
}

@keyframes logo-pulse-heavy {
  0% { transform: scale(0.85); opacity: 0.3; }
  100% { transform: scale(1.1); opacity: 0.6; }
}

.splash-logo-img {
  position: relative;
  z-index: 10;
  height: 320px;
  width: auto;
  object-fit: contain;
  filter: drop-shadow(0 0 35px rgba(102, 126, 234, 0.7));
  animation: splash-logo-float-slow 3s ease-in-out infinite alternate;
}

@keyframes splash-logo-float-slow {
  0% { transform: translateY(8px); }
  100% { transform: translateY(-8px); }
}

@keyframes rotate-heavy-cw {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@keyframes rotate-heavy-ccw {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(-360deg); }
}

.splash-particles-container {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.splash-particle {
  position: absolute;
  border-radius: 50%;
  background: rgba(102, 126, 234, 0.8);
  box-shadow: 0 0 10px rgba(102, 126, 234, 0.8);
  animation: splash-particle-burst var(--duration, 2s) ease-out infinite;
  animation-delay: var(--delay, 0s);
}

@keyframes splash-particle-burst {
  0% { transform: translate(0, 0) scale(1); opacity: 1; }
  100% { transform: translate(var(--tx), var(--ty)) scale(0); opacity: 0; }
}

.splash-fade-leave-active {
  transition: all 0.8s cubic-bezier(0.7, 0, 0.3, 1);
}

.splash-fade-leave-to {
  opacity: 0;
  transform: scale(1.15);
  filter: blur(10px);
}
</style>
