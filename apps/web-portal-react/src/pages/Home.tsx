import { useEffect, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { loadStripe } from "@stripe/stripe-js"
import {
  HiOutlineArrowLeftOnRectangle,
  HiOutlineClipboardDocumentList,
  HiOutlineUserCircle,
} from "react-icons/hi2"

import { useAuth } from "../hooks/useAuth"
import { getProductCategories, type ProductCategoryItem } from "../api/product-category"
import { getProductsByCategory, type ProductItem } from "../api/product"
import { createOrder, type OrderDto } from "../api/order"
import { createPayment, getPaymentByOrder } from "../api/payment"

import CategorySidebar from "../components/pos/CategorySidebar"
import ProductGrid from "../components/pos/ProductGrid"
import CurrentOrderPanel, {
  type CartItem,
} from "../components/pos/CurrentOrderPanel"
import CheckoutReviewDialog from "../components/pos/CheckoutReviewDialog"
import PaymentDialog from "../components/pos/PaymentDialog"
import Toast from "../components/pos/Toast"

const stripePromise = loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY)

type ToastState = {
  open: boolean
  type: "success" | "error"
  message: string
}

type PaymentMode = "menu" | "stripe" | "pos"

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

export default function Home() {
  const navigate = useNavigate()
  const auth = useAuth()

  const [initializing, setInitializing] = useState(true)
  const [loadingCategories, setLoadingCategories] = useState(false)
  const [loadingProducts, setLoadingProducts] = useState(false)
  const [pageError, setPageError] = useState("")

  const [categories, setCategories] = useState<ProductCategoryItem[]>([])
  const [selectedCategoryId, setSelectedCategoryId] = useState("")
  const [products, setProducts] = useState<ProductItem[]>([])

  const [cartItems, setCartItems] = useState<CartItem[]>([])
  const [customerName, setCustomerName] = useState("")

  const [checkoutDialogOpen, setCheckoutDialogOpen] = useState(false)
  const [checkoutSubmitting, setCheckoutSubmitting] = useState(false)
  const [checkoutError, setCheckoutError] = useState("")

  const [paymentDialogOpen, setPaymentDialogOpen] = useState(false)
  const [paymentMode, setPaymentMode] = useState<PaymentMode>("menu")
  const [paymentActionLoading, setPaymentActionLoading] = useState(false)
  const [paymentError, setPaymentError] = useState("")
  const [pollingPayment, setPollingPayment] = useState(false)

  const [createdOrder, setCreatedOrder] = useState<OrderDto | null>(null)
  const [stripeClientSecret, setStripeClientSecret] = useState<string | null>(null)

  const [toast, setToast] = useState<ToastState>({
    open: false,
    type: "success",
    message: "",
  })

  useEffect(() => {
    async function init() {
      try {
        await auth.initialize()
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

  useEffect(() => {
    if (!toast.open) return

    const timer = window.setTimeout(() => {
      setToast((prev) => ({ ...prev, open: false }))
    }, 3000)

    return () => window.clearTimeout(timer)
  }, [toast.open])

  async function loadCategories() {
    setLoadingCategories(true)
    setPageError("")

    try {
      const result = await getProductCategories()
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
        setPageError(error.message || "Failed to load menu.")
      } else {
        setPageError("Failed to load menu.")
      }
    } finally {
      setLoadingCategories(false)
    }
  }

  async function loadProducts(categoryPublicId: string) {
    setLoadingProducts(true)
    setPageError("")

    try {
      const result = await getProductsByCategory(categoryPublicId)
      const availableProducts = result
        .filter((x) => x.isAvailable)
        .sort((a, b) => a.displayOrder - b.displayOrder)

      setProducts(availableProducts)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPageError(error.message || "Failed to load products.")
      } else {
        setPageError("Failed to load products.")
      }
    } finally {
      setLoadingProducts(false)
    }
  }

  function showToast(type: "success" | "error", message: string) {
    setToast({
      open: true,
      type,
      message,
    })
  }

  function openCheckoutDialog() {
    if (cartItems.length === 0) return
    setCheckoutError("")
    setCheckoutDialogOpen(true)
  }

  async function handlePayFromReview() {
    if (cartItems.length === 0) return

    setCheckoutSubmitting(true)
    setCheckoutError("")

    try {
      const order = await createOrder({
        customerName: customerName.trim() || null,
        customerPhone: null,
        paymentMethod: "Pending",
        items: cartItems.map((item) => ({
          productPublicId: item.id,
          quantity: item.quantity,
          notes: null,
        })),
      })

      setCreatedOrder(order)
      setCheckoutDialogOpen(false)
      setPaymentDialogOpen(true)
      setPaymentMode("menu")
      setPaymentError("")
    } catch (error: unknown) {
      if (error instanceof Error) {
        setCheckoutError(error.message || "Failed to create order.")
      } else {
        setCheckoutError("Failed to create order.")
      }
    } finally {
      setCheckoutSubmitting(false)
    }
  }

  async function handleCashPayment() {
    if (!createdOrder) return

    setPaymentActionLoading(true)
    setPaymentError("")

    try {
      await createPayment({
        orderPublicId: createdOrder.publicId,
        provider: "Cash",
        paymentMethod: "Cash",
      })

      const paidOrderNumber = createdOrder.orderNumber

      resetAfterPayment()
      showToast("success", `Order ${paidOrderNumber} paid successfully by cash.`)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPaymentError(error.message || "Failed to complete cash payment.")
      } else {
        setPaymentError("Failed to complete cash payment.")
      }
    } finally {
      setPaymentActionLoading(false)
    }
  }

  async function handleCardEntry() {
    if (!createdOrder) return

    setPaymentActionLoading(true)
    setPaymentError("")

    try {
      const payment = await createPayment({
        orderPublicId: createdOrder.publicId,
        provider: "Stripe",
        paymentMethod: "Card",
      })

      if (!payment.providerClientSecret) {
        throw new Error("Stripe client secret is missing.")
      }

      setStripeClientSecret(payment.providerClientSecret)
      setPaymentMode("stripe")
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPaymentError(error.message || "Failed to start card payment.")
      } else {
        setPaymentError("Failed to start card payment.")
      }
    } finally {
      setPaymentActionLoading(false)
    }
  }

  function handlePosTerminal() {
    setPaymentError("")
    setPaymentMode("pos")
  }

  async function pollPaymentStatus(orderPublicId: string, orderNumber: string) {
    setPollingPayment(true)
    setPaymentError("")

    try {
      const maxAttempts = 15

      for (let i = 0; i < maxAttempts; i++) {
        const payment = await getPaymentByOrder(orderPublicId)

        if (payment.status === "Paid") {
          resetAfterPayment()
          showToast("success", `Order ${orderNumber} paid successfully.`)
          return
        }

        if (payment.status === "Failed") {
          setPaymentError(payment.failureReason || "Payment failed.")
          return
        }

        await new Promise((resolve) => setTimeout(resolve, 1500))
      }

      setPaymentError("Payment confirmation timed out. Please check the order status.")
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPaymentError(error.message || "Failed to confirm payment status.")
      } else {
        setPaymentError("Failed to confirm payment status.")
      }
    } finally {
      setPollingPayment(false)
    }
  }

  function resetAfterPayment() {
    setPaymentDialogOpen(false)
    setPaymentMode("menu")
    setCreatedOrder(null)
    setStripeClientSecret(null)
    setCartItems([])
    setCustomerName("")
    setPaymentError("")
    setCheckoutError("")
  }

  function closePaymentDialog() {
    setPaymentDialogOpen(false)
    setPaymentMode("menu")
    setStripeClientSecret(null)
    setPaymentError("")
    setPollingPayment(false)
  }

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

  function addToCart(product: ProductItem) {
    if (!selectedCategory) return

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

  function goOrders() {
    navigate("/portal/orders")
  }

  function goProfile() {
    navigate("/portal/profile")
  }

  function logout() {
    auth.logout()
    navigate("/portal/login", { replace: true })
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
      <Toast open={toast.open} type={toast.type} message={toast.message} />

      <div className="mx-auto flex max-w-7xl flex-col gap-6">
        <section className="rounded-3xl border border-slate-200 bg-white px-6 py-6 sm:px-8">
          <div className="flex flex-col gap-6 xl:flex-row xl:items-start xl:justify-between">
            <div className="min-w-0">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Current merchant
              </div>

              <h1 className="mt-2 truncate text-3xl font-semibold tracking-tight text-slate-800">
                {auth.currentTenantName || "Workspace"}
              </h1>

              <div className="mt-4 flex flex-wrap items-center gap-3">
                <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                  Role: {auth.currentMembership?.role || "Unknown"}
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
                label="Profile"
                icon={<HiOutlineUserCircle className="h-5 w-5" />}
                onClick={goProfile}
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

        {pageError ? (
          <div
            className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
            role="alert"
          >
            {pageError}
          </div>
        ) : null}

        <section className="grid gap-6 xl:grid-cols-[240px_minmax(0,1fr)_360px]">
          <CategorySidebar
            categories={categories}
            selectedCategoryId={selectedCategoryId}
            loading={loadingCategories}
            onSelect={setSelectedCategoryId}
          />

          <ProductGrid
            categoryName={selectedCategory?.name || ""}
            loading={loadingProducts}
            products={products}
            onAdd={addToCart}
          />

          <CurrentOrderPanel
            cartItems={cartItems}
            totalItems={totalItems}
            subtotal={subtotal}
            customerName={customerName}
            onCustomerNameChange={setCustomerName}
            onIncreaseQuantity={increaseQuantity}
            onDecreaseQuantity={decreaseQuantity}
            onRemoveItem={removeItem}
            onClearCart={clearCart}
            onCheckout={openCheckoutDialog}
          />
        </section>

        <CheckoutReviewDialog
          open={checkoutDialogOpen}
          cartItems={cartItems}
          customerName={customerName}
          subtotal={subtotal}
          submitting={checkoutSubmitting}
          errorMessage={checkoutError}
          onClose={() => setCheckoutDialogOpen(false)}
          onPay={() => void handlePayFromReview()}
        />

        <PaymentDialog
          open={paymentDialogOpen}
          order={createdOrder}
          paymentMode={paymentMode}
          stripePromise={stripePromise}
          stripeClientSecret={stripeClientSecret}
          pollingPayment={pollingPayment}
          actionLoading={paymentActionLoading}
          errorMessage={paymentError}
          onClose={closePaymentDialog}
          onCashPayment={() => void handleCashPayment()}
          onCardEntry={() => void handleCardEntry()}
          onPosTerminal={handlePosTerminal}
          onStripeSuccess={pollPaymentStatus}
          onBackToMethods={() => {
            setPaymentMode("menu")
            setPaymentError("")
          }}
        />
      </div>
    </div>
  )
}