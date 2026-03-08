import { http } from "./http";
import type { ApiResponse } from "./types";

export type TenantMemberDto = {
  membershipPublicId: string
  userPublicId: string
  email: string
  userName?: string
  role: "Admin" | "Member"
  isActive: boolean
  joinedAt: string
}

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

export async function removeTenantMember(
  membershipPublicId: string
): Promise<boolean> {
  const response = await http.delete<ApiResponse<boolean>>(
    `/api/tenant/members/${membershipPublicId}`
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to remove tenant member.");
  }

  return result.data;
}

export async function changeTenantMemberRole(
  membershipPublicId: string,
  role: string
): Promise<boolean> {
  const response = await http.patch<ApiResponse<boolean>>(
    `/api/tenant/members/${membershipPublicId}/role`,
    { role }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to change member role.");
  }

  return result.data;
}