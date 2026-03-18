import { useEffect, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
  approveProcurement,
  getTransactionDetail,
  rejectProcurement,
} from "../api/transaction-admin"
import "./TransactionDetail.css"

type TransactionDetail = {
  transactionPublicId: string
  tenantPublicId: string
  tenantName: string
  type: string
  title: string
  description?: string
  amount: number
  currency: string
  status: string
  paymentStatus: string
  riskStatus: string
  createdByUserPublicId: string
  createdAtUtc: string
  approvedByUserPublicId?: string
  approvedAtUtc?: string
  paidByUserPublicId?: string
  paidAtUtc?: string
  paymentReference?: string
  failureReason?: string
  refundedByUserPublicId?: string
  refundedAtUtc?: string
}

export default function TransactionDetail() {
  const { transactionPublicId } = useParams()
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [rejectReason, setRejectReason] = useState("")
  const [rejectDialogVisible, setRejectDialogVisible] = useState(false)
  const [detail, setDetail] = useState<TransactionDetail | null>(null)
  const [errorMessage, setErrorMessage] = useState("")

  const isProcurement = detail?.type === "Procurement"
  const canReview =
    !!detail &&
    detail.type === "Procurement" &&
    detail.status === "Submitted"

  useEffect(() => {
    void load()
  }, [transactionPublicId])

  async function load() {
    if (!transactionPublicId) {
      window.alert("Transaction id is missing.")
      navigate("/admin/transactions")
      return
    }

    setLoading(true)
    setErrorMessage("")

    try {
      const result = await getTransactionDetail(transactionPublicId)
      setDetail(result)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to load transaction detail.")
      } else {
        setErrorMessage("Failed to load transaction detail.")
      }

      navigate("/admin/transactions")
    } finally {
      setLoading(false)
    }
  }

  async function handleApprove() {
    if (!detail) return

    setActionLoading(true)

    try {
      await approveProcurement(detail.transactionPublicId)
      window.alert("Procurement approved.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        window.alert(err.message || "Failed to approve procurement.")
      } else {
        window.alert("Failed to approve procurement.")
      }
    } finally {
      setActionLoading(false)
    }
  }

  function openRejectDialog() {
    setRejectReason("")
    setRejectDialogVisible(true)
  }

  async function confirmReject() {
    if (!detail) return

    if (!rejectReason.trim()) {
      window.alert("Reject reason is required.")
      return
    }

    setActionLoading(true)

    try {
      await rejectProcurement(detail.transactionPublicId, {
        reason: rejectReason.trim(),
      })

      window.alert("Procurement rejected.")
      setRejectDialogVisible(false)
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        window.alert(err.message || "Failed to reject procurement.")
      } else {
        window.alert("Failed to reject procurement.")
      }
    } finally {
      setActionLoading(false)
    }
  }

  function formatDateTime(value?: string) {
    if (!value) return "-"
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value
    return date.toLocaleString()
  }

  function formatAmount(amount: number, currency: string) {
    return `${amount} ${currency}`
  }

  function statusClass(status: string) {
    switch (status) {
      case "Completed":
      case "Approved":
        return "tag tag-success"
      case "Failed":
      case "Rejected":
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

  return (
    <div className="detail-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Transaction Detail</h1>
          <p className="page-subtitle">
            Review full transaction information within the current tenant.
          </p>
        </div>

        <button
          className="secondary-btn"
          onClick={() => navigate("/admin/transactions")}
        >
          Back to transactions
        </button>
      </div>

      {loading ? <div className="loading-block">Loading...</div> : null}
      {errorMessage ? <div className="alert error">{errorMessage}</div> : null}

      <div className="summary-card">
        {detail ? (
          <>
            <div className="summary-top">
              <div>
                <div className="summary-type">{detail.type}</div>
                <h2 className="summary-title">{detail.title}</h2>
                <div className="summary-tenant">{detail.tenantName}</div>
              </div>

              <div className="summary-right">
                <div className="summary-amount">
                  {formatAmount(detail.amount, detail.currency)}
                </div>

                <div className="summary-tags">
                  <span className={statusClass(detail.status)}>
                    {detail.status}
                  </span>

                  <span className={paymentClass(detail.paymentStatus)}>
                    {detail.paymentStatus}
                  </span>
                </div>
              </div>
            </div>

            {canReview ? (
              <div className="review-actions">
                <button
                  className="success-btn"
                  disabled={actionLoading}
                  onClick={() => void handleApprove()}
                >
                  {actionLoading ? "Approving..." : "Approve"}
                </button>

                <button
                  className="danger-btn"
                  disabled={actionLoading}
                  onClick={openRejectDialog}
                >
                  Reject
                </button>
              </div>
            ) : null}
          </>
        ) : null}
      </div>

      {detail ? (
        <>
          <div className="content-card">
            <div className="card-title">
              {isProcurement
                ? "Procurement Information"
                : "Transaction Information"}
            </div>

            <div className="info-list">
              <div className="info-row">
                <span className="info-label">Title</span>
                <span className="info-value">{detail.title}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Description</span>
                <span className="info-value">{detail.description || "-"}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Amount</span>
                <span className="info-value strong">
                  {formatAmount(detail.amount, detail.currency)}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Type</span>
                <span className="info-value">{detail.type}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Status</span>
                <span className="info-value">{detail.status}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Payment status</span>
                <span className="info-value">{detail.paymentStatus}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Risk status</span>
                <span className="info-value">{detail.riskStatus}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Payment reference</span>
                <span className="info-value">
                  {detail.paymentReference || "-"}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Failure reason</span>
                <span className="info-value">
                  {detail.failureReason || "-"}
                </span>
              </div>
            </div>
          </div>

          <div className="content-card">
            <div className="card-title">Timeline & Metadata</div>

            <div className="info-list">
              <div className="info-row">
                <span className="info-label">Transaction ID</span>
                <span className="info-value mono">
                  {detail.transactionPublicId}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Tenant ID</span>
                <span className="info-value mono">{detail.tenantPublicId}</span>
              </div>

              <div className="info-row">
                <span className="info-label">Created by</span>
                <span className="info-value mono">
                  {detail.createdByUserPublicId}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Created at</span>
                <span className="info-value">
                  {formatDateTime(detail.createdAtUtc)}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Approved by</span>
                <span className="info-value mono">
                  {detail.approvedByUserPublicId || "-"}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Approved at</span>
                <span className="info-value">
                  {formatDateTime(detail.approvedAtUtc)}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Paid by</span>
                <span className="info-value mono">
                  {detail.paidByUserPublicId || "-"}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Paid at</span>
                <span className="info-value">
                  {formatDateTime(detail.paidAtUtc)}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Refunded by</span>
                <span className="info-value mono">
                  {detail.refundedByUserPublicId || "-"}
                </span>
              </div>

              <div className="info-row">
                <span className="info-label">Refunded at</span>
                <span className="info-value">
                  {formatDateTime(detail.refundedAtUtc)}
                </span>
              </div>
            </div>
          </div>
        </>
      ) : null}

      {rejectDialogVisible ? (
        <div className="dialog-backdrop">
          <div className="dialog-card">
            <div className="dialog-header">
              <div className="dialog-title">Reject Procurement</div>
            </div>

            <div className="dialog-form">
              <div className="form-item">
                <label>Reject reason</label>
                <textarea
                  rows={4}
                  value={rejectReason}
                  onChange={(e) => setRejectReason(e.target.value)}
                  placeholder="Please enter reject reason"
                />
              </div>
            </div>

            <div className="dialog-footer">
              <button
                className="secondary-btn"
                onClick={() => setRejectDialogVisible(false)}
              >
                Cancel
              </button>

              <button
                className="danger-btn"
                disabled={actionLoading}
                onClick={() => void confirmReject()}
              >
                {actionLoading ? "Rejecting..." : "Confirm Reject"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}