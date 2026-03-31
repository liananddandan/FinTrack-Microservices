import { tenantHttp } from "../lib/http"
import type { ApiResponse } from "./types"

export type CreatePaymentRequest = {
  orderPublicId: string
  provider: string
  paymentMethod: string
}

export type PaymentDto = {
  paymentPublicId: string
  orderPublicId: string
  provider: string
  paymentMethod: string
  status: string
  amount: number
  currency: string
  providerPaymentReference: string | null
  providerClientSecret: string | null
  failureReason: string | null
  createdAt: string
  startedAt: string | null
  paidAt: string | null
  refundedAt: string | null
}

export async function createPayment(
  request: CreatePaymentRequest
): Promise<PaymentDto> {
  const response = await tenantHttp.post<ApiResponse<PaymentDto>>(
    "/api/payments",
    request
  )

  if (!response.data.data) {
    throw new Error(response.data.message || "Failed to create payment.")
  }

  return response.data.data
}

export async function getPaymentByOrder(
  orderPublicId: string
): Promise<PaymentDto> {
  const response = await tenantHttp.get<ApiResponse<PaymentDto>>(
    `/api/payments/by-order/${orderPublicId}`
  )

  if (!response.data.data) {
    throw new Error(response.data.message || "Payment not found.")
  }

  return response.data.data
}