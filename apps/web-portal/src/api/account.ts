import { http } from "./http"
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

export type LoginResult = {
  accessToken: string
  refreshToken: string
  memberships: LoginMembershipDto[]
}

export type CurrentUserResult = {
  userPublicId: string
  email: string
  userName?: string
  memberships?: LoginMembershipDto[]
}

export async function login(request: LoginRequest): Promise<LoginResult> {
  const response = await http.post<ApiResponse<LoginResult>>(
    "/api/account/login",
    request
  )

  const result = response.data

  if (!result.data) {
    throw new Error(result.message || "Login failed")
  }

  return result.data
}

export async function getCurrentUser(): Promise<CurrentUserResult> {
  const response = await http.get<ApiResponse<CurrentUserResult>>(
    "/api/account/me"
  )

  const result = response.data

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch current user")
  }

  return result.data
}