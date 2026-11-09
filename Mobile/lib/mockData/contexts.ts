// Pratik bağlamlarının UI metadata'sı. Backend'de context endpoint'i YOK — isim,
// açıklama, renk ve ikon tamamen client'a ait, bu yüzden burada duruyor.
// iconName string olarak tutulur; component'lerde getContextIcon ile bileşene map'lenir.
//
// İsim ve açıklama çeviri dosyalarında (locales/<dil>/contexts.ts, anahtar = bağlam id'si);
// burada yalnızca dilden bağımsız alanlar durur. Metinler çağrı anındaki dille çözülür:
// bileşenlerde useContexts(), bileşen dışında getContexts() / getContextName().
//
// Senaryo sayıları BURADA TUTULMAZ: scenariosCount/completedCount gerçek senaryo
// listesinden hesaplanır (bkz. buildContextProgress).

import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';

import { t } from '@/lib/i18n';
import type { Context, ContextId } from '@/types/scenario';

type ContextMeta = Omit<Context, 'name' | 'description'>;

const CONTEXT_META: readonly ContextMeta[] = [
  { id: 'is', iconName: 'Briefcase', color: '#2D7FF9', softColor: '#E7F0FE', isFree: true },
  { id: 'aile', iconName: 'UsersThree', color: '#FF7043', softColor: '#FFEAE2', isFree: false },
  { id: 'arkadas', iconName: 'SmileyWink', color: '#F7A93B', softColor: '#FEF1DF', isFree: false },
  { id: 'romantik', iconName: 'Heart', color: '#F25287', softColor: '#FDE6EE', isFree: false },
  { id: 'satici', iconName: 'ShoppingBag', color: '#8B6DF0', softColor: '#EDE7FD', isFree: false },
  { id: 'komsu', iconName: 'HouseLine', color: '#14A39A', softColor: '#DFF5F3', isFree: false },
  { id: 'okul', iconName: 'GraduationCap', color: '#3DA35D', softColor: '#E4F5E8', isFree: false },
  { id: 'dijital', iconName: 'DeviceMobile', color: '#0E9FC0', softColor: '#DFF3F8', isFree: false },
];

export function getContextName(id: ContextId): string {
  return t(`contexts.${id}.name`);
}

export function getContextDescription(id: ContextId): string {
  return t(`contexts.${id}.description`);
}

/** Bağlamlar, isim/açıklama şu anki dilde. */
export function getContexts(): Context[] {
  return CONTEXT_META.map((meta) => ({
    ...meta,
    name: getContextName(meta.id),
    description: getContextDescription(meta.id),
  }));
}

/** Bileşenler için: dil değişince yeniden hesaplanır. */
export function useContexts(): Context[] {
  const { i18n } = useTranslation();
  const language = i18n.language;
  // eslint-disable-next-line react-hooks/exhaustive-deps -- dil değişince metinler yeniden çözülür
  return useMemo(() => getContexts(), [language]);
}
