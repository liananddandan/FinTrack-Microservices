import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import {
  approveProcurement,
  getTenantTransactions,
  rejectProcurement,
} from "../api/transaction-admin"
import "./Transactions.css"

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
  paymentStatus: string
  pageNumber: number
  pageSize: number
}

export default function Transactions() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [items, setItems] = useState<TransactionItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [errorMessage, setErrorMessage] = useState("")

  const [query, setQuery] = useState<QueryState>({
    type: "",
    status: "",
    paymentStatus: "",
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
        paymentStatus: query.paymentStatus || undefined,
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
      paymentStatus: "",
      pageNumber: 1,
      pageSize: 10,
    }

    setQuery(nextQuery)

    void loadWith(nextQuery)
  }

  async function loadWith(nextQuery: QueryState) {
    setLoading(true)
    setErrorMessage("")

    try {
      const result = await getTenantTransactions({
        type: nextQuery.type || undefined,
        status: nextQuery.status || undefined,
        paymentStatus: nextQuery.paymentStatus || undefined,
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

  function statusClass(status: string) {
    switch (status) {
      case "Completed":
        return "tag tag-success"
      case "Failed":
        return "tag tag-danger"
      case "Submitted":
        return "tag tag-warning"
      case "Cancelled":
        return "tag tag-info"
      default:
        return "tag tag-info"
    }
  }

  function paymentClass(status: string) {
    switch (status) {
      case "Succeeded":
        return "tag tag-success"
      case "Failed":
        return "tag tag-danger"
      case "Processing":
        return "tag tag-warning"
      default:
        return "tag tag-info"
    }
  }

  function goTransactionDetail(row: TransactionItem) {
    navigate(`/admin/transactions/${row.transactionPublicId}`)
  }

  async function handleApprove(
    event: React.MouseEvent<HTMLButtonElement>,
    row: TransactionItem
  ) {
    event.stopPropagation()

    try {
      await approveProcurement(row.transactionPublicId)
      window.alert("Procurement approved.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        window.alert(err.message || "Failed to approve.")
      } else {
        window.alert("Failed to approve.")
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

    try {
      await rejectProcurement(row.transactionPublicId, {
        reason: reason.trim(),
      })
      window.alert("Procurement rejected.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        window.alert(err.message || "Failed to reject.")
      } else {
        window.alert("Failed to reject.")
      }
    }
  }

  const totalPages = Math.max(1, Math.ceil(totalCount / query.pageSize))

  return (
    <div className="transactions-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Transactions</h1>
          <p className="page-subtitle">
            Review all transactions within the current tenant.
          </p>
        </div>
      </div>

      <div className="filter-card">
        <div className="filter-row">
          <select
            className="filter-item"
            value={query.type}
            onChange={(e) => updateQuery("type", e.target.value)}
          >
            <option value="">Type</option>
            <option value="Donation">Donation</option>
            <option value="Procurement">Procurement</option>
          </select>

          <select
            className="filter-item"
            value={query.status}
            onChange={(e) => updateQuery("status", e.target.value)}
          >
            <option value="">Status</option>
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted</option>
            <option value="Completed">Completed</option>
            <option value="Failed">Failed</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Rejected">Rejected</option>
          </select>

          <select
            className="filter-item"
            value={query.paymentStatus}
            onChange={(e) => updateQuery("paymentStatus", e.target.value)}
          >
            <option value="">Payment</option>
            <option value="NotStarted">NotStarted</option>
            <option value="Processing">Processing</option>
            <option value="Succeeded">Succeeded</option>
            <option value="Failed">Failed</option>
          </select>

          <button className="primary-btn" onClick={() => void handleSearch()}>
            Search
          </button>

          <button className="secondary-btn" onClick={resetFilters}>
            Reset
          </button>
        </div>
      </div>

      <div className="table-card">
        {loading ? <div className="loading-text">Loading...</div> : null}
        {errorMessage ? <div className="alert error">{errorMessage}</div> : null}

        <div className="table-wrap">
          <table className="transactions-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Type</th>
                <th>Amount</th>
                <th>Status</th>
                <th>Payment</th>
                <th>Created At</th>
                <th>Action</th>
                <th>View</th>
              </tr>
            </thead>

            <tbody>
              {items.length === 0 ? (
                <tr>
                  <td colSpan={8} className="empty-cell">
                    No transactions found.
                  </td>
                </tr>
              ) : (
                items.map((row) => (
                  <tr
                    key={row.transactionPublicId}
                    className="clickable-row"
                    onClick={() => goTransactionDetail(row)}
                  >
                    <td>{row.title}</td>
                    <td>
                      <span className="tag tag-light">{row.type}</span>
                    </td>
                    <td>
                      <span className="strong">
                        {row.amount} {row.currency}
                      </span>
                    </td>
                    <td>
                      <span className={statusClass(row.status)}>
                        {row.status}
                      </span>
                    </td>
                    <td>
                      <span className={paymentClass(row.paymentStatus)}>
                        {row.paymentStatus}
                      </span>
                    </td>
                    <td>{formatDateTime(row.createdAtUtc)}</td>
                    <td>
                      {row.type === "Procurement" && row.status === "Submitted" ? (
                        <div className="action-buttons">
                          <button
                            className="link-btn success-text"
                            onClick={(event) => void handleApprove(event, row)}
                          >
                            Approve
                          </button>
                          <button
                            className="link-btn danger-text"
                            onClick={(event) => void handleReject(event, row)}
                          >
                            Reject
                          </button>
                        </div>
                      ) : (
                        <span>-</span>
                      )}
                    </td>
                    <td>
                      <button
                        className="link-btn"
                        onClick={(event) => {
                          event.stopPropagation()
                          goTransactionDetail(row)
                        }}
                      >
                        View
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="pagination-wrap">
          <button
            className="secondary-btn"
            disabled={query.pageNumber <= 1}
            onClick={() => void handlePageChange(query.pageNumber - 1)}
          >
            Previous
          </button>

          <span className="pagination-text">
            Page {query.pageNumber} of {totalPages} · Total {totalCount}
          </span>

          <button
            className="secondary-btn"
            disabled={query.pageNumber >= totalPages}
            onClick={() => void handlePageChange(query.pageNumber + 1)}
          >
            Next
          </button>
        </div>
      </div>
    </div>
  )
}