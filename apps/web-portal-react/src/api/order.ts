import { tenantHttp } from "../lib/http"
import type { ApiResponse, PagedResult } from "./types"

export type CreateOrderItemRequest = {
  productPublicId: string
  quantity: number
  notes: string | null
}

export type CreateOrderRequest = {
  customerName: string | null
  customerPhone: string | null
  paymentMethod: string
  items: CreateOrderItemRequest[]
}

export type OrderItemDto = {
  productPublicId: string
  productNameSnapshot: string
  unitPrice: number
  quantity: number
  lineTotal: number
  notes: string | null
}

export type OrderDto = {
  publicId: string
  orderNumber: string
  customerName: string | null
  customerPhone: string | null
  createdByUserPublicId: string
  createdByUserNameSnapshot: string
  subtotalAmount: number
  gstRate: number
  gstAmount: number
  discountAmount: number
  totalAmount: number
  status: string
  paymentStatus: string
  paymentMethod: string
  createdAt: string
  paidAt: string | null
  items: OrderItemDto[]
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

export async function createOrder(
  request: CreateOrderRequest
): Promise<OrderDto> {
  const response = await tenantHttp.post<ApiResponse<OrderDto>>(
    "/api/orders",
    request
  )

  return response.data.data
}

export async function getOrderById(orderPublicId: string): Promise<OrderDto> {
  const response = await tenantHttp.get<ApiResponse<OrderDto>>(
    `/api/orders/${orderPublicId}`
  )

  return response.data.data
}

export async function getOrders(params?: {
  createdByMe?: boolean
  status?: string
  fromUtc?: string
  toUtc?: string
  pageNumber?: number
  pageSize?: number
}): Promise<PagedResult<OrderListItemDto>> {
  const response = await tenantHttp.get<ApiResponse<PagedResult<OrderListItemDto>>>(
    "/api/orders",
    { params }
  )

  return response.data.data
}

export async function cancelOrder(orderPublicId: string): Promise<boolean> {
  const response = await tenantHttp.post<ApiResponse<boolean>>(
    `/api/orders/${orderPublicId}/cancel`,
    {}
  )

  return response.data.data
}