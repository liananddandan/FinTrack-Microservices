export type AuditLogItem = {
  publicId: string
  tenantPublicId: string
  actorUserPublicId?: string
  actorDisplayName?: string
  actionType: string
  category: string
  targetType?: string
  targetPublicId?: string
  targetDisplay?: string
  source?: string
  correlationId?: string
  occurredAtUtc: string
  metadataJson: string
  summary: string
}

export type AuditLogQuery = {
  actionType?: string
  actorUserPublicId?: string
  targetPublicId?: string
  fromUtc?: string
  toUtc?: string
  pageNumber?: number
  pageSize?: number
}