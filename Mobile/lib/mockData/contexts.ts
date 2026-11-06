// Pratik bağlamlarının UI metadata'sı. Backend'de context endpoint'i YOK — isim,
// açıklama, renk ve ikon tamamen client'a ait, bu yüzden burada duruyor.
// iconName string olarak tutulur; component'lerde getContextIcon ile bileşene map'lenir.
//
// Senaryo sayıları BURADA TUTULMAZ: scenariosCount/completedCount gerçek senaryo
// listesinden hesaplanır (bkz. buildContextProgress).

import type { Context } from '@/types/scenario';

export const contexts: Context[] = [
  {
    id: 'is',
    name: 'İş & Kariyer',
    description: 'Patron, mesai, müşteri baskısı',
    iconName: 'Briefcase',
    color: '#2D7FF9',
    softColor: '#E7F0FE',
    isFree: true,
  },
  {
    id: 'aile',
    name: 'Aile & Akraba',
    description: 'Anne baba, kayınvalide ricaları',
    iconName: 'UsersThree',
    color: '#FF7043',
    softColor: '#FFEAE2',
    isFree: false,
  },
  {
    id: 'arkadas',
    name: 'Arkadaş & Sosyal',
    description: 'Davetler, gruplar, sosyal baskı',
    iconName: 'SmileyWink',
    color: '#F7A93B',
    softColor: '#FEF1DF',
    isFree: false,
  },
  {
    id: 'romantik',
    name: 'Romantik İlişkiler',
    description: 'Partner, eski sevgililer',
    iconName: 'Heart',
    color: '#F25287',
    softColor: '#FDE6EE',
    isFree: false,
  },
  {
    id: 'satici',
    name: 'Satıcı & Hizmet',
    description: 'Mağaza, banka, usta, telefon teklifleri',
    iconName: 'ShoppingBag',
    color: '#8B6DF0',
    softColor: '#EDE7FD',
    isFree: false,
  },
  {
    id: 'komsu',
    name: 'Ev & Komşu',
    description: 'Ev arkadaşı, komşu, ev sahibi',
    iconName: 'HouseLine',
    color: '#14A39A',
    softColor: '#DFF5F3',
    isFree: false,
  },
  {
    id: 'okul',
    name: 'Okul & Kampüs',
    description: 'Grup ödevi, hoca, sınıf arkadaşı',
    iconName: 'GraduationCap',
    color: '#3DA35D',
    softColor: '#E4F5E8',
    isFree: false,
  },
  {
    id: 'dijital',
    name: 'Dijital & Sosyal Medya',
    description: 'Gruplar, mesajlar, paylaşım baskısı',
    iconName: 'DeviceMobile',
    color: '#0E9FC0',
    softColor: '#DFF3F8',
    isFree: false,
  },
];
