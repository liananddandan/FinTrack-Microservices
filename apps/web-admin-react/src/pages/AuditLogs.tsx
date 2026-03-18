import { useEffect, useState } from "react"
import { getAuditLogs, type AuditLogItem } from "../api/audit-log"
import "./AuditLogs.css"

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
    setFilters({
      actionType: "",
      fromUtc: "",
      toUtc: "",
    })

    void loadLogsWith(
      {
        actionType: "",
        fromUtc: "",
        toUtc: "",
      },
      1
    )
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
    <div className="audit-page">
      <div className="audit-topbar">
        <div>
          <h2 className="audit-title">Audit Logs</h2>
          <p className="audit-subtitle">
            Review administrative actions and membership-related activities in
            the current organization.
          </p>
        </div>
      </div>

      <div className="audit-filter-card">
        <div className="audit-filters">
          <select
            className="filter-item"
            value={filters.actionType}
            onChange={(e) => updateFilter("actionType", e.target.value)}
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
            className="filter-item filter-date"
            type="datetime-local"
            value={filters.fromUtc}
            onChange={(e) => updateFilter("fromUtc", e.target.value)}
          />

          <input
            className="filter-item filter-date"
            type="datetime-local"
            value={filters.toUtc}
            onChange={(e) => updateFilter("toUtc", e.target.value)}
          />

          <button className="primary-btn" onClick={() => void handleSearch()}>
            Search
          </button>
          <button className="secondary-btn" onClick={resetFilters}>
            Reset
          </button>
        </div>
      </div>

      <div className="audit-table-card">
        {loading ? <div className="loading-block">Loading audit logs...</div> : null}
        {errorMessage ? <div className="alert error">{errorMessage}</div> : null}

        <div className="table-wrap">
          <table className="audit-table">
            <thead>
              <tr>
                <th>Time</th>
                <th>Actor</th>
                <th>Action</th>
                <th>Target</th>
                <th>Summary</th>
                <th>Details</th>
              </tr>
            </thead>

            <tbody>
              {logs.length === 0 ? (
                <tr>
                  <td colSpan={6} className="empty-cell">
                    No audit logs found.
                  </td>
                </tr>
              ) : (
                logs.map((row) => (
                  <tr key={row.publicId}>
                    <td>{formatDateTime(row.occurredAtUtc)}</td>
                    <td>{row.actorDisplayName || "-"}</td>
                    <td>{row.actionType}</td>
                    <td>{row.targetDisplay || "-"}</td>
                    <td>{row.summary}</td>
                    <td>
                      <button className="link-btn" onClick={() => openDetails(row)}>
                        View
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="audit-pagination">
          <button
            className="secondary-btn"
            disabled={pageNumber <= 1}
            onClick={() => void handlePageChange(pageNumber - 1)}
          >
            Previous
          </button>

          <span className="pagination-text">
            Page {pageNumber} of {totalPages} · Total {totalCount}
          </span>

          <button
            className="secondary-btn"
            disabled={pageNumber >= totalPages}
            onClick={() => void handlePageChange(pageNumber + 1)}
          >
            Next
          </button>
        </div>
      </div>

      {detailsVisible && selectedLog ? (
        <div className="dialog-backdrop">
          <div className="dialog-card">
            <div className="dialog-header">
              <div className="dialog-title">Audit Log Details</div>
            </div>

            <div className="detail-row">
              <strong>Time:</strong> {formatDateTime(selectedLog.occurredAtUtc)}
            </div>
            <div className="detail-row">
              <strong>Actor:</strong> {selectedLog.actorDisplayName || "-"}
            </div>
            <div className="detail-row">
              <strong>Action:</strong> {selectedLog.actionType}
            </div>
            <div className="detail-row">
              <strong>Target:</strong> {selectedLog.targetDisplay || "-"}
            </div>
            <div className="detail-row">
              <strong>Source:</strong> {selectedLog.source || "-"}
            </div>
            <div className="detail-row">
              <strong>Correlation Id:</strong> {selectedLog.correlationId || "-"}
            </div>

            <div className="detail-json-title">Metadata</div>
            <pre className="detail-json">
              {formatJson(selectedLog.metadataJson)}
            </pre>

            <div className="dialog-footer">
              <button className="secondary-btn" onClick={closeDetails}>
                Close
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}