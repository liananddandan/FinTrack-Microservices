import { createAccountApi } from "@fintrack/web-shared"
import { publicHttp, accountHttp } from "./http"

export const accountApi = createAccountApi({
    publicHttp,
    accountHttp,
})