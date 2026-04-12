import { createPaymentApi } from "@fintrack/web-shared"
import { tenantHttp } from "./http"

export const paymentApi = createPaymentApi(tenantHttp)