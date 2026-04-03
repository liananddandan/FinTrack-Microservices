import { createProductCategoryApi } from "@fintrack/web-shared"
import { tenantHttp } from "./http"

export const productCategoryApi = createProductCategoryApi(tenantHttp)