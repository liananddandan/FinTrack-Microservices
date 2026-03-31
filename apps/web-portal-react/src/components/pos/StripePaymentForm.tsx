import { useState } from "react"
import { PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js"

type Props = {
  orderPublicId: string
  orderNumber: string
  onSuccess: (orderPublicId: string, orderNumber: string) => Promise<void>
}

export default function StripePaymentForm({
  orderPublicId,
  orderNumber,
  onSuccess,
}: Props) {
  const stripe = useStripe()
  const elements = useElements()
  const [submitting, setSubmitting] = useState(false)
  const [message, setMessage] = useState("")

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault()

    if (!stripe || !elements) return

    setSubmitting(true)
    setMessage("")

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: window.location.origin + "/portal/orders",
      },
      redirect: "if_required",
    })

    if (error) {
      setMessage(error.message || "Payment failed.")
      setSubmitting(false)
      return
    }

    setMessage("Payment submitted. Waiting for final confirmation...")
    await onSuccess(orderPublicId, orderNumber)
    setSubmitting(false)
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="text-sm text-slate-600">Order {orderNumber}</div>

      <PaymentElement />

      {message ? (
        <div className="rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
          {message}
        </div>
      ) : null}

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={!stripe || !elements || submitting}
          className="h-11 rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white disabled:opacity-50"
        >
          {submitting ? "Processing..." : "Pay"}
        </button>
      </div>
    </form>
  )
}