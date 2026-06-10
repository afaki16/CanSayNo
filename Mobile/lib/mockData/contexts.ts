// Pratik bağlamları mock'u. Her bağlamın kendi rengi, soft tonu ve Phosphor ikon adı var.
// iconName string olarak tutulur; component'lerde getContextIcon ile bileşene map'lenir.

import type { Context } from '@/types/scenario';

export const mockContexts: Context[] = [
  {
    id: 'is',
    name: 'İş & Kariyer',
    description: 'Patron, mesai, müşteri baskısı',
    iconName: 'Briefcase',
    color: '#2A9D8F',
    softColor: '#E8F5F3',
    isFree: true,
    scenariosCount: 15,
    completedCount: 4,
  },
  {
    id: 'aile',
    name: 'Aile & Akraba',
    description: 'Anne baba, kayınvalide ricaları',
    iconName: 'UsersThree',
    color: '#F4A261',
    softColor: '#FDF0E5',
    isFree: false,
    scenariosCount: 10,
    completedCount: 0,
  },
  {
    id: 'arkadas',
    name: 'Arkadaş & Sosyal',
    description: 'Davetler, gruplar, sosyal baskı',
    iconName: 'SmileyWink',
    color: '#E9C46A',
    softColor: '#FCF5E0',
    isFree: false,
    scenariosCount: 9,
    completedCount: 0,
  },
  {
    id: 'romantik',
    name: 'Romantik İlişkiler',
    description: 'Partner, eski sevgililer',
    iconName: 'Heart',
    color: '#E76F51',
    softColor: '#FDECE8',
    isFree: false,
    scenariosCount: 8,
    completedCount: 0,
  },
  {
    id: 'satici',
    name: 'Satıcı & Pazarlama',
    description: 'Telefon, mağaza, MLM teklifleri',
    iconName: 'ShoppingBag',
    color: '#9D72E0',
    softColor: '#F0E8FC',
    isFree: false,
    scenariosCount: 9,
    completedCount: 0,
  },
];
