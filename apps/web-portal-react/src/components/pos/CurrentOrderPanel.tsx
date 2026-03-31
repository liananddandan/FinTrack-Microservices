import {
  HiOutlinePlus,
  HiOutlineMinus,
  HiOutlineTrash,
  HiOutlineCreditCard,
} from "react-icons/hi2"

export type CartItem = {
  id: string
  name: string
  price: number
  quantity: number
  categoryName: string
}

type Props = {
  cartItems: CartItem[]
  totalItems: number
  subtotal: number
  customerName: string
  onCustomerNameChange: (value: string) => void
  onIncreaseQuantity: (id: string) => void
  onDecreaseQuantity: (id: string) => void
  onRemoveItem: (id: string) => void
  onClearCart: () => void
  onCheckout: () => void
}

export default function CurrentOrderPanel({
  cartItems,
  totalItems,
  subtotal,
  customerName,
  onCustomerNameChange,
  onIncreaseQuantity,
  onDecreaseQuantity,
  onRemoveItem,
  onClearCart,
  onCheckout,
}: Props) {
  return (
    <div className="rounded-3xl border border-slate-200 bg-white p-5">
      <div className="mb-4 flex items-center justify-between gap-4">
        <div>
          <h2 className="text-base font-semibold text-slate-800">
            Current order
          </h2>
          <p className="mt-1 text-sm text-slate-500">
            {totalItems} item{totalItems === 1 ? "" : "s"} selected
          </p>
        </div>

        {cartItems.length > 0 ? (
          <button
            type="button"
            onClick={onClearCart}
            className="text-sm font-medium text-rose-600 transition hover:text-rose-500"
          >
            Clear
          </button>
        ) : null}
      </div>

      <div className="flex h-[620px] min-h-0 flex-col">
        <div className="min-h-0 flex-1 overflow-y-auto pr-1">
          {cartItems.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
              No items in the order yet.
            </div>
          ) : (
            <div className="space-y-3">
              {cartItems.map((item) => (
                <div
                  key={item.id}
                  className="rounded-xl border border-slate-200 bg-slate-50 p-3"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-slate-800">
                        {item.name}
                      </p>
                      <p className="mt-1 text-xs text-slate-500">
                        {item.categoryName}
                      </p>
                      <p className="mt-1 text-xs text-slate-500">
                        ${item.price.toFixed(2)} each
                      </p>
                    </div>

                    <button
                      type="button"
                      onClick={() => onRemoveItem(item.id)}
                      className="text-slate-400 transition hover:text-rose-600"
                    >
                      <HiOutlineTrash className="h-4 w-4" />
                    </button>
                  </div>

                  <div className="mt-3 flex items-center justify-between">
                    <div className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-2 py-1">
                      <button
                        type="button"
                        onClick={() => onDecreaseQuantity(item.id)}
                        className="rounded p-1 text-slate-600 transition hover:bg-slate-100"
                      >
                        <HiOutlineMinus className="h-4 w-4" />
                      </button>

                      <span className="min-w-[20px] text-center text-sm font-medium text-slate-800">
                        {item.quantity}
                      </span>

                      <button
                        type="button"
                        onClick={() => onIncreaseQuantity(item.id)}
                        className="rounded p-1 text-slate-600 transition hover:bg-slate-100"
                      >
                        <HiOutlinePlus className="h-4 w-4" />
                      </button>
                    </div>

                    <div className="text-sm font-semibold text-slate-800">
                      ${(item.price * item.quantity).toFixed(2)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="mt-4">
          <label className="mb-2 block text-sm font-medium text-slate-700">
            Customer name
          </label>
          <input
            type="text"
            value={customerName}
            onChange={(e) => onCustomerNameChange(e.target.value)}
            placeholder="Optional"
            className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
          />
        </div>

        <div className="mt-4 shrink-0 rounded-2xl border border-slate-200 bg-slate-50 p-4">
          <div className="flex items-center justify-between text-sm text-slate-600">
            <span>Subtotal</span>
            <span>${subtotal.toFixed(2)}</span>
          </div>

          <div className="mt-3 flex items-center justify-between text-base font-semibold text-slate-800">
            <span>Total</span>
            <span>${subtotal.toFixed(2)}</span>
          </div>

          <button
            type="button"
            onClick={onCheckout}
            disabled={cartItems.length === 0}
            className="mt-5 inline-flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <HiOutlineCreditCard className="h-5 w-5" />
            Checkout
          </button>
        </div>
      </div>
    </div>
  )
}