import { createTenantApi } from "@fintrack/web-shared"
import { publicHttp, accountHttp, tenantHttp } from "./http"

export const tenantApi = createTenantApi({
  publicHttp,
  accountHttp,
  tenantHttp,
})