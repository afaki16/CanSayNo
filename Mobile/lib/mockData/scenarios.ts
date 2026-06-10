// Senaryo mock'u. Şimdilik sadece "İş & Kariyer" bağlamı detaylandırıldı (6 senaryo);
// diğer bağlamlar API/içerik genişledikçe eklenecek. Premium bağlamlarda senaryo
// olmaması, listede "boş state" (paywall yönlendirmesi) tetikler.

import type { ContextId, Scenario } from '@/types/scenario';

export const mockScenarios: Scenario[] = [
  {
    id: 'is_001',
    contextId: 'is',
    title: 'Cuma 18:00 Mesajı',
    shortDescription: 'Patronun hafta sonu acil iş bahanesiyle çalışma talebi',
    difficulty: 2,
    estimatedDurationMinutes: 5,
    scene: {
      setting: 'Cuma akşam 18:00, hafta sonu planın var',
      situation:
        'Telefonun titriyor. Patronundan WhatsApp mesajı geldi: "Müşteri toplantısı pazartesi sabah 9\'a alındı, sunumu revize etmeliyiz. Birkaç saatini ayırabilir misin?"',
      userGoal: 'Hafta sonu planını korumak — ama köprüleri yakmadan',
      hint: 'Övgüyle gelen taleplere karşı dikkatli ol; duygusal yatırım yaratır.',
    },
    character: {
      name: 'Murat Bey',
      age: 45,
      role: 'Orta düzey yönetici, doğrudan amirin',
      relationship: '2 yıldır birlikte çalışıyorsunuz, genelde iyi geçinirsiniz',
      archetype: 'babacan_manipulator',
    },
    isCompleted: true,
    bestScore: 82,
    attemptsCount: 3,
    tags: ['mesai', 'patron', 'haftasonu'],
  },
  {
    id: 'is_002',
    contextId: 'is',
    title: '"Sadece bir saat" toplantısı',
    shortDescription: 'Toplantının 3 saate uzaması, çıkmak istiyorsun',
    difficulty: 2,
    estimatedDurationMinutes: 4,
    scene: {
      setting: 'Salı öğleden sonra, "1 saatlik" planlanan ekip toplantısı',
      situation:
        '"Sadece bir saat" denilen toplantı 2.5 saati geçti. Senin başka önemli işlerin bekliyor. Yönetici toplantıyı uzatmaya devam ediyor: "5 dakika daha lütfen, şu konuyu da çözelim."',
      userGoal: 'Toplantıdan saygıyla ayrılmak',
      hint: 'Yapacağın işin önemini hatırlat, suçluluk hissetme.',
    },
    character: {
      name: 'Ayşe Hanım',
      age: 38,
      role: 'Proje yöneticisi',
      relationship: 'Aynı kıdemdesiniz, profesyonel ilişki',
      archetype: 'masum_ricaci',
    },
    isCompleted: true,
    bestScore: 65,
    attemptsCount: 2,
    tags: ['toplanti', 'zaman', 'sinirlar'],
  },
  {
    id: 'is_003',
    contextId: 'is',
    title: 'Yıllık izin iptal talebi',
    shortDescription: 'Onaylanan izninin son anda iptal edilmek istenmesi',
    difficulty: 3,
    estimatedDurationMinutes: 6,
    scene: {
      setting: 'Pazartesi sabahı, 2 hafta sonra başlayacak yıllık izninin var',
      situation:
        'Müdürün ofisine çağırıldın. "Önemli bir proje çıktı, izni 2 ay sonraya erteleyebilir misin? Sana güveniyorum, sen olmadan olmaz."',
      userGoal: 'Önceden planlanmış izninizi korumak',
      hint: 'İzin hakkındır, lütuf değil. Yine de profesyonel kal.',
    },
    character: {
      name: 'Kemal Bey',
      age: 52,
      role: 'Departman müdürü',
      relationship: 'Bir yıldır birlikte çalışıyorsunuz, kıdemli',
      archetype: 'otoriter',
    },
    isCompleted: true,
    bestScore: 91,
    attemptsCount: 1,
    tags: ['izin', 'hak', 'otorite'],
  },
  {
    id: 'is_004',
    contextId: 'is',
    title: '"Sen yaparsın" tuzağı',
    shortDescription: 'Kendi işin değilken yardım istenmesi',
    difficulty: 3,
    estimatedDurationMinutes: 5,
    scene: {
      setting: 'Çarşamba öğleden sonra, kendi işlerin yığılmış durumda',
      situation:
        'Bir iş arkadaşın masana geldi: "Şu raporu sen yaparsan çok daha iyi olur, sen bu konuyu benden iyi biliyorsun. Sadece 1-2 saatlik iş." Aslında 4-5 saatlik iş ve onun sorumluluğu.',
      userGoal: 'Kendi önceliklerini korumak, görev devrini kabul etmemek',
      hint: 'Övgüyü kabul et, ama görevi reddet.',
    },
    character: {
      name: 'Selin',
      age: 29,
      role: 'Aynı seviyede iş arkadaşı',
      relationship: 'Arkadaşça, ama bu tür "ricalar" sık geliyor',
      archetype: 'tatli_dilli_satici',
    },
    isCompleted: false,
    bestScore: null,
    attemptsCount: 0,
    tags: ['gorev_devri', 'meslektaslar'],
  },
  {
    id: 'is_005',
    contextId: 'is',
    title: 'Maaş zammı görüşmesini erteleme',
    shortDescription: 'Yıllık değerlendirmede zam talebini erteleme baskısı',
    difficulty: 4,
    estimatedDurationMinutes: 7,
    scene: {
      setting: 'Yıllık performans değerlendirme görüşmesi',
      situation:
        'Müdürün senin performansını övüyor ama zam konusunda diyor ki: "Şirket şu an zor durumda, 6 ay daha sabret. Sana söz veriyorum, sonra her şeyi telafi edeceğiz." Halbuki son 2 yıldır benzer sözler veriliyor.',
      userGoal: 'Zammı kararlı bir şekilde talep etmek, ertelemeyi kabul etmemek',
      hint: 'Geçmiş vaatleri hatırlat, somut tarih iste.',
    },
    character: {
      name: 'Tarık Bey',
      age: 48,
      role: 'Doğrudan amirin',
      relationship: '3 yıldır birlikte çalışıyorsunuz',
      archetype: 'babacan_manipulator',
    },
    isCompleted: false,
    bestScore: null,
    attemptsCount: 0,
    tags: ['maas', 'kariyer', 'ertelemek'],
  },
  {
    id: 'is_006',
    contextId: 'is',
    title: 'Cuma akşamı şirket etkinliği',
    shortDescription: 'Zorunlu hissettirilen sosyal etkinlik baskısı',
    difficulty: 1,
    estimatedDurationMinutes: 3,
    scene: {
      setting: "Cuma sabahı, akşam şirket happy hour'u var",
      situation:
        "İK'dan duyuru: \"Bu akşam 18:00'da tüm ekip katılımı zorunlu, takım ruhu için önemli!\" Senin başka planın var ve \"zorunlu\" denilen şey aslında resmi olarak zorunlu değil.",
      userGoal: 'Profesyonel kalarak katılmadığını bildirmek',
      hint: 'Mazerete gerek yok; basit ve net olabilirsin.',
    },
    character: {
      name: 'Esra Hanım',
      age: 35,
      role: 'İK uzmanı',
      relationship: 'Resmi mesafeli ilişki',
      archetype: 'otoriter',
    },
    isCompleted: false,
    bestScore: null,
    attemptsCount: 0,
    tags: ['sosyal', 'sirket', 'zorunluluk'],
  },
];

// Helper: bağlam id'sine göre senaryoları filtrele
export function getScenariosByContext(contextId: ContextId): Scenario[] {
  return mockScenarios.filter((s) => s.contextId === contextId);
}

// Helper: senaryo id'sine göre tek senaryo getir
export function getScenarioById(id: string): Scenario | undefined {
  return mockScenarios.find((s) => s.id === id);
}
