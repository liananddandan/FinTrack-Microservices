import type { CartItem } from "./CurrentOrderPanel"

type Props = {
  open: boolean
  cartItems: CartItem[]
  customerName: string
  subtotal: number
  submitting: boolean
  errorMessage: string
  onClose: () => void
  onPay: () => void
}

export default function CheckoutReviewDialog({
  open,
  cartItems,
  customerName,
  subtotal,
  submitting,
  errorMessage,
  onClose,
  onPay,
}: Props) {
  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4">
      <div className="w-full max-w-2xl rounded-3xl border border-slate-200 bg-white p-6 shadow-2xl">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-slate-800">Complete Sale</h2>
            <p className="mt-1 text-sm text-slate-500">
              Review the order before starting payment.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="rounded-lg px-2 py-1 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
          >
            ✕
          </button>
        </div>

        <div className="mt-5 grid gap-6 lg:grid-cols-[minmax(0,1fr)_260px]">
          <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
            <div className="mb-4 text-sm font-medium text-slate-700">Order items</div>

            {cartItems.length === 0 ? (
              <div className="text-sm text-slate-500">No items.</div>
            ) : (
              <div className="max-h-[360px] overflow-y-auto pr-1 space-y-3">
                {cartItems.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-start justify-between gap-4 border-b border-slate-200 pb-3 last:border-b-0 last:pb-0"
                  >
                    <div className="min-w-0">
                      <div className="truncate text-sm font-medium text-slate-800">
                        {item.name}
                      </div>
                      <div className="mt-1 text-xs text-slate-500">
                        {item.categoryName}
                      </div>
                      <div className="mt-1 text-xs text-slate-500">
                        ${item.price.toFixed(2)} × {item.quantity}
                      </div>
                    </div>

                    <div className="shrink-0 text-sm font-semibold text-slate-800">
                      ${(item.price * item.quantity).toFixed(2)}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="rounded-2xl border border-slate-200 bg-white p-4">
            <div className="flex items-center justify-between text-sm text-slate-600">
              <span>Customer</span>
              <span className="font-medium text-slate-800">
                {customerName.trim() || "Walk-in"}
              </span>
            </div>

            <div className="mt-3 flex items-center justify-between text-sm text-slate-600">
              <span>Items</span>
              <span className="font-medium text-slate-800">
                {cartItems.reduce((sum, item) => sum + item.quantity, 0)}
              </span>
            </div>

            <div className="mt-3 flex items-center justify-between text-sm text-slate-600">
              <span>Subtotal</span>
              <span className="font-medium text-slate-800">
                ${subtotal.toFixed(2)}
              </span>
            </div>

            <div className="mt-4 border-t border-slate-200 pt-4 flex items-center justify-between text-base font-semibold text-slate-800">
              <span>Total</span>
              <span>${subtotal.toFixed(2)}</span>
            </div>
          </div>
        </div>

        {errorMessage ? (
          <div className="mt-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {errorMessage}
          </div>
        ) : null}

        <div className="mt-6 flex gap-3">
          <button
            type="button"
            onClick={onClose}
            className="inline-flex h-11 flex-1 items-center justify-center rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-slate-400"
          >
            Cancel
          </button>

          <button
            type="button"
            onClick={onPay}
            disabled={submitting}
            className="inline-flex h-11 flex-1 items-center justify-center rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:opacity-50"
          >
            {submitting ? "Creating order..." : "Pay"}
          </button>
        </div>
      </div>
    </div>
  )
}