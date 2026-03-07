import { http } from "./http";
import type { ApiResponse } from "./types";

export type TenantMemberDto = {
  userPublicId: string;
  email: string;
  userName: string;
  role: string;
  isActive: boolean;
  joinedAt: string;
};

export async function getTenantMembers(): Promise<TenantMemberDto[]> {
  const response = await http.get<ApiResponse<TenantMemberDto[]>>(
    "/api/tenant/members"
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch tenant members.");
  }

  return result.data;
}