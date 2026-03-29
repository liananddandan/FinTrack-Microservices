import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import {
  getOrderSummary,
  getOrders,
  type OrderSummaryDto,
  type OrderListItemDto,
} from "../api/order-admin"
import {
  HiOutlineBuildingOffice2,
  HiOutlineBanknotes,
  HiOutlineClipboardDocumentList,
  HiOutlineChartBar,
  HiOutlineXCircle,
  HiOutlineArrowRight,
} from "react-icons/hi2"

function MetricCard({
  label,
  value,
  icon,
}: {
  label: string
  value: string | number
  icon: React.ReactNode
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5">
      <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
        {icon}
      </div>
      <p className="mt-4 text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-slate-800">{value}</p>
    </div>
  )
}

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

export default function Overview() {
  const navigate = useNavigate()
  const auth = useAuth()

  const [loading, setLoading] = useState(false)
  const [summary, setSummary] = useState<OrderSummaryDto | null>(null)
  const [recentOrders, setRecentOrders] = useState<OrderListItemDto[]>([])
  const [errorMessage, setErrorMessage] = useState("")

  useEffect(() => {
    void load()
  }, [])

  async function load() {
    setLoading(true)
    setErrorMessage("")

    const today = new Date()
    const fromUtc = new Date(today)
    fromUtc.setHours(0, 0, 0, 0)

    const toUtc = new Date(today)
    toUtc.setHours(23, 59, 59, 999)

    const [summaryResult, ordersResult] = await Promise.allSettled([
      getOrderSummary({
        fromUtc: fromUtc.toISOString(),
        toUtc: toUtc.toISOString(),
      }),
      getOrders({
        pageNumber: 1,
        pageSize: 5,
      }),
    ])

    if (summaryResult.status === "fulfilled") {
      setSummary(summaryResult.value)
    } else {
      setErrorMessage("Failed to load overview data.")
    }

    if (ordersResult.status === "fulfilled") {
      setRecentOrders(ordersResult.value.items)
    } else {
      setErrorMessage("Failed to load overview data.")
    }

    setLoading(false)
  }

  function formatAmount(value: number) {
    return new Intl.NumberFormat("en-NZ", {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value)
  }

  function formatDateTime(value: string) {
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

  function goOrders() {
    navigate("/orders")
  }

  function goMenu() {
    navigate("/menu")
  }

  function goOrderDetail(row: OrderListItemDto) {
    navigate(`/orders/${row.publicId}`)
  }

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
        <div className="flex items-start gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
            <HiOutlineBuildingOffice2 className="h-6 w-6" />
          </div>

          <div>
            <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
              {auth.currentTenantName || "Overview"}
            </h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              Track daily sales and recent order activity for the current tenant.
            </p>

            <div className="mt-5 flex flex-wrap items-center gap-3">
              <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                Admin: {auth.userName || auth.userEmail || "-"}
              </span>
            </div>
          </div>
        </div>
      </section>

      {loading ? (
        <div className="text-sm text-slate-500">Loading overview...</div>
      ) : null}

      {errorMessage ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Today revenue"
          value={summary ? formatAmount(summary.totalRevenue) : "-"}
          icon={<HiOutlineBanknotes className="h-5 w-5" />}
        />

        <MetricCard
          label="Orders today"
          value={summary?.orderCount ?? "-"}
          icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
        />

        <MetricCard
          label="Avg order value"
          value={summary ? formatAmount(summary.averageOrderValue) : "-"}
          icon={<HiOutlineChartBar className="h-5 w-5" />}
        />

        <MetricCard
          label="Cancelled orders"
          value={summary?.cancelledOrderCount ?? "-"}
          icon={<HiOutlineXCircle className="h-5 w-5" />}
        />
      </section>

      <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
        <div className="mb-4 flex items-center justify-between gap-4">
          <div>
            <h2 className="text-lg font-semibold text-slate-800">
              Recent orders
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Latest order activity in this tenant.
            </p>
          </div>

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={goMenu}
              className="inline-flex items-center gap-1 text-sm text-slate-600 transition hover:text-indigo-600"
            >
              Menu
            </button>

            <button
              type="button"
              onClick={goOrders}
              className="inline-flex items-center gap-1 text-sm text-indigo-600 transition hover:text-indigo-500"
            >
              View all
              <HiOutlineArrowRight className="h-4 w-4" />
            </button>
          </div>
        </div>

        <div className="overflow-x-auto rounded-2xl border border-slate-200">
          <table className="min-w-full border-collapse text-left">
            <thead className="bg-slate-50">
              <tr className="text-sm text-slate-600">
                <th className="px-4 py-3 font-medium">Order</th>
                <th className="px-4 py-3 font-medium">Customer</th>
                <th className="px-4 py-3 font-medium">Amount</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Payment</th>
                <th className="px-4 py-3 font-medium">Created at</th>
              </tr>
            </thead>

            <tbody>
              {recentOrders.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-10 text-center text-sm text-slate-500"
                  >
                    No orders found.
                  </td>
                </tr>
              ) : (
                recentOrders.map((row) => (
                  <tr
                    key={row.publicId}
                    className="cursor-pointer border-t border-slate-200 hover:bg-slate-50"
                    onClick={() => goOrderDetail(row)}
                  >
                    <td className="px-4 py-4 text-sm font-medium text-slate-800">
                      {row.orderNumber}
                    </td>

                    <td className="px-4 py-4 text-sm text-slate-600">
                      {row.customerName || "Walk-in"}
                    </td>

                    <td className="px-4 py-4 text-sm font-medium text-slate-800">
                      {formatAmount(row.totalAmount)}
                    </td>

                    <td className="px-4 py-4">
                      <Badge tone={statusTone(row.status)}>{row.status}</Badge>
                    </td>

                    <td className="px-4 py-4">
                      <Badge tone={paymentTone(row.paymentStatus)}>
                        {row.paymentStatus}
                      </Badge>
                    </td>

                    <td className="px-4 py-4 text-sm text-slate-600">
                      {formatDateTime(row.createdAt)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  )
}