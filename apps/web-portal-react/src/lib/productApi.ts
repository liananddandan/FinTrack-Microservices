import { createProductApi } from "@fintrack/web-shared"
import { tenantHttp } from "./http"

export const productApi = createProductApi(tenantHttp)