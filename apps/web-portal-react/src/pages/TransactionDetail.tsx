import { useEffect, useMemo, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
  getTransactionDetail,
  submitProcurement,
  type TransactionDetail as TransactionDetailModel,
} from "../api/transactions"
import {
  HiOutlineArrowLeft,
  HiOutlineClipboardDocumentList,
  HiOutlineArrowPath,
} from "react-icons/hi2"

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

  function paymentTone(status: string) {
    switch (status) {
      case "Succeeded":
        return "success"
      case "Processing":
        return "warning"
      case "Failed":
        return "danger"
      default:
        return "default"
    }
  }

  async function load() {
    if (!transactionPublicId) {
      setErrorMessage("Transaction id is missing.")
      navigate("/portal/my-transactions", { replace: true })
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
        err instanceof Error ? err.message : "Failed to load transaction detail."
      setErrorMessage(message)
      navigate("/portal/my-transactions", { replace: true })
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
  }, [transactionPublicId])

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto flex max-w-5xl flex-col gap-6">
        {/* Header */}
        <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div className="flex items-start gap-4">
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
                <HiOutlineClipboardDocumentList className="h-6 w-6" />
              </div>

              <div>
                <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
                  Transaction detail
                </h1>
                <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
                  Review the full information for this record in the current workspace.
                </p>
              </div>
            </div>

            <button
              type="button"
              onClick={() => navigate("/portal/my-transactions")}
              className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <HiOutlineArrowLeft className="h-4 w-4" />
              Back to list
            </button>
          </div>
        </section>

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
          {loading ? (
            <div className="text-sm text-slate-500">Loading...</div>
          ) : detail ? (
            <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
              <div>
                <div className="text-sm font-medium text-indigo-600">
                  {detail.type === "Donation" ? "Income" : detail.type}
                </div>

                <h2 className="mt-2 text-2xl font-semibold text-slate-800">
                  {detail.title}
                </h2>

                <p className="mt-2 text-sm text-slate-500">
                  {detail.tenantName || "Current workspace"}
                </p>
              </div>

              <div className="lg:text-right">
                <div className="text-2xl font-semibold text-slate-800">
                  {detail.amount} {detail.currency}
                </div>

                <div className="mt-3 flex flex-wrap gap-2 lg:justify-end">
                  <Badge tone={statusTone(detail.status)}>{detail.status}</Badge>

                  {detail.paymentStatus &&
                  detail.paymentStatus !== "NotStarted" ? (
                    <Badge tone={paymentTone(detail.paymentStatus)}>
                      {detail.paymentStatus}
                    </Badge>
                  ) : null}
                </div>

                {detail.type === "Procurement" && detail.status === "Draft" ? (
                  <div className="mt-4">
                    <button
                      type="button"
                      disabled={actionLoading}
                      onClick={handleSubmitProcurement}
                      className="inline-flex items-center gap-2 rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      <HiOutlineArrowPath className="h-4 w-4" />
                      {actionLoading ? "Submitting..." : "Submit for approval"}
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
            {isDonation ? (
              <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
                <h3 className="text-lg font-semibold text-slate-800">
                  Income information
                </h3>

                <div className="mt-5 rounded-2xl border border-slate-100 bg-slate-50 px-5">
                  <InfoRow
                    label="Amount"
                    value={`${detail.amount} ${detail.currency}`}
                  />
                  <InfoRow
                    label="Status"
                    value={detail.status}
                  />
                  <InfoRow
                    label="Reference"
                    value={detail.paymentReference || "-"}
                  />
                  <InfoRow
                    label="Note"
                    value={detail.description || "-"}
                  />
                  <InfoRow
                    label="Failure reason"
                    value={detail.failureReason || "-"}
                  />
                </div>
              </section>
            ) : null}

            {isProcurement ? (
              <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
                <h3 className="text-lg font-semibold text-slate-800">
                  Procurement information
                </h3>

                <div className="mt-5 rounded-2xl border border-slate-100 bg-slate-50 px-5">
                  <InfoRow
                    label="Requested amount"
                    value={`${detail.amount} ${detail.currency}`}
                  />
                  <InfoRow label="Status" value={detail.status} />
                  <InfoRow label="Risk status" value={detail.riskStatus || "-"} />
                  <InfoRow label="Reason" value={detail.description || "-"} />
                  {detail.paymentStatus &&
                  detail.paymentStatus !== "NotStarted" ? (
                    <InfoRow label="Execution status" value={detail.paymentStatus} />
                  ) : null}
                </div>
              </section>
            ) : null}

            <section className="rounded-3xl border border-slate-200 bg-white p-6 sm:p-8">
              <h3 className="text-lg font-semibold text-slate-800">
                Timeline and metadata
              </h3>

              <div className="mt-5 rounded-2xl border border-slate-100 bg-slate-50 px-5">
                <InfoRow
                  label="Transaction ID"
                  value={detail.transactionPublicId}
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
      </div>
    </div>
  )
}