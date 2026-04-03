import { createAccountApi } from "@fintrack/web-shared"
import { accountHttp, tenantHttp, publicHttp } from "./http"

export const accountApi = createAccountApi({
  publicHttp,
  accountHttp,
  platformHttp : tenantHttp,
})