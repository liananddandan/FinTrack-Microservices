import { useEffect, useState } from "react"
import { getAuditLogs, type AuditLogItem } from "../api/audit-log"
import {
  HiOutlineDocumentMagnifyingGlass,
  HiOutlineFunnel,
  HiOutlineArrowPath,
  HiOutlineArrowRight,
} from "react-icons/hi2"

type FiltersState = {
  actionType: string
  fromUtc: string
  toUtc: string
}

export default function AuditLogs() {
  const [loading, setLoading] = useState(false)
  const [logs, setLogs] = useState<AuditLogItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [pageNumber, setPageNumber] = useState(1)
  const [pageSize] = useState(10)
  const [errorMessage, setErrorMessage] = useState("")

  const [filters, setFilters] = useState<FiltersState>({
    actionType: "",
    fromUtc: "",
    toUtc: "",
  })

  const [detailsVisible, setDetailsVisible] = useState(false)
  const [selectedLog, setSelectedLog] = useState<AuditLogItem | null>(null)

  useEffect(() => {
    void loadLogs(1)
  }, [])

  async function loadLogs(page = 1) {
    setLoading(true)
    setErrorMessage("")
    setPageNumber(page)

    try {
      const result = await getAuditLogs({
        actionType: filters.actionType || undefined,
        fromUtc: filters.fromUtc || undefined,
        toUtc: filters.toUtc || undefined,
        pageNumber: page,
        pageSize,
      })

      setLogs(result.items)
      setTotalCount(result.totalCount)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to load audit logs.")
      } else {
        setErrorMessage("Failed to load audit logs.")
      }
    } finally {
      setLoading(false)
    }
  }

  function updateFilter<K extends keyof FiltersState>(
    key: K,
    value: FiltersState[K]
  ) {
    setFilters((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  function resetFilters() {
    const nextFilters: FiltersState = {
      actionType: "",
      fromUtc: "",
      toUtc: "",
    }

    setFilters(nextFilters)
    void loadLogsWith(nextFilters, 1)
  }

  async function loadLogsWith(nextFilters: FiltersState, page = 1) {
    setLoading(true)
    setErrorMessage("")
    setPageNumber(page)

    try {
      const result = await getAuditLogs({
        actionType: nextFilters.actionType || undefined,
        fromUtc: nextFilters.fromUtc || undefined,
        toUtc: nextFilters.toUtc || undefined,
        pageNumber: page,
        pageSize,
      })

      setLogs(result.items)
      setTotalCount(result.totalCount)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setErrorMessage(error.message || "Failed to load audit logs.")
      } else {
        setErrorMessage("Failed to load audit logs.")
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleSearch() {
    await loadLogsWith(filters, 1)
  }

  async function handlePageChange(page: number) {
    await loadLogsWith(filters, page)
  }

  function openDetails(log: AuditLogItem) {
    setSelectedLog(log)
    setDetailsVisible(true)
  }

  function closeDetails() {
    setDetailsVisible(false)
    setSelectedLog(null)
  }

  function formatDateTime(value: string) {
    if (!value) return "-"
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value
    return date.toLocaleString()
  }

  function formatJson(value: string) {
    try {
      return JSON.stringify(JSON.parse(value), null, 2)
    } catch {
      return value || "{}"
    }
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      {/* Header */}
      <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
        <div className="flex items-start gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
            <HiOutlineDocumentMagnifyingGlass className="h-6 w-6" />
          </div>

          <div>
            <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
              Audit Logs
            </h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              Review administrative actions and membership-related activities in
              the current organization.
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

        <div className="grid gap-3 md:grid-cols-[1.4fr_1fr_1fr_auto_auto]">
          <select
            value={filters.actionType}
            onChange={(e) => updateFilter("actionType", e.target.value)}
            className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
          >
            <option value="">Action type</option>
            <option value="Membership.Invited">Membership.Invited</option>
            <option value="Membership.InvitationResent">
              Membership.InvitationResent
            </option>
            <option value="Membership.Accepted">Membership.Accepted</option>
            <option value="Membership.Removed">Membership.Removed</option>
            <option value="Membership.RoleChanged">
              Membership.RoleChanged
            </option>
          </select>

          <input
            type="datetime-local"
            value={filters.fromUtc}
            onChange={(e) => updateFilter("fromUtc", e.target.value)}
            className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
          />

          <input
            type="datetime-local"
            value={filters.toUtc}
            onChange={(e) => updateFilter("toUtc", e.target.value)}
            className="h-11 rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
          />

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
        <div className="text-sm text-slate-500">Loading audit logs...</div>
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
              Audit log records
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Page {pageNumber} of {totalPages} · Total {totalCount}
            </p>
          </div>
        </div>

        <div className="overflow-x-auto rounded-2xl border border-slate-200">
          <table className="min-w-full border-collapse text-left">
            <thead className="bg-slate-50">
              <tr className="text-sm text-slate-600">
                <th className="px-4 py-3 font-medium">Time</th>
                <th className="px-4 py-3 font-medium">Actor</th>
                <th className="px-4 py-3 font-medium">Action</th>
                <th className="px-4 py-3 font-medium">Target</th>
                <th className="px-4 py-3 font-medium">Summary</th>
                <th className="px-4 py-3 font-medium">Details</th>
              </tr>
            </thead>

            <tbody>
              {logs.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-10 text-center text-sm text-slate-500"
                  >
                    No audit logs found.
                  </td>
                </tr>
              ) : (
                logs.map((row) => (
                  <tr
                    key={row.publicId}
                    className="border-t border-slate-200 hover:bg-slate-50"
                  >
                    <td className="px-4 py-4 text-sm text-slate-600">
                      {formatDateTime(row.occurredAtUtc)}
                    </td>
                    <td className="px-4 py-4 text-sm text-slate-800">
                      {row.actorDisplayName || "-"}
                    </td>
                    <td className="px-4 py-4 text-sm font-medium text-slate-800">
                      {row.actionType}
                    </td>
                    <td className="px-4 py-4 text-sm text-slate-600">
                      {row.targetDisplay || "-"}
                    </td>
                    <td className="px-4 py-4 text-sm text-slate-600">
                      {row.summary}
                    </td>
                    <td className="px-4 py-4">
                      <button
                        type="button"
                        onClick={() => openDetails(row)}
                        className="inline-flex items-center gap-1 text-sm text-indigo-600 transition hover:text-indigo-500"
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
            disabled={pageNumber <= 1}
            onClick={() => void handlePageChange(pageNumber - 1)}
            className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Previous
          </button>

          <span className="text-sm text-slate-500">
            Page {pageNumber} of {totalPages}
          </span>

          <button
            type="button"
            disabled={pageNumber >= totalPages}
            onClick={() => void handlePageChange(pageNumber + 1)}
            className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600 disabled:cursor-not-allowed disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </section>

      {/* Detail dialog */}
      {detailsVisible && selectedLog ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4">
          <div className="w-full max-w-3xl rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
            <h2 className="text-xl font-semibold text-slate-800">
              Audit log details
            </h2>

            <div className="mt-6 rounded-2xl border border-slate-100 bg-slate-50 px-5">
              <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3">
                <span className="text-sm text-slate-500">Time</span>
                <span className="text-right text-sm font-medium text-slate-800">
                  {formatDateTime(selectedLog.occurredAtUtc)}
                </span>
              </div>

              <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3">
                <span className="text-sm text-slate-500">Actor</span>
                <span className="text-right text-sm font-medium text-slate-800">
                  {selectedLog.actorDisplayName || "-"}
                </span>
              </div>

              <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3">
                <span className="text-sm text-slate-500">Action</span>
                <span className="text-right text-sm font-medium text-slate-800">
                  {selectedLog.actionType}
                </span>
              </div>

              <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3">
                <span className="text-sm text-slate-500">Target</span>
                <span className="text-right text-sm font-medium text-slate-800">
                  {selectedLog.targetDisplay || "-"}
                </span>
              </div>

              <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3">
                <span className="text-sm text-slate-500">Source</span>
                <span className="text-right text-sm font-medium text-slate-800">
                  {selectedLog.source || "-"}
                </span>
              </div>

              <div className="flex items-start justify-between gap-4 py-3">
                <span className="text-sm text-slate-500">Correlation Id</span>
                <span className="text-right text-sm font-medium text-slate-800 break-all">
                  {selectedLog.correlationId || "-"}
                </span>
              </div>
            </div>

            <div className="mt-6">
              <h3 className="text-sm font-semibold text-slate-800">Metadata</h3>
              <pre className="mt-3 overflow-x-auto rounded-2xl border border-slate-200 bg-slate-50 p-4 text-xs leading-6 text-slate-700">
                {formatJson(selectedLog.metadataJson)}
              </pre>
            </div>

            <div className="mt-6 flex justify-end">
              <button
                type="button"
                onClick={closeDetails}
                className="inline-flex items-center rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}