<template>
  <!-- Giriş sayfasının sol paneli: mobil karşılama ekranının web karşılığı. Marka geçişli zemin,
       ortada "NO" balonu; karakterlerin ısrar balonları belirip geri itilir, arkadan halkalar
       yayılır, "dur" eli sallanır. Altta ürünün özü: ısrar → kibar bir "hayır". -->
  <div class="brand-hero">
    <div class="glow glow--top" />
    <div class="glow glow--bottom" />

    <div class="scene" aria-hidden="true">
      <div class="base-circle" />
      <div v-for="i in 3" :key="`ring-${i}`" class="ring" :style="{ animationDelay: `${(i - 1) * 1.2}s` }" />

      <div
        v-for="(chip, i) in chips"
        :key="chip.key"
        :class="['chip', `chip--${chip.tail}`]"
        :style="{
          ...chip.position,
          '--px': `${chip.push[0]}px`,
          '--py': `${chip.push[1]}px`,
          animationDelay: `${0.6 + i * 2.2}s`,
        }"
      >
        {{ t(`auth.${chip.key}`) }}
      </div>

      <div class="bubble-wrap">
        <div class="bubble"><span class="bubble-text">NO</span></div>
        <svg class="bubble-tail" width="46" height="34" viewBox="0 0 42 32">
          <path d="M0 0 H42 L11 31 Z" fill="#FFFFFF" />
        </svg>
      </div>

      <div class="hand-badge">
        <v-icon class="hand" size="30" color="#FF7043">mdi-hand-back-right</v-icon>
      </div>
    </div>

    <div class="copy">
      <div class="brand-badge">
        <img :src="logoSrc" alt="" class="brand-badge-logo" />
        <span>{{ t("auth.brandBadge") }}</span>
      </div>

      <div class="preview" aria-hidden="true">
        <div class="preview-bubble preview-bubble--ask">{{ t("auth.heroPreviewAsk") }}</div>
        <div class="preview-bubble preview-bubble--reply">
          <v-icon size="16" color="#FF7043">mdi-hand-back-right</v-icon>
          <span>{{ t("auth.heroPreviewReply") }}</span>
        </div>
      </div>

      <h2 class="title">{{ t("auth.heroTitle") }}</h2>
      <p class="subtitle">{{ t("auth.heroSubtitle") }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ logoSrc: string }>();

const { t } = useI18n();

// Balon konumları sahneye göre; push = geri itilirken merkezden dışarı kayma (px).
const chips = [
  { key: "heroPressure1", position: { top: "34px", left: "0" }, push: [-18, -8], tail: "right" },
  { key: "heroPressure2", position: { top: "170px", right: "0" }, push: [18, 6], tail: "left" },
  { key: "heroPressure3", position: { bottom: "30px", left: "8px" }, push: [-18, 8], tail: "right" },
] as const;
</script>

<style scoped>
.brand-hero {
  --bh-gradient: linear-gradient(135deg, #5b4be0 0%, #2d7ff9 100%);
  --bh-coral: #ff7043;
  --bh-ink: #16181d;

  position: relative;
  overflow: hidden;
  width: 100%;
  height: 100%;
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 56px 56px 48px;
  background: var(--bh-gradient);
  color: #fff;
}

.glow {
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
  filter: blur(10px);
}

.glow--top {
  top: -180px;
  left: -140px;
  width: 520px;
  height: 520px;
  background: radial-gradient(circle, rgba(255, 255, 255, 0.16), transparent 70%);
}

.glow--bottom {
  bottom: -220px;
  right: -160px;
  width: 560px;
  height: 560px;
  background: radial-gradient(circle, rgba(22, 24, 29, 0.22), transparent 70%);
}

/* ── Sahne ── */
.scene {
  position: relative;
  align-self: center;
  width: 400px;
  height: 340px;
  margin-top: 4vh;
  display: flex;
  align-items: center;
  justify-content: center;
}

.base-circle {
  position: absolute;
  width: 220px;
  height: 220px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.12);
}

.ring {
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  border: 2px solid rgba(255, 255, 255, 0.45);
  background: rgba(255, 255, 255, 0.06);
  opacity: 0;
  animation: ring 3.6s cubic-bezier(0.25, 0.6, 0.4, 1) infinite;
}

@keyframes ring {
  0% {
    opacity: 0.6;
    transform: scale(0.62);
  }
  100% {
    opacity: 0;
    transform: scale(1.25);
  }
}

.bubble-wrap {
  position: relative;
  width: 196px;
  animation: float 2.4s ease-in-out infinite alternate;
}

@keyframes float {
  to {
    transform: translateY(-7px);
  }
}

.bubble {
  height: 150px;
  border-radius: 54px;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 28px 50px rgba(36, 24, 140, 0.35);
}

.bubble-text {
  font-size: 76px;
  font-weight: 900;
  letter-spacing: -4px;
  line-height: 1;
  background: var(--bh-gradient);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.bubble-tail {
  position: absolute;
  bottom: -31px;
  left: 46px;
}

.hand-badge {
  position: absolute;
  top: 40px;
  right: 74px;
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 14px 28px rgba(36, 24, 140, 0.3);
}

.hand {
  animation: wave 3.4s ease-in-out 1.2s infinite;
  transform-origin: 50% 80%;
}

@keyframes wave {
  0%,
  100% {
    transform: rotate(0);
  }
  4% {
    transform: rotate(-14deg);
  }
  8% {
    transform: rotate(12deg);
  }
  12% {
    transform: rotate(-8deg);
  }
  16% {
    transform: rotate(0);
  }
}

.chip {
  position: absolute;
  z-index: 0;
  max-width: 170px;
  padding: 9px 14px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.96);
  color: #5a6472;
  font-size: 0.88rem;
  font-weight: 600;
  white-space: nowrap;
  box-shadow: 0 10px 22px rgba(36, 24, 140, 0.22);
  opacity: 0;
  animation: chip 6.6s ease-in-out infinite;
}

.chip--left {
  border-bottom-left-radius: 5px;
}

.chip--right {
  border-bottom-right-radius: 5px;
}

/* Belir (≈0.45 sn) → bekle (≈1.5 sn) → "NO"ya çarpıp itilerek kaybol (≈0.55 sn) → sıra bekle */
@keyframes chip {
  0% {
    opacity: 0;
    transform: translateY(8px);
  }
  7% {
    opacity: 1;
    transform: translate(0, 0) scale(1);
  }
  29% {
    opacity: 1;
    transform: translate(0, 0) scale(1);
  }
  37% {
    opacity: 0;
    transform: translate(var(--px), var(--py)) scale(0.86);
  }
  100% {
    opacity: 0;
    transform: translate(var(--px), var(--py)) scale(0.86);
  }
}

/* ── Metin ── */
.copy {
  position: relative;
  max-width: 480px;
}

.brand-badge {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  padding: 6px 16px 6px 6px;
  border-radius: 100px;
  background: rgba(255, 255, 255, 0.14);
  border: 1px solid rgba(255, 255, 255, 0.22);
  backdrop-filter: blur(12px);
  font-size: 0.82rem;
  font-weight: 600;
  letter-spacing: 0.3px;
  margin-bottom: 26px;
}

.brand-badge-logo {
  width: 26px;
  height: 26px;
  border-radius: 8px;
  display: block;
}

.preview {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 28px;
}

.preview-bubble {
  max-width: 82%;
  padding: 11px 16px;
  border-radius: 18px;
  font-size: 0.92rem;
  opacity: 0;
  animation: preview-in 0.5s cubic-bezier(0.22, 1, 0.36, 1) forwards;
}

.preview-bubble--ask {
  align-self: flex-start;
  border-bottom-left-radius: 5px;
  background: rgba(255, 255, 255, 0.16);
  border: 1px solid rgba(255, 255, 255, 0.24);
  color: rgba(255, 255, 255, 0.94);
  animation-delay: 0.3s;
}

.preview-bubble--reply {
  align-self: flex-end;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  border-bottom-right-radius: 5px;
  background: #fff;
  color: var(--bh-ink);
  font-weight: 700;
  box-shadow: 0 14px 30px rgba(20, 30, 50, 0.18);
  animation-delay: 0.9s;
}

@keyframes preview-in {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.title {
  font-size: 2.5rem;
  font-weight: 800;
  line-height: 1.1;
  letter-spacing: -1px;
  margin-bottom: 12px;
}

.subtitle {
  font-size: 1rem;
  line-height: 1.6;
  color: rgba(255, 255, 255, 0.8);
}

@media (max-height: 820px) {
  .scene {
    transform: scale(0.82);
    margin-top: 0;
  }
  .preview {
    display: none;
  }
}

/* Sistemde "hareketi azalt" açıksa sahne sabit: balonlar görünür, halkalar gizli. */
@media (prefers-reduced-motion: reduce) {
  .ring {
    display: none;
  }
  .bubble-wrap,
  .hand,
  .chip,
  .preview-bubble {
    animation: none;
  }
  .chip,
  .preview-bubble {
    opacity: 1;
  }
}
</style>
