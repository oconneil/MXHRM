export interface LoginRequest {
  userName: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userName: string;
  displayName: string;
  companyID: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}