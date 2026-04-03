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

export type GetOrdersParams = {
  createdByMe?: boolean
  status?: string
  paymentStatus?: string
  fromUtc?: string
  toUtc?: string
  pageNumber?: number
  pageSize?: number
}

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
