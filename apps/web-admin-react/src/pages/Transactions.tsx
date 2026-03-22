import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import {
  approveProcurement,
  getTenantTransactions,
  rejectProcurement,
} from "../api/transaction-admin"
import {
  HiOutlineFunnel,
  HiOutlineArrowPath,
  HiOutlineArrowRight,
  HiOutlineClipboardDocumentList
} from "react-icons/hi2"

type TransactionItem = {
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

type QueryState = {
  type: string
  status: string
  pageNumber: number
  pageSize: number
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

export default function Transactions() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [items, setItems] = useState<TransactionItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [errorMessage, setErrorMessage] = useState("")
  const [actionMessage, setActionMessage] = useState("")

  const [query, setQuery] = useState<QueryState>({
    type: "",
    status: "",
    pageNumber: 1,
    pageSize: 10,
  })

  useEffect(() => {
    void load()
  }, [])

  async function load() {
    setLoading(true)
    setErrorMessage("")

    try {
      const result = await getTenantTransactions({
        type: query.type || undefined,
        status: query.status || undefined,
        pageNumber: query.pageNumber,
        pageSize: query.pageSize,
      })

      setItems(result.items)
      setTotalCount(result.totalCount)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to load transactions.")
      } else {
        setErrorMessage("Failed to load transactions.")
      }
    } finally {
      setLoading(false)
    }
  }

  function updateQuery<K extends keyof QueryState>(key: K, value: QueryState[K]) {
    setQuery((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  function resetFilters() {
    const nextQuery: QueryState = {
      type: "",
      status: "",
      pageNumber: 1,
      pageSize: 10,
    }

    setQuery(nextQuery)
    void loadWith(nextQuery)
  }

  async function loadWith(nextQuery: QueryState) {
    setLoading(true)
    setErrorMessage("")
    setActionMessage("")

    try {
      const result = await getTenantTransactions({
        type: nextQuery.type || undefined,
        status: nextQuery.status || undefined,
        pageNumber: nextQuery.pageNumber,
        pageSize: nextQuery.pageSize,
      })

      setItems(result.items)
      setTotalCount(result.totalCount)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to load transactions.")
      } else {
        setErrorMessage("Failed to load transactions.")
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleSearch() {
    const nextQuery = {
      ...query,
      pageNumber: 1,
    }

    setQuery(nextQuery)
    await loadWith(nextQuery)
  }

  async function handlePageChange(page: number) {
    const nextQuery = {
      ...query,
      pageNumber: page,
    }

    setQuery(nextQuery)
    await loadWith(nextQuery)
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

  function goTransactionDetail(row: TransactionItem) {
    console.log("go detail", row.transactionPublicId)
    navigate(`/transactions/${row.transactionPublicId}`)
  }

  async function handleApprove(
    event: React.MouseEvent<HTMLButtonElement>,
    row: TransactionItem
  ) {
    event.stopPropagation()
    setActionMessage("")
    setErrorMessage("")

    try {
      await approveProcurement(row.transactionPublicId)
      setActionMessage("Procurement approved.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to approve.")
      } else {
        setErrorMessage("Failed to approve.")
      }
    }
  }

  async function handleReject(
    event: React.MouseEvent<HTMLButtonElement>,
    row: TransactionItem
  ) {
    event.stopPropagation()

    const reason = window.prompt("Please enter reject reason")

    if (!reason || !reason.trim()) {
      return
    }

    setActionMessage("")
    setErrorMessage("")

    try {
      await rejectProcurement(row.transactionPublicId, {
        reason: reason.trim(),
      })
      setActionMessage("Procurement rejected.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to reject.")
      } else {
        setErrorMessage("Failed to reject.")
      }
    }
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / query.pageSize))

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      <section className="rounded-3xl border border-slate-200 bg-white px-8 py-6 sm:px-10">
        <div className="flex items-start gap-4">
          {/* icon */}
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
            <HiOutlineClipboardDocumentList className="h-5 w-5" />
          </div>

          {/* text */}
          <div className="min-w-0">
            <h1 className="text-2xl font-semibold text-slate-800">
              Transactions
            </h1>

            <p className="mt-1 text-sm text-slate-500">
              Manage income entries and procurement workflows in this workspace.
            </p>

            <div className="mt-3 text-sm text-slate-500">
              Total {totalCount} records
            </div>
          </div>
        </div>
      </section>
      {/* Filters */}
      <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
        <div className="mb-4 flex items-center gap-2 text-slate-800">
          <HiOutlineFunnel className="h-5 w-5 text-slate-500" />
          <h2 className="text-base font-semibold">Filters</h2>
        </div>

        <div className="grid gap-3 md:grid-cols-[1fr_1fr_auto_auto]">
          <select
            value={query.type}
            onChange={(e) => updateQuery("type", e.target.value)}
            className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
          >
            <option value="">Type</option>
            <option value="Donation">Income</option>
            <option value="Procurement">Procurement</option>
          </select>

          <select
            value={query.status}
            onChange={(e) => updateQuery("status", e.target.value)}
            className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
          >
            <option value="">Status</option>
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Rejected">Rejected</option>
          </select>

          <button
            type="button"
            onClick={() => void handleSearch()}
            className="inline-flex h-11 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500"
          >
            Search
          </button>

          <button
            type="button"
            onClick={resetFilters}
            className="inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
          >
            <HiOutlineArrowPath className="h-4 w-4" />
            Reset
          </button>
        </div>
      </section>

      {loading ? (
        <div className="text-sm text-slate-500">Loading transactions...</div>
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

      {/* Table */}
      <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
        <div className="mb-4 flex items-center justify-between gap-4">
          <div>
            <h2 className="text-base font-semibold text-slate-800">
              Transaction list
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Page {query.pageNumber} of {totalPages} · Total {totalCount}
            </p>
          </div>
        </div>

        <div className="overflow-x-auto rounded-2xl border border-slate-200">
          <table className="min-w-full border-collapse text-left">
            <thead className="bg-slate-50">
              <tr className="text-sm text-slate-600">
                <th className="px-4 py-3 font-medium">Title</th>
                <th className="px-4 py-3 font-medium">Type</th>
                <th className="px-4 py-3 font-medium">Amount</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Created at</th>
                <th className="px-4 py-3 font-medium">Action</th>
                <th className="px-4 py-3 font-medium">View</th>
              </tr>
            </thead>

            <tbody>
              {items.length === 0 ? (
                <tr>
                  <td
                    colSpan={7}
                    className="px-4 py-10 text-center text-sm text-slate-500"
                  >
                    No transactions found.
                  </td>
                </tr>
              ) : (
                items.map((row) => (
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

                    <td className="px-4 py-4 text-sm text-slate-600">
                      {formatDateTime(row.createdAtUtc)}
                    </td>

                    <td className="px-4 py-4">
                      {row.type === "Procurement" && row.status === "Submitted" ? (
                        <div className="flex items-center gap-3">
                          <button
                            type="button"
                            className="text-sm font-medium text-emerald-700 transition hover:text-emerald-600"
                            onClick={(event) => void handleApprove(event, row)}
                          >
                            Approve
                          </button>
                          <button
                            type="button"
                            className="text-sm font-medium text-rose-700 transition hover:text-rose-600"
                            onClick={(event) => void handleReject(event, row)}
                          >
                            Reject
                          </button>
                        </div>
                      ) : (
                        <span className="text-sm text-slate-400">—</span>
                      )}
                    </td>

                    <td className="px-4 py-4">
                      <button
                        type="button"
                        className="inline-flex items-center gap-1 text-sm text-indigo-600 transition hover:text-indigo-500"
                        onClick={(event) => {
                          event.stopPropagation()
                          goTransactionDetail(row)
                        }}
                      >
                        View
                        <HiOutlineArrowRight className="h-4 w-4" />
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="mt-5 flex items-center justify-between gap-4">
          <button
            type="button"
            disabled={query.pageNumber <= 1}
            onClick={() => void handlePageChange(query.pageNumber - 1)}
            className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Previous
          </button>

          <span className="text-sm text-slate-500">
            Page {query.pageNumber} of {totalPages}
          </span>

          <button
            type="button"
            disabled={query.pageNumber >= totalPages}
            onClick={() => void handlePageChange(query.pageNumber + 1)}
            className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </section>
    </div>
  )
}