import type { AxiosInstance } from "axios"
import type { ApiResponse } from "../types"
import type {
  CreateTenantStripeOnboardingLinkDto,
  TenantStripeConnectStatusDto,
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
    async getStripeConnectStatus(): Promise<TenantStripeConnectStatusDto> {
      const response =
        await tenantHttp.get<ApiResponse<TenantStripeConnectStatusDto>>(
          "/api/tenant/stripe-connect/status"
        )

      return unwrapApiResponse(
        response,
        "Failed to fetch Stripe connect status"
      )
    },

    async createOrResumeStripeOnboardingLink(): Promise<CreateTenantStripeOnboardingLinkDto> {
      const response =
        await tenantHttp.post<ApiResponse<CreateTenantStripeOnboardingLinkDto>>(
          "/api/tenant/stripe-connect/onboarding-link",
          {}
        )

      return unwrapApiResponse(
        response,
        "Failed to create Stripe onboarding link"
      )
    },
  }
}