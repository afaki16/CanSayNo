// Hayırcı enum'larının panelde görünen Türkçe adları ve renkleri.
// Bağlam adları/renkleri mobildeki lib/mockData/contexts.ts ile aynı tutulur.

import type {
  CharacterArchetype,
  ContextId,
  PressureTactic,
  SessionStatus,
  SubscriptionStatus,
} from '~/types/hayirci'

export const CONTEXTS: { value: ContextId; title: string; color: string; idPrefix: string }[] = [
  { value: 'is', title: 'İş & Kariyer', color: '#2D7FF9', idPrefix: 'is' },
  { value: 'aile', title: 'Aile & Akraba', color: '#FF7043', idPrefix: 'aile' },
  { value: 'arkadas', title: 'Arkadaş & Sosyal', color: '#F7A93B', idPrefix: 'arkadas' },
  { value: 'romantik', title: 'Romantik İlişkiler', color: '#F25287', idPrefix: 'romantik' },
  { value: 'satici', title: 'Satıcı & Hizmet', color: '#8B6DF0', idPrefix: 'satici' },
  { value: 'komsu', title: 'Ev & Komşu', color: '#14A39A', idPrefix: 'komsu' },
  { value: 'okul', title: 'Okul & Kampüs', color: '#3DA35D', idPrefix: 'okul' },
  { value: 'dijital', title: 'Dijital & Sosyal Medya', color: '#0E9FC0', idPrefix: 'dijital' },
]

export const contextOf = (id: ContextId) => CONTEXTS.find((c) => c.value === id) ?? CONTEXTS[0]

export const ARCHETYPES: { value: CharacterArchetype; title: string }[] = [
  { value: 'babacanManipulator', title: 'Babacan manipülatör' },
  { value: 'duygusalKurban', title: 'Duygusal kurban' },
  { value: 'agresifIsrarci', title: 'Agresif ısrarcı' },
  { value: 'tatliDilliSatici', title: 'Tatlı dilli satıcı' },
  { value: 'masumRicaci', title: 'Masum ricacı' },
  { value: 'pasifAgresif', title: 'Pasif agresif' },
  { value: 'otoriter', title: 'Otoriter' },
]

export const TACTICS: { value: PressureTactic; title: string }[] = [
  { value: 'ovguVeGuven', title: 'Övgü ve güven' },
  { value: 'suclulukYukleme', title: 'Suçluluk yükleme' },
  { value: 'acilDurumYaratma', title: 'Acil durum yaratma' },
  { value: 'sosyalKanit', title: 'Sosyal kanıt ("herkes yapıyor")' },
  { value: 'otorite', title: 'Otorite' },
  { value: 'kiyaslama', title: 'Kıyaslama' },
  { value: 'karsiTeklif', title: 'Karşı teklif / pazarlık' },
  { value: 'pasifTehdit', title: 'Pasif tehdit' },
  { value: 'sahteAnlayis', title: 'Sahte anlayış ("anlıyorum ama")' },
  { value: 'duyguSomurusu', title: 'Duygu sömürüsü' },
  { value: 'kisKisSessizlik', title: 'Küskünlük / sessizlik' },
  { value: 'asiriOzurAlan', title: 'Aşırı özür bekleme' },
  { value: 'sahteZamanBaskisi', title: 'Sahte zaman baskısı' },
]

export const tacticTitle = (t: PressureTactic) => TACTICS.find((x) => x.value === t)?.title ?? t

export const DIFFICULTY_LABELS: Record<number, string> = {
  1: '1 · Çok kolay',
  2: '2 · Kolay',
  3: '3 · Orta',
  4: '4 · Zor',
  5: '5 · Çok zor',
}

export const SUBSCRIPTION_STATUS_LABELS: Record<SubscriptionStatus, string> = {
  active: 'Aktif',
  cancelled: 'İptal edildi (dönem sonuna kadar geçerli)',
  gracePeriod: 'Ödeme sorunu (ek süre)',
  expired: 'Süresi doldu',
  pending: 'Beklemede',
}

export const SESSION_STATUS_LABELS: Record<SessionStatus, string> = {
  active: 'Devam ediyor',
  completed: 'Tamamlandı',
  abandoned: 'Yarım bırakıldı',
}

export const BADGE_LABELS: Record<string, string> = {
  firstNo: 'İlk Hayır',
  threeDayStreak: '3 gün seri',
  sevenDayStreak: '7 gün seri',
  thirtyDayStreak: '30 gün seri',
  workMaster: 'İş ustası',
  perfectScore: 'Tam puan',
  nightOwl: 'Gece kuşu',
  earlyBird: 'Erkenci',
  persistent: 'Azimli (50 pratik)',
  contextExplorer: 'Kaşif (5 farklı bağlam)',
}
