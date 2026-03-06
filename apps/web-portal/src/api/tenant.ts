import { http } from "./http";

export type RegisterTenantRequest = {
  tenantName: string;
  adminName: string;
  adminEmail: string;
  adminPassword: string;
};

export async function registerTenant(request: RegisterTenantRequest) {
  const { data } = await http.post("/api/tenant/register", request);
  return data;
}