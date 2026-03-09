import { accountHttp, publicHttp } from "./http";
import type { ApiResponse } from "./types";

export type LoginMembershipDto = {
  tenantPublicId: string;
  tenantName: string;
  role: string;
};

export type LoginRequest = {
  email: string;
  password: string;
};

export type UserLoginResult = {
  tokens: {
    accessToken: string;
    refreshToken: string;
  };
  memberships: LoginMembershipDto[];
};

export type CurrentUserResult = {
  userPublicId: string;
  email: string;
  userName?: string;
  memberships?: LoginMembershipDto[];
};

export type SelectTenantRequest = {
  tenantPublicId: string;
};

export async function login(request: LoginRequest): Promise<UserLoginResult> {
  const response = await publicHttp.post<ApiResponse<UserLoginResult>>(
    "/api/account/login",
    request
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Login failed");
  }

  return result.data;
}

export async function getCurrentUser(): Promise<CurrentUserResult> {
  const response = await accountHttp.get<ApiResponse<CurrentUserResult>>(
    "/api/account/me"
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch current user");
  }

  return result.data;
}

export async function selectTenant(
  request: SelectTenantRequest
): Promise<string> {
  const response = await accountHttp.post<ApiResponse<string>>(
    "/api/account/select-tenant",
    request
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to select tenant");
  }

  return result.data;
}