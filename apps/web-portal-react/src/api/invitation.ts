import { publicHttp } from "../lib/http";
import type { ApiResponse } from "./types";

export type ResolveTenantInvitationResult = {
  invitationPublicId: string;
  tenantName: string;
  email: string;
  role: string;
  status: string;
  expiredAt: string;
};

export async function resolveTenantInvitation(
  token: string
): Promise<ResolveTenantInvitationResult> {
  const response = await publicHttp.get<ApiResponse<ResolveTenantInvitationResult>>(
    "/api/tenant/invitations/resolve",
    {
      headers: {
        Authorization: `Invite ${token}`,
      },
    }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to resolve invitation.");
  }

  return result.data;
}

export async function acceptTenantInvitation(token: string): Promise<boolean> {
  const response = await publicHttp.post<ApiResponse<boolean>>(
    "/api/tenant/invitations/accept",
    null,
    {
      headers: {
        Authorization: `Invite ${token}`,
      },
    }
  );

  const result = response.data;

  if (result.data === undefined || result.data === null) {
    throw new Error(result.message || "Failed to accept invitation.");
  }

  return result.data;
}