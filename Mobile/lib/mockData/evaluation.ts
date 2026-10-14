// Pratik sonu değerlendirme mock'u. Sonuç ekranını besler.
// Şekil, ileride POST /api/sessions/{id}/evaluate'in döneceği 5 eksenli
// değerlendirmeye göre kuruldu; endpoint gelince bu dosya silinecek.

export type EvaluationAxis =
  | 'netlik'
  | 'nezaket'
  | 'dayaniklilik'
  | 'alternatifSunma'
  | 'ozurDengesi';

export interface SessionEvaluation {
  totalScore: number; // 0-100
  axes: Record<EvaluationAxis, number>; // her eksen 0-100
  strengths: string[];
  improvements: string[];
  coachNote: string;
  xpEarned: number;
  streakAfter: number;
}

export const EVALUATION_AXIS_LABELS: Record<EvaluationAxis, string> = {
  netlik: 'Netlik',
  nezaket: 'Nezaket',
  dayaniklilik: 'Dayanıklılık',
  alternatifSunma: 'Alternatif sunma',
  ozurDengesi: 'Özür dengesi',
};

export const mockEvaluation: SessionEvaluation = {
  totalScore: 82,
  axes: {
    netlik: 90,
    nezaket: 85,
    dayaniklilik: 78,
    alternatifSunma: 88,
    ozurDengesi: 70,
  },
  strengths: [
    'Net ve kısa bir "hayır" kurdun — bahaneye sığınmadın.',
    'İlişkiyi korurken sınırını belli ettin.',
  ],
  improvements: [
    'Övgüyle gelen baskıda biraz yumuşadın; kararını tek cümlede tekrarlayabilirsin.',
  ],
  coachNote:
    'Sınırını net koydun ve alternatif sundun — bu, ilişkiyi korurken "hayır" demenin en güçlü hali. Övgü baskısında kararını tek cümleyle tekrarlamayı dene.',
  xpEarned: 40,
  streakAfter: 4,
};
