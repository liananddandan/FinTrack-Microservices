import { createTenantApi } from "@fintrack/web-shared"
import { publicHttp, accountHttp, platformHttp } from "./http"

export const tenantApi = createTenantApi({
  publicHttp,
  accountHttp,
  platformHttp
})