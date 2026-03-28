import { useEffect, useMemo, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
  approveProcurement,
  getTransactionDetail,
  rejectProcurement,
} from "../api/transaction-admin"
import {
  HiOutlineArrowLeft,
  HiOutlineClipboardDocumentList,
} from "react-icons/hi2"

type TransactionDetailModel = {
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

function InfoRow({
  label,
  value,
  mono = false,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm text-slate-500">{label}</span>
      <span
        className={`text-right text-sm font-medium text-slate-800 ${
          mono ? "font-mono break-all" : ""
        }`}
      >
        {value}
      </span>
    </div>
  )
}

export default function TransactionDetail() {
  const { transactionPublicId } = useParams()
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)
  const [rejectReason, setRejectReason] = useState("")
  const [rejectDialogVisible, setRejectDialogVisible] = useState(false)
  const [detail, setDetail] = useState<TransactionDetailModel | null>(null)
  const [errorMessage, setErrorMessage] = useState("")
  const [actionMessage, setActionMessage] = useState("")

  const isProcurement = useMemo(() => detail?.type === "Procurement", [detail])
  const isIncome = useMemo(() => detail?.type === "Donation", [detail])

  const canReview =
    !!detail &&
    detail.type === "Procurement" &&
    detail.status === "Submitted"

  useEffect(() => {
    void load()
  }, [transactionPublicId])

  async function load() {
    if (!transactionPublicId) {
      setErrorMessage("Transaction id is missing.")
      navigate("/transactions", { replace: true })
      return
    }

    setLoading(true)
    setErrorMessage("")
    setActionMessage("")

    try {
      const result = await getTransactionDetail(transactionPublicId)
      setDetail(result)
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to load transaction detail.")
      } else {
        setErrorMessage("Failed to load transaction detail.")
      }

      navigate("/transactions", { replace: true })
    } finally {
      setLoading(false)
    }
  }

  async function handleApprove() {
    if (!detail) return

    setActionLoading(true)
    setActionMessage("")
    setErrorMessage("")

    try {
      await approveProcurement(detail.transactionPublicId)
      setActionMessage("Procurement approved.")
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to approve procurement.")
      } else {
        setErrorMessage("Failed to approve procurement.")
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
      setErrorMessage("Reject reason is required.")
      return
    }

    setActionLoading(true)
    setActionMessage("")
    setErrorMessage("")

    try {
      await rejectProcurement(detail.transactionPublicId, {
        reason: rejectReason.trim(),
      })

      setActionMessage("Procurement rejected.")
      setRejectDialogVisible(false)
      await load()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to reject procurement.")
      } else {
        setErrorMessage("Failed to reject procurement.")
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

  function statusTone(status: string) {
    switch (status) {
      case "Completed":
      case "Approved":
        return "success"
      case "Failed":
      case "Rejected":
        return "danger"
      case "Submitted":
      case "Draft":
      case "Processing":
        return "warning"
      case "Cancelled":
        return "default"
      default:
        return "default"
    }
  }

  function executionTone(status: string) {
    switch (status) {
      case "Succeeded":
        return "success"
      case "Failed":
        return "danger"
      case "Processing":
        return "warning"
      default:
        return "default"
    }
  }

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6">
      <div className="flex justify-start">
        <button
          type="button"
          onClick={() => navigate("/transactions")}
          className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
        >
          <HiOutlineArrowLeft className="h-4 w-4" />
          Back to transactions
        </button>
      </div>

      {loading ? (
        <div className="text-sm text-slate-500">Loading transaction detail...</div>
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

      {/* Summary */}
      <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
        {detail ? (
          <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
            <div>
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
                <HiOutlineClipboardDocumentList className="h-6 w-6" />
              </div>

              <div className="mt-4 text-sm font-medium text-indigo-600">
                {detail.type === "Donation" ? "Income" : detail.type}
              </div>

              <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-800">
                {detail.title}
              </h1>

              <p className="mt-2 text-sm text-slate-500">
                {detail.tenantName}
              </p>
            </div>

            <div className="lg:text-right">
              <div className="text-2xl font-semibold text-slate-800">
                {formatAmount(detail.amount, detail.currency)}
              </div>

              <div className="mt-3 flex flex-wrap gap-2 lg:justify-end">
                <Badge tone={statusTone(detail.status)}>{detail.status}</Badge>

                {detail.paymentStatus &&
                detail.paymentStatus !== "NotStarted" ? (
                  <Badge tone={executionTone(detail.paymentStatus)}>
                    {detail.paymentStatus}
                  </Badge>
                ) : null}
              </div>

              {canReview ? (
                <div className="mt-5 flex flex-wrap gap-3 lg:justify-end">
                  <button
                    type="button"
                    disabled={actionLoading}
                    onClick={() => void handleApprove()}
                    className="inline-flex items-center rounded-full bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {actionLoading ? "Approving..." : "Approve"}
                  </button>

                  <button
                    type="button"
                    disabled={actionLoading}
                    onClick={openRejectDialog}
                    className="inline-flex items-center rounded-full bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-500 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    Reject
                  </button>
                </div>
              ) : null}
            </div>
          </div>
        ) : (
          <div className="text-sm text-slate-500">Transaction detail is unavailable.</div>
        )}
      </section>

      {detail ? (
        <>
          <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
            <h2 className="text-lg font-semibold text-slate-800">
              {isProcurement
                ? "Procurement information"
                : isIncome
                ? "Income information"
                : "Transaction information"}
            </h2>

            <div className="mt-5 rounded-2xl border border-slate-100 bg-slate-50 px-5">
              <InfoRow label="Title" value={detail.title} />
              <InfoRow label="Description" value={detail.description || "-"} />
              <InfoRow
                label="Amount"
                value={formatAmount(detail.amount, detail.currency)}
              />
              <InfoRow
                label="Type"
                value={detail.type === "Donation" ? "Income" : detail.type}
              />
              <InfoRow label="Status" value={detail.status} />

              {isProcurement ? (
                <>
                  <InfoRow label="Risk status" value={detail.riskStatus || "-"} />
                  {detail.paymentStatus &&
                  detail.paymentStatus !== "NotStarted" ? (
                    <InfoRow
                      label="Execution status"
                      value={detail.paymentStatus}
                    />
                  ) : null}
                </>
              ) : null}

              {isIncome ? (
                <>
                  <InfoRow
                    label="Reference"
                    value={detail.paymentReference || "-"}
                  />
                  <InfoRow
                    label="Failure reason"
                    value={detail.failureReason || "-"}
                  />
                </>
              ) : null}
            </div>
          </section>

          <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
            <h2 className="text-lg font-semibold text-slate-800">
              Timeline and metadata
            </h2>

            <div className="mt-5 rounded-2xl border border-slate-100 bg-slate-50 px-5">
              <InfoRow
                label="Transaction ID"
                value={detail.transactionPublicId}
                mono
              />
              <InfoRow
                label="Tenant ID"
                value={detail.tenantPublicId}
                mono
              />
              <InfoRow
                label="Created by"
                value={detail.createdByUserPublicId}
                mono
              />
              <InfoRow
                label="Created at"
                value={formatDateTime(detail.createdAtUtc)}
              />
              <InfoRow
                label="Approved by"
                value={detail.approvedByUserPublicId || "-"}
                mono
              />
              <InfoRow
                label="Approved at"
                value={formatDateTime(detail.approvedAtUtc)}
              />
              <InfoRow
                label="Paid by"
                value={detail.paidByUserPublicId || "-"}
                mono
              />
              <InfoRow
                label="Paid at"
                value={formatDateTime(detail.paidAtUtc)}
              />
              <InfoRow
                label="Refunded by"
                value={detail.refundedByUserPublicId || "-"}
                mono
              />
              <InfoRow
                label="Refunded at"
                value={formatDateTime(detail.refundedAtUtc)}
              />
            </div>
          </section>
        </>
      ) : null}

      {rejectDialogVisible ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4">
          <div className="w-full max-w-lg rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
            <h2 className="text-xl font-semibold text-slate-800">
              Reject procurement
            </h2>

            <p className="mt-2 text-sm leading-6 text-slate-500">
              Provide a reason for rejecting this procurement request.
            </p>

            <div className="mt-6">
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Reject reason
              </label>
              <textarea
                rows={4}
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Please enter reject reason"
                className="block w-full rounded-xl border border-slate-300 bg-white px-3 py-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setRejectDialogVisible(false)}
                className="inline-flex items-center rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
              >
                Cancel
              </button>

              <button
                type="button"
                disabled={actionLoading}
                onClick={() => void confirmReject()}
                className="inline-flex items-center rounded-full bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {actionLoading ? "Rejecting..." : "Confirm reject"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}