/**
 * Backend ErrorCode enum mirror.
 * Source: Backend/Kickstart.Domain/Common/Enums/ErrorCode.cs
 */
export const ErrorCode = {
  EmailNotConfirmed: 1013,
} as const

export type ErrorCodeValue = typeof ErrorCode[keyof typeof ErrorCode]
