import { http } from "./http";
import type { ApiResponse } from "./types";

export type CreateTenantInvitationRequest = {
  email: string;
  role: string;
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