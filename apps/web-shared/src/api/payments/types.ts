export type TenantStripeConnectStatusDto = {
  connectedAccountId: string | null
  chargesEnabled: boolean
  payoutsEnabled: boolean
  isConnected: boolean
  onboardingRequired: boolean
}

export type CreateTenantStripeOnboardingLinkDto = {
  url: string
}

export type CreatePaymentRequest = {
  orderPublicId: string
  paymentMethodType: string
}

export type CreatePaymentResultDto = {
  paymentPublicId: string
  provider: string
  paymentMethodType: string
  status: string
  clientSecret: string | null
  stripeConnectedAccountId: string | null
}

export type PaymentDetailDto = {
  paymentPublicId: string
  orderPublicId: string
  provider: string
  paymentMethodType: string
  status: string
  currency: string
  amount: number
  refundedAmount: number
  failureReason: string | null
  createdAt: string
  paidAt: string | null
  failedAt: string | null
  refundedAt: string | null
}

export type PaymentListItemDto = {
  paymentPublicId: string
  provider: string
  paymentMethodType: string
  status: string
  currency: string
  amount: number
  createdAt: string
  paidAt: string | null
}