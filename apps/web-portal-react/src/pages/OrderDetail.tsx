import { useEffect, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
  HiOutlineArrowLeft,
  HiOutlineClipboardDocumentList,
  HiOutlineXMark,
} from "react-icons/hi2"
import { cancelOrder, getOrderById, type OrderDto } from "../api/order"

function Badge({
  children,
  tone = "default",
}: {
  children: React.ReactNode
  tone?: "default" | "success" | "warning" | "danger"
}) {
  const className =
    tone === "success"
      ? "bg-emerald-50 text-emerald-700 border-emerald-200"
      : tone === "warning"
      ? "bg-amber-50 text-amber-700 border-amber-200"
      : tone === "danger"
      ? "bg-rose-50 text-rose-700 border-rose-200"
      : "bg-slate-100 text-slate-700 border-slate-200"

  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-medium ${className}`}
    >
      {children}
    </span>
  )
}

function InfoRow({
  label,
  value,
}: {
  label: string
  value: string
}) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm text-slate-500">{label}</span>
      <span className="text-right text-sm font-medium text-slate-800 break-all">
        {value}
      </span>
    </div>
  )
}

export default function OrderDetail() {
  const { orderPublicId } = useParams()
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [detail, setDetail] = useState<OrderDto | null>(null)
  const [errorMessage, setErrorMessage] = useState("")
  const [actionMessage, setActionMessage] = useState("")

  useEffect(() => {
    void load()
  }, [orderPublicId])

  async function load() {
    if (!orderPublicId) {
      setErrorMessage("Order id is missing.")
      return
    }

    setLoading(true)
    setErrorMessage("")
    setActionMessage("")

    try {
      const result = await getOrderById(orderPublicId)
      setDetail(result)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to load order detail.")
      } else {
        setErrorMessage("Failed to load order detail.")
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleCancelOrder() {
    if (!detail) {
      return
    }

    const confirmed = window.confirm(
      `Cancel order ${detail.orderNumber}?`
    )

    if (!confirmed) {
      return
    }

    setActionLoading(true)
    setErrorMessage("")
    setActionMessage("")

    try {
      await cancelOrder(detail.publicId)
      setActionMessage("Order cancelled successfully.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to cancel order.")
      } else {
        setErrorMessage("Failed to cancel order.")
      }
    } finally {
      setActionLoading(false)
    }
  }

  function formatAmount(value: number) {
    return new Intl.NumberFormat("en-NZ", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value)
  }

  function formatDateTime(value?: string | null) {
    if (!value) return "-"
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value
    return date.toLocaleString()
  }

  function statusTone(status: string) {
    switch (status) {
      case "Completed":
        return "success"
      case "Pending":
        return "warning"
      case "Cancelled":
        return "danger"
      default:
        return "default"
    }
  }

  function paymentTone(status: string) {
    switch (status) {
      case "Paid":
        return "success"
      case "Pending":
        return "warning"
      case "Failed":
        return "danger"
      default:
        return "default"
    }
  }

  const canCancel =
    !!detail &&
    detail.status !== "Cancelled"

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        <div className="flex justify-start">
          <button
            type="button"
            onClick={() => navigate("/portal/orders")}
            className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
          >
            <HiOutlineArrowLeft className="h-4 w-4" />
            Back to orders
          </button>
        </div>

        {loading ? (
          <div className="text-sm text-slate-500">Loading order detail...</div>
        ) : null}

        {actionMessage ? (
          <div
            className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
            role="status"
          >
            {actionMessage}
          </div>
        ) : null}

        {errorMessage ? (
          <div
            className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
            role="alert"
          >
            {errorMessage}
          </div>
        ) : null}

        {detail ? (
          <>
            <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
              <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
                <div>
                  <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
                    <HiOutlineClipboardDocumentList className="h-6 w-6" />
                  </div>

                  <div className="mt-4 text-sm font-medium text-indigo-600">
                    Order
                  </div>

                  <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-800">
                    {detail.orderNumber}
                  </h1>

                  <p className="mt-2 text-sm text-slate-500">
                    {detail.customerName || "Walk-in customer"}
                  </p>
                </div>

                <div className="lg:text-right">
                  <div className="text-2xl font-semibold text-slate-800">
                    {formatAmount(detail.totalAmount)}
                  </div>

                  <div className="mt-3 flex flex-wrap gap-2 lg:justify-end">
                    <Badge tone={statusTone(detail.status)}>
                      {detail.status}
                    </Badge>

                    <Badge tone={paymentTone(detail.paymentStatus)}>
                      {detail.paymentStatus}
                    </Badge>
                  </div>

                  {canCancel ? (
                    <div className="mt-5 lg:flex lg:justify-end">
                      <button
                        type="button"
                        disabled={actionLoading}
                        onClick={() => void handleCancelOrder()}
                        className="inline-flex items-center gap-2 rounded-full bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-500 disabled:cursor-not-allowed disabled:opacity-60"
                      >
                        <HiOutlineXMark className="h-4 w-4" />
                        {actionLoading ? "Cancelling..." : "Cancel order"}
                      </button>
                    </div>
                  ) : null}
                </div>
              </div>
            </section>

            <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
              <h2 className="text-lg font-semibold text-slate-800">
                Order items
              </h2>

              <div className="mt-5 overflow-x-auto rounded-2xl border border-slate-200">
                <table className="min-w-full border-collapse text-left">
                  <thead className="bg-slate-50">
                    <tr className="text-sm text-slate-600">
                      <th className="px-4 py-3 font-medium">Item</th>
                      <th className="px-4 py-3 font-medium">Quantity</th>
                      <th className="px-4 py-3 font-medium">Unit price</th>
                      <th className="px-4 py-3 font-medium">Total</th>
                      <th className="px-4 py-3 font-medium">Notes</th>
                    </tr>
                  </thead>

                  <tbody>
                    {detail.items.length === 0 ? (
                      <tr>
                        <td
                          colSpan={5}
                          className="px-4 py-10 text-center text-sm text-slate-500"
                        >
                          No order items found.
                        </td>
                      </tr>
                    ) : (
                      detail.items.map((item, index) => (
                        <tr
                          key={`${item.productPublicId}-${index}`}
                          className="border-t border-slate-200"
                        >
                          <td className="px-4 py-4 text-sm text-slate-800">
                            {item.productNameSnapshot}
                          </td>

                          <td className="px-4 py-4 text-sm text-slate-600">
                            {item.quantity}
                          </td>

                          <td className="px-4 py-4 text-sm text-slate-600">
                            {formatAmount(item.unitPrice)}
                          </td>

                          <td className="px-4 py-4 text-sm font-medium text-slate-800">
                            {formatAmount(item.lineTotal)}
                          </td>

                          <td className="px-4 py-4 text-sm text-slate-600">
                            {item.notes || "-"}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </section>

            <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
              <h2 className="text-lg font-semibold text-slate-800">
                Order information
              </h2>

              <div className="mt-5 rounded-2xl border border-slate-100 bg-slate-50 px-5">
                <InfoRow label="Order number" value={detail.orderNumber} />
                <InfoRow
                  label="Customer"
                  value={detail.customerName || "Walk-in customer"}
                />
                <InfoRow
                  label="Customer phone"
                  value={detail.customerPhone || "-"}
                />
                <InfoRow
                  label="Created by"
                  value={detail.createdByUserNameSnapshot}
                />
                <InfoRow
                  label="Payment method"
                  value={detail.paymentMethod}
                />
                <InfoRow
                  label="Created at"
                  value={formatDateTime(detail.createdAt)}
                />
                <InfoRow
                  label="Paid at"
                  value={formatDateTime(detail.paidAt)}
                />
                <InfoRow label="Status" value={detail.status} />
                <InfoRow
                  label="Payment status"
                  value={detail.paymentStatus}
                />
                <InfoRow
                  label="Subtotal"
                  value={formatAmount(detail.subtotalAmount)}
                />
                <InfoRow label="GST" value={formatAmount(detail.gstAmount)} />
                <InfoRow
                  label="Discount"
                  value={formatAmount(detail.discountAmount)}
                />
                <InfoRow label="Total" value={formatAmount(detail.totalAmount)} />
              </div>
            </section>
          </>
        ) : null}
      </div>
    </div>
  )
}