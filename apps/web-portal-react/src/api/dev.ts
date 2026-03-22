import { publicHttp } from "../lib/http";
import type { ApiResponse } from "./types";

export type DevSeedResult = {
  tenantPublicId: string;
  tenantName: string;
  adminEmail: string;
  adminPassword: string;
  memberEmail: string;
  memberPassword: string;
  donationCount: number;
  procurementCount: number;
};

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
