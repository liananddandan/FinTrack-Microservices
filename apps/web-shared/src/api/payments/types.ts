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