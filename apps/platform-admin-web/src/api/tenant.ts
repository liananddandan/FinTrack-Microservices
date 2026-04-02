import { platformHttp } from "../lib/http"
import type { ApiResponse } from "./types"

export type TenantSummaryDto = {
  tenantPublicId: string
  tenantName: string
  isActive: boolean
  createdAt: string
}

export async function getPlatformTenants(): Promise<TenantSummaryDto[]> {
  const response = await platformHttp.get<ApiResponse<TenantSummaryDto[]>>(
    "/api/platform/tenants"
  )

  return response.data.data ?? []
}