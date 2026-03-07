import axios from "axios";
import { useAuthStore } from "../stores/auth";
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

function getAccountAuthHeader() {
  const auth = useAuthStore();

  return {
    Authorization: `Bearer ${auth.accountAccessToken}`,
  };
}

export async function login(request: LoginRequest): Promise<UserLoginResult> {
  const response = await axios.post<ApiResponse<UserLoginResult>>(
    `${import.meta.env.VITE_API_BASE_URL}/api/account/login`,
    request,
    {
      timeout: 15000,
    }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Login failed");
  }

  return result.data;
}

export async function getCurrentUser(): Promise<CurrentUserResult> {
  const response = await axios.get<ApiResponse<CurrentUserResult>>(
    `${import.meta.env.VITE_API_BASE_URL}/api/account/me`,
    {
      headers: getAccountAuthHeader(),
      timeout: 15000,
    }
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
  const response = await axios.post<ApiResponse<string>>(
    `${import.meta.env.VITE_API_BASE_URL}/api/account/select-tenant`,
    request,
    {
      headers: getAccountAuthHeader(),
      timeout: 15000,
    }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to select tenant");
  }

  return result.data;
}