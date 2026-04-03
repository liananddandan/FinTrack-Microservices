import { createAuditLogApi } from "@fintrack/web-shared"
import { tenantHttp } from "./http"

export const auditLogApi = createAuditLogApi(tenantHttp)