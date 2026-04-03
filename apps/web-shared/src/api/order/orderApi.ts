import type { AxiosInstance } from "axios"
import type { ApiResponse, PagedResult } from "../types"
import type {
  CreateOrderRequest,
  OrderDto,
  OrderListItemDto,
  GetOrdersParams,
  GetOrderSummaryRequest,
  OrderSummaryDto,
} from "./types"

export function createOrderApi(tenantHttp: AxiosInstance) {
  function unwrapApiResponse<T>(
    response: { data: ApiResponse<T> },
    defaultMessage: string
  ): T {
    const result = response.data

    if (result.data == null) {
      throw new Error(result.message || defaultMessage)
    }

    return result.data
  }

  return {
    async createOrder(request: CreateOrderRequest): Promise<OrderDto> {
      const response = await tenantHttp.post<ApiResponse<OrderDto>>(
        "/api/orders",
        request
      )

      return unwrapApiResponse(response, "Failed to create order")
    },

    async getOrderById(orderPublicId: string): Promise<OrderDto> {
      const response = await tenantHttp.get<ApiResponse<OrderDto>>(
        `/api/orders/${orderPublicId}`
      )

      return unwrapApiResponse(response, "Failed to fetch order")
    },

    async getOrderSummary(
      params?: GetOrderSummaryRequest
    ): Promise<OrderSummaryDto> {
      const response = await tenantHttp.get<ApiResponse<OrderSummaryDto>>(
        "/api/orders/summary",
        {
          params,
        }
      )

      return unwrapApiResponse(response, "Failed to fetch order summary")
    },

    async getOrders(
      params?: GetOrdersParams
    ): Promise<PagedResult<OrderListItemDto>> {
      const response = await tenantHttp.get<
        ApiResponse<PagedResult<OrderListItemDto>>
      >("/api/orders", { params })

      return unwrapApiResponse(response, "Failed to fetch orders")
    },

    async cancelOrder(orderPublicId: string): Promise<boolean> {
      const response = await tenantHttp.post<ApiResponse<boolean>>(
        `/api/orders/${orderPublicId}/cancel`,
        {}
      )

      return unwrapApiResponse(response, "Failed to cancel order")
    },
  }
}