// Mock AI cevap script'leri. Backend yokken pratik sahnesindeki AI rolü buradan beslenir.
// Kullanıcı ne yazarsa yazsın AI, senaryonun script'indeki cevapları sırayla döndürür.
// Backend bağlanınca getAIResponseScript yerini gerçek AI API çağrısına bırakacak.

export interface AIResponseScript {
  scenarioId: string;
  // Sohbetin açılış mesajı — practice ekranında ilk AI balonu olarak çıkar.
  openingMessage: string;
  // Kullanıcının her cevabından sonra AI'ın sırayla döndüreceği yanıtlar.
  responses: string[];
  // Tüm raunt'lar bitince gösterilen kapanış mesajı.
  closingMessage: string;
}

export const mockAIResponses: AIResponseScript[] = [
  {
    scenarioId: 'is_001_cuma_mesaji',
    openingMessage:
      'Selam, müşteri toplantısı pazartesi sabah 9\'a alındı, tahmin etmiyorduk biliyorum. Sunumun analiz kısmını revize etmemiz lazım, en geç pazar akşamı. Sana çok güveniyorum bu konuda — kimseyi senin kadar iyi tanımıyor bu işi. Bu hafta sonu birkaç saatini ayırabilir misin?',
    responses: [
      // Raunt 1 (övgü + güven beyanı)
      'Anlıyorum yorgun olduğunu, biliyorum bu hafta da yoğundu. Ama sen olmadan bu işin altından kalkamayız. Ben de hafta sonu çalışacağım, beraber halletsek? Sadece birkaç saat...',
      // Raunt 2 (suçluluk yükleme)
      'Bak, ben her zaman senin yanında oldum. Geçen ay izin günü istediğinde de problem çıkarmadık. Bu sefer benim için yapsan? Şirketin geleceği bu toplantıya bağlı.',
      // Raunt 3 (karşı teklif / pasif baskı)
      'Tamam, anladım kararlı olduğunu. O zaman pazartesi sabah erken gelir misin? 7:30\'da burada olalım, son düzenlemeleri beraber yapalım. Bu kadarını yapabilirsin değil mi?',
    ],
    closingMessage:
      'Tamam, anladım. Başka bir çözüm bulurum o zaman. İyi hafta sonları sana.',
  },
  {
    scenarioId: 'is_002_toplanti_uzatma',
    openingMessage:
      'Sadece 5 dakika daha, son bir konuyu da çözelim. Yarın için bunu hazırlamamız gerek, ekipte sadece sen var bu kadar derinlemesine bilen.',
    responses: [
      'Anlıyorum işin var ama bu çok önemli. Ne kadar kalabilirsen kal, gerisini sonra hallederiz.',
      'Tamam, peki son 10 dakika daha verir misin? Sadece şu kararı alalım toplantı tamamen boşa gitmesin.',
      'Hı, peki. O zaman bu kararı yarın e-postayla nasıl alacağımıza karar verelim hızlıca, 2 dakika sürer.',
    ],
    closingMessage: 'Peki, anlıyorum. O zaman yarın devam ederiz. Görüşürüz.',
  },
  {
    scenarioId: 'is_003_izin_iptal',
    openingMessage:
      'Önemli bir konuyu konuşmamız lazım. Şu ana kadar yaptığın işten çok memnunum, biliyorsun. Ama bu yeni proje çıktı, müşteri çok ısrar etti ve sadece sen bu detayı bilen kişisin. Yıllık iznini 2 ay erteleyebilir misin?',
    responses: [
      'Sana güveniyorum bu konuda. Biliyorum, izin senin hakkın. Ama bu projeyi sen olmadan kaybedeceğiz. Şirket için lütfen bir düşün.',
      'Tamam, anladım kararlı olduğunu. Yine de bir kompromise gidebilir miyiz? İlk hafta uzaktan birkaç saat çalışsan yeter, sonra rahatça izine devam.',
      'Hı, anlıyorum. Bunu bir not olarak düşeyim. Performans değerlendirme zamanı geldiğinde bu konuyu hatırlamak isterim.',
    ],
    closingMessage: 'Peki. O zaman başka bir yol bulacağım. İyi izinler diliyorum.',
  },
  {
    scenarioId: 'is_004_sen_yaparsin',
    openingMessage:
      'Selam! Çok zor durumdayım, şu raporu sen yapsan çok iyi olur. Sen bu konuyu benden iyi biliyorsun ya, 1-2 saatlik iş aslında. Yardım eder misin?',
    responses: [
      'Yapma şimdi, sen yaparsan 1 saatte biter, ben uğraşırım 4 saat. Lütfen, sen yap, hayatımı kurtarırsın!',
      'Tamam anladım, sen de yoğunsun. Peki ya başlangıcına yardım etsen? Yapıyı kurup bana versen, gerisini ben hallederim?',
      'Hı, peki. O zaman başka birine sorayım. Belki Ahmet yapar, ama senin yaptığın gibi olmaz tabii.',
    ],
    closingMessage: 'Peki, anlıyorum. Ben hallederim, üzme kafanı.',
  },
  {
    scenarioId: 'is_005_maas_erteleme',
    openingMessage:
      'Performans değerlendirmesi konusunda çok memnunum senin işinden. Gerçekten harika bir yıldı. Ama şirket şu an zor durumda, biliyorsun. Zam konusunu 6 ay daha erteleyebilir miyiz? Söz veriyorum, sonra her şeyi telafi edeceğim.',
    responses: [
      'Anlıyorum endişeni. Ama bu sadece şirketin değil, hepimizin durumu. Birlikte bu zorluğu aşacağız. Senin gibi sadık çalışanları unutmuyorum.',
      'Tamam, anlıyorum kararlı olduğunu. Belki tam zam değil de küçük bir avans verebiliriz? 6 ay sonra gerçek zam, ama şimdi bir miktar destek?',
      'Hı, peki. Bu konuyu yönetim kuruluna götüreceğim. Ama sana söz veremem, kararı onlar verir.',
    ],
    closingMessage: 'Peki, anlıyorum endişeni. Ne diyebilirim ki başka, görüşürüz.',
  },
  {
    scenarioId: 'is_006_sirket_etkinligi',
    openingMessage:
      'Merhaba, bu akşam 18:00\'da ekip happy hour\'u olacak. Tüm ekibin katılımı zorunlu, takım ruhu için önemli. Görüşürüz!',
    responses: [
      'Anladım, ama gerçekten önemli bir etkinlik. Belki bir saat uğrayabilirsin?',
      'Tamam, anlıyorum. Bir dahaki sefere katılırsın umarım.',
    ],
    closingMessage: 'İyi akşamlar o zaman, görüşürüz.',
  },
];

// Helper: senaryo id'sine göre AI script'i getir. Yoksa undefined döner (fallback tetikler).
export function getAIResponseScript(
  scenarioId: string,
): AIResponseScript | undefined {
  return mockAIResponses.find((s) => s.scenarioId === scenarioId);
}
