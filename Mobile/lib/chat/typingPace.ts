// Karakterin "yazıyor…" süresi: gerçek biri yazıyormuş hissi için mesaj uzunluğuna, karakter
// tipine ve o anki ruh haline göre. Saf fonksiyon — UI zamanlamasını sohbet ekranı kurar.
//
// Örnek: 60 karakterlik mesaj, nötr karakter ≈ 450 + 60×32 ≈ 2.4 sn. Agresif ısrarcı ve sinirli
// ise ≈ 1.3 sn (hızlı, art arda); duygusal kurban ve kırgın ise ≈ 3.2 sn'ye kadar (duraksayarak).

import type { ApiCharacterArchetype } from '@/types/api/scenario';
import type { ApiCharacterMood } from '@/types/api/session';

const BASE_MS = 450; // yazmaya başlama payı
const PER_CHAR_MS = 32; // ≈ 30 karakter/sn — telefonda hızlı biri
const MIN_MS = 700;
const MAX_MS = 3200;

// < 1 hızlı yazar, > 1 yavaş/duraksayarak.
const ARCHETYPE_PACE: Record<ApiCharacterArchetype, number> = {
  agresifIsrarci: 0.7,
  tatliDilliSatici: 0.8,
  otoriter: 0.9,
  babacanManipulator: 1,
  masumRicaci: 1.05,
  pasifAgresif: 1.25,
  duygusalKurban: 1.3,
};

const MOOD_PACE: Record<ApiCharacterMood, number> = {
  annoyed: 0.8,
  insistent: 0.9,
  calm: 1,
  yielding: 1.05,
  hurt: 1.25,
};

export function typingDelayMs(
  text: string,
  archetype?: ApiCharacterArchetype,
  mood?: ApiCharacterMood | null,
): number {
  const raw = BASE_MS + text.length * PER_CHAR_MS;
  const factor = (archetype ? ARCHETYPE_PACE[archetype] : 1) * (mood ? MOOD_PACE[mood] : 1);
  return Math.round(Math.min(MAX_MS, Math.max(MIN_MS, raw * factor)));
}
