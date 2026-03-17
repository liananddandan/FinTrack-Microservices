import { useEffect, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { getMyTransactions, type TransactionListItem } from "../api/transactions"
import "./MyTransactions.css"

type Filters = {
  keyword: string
  type: string
  status: string
  paymentStatus: string
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
    paymentStatus: "",
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
          err instanceof Error ? err.message : "Failed to load transactions"
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
      const matchesKeyword =
        !keyword ||
        item.tenantName.toLowerCase().includes(keyword) ||
        item.title.toLowerCase().includes(keyword) ||
        item.type.toLowerCase().includes(keyword)

      const matchesType = !filters.type || item.type === filters.type
      const matchesStatus = !filters.status || item.status === filters.status
      const matchesPaymentStatus =
        !filters.paymentStatus || item.paymentStatus === filters.paymentStatus

      return (
        matchesKeyword &&
        matchesType &&
        matchesStatus &&
        matchesPaymentStatus
      )
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
      paymentStatus: "",
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

  function statusClass(status: string) {
    switch (status) {
      case "Completed":
        return "tag-success"
      case "Failed":
        return "tag-danger"
      case "Submitted":
        return "tag-warning"
      case "Cancelled":
        return "tag-info"
      default:
        return "tag-info"
    }
  }

  function paymentClass(status: string) {
    switch (status) {
      case "Succeeded":
        return "tag-success"
      case "Failed":
        return "tag-danger"
      case "Processing":
        return "tag-warning"
      default:
        return "tag-info"
    }
  }

  function goDetail(row: TransactionListItem) {
    navigate(`/transactions/${row.transactionPublicId}`)
  }

  return (
    <div className="transactions-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">My Transactions</h1>
          <p className="page-subtitle">
            Review your donations and procurement requests in the current tenant.
          </p>
        </div>
      </div>

      <div className="filter-card">
        <div className="filter-row">
          <input
            className="filter-input keyword-input"
            type="text"
            placeholder="Search by tenant, title, or type"
            value={filters.keyword}
            onChange={(e) => updateFilter("keyword", e.target.value)}
          />

          <select
            className="filter-select filter-item"
            value={filters.type}
            onChange={(e) => updateFilter("type", e.target.value)}
          >
            <option value="">Type</option>
            <option value="Donation">Donation</option>
            <option value="Procurement">Procurement</option>
          </select>

          <select
            className="filter-select filter-item"
            value={filters.status}
            onChange={(e) => updateFilter("status", e.target.value)}
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
            className="filter-select filter-item"
            value={filters.paymentStatus}
            onChange={(e) => updateFilter("paymentStatus", e.target.value)}
          >
            <option value="">Payment</option>
            <option value="NotStarted">NotStarted</option>
            <option value="Processing">Processing</option>
            <option value="Succeeded">Succeeded</option>
            <option value="Failed">Failed</option>
          </select>

          <button className="reset-btn" type="button" onClick={resetFilters}>
            Reset
          </button>
        </div>
      </div>

      <div className="table-card">
        <div className="table-header">
          <div className="table-title">Transaction List</div>
          <div className="table-count">
            {filteredItems.length} item{filteredItems.length !== 1 ? "s" : ""}
          </div>
        </div>

        {errorMessage ? (
          <div className="table-alert" role="alert">
            {errorMessage}
          </div>
        ) : null}

        <div className="table-wrapper">
          <table className="transaction-table">
            <thead>
              <tr>
                <th>Tenant</th>
                <th>Title</th>
                <th>Type</th>
                <th>Amount</th>
                <th>Status</th>
                <th>Payment</th>
                <th>Created At</th>
                <th>Action</th>
              </tr>
            </thead>

            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={8} className="table-empty">
                    Loading...
                  </td>
                </tr>
              ) : filteredItems.length === 0 ? (
                <tr>
                  <td colSpan={8} className="table-empty">
                    No transactions found.
                  </td>
                </tr>
              ) : (
                filteredItems.map((row) => (
                  <tr
                    key={row.transactionPublicId}
                    className="clickable-row"
                    onClick={() => goDetail(row)}
                  >
                    <td>{row.tenantName}</td>
                    <td>{row.title}</td>
                    <td>
                      <span className="tag tag-info">{row.type}</span>
                    </td>
                    <td>
                      <span className="amount-text">{amountText(row)}</span>
                    </td>
                    <td>
                      <span className={`tag ${statusClass(row.status)}`}>
                        {row.status}
                      </span>
                    </td>
                    <td>
                      <span className={`tag ${paymentClass(row.paymentStatus)}`}>
                        {row.paymentStatus}
                      </span>
                    </td>
                    <td>{formatDateTime(row.createdAtUtc)}</td>
                    <td>
                      <button
                        type="button"
                        className="link-btn"
                        onClick={(e) => {
                          e.stopPropagation()
                          goDetail(row)
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
      </div>
    </div>
  )
}