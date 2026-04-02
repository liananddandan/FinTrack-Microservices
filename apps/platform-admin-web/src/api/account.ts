import { accountHttp, platformHttp, publicHttp } from "../lib/http"
import type { ApiResponse } from "./types"

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
  memberships: Array<{
    tenantPublicId: string
    tenantName: string
    role: string
  }>
}

export type UserProfile = {
  userPublicId: string
  email: string
  userName?: string
}

export type PlatformTokenDto = {
  platformAccessToken: string
  platformRole: string
}

export async function login(request: LoginRequest): Promise<UserLoginResult> {
  const response = await publicHttp.post<ApiResponse<UserLoginResult>>(
    "/api/account/login",
    request
  )

  return response.data.data
}

export async function getCurrentUser(): Promise<UserProfile> {
  const response = await accountHttp.get<ApiResponse<UserProfile>>("/api/account/me")
  return response.data.data
}

export async function selectPlatform(): Promise<PlatformTokenDto> {
  const response = await accountHttp.post<ApiResponse<PlatformTokenDto>>(
    "/api/account/select-platform"
  )

  return response.data.data
}

export async function getPlatformOverview(): Promise<{
  message: string
}> {
  const response = await platformHttp.get<ApiResponse<{ message: string }>>(
    "/api/platform/overview"
  )

  return response.data.data
}