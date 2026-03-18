import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import {
  getTenantTransactionSummary,
  getTenantTransactions,
} from "../api/transaction-admin"
import "./Overview.css"

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
    <div className="overview-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Overview</h1>
          <p className="page-subtitle">
            Monitor your tenant’s balance, transactions, and activity in one
            place.
          </p>
        </div>
      </div>

      {loading ? <div className="loading-text">Loading...</div> : null}
      {summaryError ? <div className="alert error">{summaryError}</div> : null}
      {transactionsError ? (
        <div className="alert error">{transactionsError}</div>
      ) : null}

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-label">Current Balance</div>
          <div className="stat-value">
            {summary ? formatAmount(summary.currentBalance) : "-"}
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-label">Total Donations</div>
          <div className="stat-value">
            {summary ? formatAmount(summary.totalDonationAmount) : "-"}
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-label">Total Procurements</div>
          <div className="stat-value">
            {summary ? formatAmount(summary.totalProcurementAmount) : "-"}
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-label">Total Transactions</div>
          <div className="stat-value">
            {summary?.totalTransactionCount ?? "-"}
          </div>
        </div>
      </div>

      <div className="content-grid">
        <div className="panel-card">
          <div className="panel-header">
            <div>
              <div className="panel-title">Tenant Information</div>
              <div className="panel-subtitle">
                Current tenant context and administrator details.
              </div>
            </div>
          </div>

          <div className="info-list">
            <div className="info-row">
              <span className="info-label">Tenant Name</span>
              <span className="info-value">
                {summary?.tenantName || auth.currentTenantName || "-"}
              </span>
            </div>

            <div className="info-row">
              <span className="info-label">Tenant ID</span>
              <span className="info-value mono">
                {summary?.tenantPublicId || auth.currentTenantPublicId || "-"}
              </span>
            </div>

            <div className="info-row">
              <span className="info-label">Current User</span>
              <span className="info-value">
                {auth.userName || auth.userEmail || "-"}
              </span>
            </div>

            <div className="info-row">
              <span className="info-label">Role</span>
              <span className="info-value">
                <span className="tag tag-primary">
                  {auth.currentMembership?.role || "-"}
                </span>
              </span>
            </div>
          </div>
        </div>

        <div className="panel-card">
          <div className="panel-header">
            <div>
              <div className="panel-title">Quick Actions</div>
              <div className="panel-subtitle">
                Go to the main administration areas.
              </div>
            </div>
          </div>

          <div className="action-grid">
            <button className="action-tile" onClick={goTransactions}>
              <div className="action-title">Transactions</div>
              <div className="action-text">
                View and manage tenant-wide transactions.
              </div>
            </button>

            <button className="action-tile" onClick={goAuditLogs}>
              <div className="action-title">Audit Logs</div>
              <div className="action-text">
                Review important tenant activities and actions.
              </div>
            </button>

            <button className="action-tile" onClick={goMembers}>
              <div className="action-title">Members</div>
              <div className="action-text">
                View and manage tenant members.
              </div>
            </button>

            <button className="action-tile" onClick={goInvitations}>
              <div className="action-title">Invitations</div>
              <div className="action-text">
                Manage pending and sent tenant invitations.
              </div>
            </button>
          </div>
        </div>
      </div>

      <div className="panel-card">
        <div className="panel-header">
          <div>
            <div className="panel-title">Recent Transactions</div>
            <div className="panel-subtitle">
              Latest transactions in this tenant.
            </div>
          </div>

          <button className="link-btn" onClick={goTransactions}>
            View all
          </button>
        </div>

        <div className="table-wrap">
          <table className="recent-table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Type</th>
                <th>Amount</th>
                <th>Status</th>
                <th>Payment</th>
                <th>Created At</th>
              </tr>
            </thead>

            <tbody>
              {recentTransactions.length === 0 ? (
                <tr>
                  <td colSpan={6} className="empty-cell">
                    No transactions found.
                  </td>
                </tr>
              ) : (
                recentTransactions.map((row) => (
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