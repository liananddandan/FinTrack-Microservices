import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type {
  TenantContextDto,
  TenantRegistrationRequest,
  TenantRegistrationResult,
  ResolveTenantInvitationResult,
  CreateTenantInvitationRequest,
  TenantInvitationDto,
  TenantMemberDto,
  TenantMemberRole,
  TenantSummaryDto,
  TenantDomainMappingDto,
  CreateTenantDomainRequest,
  UpdateTenantDomainRequest,
} from "./types"

export function createTenantApi(params: {
  publicHttp: AxiosInstance
  accountHttp?: AxiosInstance
  tenantHttp?: AxiosInstance
  platformHttp?: AxiosInstance
}) {
  const { publicHttp, accountHttp, tenantHttp, platformHttp } = params

  function unwrapApiResponse<T>(
    response: { data: ApiResponse<T> },
    defaultMessage: string
  ): T {
    const result = response.data

    if (result.data == null) {
      throw new Error(result.message || defaultMessage)
    }

    return result.data
  }

  function requireAccountHttp(): AxiosInstance {
    if (!accountHttp) {
      throw new Error("accountHttp is not configured.")
    }

    return accountHttp
  }

  function requireTenantHttp(): AxiosInstance {
    if (!tenantHttp) {
      throw new Error("tenantHttp is not configured.")
    }

    return tenantHttp
  }

  function requirePlatformHttp(): AxiosInstance {
    if (!platformHttp) {
      throw new Error("platformHttp is not configured.")
    }

    return platformHttp
  }

  return {
    async getTenantContext(): Promise<TenantContextDto | null> {
      const response = await publicHttp.get<ApiResponse<TenantContextDto | null>>(
        "/api/tenant/context"
      )

      return response.data.data ?? null
    },

    async registerTenant(
      request: TenantRegistrationRequest
    ): Promise<TenantRegistrationResult> {
      const response = await publicHttp.post<ApiResponse<TenantRegistrationResult>>(
        "/api/tenant/register",
        request
      )

      return unwrapApiResponse(response, "Register tenant failed")
    },

    async resolveTenantInvitation(
      token: string
    ): Promise<ResolveTenantInvitationResult> {
      const response =
        await publicHttp.get<ApiResponse<ResolveTenantInvitationResult>>(
          "/api/tenant/invitations/resolve",
          {
            headers: {
              Authorization: `Invite ${token}`,
            },
          }
        )

      return unwrapApiResponse(response, "Failed to resolve invitation.")
    },

    async acceptTenantInvitation(token: string): Promise<boolean> {
      const response = await publicHttp.post<ApiResponse<boolean>>(
        "/api/tenant/invitations/accept",
        null,
        {
          headers: {
            Authorization: `Invite ${token}`,
          },
        }
      )

      return unwrapApiResponse(response, "Failed to accept invitation.")
    },

    async createTenantInvitation(
      request: CreateTenantInvitationRequest
    ): Promise<boolean> {
      const client = requireTenantHttp()
      const response = await client.post<ApiResponse<boolean>>(
        "/api/tenant/invitations",
        request
      )

      return unwrapApiResponse(response, "Failed to create invitation.")
    },

    async getTenantInvitations(): Promise<TenantInvitationDto[]> {
      const client = requireTenantHttp()
      const response = await client.get<ApiResponse<TenantInvitationDto[]>>(
        "/api/tenant/invitations"
      )

      return unwrapApiResponse(response, "Failed to fetch invitations.")
    },

    async resendTenantInvitation(
      invitationPublicId: string
    ): Promise<boolean> {
      const client = requireTenantHttp()
      const response = await client.post<ApiResponse<boolean>>(
        `/api/tenant/invitations/${invitationPublicId}/resend`
      )

      return unwrapApiResponse(response, "Failed to resend invitation.")
    },

    async getTenantMembers(): Promise<TenantMemberDto[]> {
      const client = requireTenantHttp()
      const response = await client.get<ApiResponse<TenantMemberDto[]>>(
        "/api/tenant/members"
      )

      return unwrapApiResponse(response, "Failed to fetch tenant members.")
    },

    async removeTenantMember(membershipPublicId: string): Promise<boolean> {
      const client = requireTenantHttp()
      const response = await client.delete<ApiResponse<boolean>>(
        `/api/tenant/members/${membershipPublicId}`
      )

      return unwrapApiResponse(response, "Failed to remove tenant member.")
    },

    async changeTenantMemberRole(
      membershipPublicId: string,
      role: TenantMemberRole
    ): Promise<boolean> {
      const client = requireTenantHttp()
      const response = await client.patch<ApiResponse<boolean>>(
        `/api/tenant/members/${membershipPublicId}/role`,
        { role }
      )

      return unwrapApiResponse(response, "Failed to change member role.")
    },

    async getCurrentTenantSettings(): Promise<unknown> {
      const client = requireTenantHttp()
      const response = await client.get<ApiResponse<unknown>>(
        "/api/tenant/settings"
      )

      return unwrapApiResponse(response, "Failed to fetch tenant settings")
    },

    async getMyTenantAccess(): Promise<unknown> {
      const client = requireAccountHttp()
      const response = await client.get<ApiResponse<unknown>>(
        "/api/account/tenant-access"
      )

      return unwrapApiResponse(response, "Failed to fetch tenant access")
    },
    async getPlatformTenants(): Promise<TenantSummaryDto[]> {
      const client = requirePlatformHttp()
      const response = await client.get<ApiResponse<TenantSummaryDto[]>>(
        "/api/platform/tenants"
      )

      return unwrapApiResponse(response, "Failed to fetch platform tenants.")
    },
    async getTenantDomains(
      tenantPublicId: string
    ): Promise<TenantDomainMappingDto[]> {
      const client = requirePlatformHttp()
      const response = await client.get<ApiResponse<TenantDomainMappingDto[]>>(
        `/api/platform/tenant-domains/by-tenant/${tenantPublicId}`
      )

      return unwrapApiResponse(response, "Failed to fetch tenant domains.")
    },

    async createTenantDomain(
      request: CreateTenantDomainRequest
    ): Promise<TenantDomainMappingDto> {
      const client = requirePlatformHttp()
      const response = await client.post<ApiResponse<TenantDomainMappingDto>>(
        "/api/platform/tenant-domains",
        request
      )

      return unwrapApiResponse(response, "Failed to create tenant domain.")
    },

    async updateTenantDomain(
      domainPublicId: string,
      request: UpdateTenantDomainRequest
    ): Promise<TenantDomainMappingDto> {
      const client = requirePlatformHttp()
      const response = await client.put<ApiResponse<TenantDomainMappingDto>>(
        `/api/platform/tenant-domains/${domainPublicId}`,
        request
      )

      return unwrapApiResponse(response, "Failed to update tenant domain.")
    },

    async setTenantDomainActive(
      domainPublicId: string,
      isActive: boolean
    ): Promise<boolean> {
      const client = requirePlatformHttp()
      const response = await client.patch<ApiResponse<boolean>>(
        `/api/platform/tenant-domains/${domainPublicId}/active`,
        { isActive }
      )

      return unwrapApiResponse(response, "Failed to update tenant domain status.")
    },

    async deleteTenantDomain(domainPublicId: string): Promise<boolean> {
      const client = requirePlatformHttp()
      const response = await client.delete<ApiResponse<boolean>>(
        `/api/platform/tenant-domains/${domainPublicId}`
      )

      return unwrapApiResponse(response, "Failed to delete tenant domain.")
    },
  }
}