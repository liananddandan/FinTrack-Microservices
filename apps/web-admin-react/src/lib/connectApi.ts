import { createStripeConnectApi } from "@fintrack/web-shared"
import { tenantHttp } from "./http"

export const paymentApi = createStripeConnectApi(tenantHttp)