import { createDevApi } from "@fintrack/web-shared"
import { publicHttp } from "./http"

export const devApi = createDevApi(publicHttp)