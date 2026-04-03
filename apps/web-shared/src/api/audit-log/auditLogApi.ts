import type { AxiosInstance } from "axios"
import type { ApiResponse, PagedResult } from "../types"
import type { AuditLogItem, AuditLogQuery } from "./types"

export function createAuditLogApi(tenantHttp: AxiosInstance) {
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

  return {
    async getAuditLogs(
      query: AuditLogQuery
    ): Promise<PagedResult<AuditLogItem>> {
      const response = await tenantHttp.get<ApiResponse<PagedResult<AuditLogItem>>>(
        "/api/audit-logs",
        { params: query }
      )

      return unwrapApiResponse(response, "Failed to fetch audit logs.")
    },
  }
}