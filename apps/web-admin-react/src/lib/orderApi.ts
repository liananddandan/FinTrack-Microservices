import { createOrderApi } from "@fintrack/web-shared"
import { tenantHttp } from "./http"

export const orderApi = createOrderApi(tenantHttp)