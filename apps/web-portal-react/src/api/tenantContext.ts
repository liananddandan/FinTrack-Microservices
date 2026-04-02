import { publicHttp } from "../lib/http"
import type { ApiResponse } from "./types"

export type TenantContextDto = {
  tenantPublicId: string
  tenantName: string
  host: string
  isActive: boolean
}

export async function getTenantContext(): Promise<TenantContextDto | null> {
  const response = await publicHttp.get<ApiResponse<TenantContextDto | null>>(
    "/api/tenant/context"
  )

  return response.data.data ?? null
}