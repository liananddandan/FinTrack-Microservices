export type LoginMembershipDto = {
  tenantPublicId: string
  tenantName: string
  role: string
}

export type LoginRequest = {
  email: string
  password: string
}

export type JwtTokenPair = {
  accessToken: string
  refreshToken: string
}

export type UserLoginResult = {
  tokens: JwtTokenPair
  memberships: LoginMembershipDto[]
}

export type CurrentUserResult = {
  userPublicId: string
  email: string
  userName?: string
  memberships?: LoginMembershipDto[]
}

export type RegisterUserRequest = {
  userName: string
  email: string
  password: string
  fullName: string
}

export type RegisterTenantRequest = {
  tenantName: string
  ownerFullName: string
  email: string
  password: string
  confirmPassword: string
}

export type PlatformTokenDto = {
  platformAccessToken: string
  platformRole: string
}

export type VerifyEmailRequest = {
  token: string
}