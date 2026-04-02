import { accountHttp, publicHttp } from "../lib/http"
import type { ApiResponse } from "./types"
import type { UserProfile } from "../lib/authStore"

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

export async function registerTenant(request: RegisterTenantRequest) {
  const response = await publicHttp.post<ApiResponse<null>>(
    "/api/account/register-tenant",
    request
  )

  return response.data
}

export async function registerUser(request: RegisterUserRequest) {
  const response = await publicHttp.post<ApiResponse<null>>(
    "/api/account/register-user",
    request
  )

  return response.data
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

export async function selectTenant(): Promise<string> {
  const response = await accountHttp.post<ApiResponse<string>>(
    "/api/account/select-tenant",
    {}
  )

  return response.data.data
}