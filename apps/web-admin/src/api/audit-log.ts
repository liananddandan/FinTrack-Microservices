import { http } from "./http";
import type { ApiResponse } from "./types";

export type AuditLogItem = {
  publicId: string;
  tenantPublicId: string;
  actorUserPublicId?: string;
  actorDisplayName?: string;
  actionType: string;
  category: string;
  targetType?: string;
  targetPublicId?: string;
  targetDisplay?: string;
  source?: string;
  correlationId?: string;
  occurredAtUtc: string;
  metadataJson: string;
  summary: string;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type AuditLogQuery = {
  actionType?: string;
  actorUserPublicId?: string;
  targetPublicId?: string;
  fromUtc?: string;
  toUtc?: string;
  pageNumber?: number;
  pageSize?: number;
};

export async function getAuditLogs(
  query: AuditLogQuery
): Promise<PagedResult<AuditLogItem>> {
  const response = await http.get<ApiResponse<PagedResult<AuditLogItem>>>(
    "/api/audit-logs",
    { params: query }
  );

  const result = response.data;

  if (!result.data) {
    throw new Error(result.message || "Failed to fetch audit logs.");
  }

  return result.data;
}