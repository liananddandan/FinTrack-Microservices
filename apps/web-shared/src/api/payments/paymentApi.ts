import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type {
  CreatePaymentRequest,
  CreatePaymentResultDto,
  PaymentDetailDto,
  PaymentListItemDto,
} from "./types"

export function createPaymentApi(tenantHttp: AxiosInstance) {
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
    async createPayment(
      request: CreatePaymentRequest
    ): Promise<CreatePaymentResultDto> {
      const response = await tenantHttp.post<ApiResponse<CreatePaymentResultDto>>(
        "/api/payments",
        request
      )

      return unwrapApiResponse(response, "Failed to create payment")
    },

    async getPaymentById(paymentPublicId: string): Promise<PaymentDetailDto> {
      const response = await tenantHttp.get<ApiResponse<PaymentDetailDto>>(
        `/api/payments/${paymentPublicId}`
      )

      return unwrapApiResponse(response, "Failed to fetch payment")
    },

    async getPaymentsByOrder(
      orderPublicId: string
    ): Promise<PaymentListItemDto[]> {
      const response = await tenantHttp.get<ApiResponse<PaymentListItemDto[]>>(
        `/api/orders/${orderPublicId}/payments`
      )

      return unwrapApiResponse(response, "Failed to fetch order payments")
    },
  }
}