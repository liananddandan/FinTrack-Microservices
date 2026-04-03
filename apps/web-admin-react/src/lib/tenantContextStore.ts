import { createTenantContextStore } from "@fintrack/web-shared"
import { tenantApi } from "./tenantApi"

export const tenantContextStore = createTenantContextStore(() =>
  tenantApi.getTenantContext()
)