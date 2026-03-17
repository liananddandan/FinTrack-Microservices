import { useEffect, useMemo, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
  getTransactionDetail,
  submitProcurement,
  type TransactionDetail as TransactionDetailModel,
} from "../api/transactions"
import "./TransactionDetail.css"

export default function TransactionDetail() {
  const { transactionPublicId } = useParams<{ transactionPublicId: string }>()
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [detail, setDetail] = useState<TransactionDetailModel | null>(null)
  const [errorMessage, setErrorMessage] = useState("")
  const [actionMessage, setActionMessage] = useState("")

  const isDonation = useMemo(() => detail?.type === "Donation", [detail])
  const isProcurement = useMemo(() => detail?.type === "Procurement", [detail])

  function formatDateTime(value?: string) {
    if (!value) return "-"
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value
    return date.toLocaleString()
  }

  function statusClass(status: string) {
    switch (status) {
      case "Completed":
        return "tag-success"
      case "Failed":
      case "Rejected":
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

  async function load() {
    if (!transactionPublicId) {
      setErrorMessage("Transaction id is missing.")
      navigate("/my-transactions")
      return
    }

    setLoading(true)
    setErrorMessage("")
    setActionMessage("")

    try {
      const data = await getTransactionDetail(transactionPublicId)
      setDetail(data)
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to load transaction detail"
      setErrorMessage(message)
      navigate("/my-transactions")
    } finally {
      setLoading(false)
    }
  }

  async function handleSubmitProcurement() {
    if (!detail) return

    setActionLoading(true)
    setActionMessage("")

    try {
      await submitProcurement(detail.transactionPublicId)
      setActionMessage("Procurement submitted successfully.")
      await load()
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to submit procurement."
      setActionMessage(message)
    } finally {
      setActionLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [transactionPublicId])

  return (
    <div className="detail-page">
      <div className="detail-shell">
        <div className="page-header">
          <div>
            <h1 className="page-title">Transaction Detail</h1>
            <p className="page-subtitle">
              Review the full information for this transaction.
            </p>
          </div>

          <button
            type="button"
            className="secondary-btn"
            onClick={() => navigate("/my-transactions")}
          >
            Back to list
          </button>
        </div>

        <div className="summary-card">
          {loading ? (
            <div className="loading-state">Loading...</div>
          ) : detail ? (
            <div className="summary-top">
              <div>
                <div className="summary-type">{detail.type}</div>
                <h2 className="summary-title">{detail.title}</h2>
                <div className="summary-tenant">{detail.tenantName}</div>
              </div>

              <div className="summary-right">
                <div className="summary-amount">
                  {detail.amount} {detail.currency}
                </div>

                <div className="summary-tags">
                  <span className={`tag ${statusClass(detail.status)}`}>
                    {detail.status}
                  </span>
                  <span className={`tag ${paymentClass(detail.paymentStatus)}`}>
                    {detail.paymentStatus}
                  </span>
                </div>

                {detail.type === "Procurement" && detail.status === "Draft" ? (
                  <div className="action-bar">
                    <button
                      type="button"
                      className="primary-btn"
                      disabled={actionLoading}
                      onClick={handleSubmitProcurement}
                    >
                      {actionLoading ? "Submitting..." : "Submit for Approval"}
                    </button>
                  </div>
                ) : null}
              </div>
            </div>
          ) : null}
        </div>

        {actionMessage ? (
          <div className="page-alert" role="status">
            {actionMessage}
          </div>
        ) : null}

        {errorMessage ? (
          <div className="page-alert error" role="alert">
            {errorMessage}
          </div>
        ) : null}

        {detail ? (
          <>
            {isDonation ? (
              <div className="content-card">
                <div className="card-title">Donation Information</div>

                <div className="info-list">
                  <div className="info-row">
                    <span className="info-label">Donation amount</span>
                    <span className="info-value strong">
                      {detail.amount} {detail.currency}
                    </span>
                  </div>

                  <div className="info-row">
                    <span className="info-label">Payment status</span>
                    <span className="info-value">{detail.paymentStatus}</span>
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

                  <div className="info-row">
                    <span className="info-label">Message</span>
                    <span className="info-value">
                      {detail.description || "-"}
                    </span>
                  </div>
                </div>
              </div>
            ) : null}

            {isProcurement ? (
              <div className="content-card">
                <div className="card-title">Procurement Information</div>

                <div className="info-list">
                  <div className="info-row">
                    <span className="info-label">Requested amount</span>
                    <span className="info-value strong">
                      {detail.amount} {detail.currency}
                    </span>
                  </div>

                  <div className="info-row">
                    <span className="info-label">Request status</span>
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
                    <span className="info-label">Reason</span>
                    <span className="info-value">
                      {detail.description || "-"}
                    </span>
                  </div>
                </div>
              </div>
            ) : null}

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
        ) : !loading ? (
          <div className="content-card empty-card">
            Transaction detail is unavailable.
          </div>
        ) : null}
      </div>
    </div>
  )
}