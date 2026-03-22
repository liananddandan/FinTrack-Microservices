import { useEffect, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import {
  getMyTransactions,
  type TransactionListItem,
} from "../api/transactions"
import {
  HiOutlineFunnel,
  HiOutlineArrowPath,
  HiOutlineArrowRight,
  HiOutlineClipboardDocumentList,
} from "react-icons/hi2"

type Filters = {
  keyword: string
  type: string
  status: string
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

export default function MyTransactions() {
  const navigate = useNavigate()

  const [items, setItems] = useState<TransactionListItem[]>([])
  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")

  const [filters, setFilters] = useState<Filters>({
    keyword: "",
    type: "",
    status: "",
  })

  useEffect(() => {
    async function load() {
      setLoading(true)
      setErrorMessage("")

      try {
        const res = await getMyTransactions()
        setItems(res.items)
      } catch (err) {
        const message =
          err instanceof Error ? err.message : "Failed to load transactions."
        setErrorMessage(message)
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [])

  const filteredItems = useMemo(() => {
    const keyword = filters.keyword.trim().toLowerCase()

    return items.filter((item) => {
      const tenantName = item.tenantName?.toLowerCase?.() ?? ""
      const title = item.title?.toLowerCase?.() ?? ""
      const type = item.type?.toLowerCase?.() ?? ""

      const matchesKeyword =
        !keyword ||
        tenantName.includes(keyword) ||
        title.includes(keyword) ||
        type.includes(keyword)

      const matchesType = !filters.type || item.type === filters.type
      const matchesStatus = !filters.status || item.status === filters.status

      return matchesKeyword && matchesType && matchesStatus
    })
  }, [items, filters])

  function updateFilter<K extends keyof Filters>(key: K, value: Filters[K]) {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  function resetFilters() {
    setFilters({
      keyword: "",
      type: "",
      status: "",
    })
  }

  function formatDateTime(value: string) {
    if (!value) return "-"
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value
    return date.toLocaleString()
  }

  function amountText(row: TransactionListItem) {
    return `${row.amount} ${row.currency}`
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

  function goDetail(row: TransactionListItem) {
    navigate(`/portal/transactions/${row.transactionPublicId}`)
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto flex max-w-6xl flex-col gap-6">
        {/* Header */}
        <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
              <HiOutlineClipboardDocumentList className="h-6 w-6" />
            </div>

            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
                My transactions
              </h1>
              <p className="mt-2 max-w-3xl text-sm leading-6 text-slate-600">
                Review the records you created in the current workspace, including
                income entries and procurement requests.
              </p>
            </div>
          </div>
        </section>

        {/* Filters */}
        <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
          <div className="mb-4 flex items-center gap-2 text-slate-800">
            <HiOutlineFunnel className="h-5 w-5 text-slate-500" />
            <h2 className="text-base font-semibold">Filters</h2>
          </div>

          <div className="grid gap-3 md:grid-cols-[2fr_1fr_1fr_auto]">
            <input
              type="text"
              placeholder="Search by tenant, title, or type"
              value={filters.keyword}
              onChange={(e) => updateFilter("keyword", e.target.value)}
              className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />

            <select
              value={filters.type}
              onChange={(e) => updateFilter("type", e.target.value)}
              className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            >
              <option value="">Type</option>
              <option value="Donation">Income</option>
              <option value="Procurement">Procurement</option>
            </select>

            <select
              value={filters.status}
              onChange={(e) => updateFilter("status", e.target.value)}
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
              onClick={resetFilters}
              className="inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <HiOutlineArrowPath className="h-4 w-4" />
              Reset
            </button>
          </div>
        </section>

        {/* Table */}
        <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
          <div className="mb-4 flex items-center justify-between gap-4">
            <div>
              <h2 className="text-base font-semibold text-slate-800">
                Transaction list
              </h2>
              <p className="mt-1 text-sm text-slate-500">
                {filteredItems.length} item{filteredItems.length !== 1 ? "s" : ""}
              </p>
            </div>
          </div>

          {errorMessage ? (
            <div
              className="mb-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              role="alert"
            >
              {errorMessage}
            </div>
          ) : null}

          <div className="overflow-x-auto rounded-2xl border border-slate-200">
            <table className="min-w-full border-collapse text-left">
              <thead className="bg-slate-50">
                <tr className="text-sm text-slate-600">
                  <th className="px-4 py-3 font-medium">Tenant</th>
                  <th className="px-4 py-3 font-medium">Title</th>
                  <th className="px-4 py-3 font-medium">Type</th>
                  <th className="px-4 py-3 font-medium">Amount</th>
                  <th className="px-4 py-3 font-medium">Status</th>
                  <th className="px-4 py-3 font-medium">Created at</th>
                  <th className="px-4 py-3 font-medium">Action</th>
                </tr>
              </thead>

              <tbody>
                {loading ? (
                  <tr>
                    <td
                      colSpan={7}
                      className="px-4 py-10 text-center text-sm text-slate-500"
                    >
                      Loading...
                    </td>
                  </tr>
                ) : filteredItems.length === 0 ? (
                  <tr>
                    <td
                      colSpan={7}
                      className="px-4 py-10 text-center text-sm text-slate-500"
                    >
                      No transactions found.
                    </td>
                  </tr>
                ) : (
                  filteredItems.map((row) => (
                    <tr
                      key={row.transactionPublicId}
                      className="cursor-pointer border-t border-slate-200 hover:bg-slate-50"
                      onClick={() => goDetail(row)}
                    >
                      <td className="px-4 py-4 text-sm text-slate-700">
                        {row.tenantName || "—"}
                      </td>

                      <td className="px-4 py-4 text-sm font-medium text-slate-800">
                        {row.title}
                      </td>

                      <td className="px-4 py-4">
                        <Badge>{row.type === "Donation" ? "Income" : row.type}</Badge>
                      </td>

                      <td className="px-4 py-4 text-sm font-medium text-slate-800">
                        {amountText(row)}
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
                        <button
                          type="button"
                          className="inline-flex items-center gap-1 text-sm text-indigo-600 transition hover:text-indigo-500"
                          onClick={(e) => {
                            e.stopPropagation()
                            goDetail(row)
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
        </section>
      </div>
    </div>
  )
}