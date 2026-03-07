import { http } from "./http";
import type { ApiResponse } from "./types";

export type CreateTenantInvitationRequest = {
  email: string;
  role: string;
};

export type TenantInvitationDto = {
  invitationPublicId: string;
  email: string;
  role: string;
  status: string;
  createdAt: string;
  acceptedAt?: string | null;
  expiredAt: string;
  createdByUserEmail: string;
};

export async function createTenantInvitation(
  request: CreateTenantInvitationRequest
): Promise<boolean> {
  const response = await http.post<ApiResponse<boolean>>(
    "/api/tenant/invitations",
    request
  );

  const result = response.data;

  if (result.data === undefined || result.data === null) {
    throw new Error(result.message || "Failed to create invitation.");
  }

  return result.data;
}

export async function getTenantInvitations(): Promise<TenantInvitationDto[]> {
  const response = await http.get<ApiResponse<TenantInvitationDto[]>>(
    "/api/tenant/invitations"
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch invitations.");
  }

  return result.data;
}