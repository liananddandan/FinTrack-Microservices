import { tenantHttp } from "../lib/http"
import type { ApiResponse, PagedResult } from "./types"

export type GetOrderSummaryRequest = {
  createdByMe?: boolean
  fromUtc?: string
  toUtc?: string
}

export type OrderSummaryDto = {
  orderCount: number
  totalRevenue: number
  averageOrderValue: number
  cancelledOrderCount: number
}

export type GetOrdersRequest = {
  createdByMe?: boolean
  status?: string
  paymentStatus?: string
  fromUtc?: string
  toUtc?: string
  pageNumber?: number
  pageSize?: number
}

export type OrderListItemDto = {
  publicId: string
  orderNumber: string
  customerName: string | null
  totalAmount: number
  status: string
  paymentStatus: string
  paymentMethod: string
  createdByUserNameSnapshot: string
  createdAt: string
}

export async function getOrderSummary(
  request?: GetOrderSummaryRequest
): Promise<OrderSummaryDto> {
  const response = await tenantHttp.get<ApiResponse<OrderSummaryDto>>(
    "/api/orders/summary",
    {
      params: request,
    }
  )

  return response.data.data
}

export async function getOrders(
  request?: GetOrdersRequest
): Promise<PagedResult<OrderListItemDto>> {
  const response = await tenantHttp.get<ApiResponse<PagedResult<OrderListItemDto>>>(
    "/api/orders",
    {
      params: request,
    }
  )

  return response.data.data
}