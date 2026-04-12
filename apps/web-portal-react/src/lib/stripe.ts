import { loadStripe } from "@stripe/stripe-js"

exportasync function getStripe(stripeConnectedAccountId: string | null) {
  return await loadStripe(
    import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY,
    stripeConnectedAccountId
      ? { stripeAccount: stripeConnectedAccountId }
      : undefined
  )
}