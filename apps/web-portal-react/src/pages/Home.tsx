import { useEffect, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import { tenantContextStore } from "../lib/tenantContextStore"
import { productCategoryApi } from "../lib/productCategoryApi"
import type { ProductCategoryItem, ProductItem } from "@fintrack/web-shared"
import { productApi } from "../lib/productApi"
import { orderApi } from "../lib/orderApi"
import { paymentApi } from "../lib/paymentApi"
import { Elements, PaymentElement, useElements, useStripe } from "@stripe/react-stripe-js"
import { loadStripe, type Stripe } from "@stripe/stripe-js"
import {
  HiOutlineArrowLeftOnRectangle,
  HiOutlineQueueList,
  HiOutlineClipboardDocumentList,
  HiOutlinePlus,
  HiOutlineMinus,
  HiOutlineTrash,
  HiOutlineCreditCard,
  HiOutlineXMark,
} from "react-icons/hi2"

type CartItem = {
  id: string
  name: string
  price: number
  quantity: number
  categoryName: string
}

type PaymentMethodType = "Cash" | "Card" | "Terminal"

type CreatedOrder = {
  publicId: string
  orderNumber: string
}

function TopActionButton({
  label,
  icon,
  onClick,
  danger = false,
}: {
  label: string
  icon: React.ReactNode
  onClick: () => void
  danger?: boolean
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={[
        "inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm transition",
        danger
          ? "border border-rose-200 bg-rose-50 text-rose-700 hover:border-rose-300 hover:bg-rose-100"
          : "border border-slate-300 bg-white text-slate-700 hover:border-indigo-500 hover:text-indigo-600",
      ].join(" ")}
    >
      {icon}
      {label}
    </button>
  )
}

function PaymentModal({
  open,
  createdOrder,
  paymentMethodType,
  paymentPublicId,
  clientSecret,
  stripeConnectedAccountId,
  onClose,
  onDone,
}: {
  open: boolean
  createdOrder: CreatedOrder | null
  paymentMethodType: PaymentMethodType
  paymentPublicId: string
  clientSecret: string | null
  stripeConnectedAccountId: string | null
  onClose: () => void
  onDone: () => void
}) {
  const [stripeInstance, setStripeInstance] = useState<Stripe | null>(null)

  useEffect(() => {
    async function initStripe() {
      if (!open || paymentMethodType !== "Card") {
        setStripeInstance(null)
        return
      }

      const stripe = await loadStripe(
        import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY,
        stripeConnectedAccountId
          ? { stripeAccount: stripeConnectedAccountId }
          : undefined
      )

      setStripeInstance(stripe)
    }

    void initStripe()
  }, [open, paymentMethodType, stripeConnectedAccountId])

  if (!open) {
    return null
  }

  const isCash = paymentMethodType === "Cash"
  const isCard = paymentMethodType === "Card"
  const isTerminal = paymentMethodType === "Terminal"

  return (
    <Modal open={open} onClose={onClose}>
      <div className="p-6">
        <h2 className="text-2xl font-semibold text-slate-900">Payment</h2>

        <div className="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-700">
          <div>Order: {createdOrder?.orderNumber}</div>
          <div className="mt-2">Payment Method: {paymentMethodType}</div>
          <div className="mt-2">Payment ID: {paymentPublicId}</div>
        </div>

        {isCash ? (
          <div className="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-700">
            Cash payment recorded successfully.
          </div>
        ) : null}

        {isCard ? (
          <div className="mt-6">
            {!clientSecret ? (
              <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
                Missing client secret.
              </div>
            ) : !stripeInstance ? (
              <div className="rounded-xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-500">
                Loading Stripe...
              </div>
            ) : (
              <Elements
                stripe={stripeInstance}
                options={{ clientSecret }}
              >
                <CardPaymentSection
                  paymentPublicId={paymentPublicId}
                  onDone={onDone} />
              </Elements>
            )}
          </div>
        ) : null}

        {isTerminal ? (
          <div className="mt-6 rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm text-amber-700">
            Terminal payment is not implemented yet.
          </div>
        ) : null}

        {!isCard ? (
          <button
            type="button"
            onClick={onDone}
            className="mt-6 inline-flex h-12 w-full items-center justify-center rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500"
          >
            Done
          </button>
        ) : null}
      </div>
    </Modal>
  )
}

function CardPaymentSection({
  paymentPublicId,
  onDone,
}: {
  paymentPublicId: string
  onDone: () => void
}) {
  const stripe = useStripe()
  const elements = useElements()

  const [submitting, setSubmitting] = useState(false)
  const [polling, setPolling] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")

  async function pollPaymentStatus() {
    setPolling(true)
    setErrorMessage("")
    setSuccessMessage("Waiting for payment confirmation...")

    try {
      const maxAttempts = 20
      const intervalMs = 1500

      for (let attempt = 0; attempt < maxAttempts; attempt++) {
        const payment = await paymentApi.getPaymentById(paymentPublicId)

        if (payment.status === "Succeeded") {
          setSuccessMessage("Payment completed successfully.")
          setPolling(false)
          onDone()
          return
        }

        if (payment.status === "Failed") {
          setErrorMessage(payment.failureReason || "Payment failed.")
          setSuccessMessage("")
          setPolling(false)
          return
        }

        if (payment.status === "Cancelled") {
          setErrorMessage("Payment was cancelled.")
          setSuccessMessage("")
          setPolling(false)
          return
        }

        await new Promise((resolve) => setTimeout(resolve, intervalMs))
      }

      setErrorMessage(
        "Payment confirmation timed out. Please refresh payment status manually."
      )
      setSuccessMessage("")
      setPolling(false)
    } catch (error) {
      setErrorMessage(
        error instanceof Error
          ? error.message
          : "Failed to check payment status."
      )
      setSuccessMessage("")
      setPolling(false)
    }
  }

  async function handleSubmit() {
    if (!stripe || !elements) {
      return
    }

    setSubmitting(true)
    setErrorMessage("")
    setSuccessMessage("")

    const { error } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: window.location.href,
      },
      redirect: "if_required",
    })

    if (error) {
      setErrorMessage(error.message || "Payment failed.")
      setSubmitting(false)
      return
    }

    setSubmitting(false)
    await pollPaymentStatus()
  }

  async function handleRefreshStatus() {
    await pollPaymentStatus()
  }

  return (
    <div className="space-y-4">
      <div className="rounded-xl border border-slate-200 bg-white p-4">
        <PaymentElement />
      </div>

      {errorMessage ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      {successMessage ? (
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-700">
          {successMessage}
        </div>
      ) : null}

      <div className="flex gap-3">
        <button
          type="button"
          onClick={() => void handleSubmit()}
          disabled={!stripe || !elements || submitting || polling}
          className="inline-flex h-12 flex-1 items-center justify-center rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting
            ? "Submitting..."
            : polling
              ? "Waiting..."
              : "Pay now"}
        </button>

        <button
          type="button"
          onClick={() => void handleRefreshStatus()}
          disabled={submitting || polling}
          className="inline-flex h-12 items-center justify-center rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-slate-400 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Refresh
        </button>
      </div>
    </div>
  )

  return (
    <div className="space-y-4">
      <div className="rounded-xl border border-slate-200 bg-white p-4">
        <PaymentElement />
      </div>

      {errorMessage ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      {successMessage ? (
        <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm text-emerald-700">
          {successMessage}
        </div>
      ) : null}

      <button
        type="button"
        onClick={() => void handleSubmit()}
        disabled={!stripe || !elements || submitting}
        className="inline-flex h-12 w-full items-center justify-center rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {submitting ? "Processing..." : "Pay now"}
      </button>
    </div>
  )
}

function Modal({
  open,
  onClose,
  children,
}: {
  open: boolean
  onClose: () => void
  children: React.ReactNode
}) {
  if (!open) {
    return null
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 px-4">
      <div className="relative max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-3xl bg-white shadow-2xl">
        <button
          type="button"
          onClick={onClose}
          className="absolute right-4 top-4 rounded-full p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600"
        >
          <HiOutlineXMark className="h-5 w-5" />
        </button>

        {children}
      </div>
    </div>
  )
}

export default function Home() {
  const navigate = useNavigate()
  const auth = useAuth()

  const [tenantContext, setTenantContext] = useState(
    tenantContextStore.tenantContext
  )

  const [initializing, setInitializing] = useState(true)
  const [loadingCategories, setLoadingCategories] = useState(false)
  const [loadingProducts, setLoadingProducts] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")

  const [categories, setCategories] = useState<ProductCategoryItem[]>([])
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>("")
  const [products, setProducts] = useState<ProductItem[]>([])
  const [cartItems, setCartItems] = useState<CartItem[]>([])

  const [customerName, setCustomerName] = useState("")
  const [checkoutSuccessMessage, setCheckoutSuccessMessage] = useState("")

  const [isReviewOpen, setIsReviewOpen] = useState(false)
  const [isPaymentOpen, setIsPaymentOpen] = useState(false)

  const [paymentMethodType, setPaymentMethodType] =
    useState<PaymentMethodType>("Cash")

  const [creatingOrder, setCreatingOrder] = useState(false)
  const [creatingPayment, setCreatingPayment] = useState(false)

  const [createdOrder, setCreatedOrder] = useState<CreatedOrder | null>(null)
  const [paymentPublicId, setPaymentPublicId] = useState("")
  const [clientSecret, setClientSecret] = useState<string | null>(null)
  const [stripeConnectedAccountId, setStripeConnectedAccountId] = useState<string | null>(null)

  useEffect(() => {
    const unsubscribe = tenantContextStore.subscribe(() => {
      setTenantContext(tenantContextStore.tenantContext)
    })

    setTenantContext(tenantContextStore.tenantContext)

    return unsubscribe
  }, [])

  useEffect(() => {
    async function init() {
      try {
        await loadCategories()
      } finally {
        setInitializing(false)
      }
    }

    void init()
  }, [])

  useEffect(() => {
    if (selectedCategoryId) {
      void loadProducts(selectedCategoryId)
    } else {
      setProducts([])
    }
  }, [selectedCategoryId])

  const currentMembership = useMemo(() => {
    const tenantPublicId = tenantContext?.tenantPublicId

    if (!tenantPublicId) {
      return null
    }

    return (
      auth.resolvedMemberships.find(
        (membership) => membership.tenantPublicId === tenantPublicId
      ) ?? null
    )
  }, [auth.resolvedMemberships, tenantContext?.tenantPublicId])

  const selectedCategory = useMemo(
    () => categories.find((x) => x.publicId === selectedCategoryId) ?? null,
    [categories, selectedCategoryId]
  )

  const subtotal = useMemo(
    () => cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0),
    [cartItems]
  )

  const totalItems = useMemo(
    () => cartItems.reduce((sum, item) => sum + item.quantity, 0),
    [cartItems]
  )

  async function loadCategories() {
    setLoadingCategories(true)
    setErrorMessage("")

    try {
      const result = await productCategoryApi.getProductCategories()
      const activeCategories = result
        .filter((x) => x.isActive)
        .sort((a, b) => a.displayOrder - b.displayOrder)

      setCategories(activeCategories)

      if (activeCategories.length > 0) {
        setSelectedCategoryId(activeCategories[0].publicId)
      } else {
        setSelectedCategoryId("")
      }
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to load menu.")
      } else {
        setErrorMessage("Failed to load menu.")
      }
    } finally {
      setLoadingCategories(false)
    }
  }

  async function loadProducts(categoryPublicId: string) {
    setLoadingProducts(true)
    setErrorMessage("")

    try {
      const result = await productApi.getProductsByCategory(categoryPublicId)
      const availableProducts = result
        .filter((x) => x.isAvailable)
        .sort((a, b) => a.displayOrder - b.displayOrder)

      setProducts(availableProducts)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to load products.")
      } else {
        setErrorMessage("Failed to load products.")
      }
    } finally {
      setLoadingProducts(false)
    }
  }

  function goOrders() {
    navigate("/portal/orders")
  }

  function logout() {
    auth.logout()
    navigate("/portal/login", { replace: true })
  }

  function addToCart(product: ProductItem) {
    if (!selectedCategory) {
      return
    }

    setCartItems((prev) => {
      const existing = prev.find((x) => x.id === product.publicId)

      if (existing) {
        return prev.map((x) =>
          x.id === product.publicId ? { ...x, quantity: x.quantity + 1 } : x
        )
      }

      return [
        ...prev,
        {
          id: product.publicId,
          name: product.name,
          price: product.price,
          quantity: 1,
          categoryName: selectedCategory.name,
        },
      ]
    })
  }

  function increaseQuantity(id: string) {
    setCartItems((prev) =>
      prev.map((x) => (x.id === id ? { ...x, quantity: x.quantity + 1 } : x))
    )
  }

  function decreaseQuantity(id: string) {
    setCartItems((prev) =>
      prev
        .map((x) => (x.id === id ? { ...x, quantity: x.quantity - 1 } : x))
        .filter((x) => x.quantity > 0)
    )
  }

  function removeItem(id: string) {
    setCartItems((prev) => prev.filter((x) => x.id !== id))
  }

  function clearCart() {
    setCartItems([])
  }

  function resetCheckoutFlow() {
    setIsReviewOpen(false)
    setIsPaymentOpen(false)
    setCreatedOrder(null)
    setPaymentPublicId("")
    setClientSecret(null)
    setStripeConnectedAccountId(null)
    setPaymentMethodType("Cash")
    setCartItems([])
    setCustomerName("")
  }

  async function handleConfirmAndPay() {
    if (cartItems.length === 0) {
      return
    }

    setCreatingOrder(true)
    setErrorMessage("")
    setCheckoutSuccessMessage("")

    try {
      const order = await orderApi.createOrder({
        customerName: customerName.trim() || null,
        customerPhone: null,
        paymentMethod: paymentMethodType,
        items: cartItems.map((item) => ({
          productPublicId: item.id,
          quantity: item.quantity,
          notes: null,
        })),
      })

      setCreatedOrder({
        publicId: order.publicId,
        orderNumber: order.orderNumber,
      })

      setCreatingPayment(true)

      const payment = await paymentApi.createPayment({
        orderPublicId: order.publicId,
        paymentMethodType,
      })

      setPaymentPublicId(payment.paymentPublicId)
      setClientSecret(payment.clientSecret)
      setStripeConnectedAccountId(payment.stripeConnectedAccountId)
      setIsReviewOpen(false)
      setIsPaymentOpen(true)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to create payment.")
      } else {
        setErrorMessage("Failed to create payment.")
      }
    } finally {
      setCreatingOrder(false)
      setCreatingPayment(false)
    }
  }

  function handlePaymentDone() {
    setCheckoutSuccessMessage(
      createdOrder
        ? `Order ${createdOrder.orderNumber} completed successfully.`
        : "Payment completed successfully."
    )

    resetCheckoutFlow()
  }

  if (initializing) {
    return (
      <div className="min-h-screen bg-slate-50 px-6 py-10">
        <div className="mx-auto max-w-7xl text-sm text-slate-500">
          Loading workspace...
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-6">
      <div className="mx-auto flex max-w-7xl flex-col gap-6">
        <section className="rounded-3xl border border-slate-200 bg-white px-6 py-6 sm:px-8">
          <div className="flex flex-col gap-6 xl:flex-row xl:items-start xl:justify-between">
            <div className="min-w-0">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Current merchant
              </div>

              <h1 className="mt-2 truncate text-3xl font-semibold tracking-tight text-slate-800">
                {tenantContext?.tenantName || "Workspace"}
              </h1>

              <div className="mt-4 flex flex-wrap items-center gap-3">
                <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                  Role: {currentMembership?.role || "Unknown"}
                </span>

                <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                  User: {auth.userName || auth.userEmail || "Unknown user"}
                </span>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-3">
              <TopActionButton
                label="Orders"
                icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
                onClick={goOrders}
              />

              <TopActionButton
                label="Sign out"
                icon={<HiOutlineArrowLeftOnRectangle className="h-5 w-5" />}
                onClick={logout}
                danger
              />
            </div>
          </div>
        </section>

        {errorMessage ? (
          <div
            className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
            role="alert"
          >
            {errorMessage}
          </div>
        ) : null}

        {checkoutSuccessMessage ? (
          <div
            className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
            role="status"
          >
            {checkoutSuccessMessage}
          </div>
        ) : null}

        <section className="grid gap-6 xl:grid-cols-[240px_minmax(0,1fr)_360px]">
          <div className="rounded-3xl border border-slate-200 bg-white p-5">
            <div className="mb-4 flex items-center gap-2 text-slate-800">
              <HiOutlineQueueList className="h-5 w-5 text-slate-500" />
              <h2 className="text-base font-semibold">Categories</h2>
            </div>

            {loadingCategories ? (
              <div className="text-sm text-slate-500">Loading categories...</div>
            ) : categories.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">
                No categories available yet.
              </div>
            ) : (
              <div className="space-y-2">
                {categories.map((category) => {
                  const isActive = selectedCategoryId === category.publicId

                  return (
                    <button
                      key={category.publicId}
                      type="button"
                      onClick={() => setSelectedCategoryId(category.publicId)}
                      className={[
                        "w-full rounded-2xl border px-4 py-3 text-left transition",
                        isActive
                          ? "border-indigo-200 bg-indigo-50 text-indigo-700"
                          : "border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50",
                      ].join(" ")}
                    >
                      <div className="flex items-center justify-between gap-3">
                        <span className="truncate text-sm font-medium">
                          {category.name}
                        </span>
                        <span className="text-xs text-slate-500">
                          #{category.displayOrder}
                        </span>
                      </div>
                    </button>
                  )
                })}
              </div>
            )}
          </div>

          <div className="rounded-3xl border border-slate-200 bg-white p-5">
            <div className="mb-4 flex items-center justify-between gap-4">
              <div>
                <h2 className="text-base font-semibold text-slate-800">
                  {selectedCategory?.name || "Menu"}
                </h2>
                <p className="mt-1 text-sm text-slate-500">
                  Select products and add them to the current order.
                </p>
              </div>
            </div>

            {!selectedCategory ? (
              <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
                Choose a category to view products.
              </div>
            ) : loadingProducts ? (
              <div className="text-sm text-slate-500">Loading products...</div>
            ) : products.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
                No products available in this category yet.
              </div>
            ) : (
              <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
                {products.map((product) => (
                  <div
                    key={product.publicId}
                    className="rounded-xl border border-slate-200 bg-white p-3"
                  >
                    <div className="text-sm font-medium text-slate-800 truncate">
                      {product.name}
                    </div>
                    <div className="mt-1 text-sm text-slate-500">
                      ${product.price.toFixed(2)}
                    </div>
                    <button
                      type="button"
                      onClick={() => addToCart(product)}
                      className="mt-3 w-full rounded-lg bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
                    >
                      Add
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

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
                  onClick={clearCart}
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
                            onClick={() => removeItem(item.id)}
                            className="text-slate-400 transition hover:text-rose-600"
                          >
                            <HiOutlineTrash className="h-4 w-4" />
                          </button>
                        </div>

                        <div className="mt-3 flex items-center justify-between">
                          <div className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-2 py-1">
                            <button
                              type="button"
                              onClick={() => decreaseQuantity(item.id)}
                              className="rounded p-1 text-slate-600 transition hover:bg-slate-100"
                            >
                              <HiOutlineMinus className="h-4 w-4" />
                            </button>

                            <span className="min-w-[20px] text-center text-sm font-medium text-slate-800">
                              {item.quantity}
                            </span>

                            <button
                              type="button"
                              onClick={() => increaseQuantity(item.id)}
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
                  onChange={(e) => setCustomerName(e.target.value)}
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
                  onClick={() => setIsReviewOpen(true)}
                  disabled={cartItems.length === 0}
                  className="mt-5 inline-flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <HiOutlineCreditCard className="h-5 w-5" />
                  Checkout
                </button>
              </div>
            </div>
          </div>
        </section>

        <Modal open={isReviewOpen} onClose={() => setIsReviewOpen(false)}>
          <div className="p-6">
            <h2 className="text-2xl font-semibold text-slate-900">
              Review order
            </h2>
            <p className="mt-2 text-sm text-slate-500">
              Confirm items and choose a payment method.
            </p>

            <div className="mt-6 space-y-3">
              {cartItems.map((item) => (
                <div
                  key={item.id}
                  className="flex items-center justify-between rounded-xl border border-slate-200 px-4 py-3"
                >
                  <div>
                    <div className="text-sm font-medium text-slate-800">
                      {item.name}
                    </div>
                    <div className="text-xs text-slate-500">
                      {item.quantity} × ${item.price.toFixed(2)}
                    </div>
                  </div>

                  <div className="text-sm font-semibold text-slate-800">
                    ${(item.price * item.quantity).toFixed(2)}
                  </div>
                </div>
              ))}
            </div>

            <div className="mt-6 rounded-xl border border-slate-200 bg-slate-50 p-4">
              <div className="text-sm text-slate-600">
                Customer name:{" "}
                <span className="font-medium text-slate-800">
                  {customerName.trim() || "N/A"}
                </span>
              </div>

              <div className="mt-4">
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Payment method
                </label>

                <select
                  value={paymentMethodType}
                  onChange={(e) =>
                    setPaymentMethodType(e.target.value as PaymentMethodType)
                  }
                  className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                >
                  <option value="Cash">Cash</option>
                  <option value="Card">Card</option>
                  <option value="Terminal">Terminal</option>
                </select>
              </div>

              <div className="mt-4 flex items-center justify-between border-t border-slate-200 pt-4">
                <span className="text-base font-medium text-slate-700">Total</span>
                <span className="text-lg font-semibold text-slate-900">
                  ${subtotal.toFixed(2)}
                </span>
              </div>
            </div>

            <button
              type="button"
              onClick={() => void handleConfirmAndPay()}
              disabled={creatingOrder || creatingPayment}
              className="mt-6 inline-flex h-12 w-full items-center justify-center rounded-2xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {creatingOrder || creatingPayment
                ? "Processing..."
                : "Confirm and Pay"}
            </button>
          </div>
        </Modal>

        <PaymentModal
          open={isPaymentOpen}
          createdOrder={createdOrder}
          paymentMethodType={paymentMethodType}
          paymentPublicId={paymentPublicId}
          clientSecret={clientSecret}
          stripeConnectedAccountId={stripeConnectedAccountId}
          onClose={() => {
            setIsPaymentOpen(false)
            setPaymentPublicId("")
            setClientSecret(null)
            setStripeConnectedAccountId(null)
          }}
          onDone={handlePaymentDone}
        />
      </div>
    </div>
  )
}