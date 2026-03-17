import { accountHttp, publicHttp } from "../lib/http"
import { authStore } from "../lib/authStore"
import type { ApiResponse } from "./types"

export type LoginMembershipDto = {
  tenantPublicId: string
  tenantName: string
  role: string
}

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterUserRequest = {
  userName: string
  email: string
  password: string
}

export type RegisterUserResult = {
  userPublicId: string
  email: string
  userName: string
}

export type UserLoginResult = {
  tokens: {
    accessToken: string
    refreshToken: string
  }
  memberships: LoginMembershipDto[]
}

export type CurrentUserResult = {
  userPublicId: string
  email: string
  userName?: string
  memberships?: LoginMembershipDto[]
}

export type SelectTenantRequest = {
  tenantPublicId: string
}

function ensureAccountToken() {
  const { accountAccessToken } = authStore.getState()

  if (!accountAccessToken) {
    throw new Error("Account access token is missing.")
  }
}

export async function login(request: LoginRequest): Promise<UserLoginResult> {
  const response = await publicHttp.post<ApiResponse<UserLoginResult>>(
    "/api/account/login",
    request
  )

  const result = response.data

  if (!result.data) {
    throw new Error(result.message || "Login failed")
  }

  return result.data
}

export async function registerUser(
  request: RegisterUserRequest
): Promise<RegisterUserResult> {
  const response = await publicHttp.post<ApiResponse<RegisterUserResult>>(
    "/api/account/register",
    request
  )

  const result = response.data

  if (!result.data) {
    throw new Error(result.message || "User registration failed")
  }

  return result.data
}

export async function getCurrentUser(): Promise<CurrentUserResult> {
  ensureAccountToken()

  const response = await accountHttp.get<ApiResponse<CurrentUserResult>>(
    "/api/account/me"
  )

  const result = response.data

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch current user")
  }

  return result.data
}

export async function selectTenant(
  request: SelectTenantRequest
): Promise<string> {
  ensureAccountToken()

  const response = await accountHttp.post<ApiResponse<string>>(
    "/api/account/select-tenant",
    request
  )

  const result = response.data

  if (!result.data) {
    throw new Error(result.message || "Failed to select tenant")
  }

  return result.data
}