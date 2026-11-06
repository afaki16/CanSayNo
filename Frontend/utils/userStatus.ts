// Kullanıcı durumu API'den camelCase metin gelir ("active"); eski kod sayı (1) bekliyordu.
// İkisini de kabul edip backend'in sayısal değerine çevirir (Domain UserStatus).

import { UserStatus } from '~/types/user'

const BY_NAME: Record<string, UserStatus> = {
  inactive: UserStatus.Inactive,
  active: UserStatus.Active,
  banned: UserStatus.Banned,
  pendingVerification: UserStatus.PendingVerification,
}

export const toUserStatusNumber = (status: unknown): UserStatus => {
  if (typeof status === 'number') return status as UserStatus
  if (typeof status === 'string') {
    if (status in BY_NAME) return BY_NAME[status]
    const n = Number(status)
    if (Number.isInteger(n)) return n as UserStatus
  }
  return UserStatus.Inactive
}

export const isUserActive = (status: unknown) => toUserStatusNumber(status) === UserStatus.Active
