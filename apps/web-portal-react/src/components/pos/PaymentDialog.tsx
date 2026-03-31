import { Elements } from "@stripe/react-stripe-js"
import type { Stripe } from "@stripe/stripe-js"
import type { OrderDto } from "../../api/order"
import StripePaymentForm from "./StripePaymentForm"

type PaymentMode = "menu" | "stripe" | "pos"

type Props = {
  open: boolean
  order: OrderDto | null
  paymentMode: PaymentMode
  stripePromise: Promise<Stripe | null>
  stripeClientSecret: string | null
  pollingPayment: boolean
  actionLoading: boolean
  errorMessage: string
  onClose: () => void
  onCashPayment: () => void
  onCardEntry: () => void
  onPosTerminal: () => void
  onStripeSuccess: (orderPublicId: string, orderNumber: string) => Promise<void>
  onBackToMethods: () => void
}

export default function PaymentDialog({
  open,
  order,
  paymentMode,
  stripePromise,
  stripeClientSecret,
  pollingPayment,
  actionLoading,
  errorMessage,
  onClose,
  onCashPayment,
  onCardEntry,
  onPosTerminal,
  onStripeSuccess,
  onBackToMethods,
}: Props) {
  if (!open || !order) return null

  const title =
    paymentMode === "stripe"
      ? "Card Entry"
      : paymentMode === "pos"
      ? "POS Terminal"
      : "Payment"

  const description =
    paymentMode === "stripe"
      ? "Use manual card entry for demo payment."
      : paymentMode === "pos"
      ? "This mode is reserved for Stripe Terminal integration."
      : "Order created successfully. Choose how to collect payment."

  return (
    <div className="fixed inset-0 z-[60] bg-slate-900/40 px-4 py-6">
      <div className="flex min-h-full items-center justify-center">
        <div className="w-full max-w-xl max-h-[85vh] overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-2xl">
          <div className="flex h-full max-h-[85vh] flex-col">
            <div className="shrink-0 p-6">
              <div className="flex items-start gap-3">
                {paymentMode !== "menu" ? (
                  <button
                    type="button"
                    onClick={onBackToMethods}
                    className="mt-1 inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-full border border-slate-300 text-slate-600 transition hover:border-slate-400 hover:bg-slate-100"
                    aria-label="Back"
                  >
                    ←
                  </button>
                ) : null}

                <div>
                  <h2 className="text-xl font-semibold text-slate-800">{title}</h2>
                  <p className="mt-1 text-sm text-slate-500">{description}</p>
                </div>
              </div>

              {errorMessage ? (
                <div className="mt-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                  {errorMessage}
                </div>
              ) : null}

              <div className="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4">
                <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-700">
                  Order Information
                </h3>

                <div className="mt-4 space-y-3">
                  <div className="flex items-center justify-between text-sm text-slate-600">
                    <span>Order number</span>
                    <span className="font-medium text-slate-800">
                      {order.orderNumber}
                    </span>
                  </div>

                  <div className="flex items-center justify-between text-sm text-slate-600">
                    <span>Customer</span>
                    <span className="font-medium text-slate-800">
                      {order.customerName || "Walk-in"}
                    </span>
                  </div>

                  <div className="flex items-center justify-between text-sm text-slate-600">
                    <span>Status</span>
                    <span className="font-medium text-amber-600">
                      {order.paymentStatus}
                    </span>
                  </div>

                  <div className="border-t border-slate-200 pt-3 flex items-center justify-between text-base font-semibold text-slate-800">
                    <span>Total</span>
                    <span>${order.totalAmount.toFixed(2)}</span>
                  </div>
                </div>
              </div>
            </div>

            <div className="min-h-0 flex-1 overflow-y-auto px-6 pb-6">
              {paymentMode === "menu" ? (
                <div>
                  <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-700">
                    Payment Method
                  </h3>
                  <p className="mt-2 text-sm text-slate-500">
                    Choose the payment flow for this order.
                  </p>

                  <div className="mt-4 grid grid-cols-3 gap-3">
                    <button
                      type="button"
                      onClick={onCashPayment}
                      disabled={actionLoading}
                      className="inline-flex h-12 w-full items-center justify-center rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:opacity-50"
                    >
                      Cash
                    </button>

                    <button
                      type="button"
                      onClick={onCardEntry}
                      disabled={actionLoading}
                      className="inline-flex h-12 w-full items-center justify-center rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:opacity-50"
                    >
                      Card Entry
                    </button>

                    <button
                      type="button"
                      onClick={onPosTerminal}
                      disabled={actionLoading}
                      className="inline-flex h-12 w-full items-center justify-center rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:opacity-50"
                    >
                      POS Terminal
                    </button>
                  </div>

                  <button
                    type="button"
                    onClick={onClose}
                    className="mt-6 inline-flex h-11 w-full items-center justify-center rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-slate-400"
                  >
                    Close
                  </button>
                </div>
              ) : null}

              {paymentMode === "stripe" && stripeClientSecret ? (
                <div>
                  <Elements
                    stripe={stripePromise}
                    options={{ clientSecret: stripeClientSecret }}
                  >
                    <StripePaymentForm
                      orderPublicId={order.publicId}
                      orderNumber={order.orderNumber}
                      onSuccess={onStripeSuccess}
                    />
                  </Elements>

                  {pollingPayment ? (
                    <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-600">
                      Waiting for payment confirmation...
                    </div>
                  ) : null}
                </div>
              ) : null}

              {paymentMode === "pos" ? (
                <div>
                  <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center">
                    <div className="text-base font-medium text-slate-800">
                      POS Terminal coming soon
                    </div>
                    <div className="mt-2 text-sm text-slate-500">
                      Later this button will send the payment to a simulated terminal.
                    </div>
                  </div>
                </div>
              ) : null}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}