import { publicHttp } from "../lib/http";
import type { ApiResponse } from "./types";

export type DemoTenantSeed = {
  tenantPublicId: string
  tenantName: string
  adminEmail: string
  adminPassword: string
  memberEmail: string
  memberPassword: string
  categoryCount: number
  productCount: number
  orderCount: number
}

export type DevSeedResult = {
  tenants: DemoTenantSeed[]
}

export async function seedDemoData(): Promise<DevSeedResult> {
  const response = await publicHttp.post<ApiResponse<DevSeedResult>>(
    "/api/dev/seed",
    {}
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Seed demo data failed");
  }

  return result.data;
}
