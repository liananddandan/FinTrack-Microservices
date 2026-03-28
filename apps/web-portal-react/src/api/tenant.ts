import { publicHttp } from "../lib/http";
import type { ApiResponse } from "./types";

export type RegisterTenantRequest = {
  tenantName: string;
  adminName: string;
  adminEmail: string;
  adminPassword: string;
};

export type RegisterTenantResult = {
  tenantPublicId: string;
  userPublicId: string;
  adminEmail: string;
};

export async function registerTenant(
  request: RegisterTenantRequest
): Promise<RegisterTenantResult> {
  const response = await publicHttp.post<ApiResponse<RegisterTenantResult>>(
    "/api/tenant/register",
    request
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Register tenant failed");
  }

  return result.data;
}