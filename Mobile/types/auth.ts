// Auth domain tipleri. Backend (.NET 9 Clean Architecture, JWT + refresh token)
// şemasına yakın tutuldu; API bağlandığında bu tipler genişletilebilir.

export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  emailVerified: boolean;
  // İleride: avatar, roles, premiumStatus, vs.
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt?: number; // unix timestamp (ms)
}

export interface LoginResponse {
  user: User;
  tokens: AuthTokens;
}
