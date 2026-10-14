// Son pratik aktiviteleri mock'u. Home ekranında "Son Pratiklerin" listesini besler.

export interface RecentActivity {
  id: string;
  scenarioId: string;
  scenarioTitle: string;
  contextId: string;
  contextName: string;
  contextColor: string;
  score: number;
  completedAt: string;
}

export const mockRecentActivities: RecentActivity[] = [
  {
    id: '1',
    scenarioId: 'is_001_cuma_mesaji',
    scenarioTitle: 'Cuma 18:00 Mesajı',
    contextId: 'is',
    contextName: 'İş & Kariyer',
    contextColor: '#2D7FF9',
    score: 82,
    completedAt: '2024-01-15T18:30:00Z',
  },
  {
    id: '2',
    scenarioId: 'is_002_toplanti_uzatma',
    scenarioTitle: '"Sadece bir saat" toplantısı',
    contextId: 'is',
    contextName: 'İş & Kariyer',
    contextColor: '#2D7FF9',
    score: 65,
    completedAt: '2024-01-14T20:15:00Z',
  },
  {
    id: '3',
    scenarioId: 'is_003_izin_iptal',
    scenarioTitle: 'Yıllık izin iptal talebi',
    contextId: 'is',
    contextName: 'İş & Kariyer',
    contextColor: '#2D7FF9',
    score: 91,
    completedAt: '2024-01-13T14:20:00Z',
  },
];
