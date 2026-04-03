import { createAccountApi } from "@fintrack/web-shared"
import { publicHttp, accountHttp, platformHttp } from "./http"

export const accountApi = createAccountApi({
  publicHttp,
  accountHttp,
  platformHttp,
})