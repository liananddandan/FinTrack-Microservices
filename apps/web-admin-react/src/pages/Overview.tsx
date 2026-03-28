import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import {
  getTenantTransactionSummary,
  getTenantTransactions,
} from "../api/transaction-admin"
import {
  HiOutlineBuildingOffice2,
  HiOutlineBanknotes,
  HiOutlineArrowTrendingUp,
  HiOutlineDocumentText,
  HiOutlineClipboardDocumentList,
  HiOutlineUsers,
  HiOutlineEnvelope,
  HiOutlineShieldCheck,
  HiOutlineArrowRight,
} from "react-icons/hi2"

type SummaryDto = {
  tenantPublicId: string
  tenantName: string
  currentBalance: number
  totalDonationAmount: number
  totalProcurementAmount: number
  totalTransactionCount: number
}

type TransactionListItem = {
  transactionPublicId: string
  tenantPublicId: string
  tenantName: string
  type: string
  title: string
  amount: number
  currency: string
  status: string
  paymentStatus: string
  riskStatus: string
  createdAtUtc: string
}

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

function ActionCard({
  title,
  description,
  onClick,
  icon,
}: {
  title: string
  description: string
  onClick: () => void
  icon: React.ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="group rounded-2xl border border-slate-200 bg-white p-5 text-left transition hover:border-indigo-400 hover:bg-slate-50"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
          {icon}
        </div>

        <HiOutlineArrowRight className="mt-1 h-5 w-5 text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-600" />
      </div>

      <div className="mt-4">
        <p className="text-sm font-semibold text-slate-800">{title}</p>
        <p className="mt-2 text-sm leading-6 text-slate-600">{description}</p>
      </div>
    </button>
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
  const [summary, setSummary] = useState<SummaryDto | null>(null)
  const [recentTransactions, setRecentTransactions] = useState<
    TransactionListItem[]
  >([])
  const [summaryError, setSummaryError] = useState("")
  const [transactionsError, setTransactionsError] = useState("")

  useEffect(() => {
    void load()
  }, [])

  async function load() {
    setLoading(true)
    setSummaryError("")
    setTransactionsError("")

    const [summaryResult, transactionResult] = await Promise.allSettled([
      getTenantTransactionSummary(),
      getTenantTransactions({
        pageNumber: 1,
        pageSize: 5,
      }),
    ])

    if (summaryResult.status === "fulfilled") {
      setSummary(summaryResult.value)
    } else {
      setSummaryError("Failed to load summary.")
    }

    if (transactionResult.status === "fulfilled") {
      setRecentTransactions(transactionResult.value.items)
    } else {
      setTransactionsError("Failed to load recent transactions.")
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
      case "Approved":
        return "success"
      case "Submitted":
      case "Draft":
      case "Processing":
        return "warning"
      case "Failed":
      case "Rejected":
      case "Cancelled":
        return "danger"
      default:
        return "default"
    }
  }

  function executionTone(status: string) {
    switch (status) {
      case "Succeeded":
        return "success"
      case "Processing":
        return "warning"
      case "Failed":
        return "danger"
      default:
        return "default"
    }
  }

  function goTransactions() {
    navigate("/admin/transactions")
  }

  function goAuditLogs() {
    navigate("/admin/audit-logs")
  }

  function goMembers() {
    navigate("/admin/members")
  }

  function goInvitations() {
    navigate("/admin/invitations")
  }

  function goTransactionDetail(row: TransactionListItem) {
    navigate(`/admin/transactions/${row.transactionPublicId}`)
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
              {summary?.tenantName || auth.currentTenantName || "Overview"}
            </h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              Monitor tenant-wide transaction activity, member operations, and
              administrative workflows in one place.
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
        <div className="text-sm text-slate-500">Loading dashboard...</div>
      ) : null}

      {summaryError ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {summaryError}
        </div>
      ) : null}

      {transactionsError ? (
        <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {transactionsError}
        </div>
      ) : null}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          label="Current balance"
          value={summary ? formatAmount(summary.currentBalance) : "-"}
          icon={<HiOutlineBanknotes className="h-5 w-5" />}
        />

        <MetricCard
          label="Total income"
          value={summary ? formatAmount(summary.totalDonationAmount) : "-"}
          icon={<HiOutlineArrowTrendingUp className="h-5 w-5" />}
        />

        <MetricCard
          label="Total procurements"
          value={summary ? formatAmount(summary.totalProcurementAmount) : "-"}
          icon={<HiOutlineDocumentText className="h-5 w-5" />}
        />

        <MetricCard
          label="Total transactions"
          value={summary?.totalTransactionCount ?? "-"}
          icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
        />
      </section>

      <section>
        <div className="mb-4">
          <h2 className="text-2xl font-semibold text-slate-800">Admin actions</h2>
          <p className="mt-2 text-sm leading-6 text-slate-500">
            Open the main management areas for this tenant.
          </p>
        </div>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <ActionCard
            title="Transactions"
            description="View and manage tenant-wide transaction records."
            onClick={goTransactions}
            icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
          />

          <ActionCard
            title="Audit logs"
            description="Review important administrative and system actions."
            onClick={goAuditLogs}
            icon={<HiOutlineShieldCheck className="h-5 w-5" />}
          />

          <ActionCard
            title="Members"
            description="View and manage members in the current tenant."
            onClick={goMembers}
            icon={<HiOutlineUsers className="h-5 w-5" />}
          />

          <ActionCard
            title="Invitations"
            description="Manage pending and issued workspace invitations."
            onClick={goInvitations}
            icon={<HiOutlineEnvelope className="h-5 w-5" />}
          />
        </div>
      </section>

      <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
        <div className="mb-4 flex items-center justify-between gap-4">
          <div>
            <h2 className="text-lg font-semibold text-slate-800">
              Recent transactions
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Latest transaction activity in this tenant.
            </p>
          </div>

          <button
            type="button"
            onClick={goTransactions}
            className="inline-flex items-center gap-1 text-sm text-indigo-600 transition hover:text-indigo-500"
          >
            View all
            <HiOutlineArrowRight className="h-4 w-4" />
          </button>
        </div>

        <div className="overflow-x-auto rounded-2xl border border-slate-200">
          <table className="min-w-full border-collapse text-left">
            <thead className="bg-slate-50">
              <tr className="text-sm text-slate-600">
                <th className="px-4 py-3 font-medium">Title</th>
                <th className="px-4 py-3 font-medium">Type</th>
                <th className="px-4 py-3 font-medium">Amount</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Execution</th>
                <th className="px-4 py-3 font-medium">Created at</th>
              </tr>
            </thead>

            <tbody>
              {recentTransactions.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-10 text-center text-sm text-slate-500"
                  >
                    No transactions found.
                  </td>
                </tr>
              ) : (
                recentTransactions.map((row) => (
                  <tr
                    key={row.transactionPublicId}
                    className="cursor-pointer border-t border-slate-200 hover:bg-slate-50"
                    onClick={() => goTransactionDetail(row)}
                  >
                    <td className="px-4 py-4 text-sm font-medium text-slate-800">
                      {row.title}
                    </td>

                    <td className="px-4 py-4">
                      <Badge>
                        {row.type === "Donation" ? "Income" : row.type}
                      </Badge>
                    </td>

                    <td className="px-4 py-4 text-sm font-medium text-slate-800">
                      {row.amount} {row.currency}
                    </td>

                    <td className="px-4 py-4">
                      <Badge tone={statusTone(row.status)}>
                        {row.status}
                      </Badge>
                    </td>

                    <td className="px-4 py-4">
                      <Badge tone={executionTone(row.paymentStatus)}>
                        {row.paymentStatus}
                      </Badge>
                    </td>

                    <td className="px-4 py-4 text-sm text-slate-600">
                      {formatDateTime(row.createdAtUtc)}
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