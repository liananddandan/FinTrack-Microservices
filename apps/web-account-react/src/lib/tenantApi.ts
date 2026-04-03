import { createTenantApi } from "@fintrack/web-shared"
import { publicHttp } from "./http"

export const tenantApi = createTenantApi({
  publicHttp,
})