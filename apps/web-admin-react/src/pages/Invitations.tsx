import { useEffect, useMemo, useState } from "react"
import {
  type TenantInvitationDto,
} from "@fintrack/web-shared"
import { tenantApi } from "../lib/tenantApi"
import {
  HiOutlineEnvelope,
  HiOutlineMagnifyingGlass,
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

function SummaryCard({
  label,
  value,
}: {
  label: string
  value: string | number
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5">
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-slate-800">{value}</p>
    </div>
  )
}

export default function Invitations() {
  const [loading, setLoading] = useState(false)
  const [keyword, setKeyword] = useState("")
  const [invitations, setInvitations] = useState<TenantInvitationDto[]>([])
  const [resendingInvitationId, setResendingInvitationId] = useState("")

  const [pageMessage, setPageMessage] = useState("")
  const [pageError, setPageError] = useState("")

  const filteredInvitations = useMemo(() => {
    const q = keyword.trim().toLowerCase()

    if (!q) return invitations

    return invitations.filter((item) => {
      const email = item.email?.toLowerCase() ?? ""
      const inviter = item.createdByUserEmail?.toLowerCase() ?? ""
      return email.includes(q) || inviter.includes(q)
    })
  }, [keyword, invitations])

  const pendingCount = useMemo(
    () => invitations.filter((x) => x.status === "Pending").length,
    [invitations]
  )

  const acceptedCount = useMemo(
    () => invitations.filter((x) => x.status === "Accepted").length,
    [invitations]
  )

  useEffect(() => {
    void loadInvitations()
  }, [])

  async function loadInvitations() {
    setLoading(true)
    setPageError("")
    setPageMessage("")

    try {
      const result = await tenantApi.getTenantInvitations()
      setInvitations(result)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPageError(error.message || "Failed to load invitations.")
      } else {
        setPageError("Failed to load invitations.")
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleResend(item: TenantInvitationDto) {
    setResendingInvitationId(item.invitationPublicId)
    setPageError("")
    setPageMessage("")

    try {
      await tenantApi.resendTenantInvitation(item.invitationPublicId)
      setPageMessage("Invitation email resent successfully.")
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPageError(error.message || "Failed to resend invitation.")
      } else {
        setPageError("Failed to resend invitation.")
      }
    } finally {
      setResendingInvitationId("")
    }
  }

  function getInitials(email: string) {
    const value = email.trim()
    if (!value) return "I"
    return value.slice(0, 1).toUpperCase()
  }

  function formatDate(value?: string | null) {
    if (!value) return "-"

    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value

    return date.toLocaleString()
  }

  function handleViewLater(item: TenantInvitationDto) {
    setPageMessage(`Details page is not implemented yet for ${item.email}.`)
  }

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      {/* Header */}
      <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
        <div className="flex items-start gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
            <HiOutlineEnvelope className="h-6 w-6" />
          </div>

          <div>
            <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
              Invitations
            </h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
              Review invitation history and monitor acceptance status for this tenant.
            </p>
          </div>
        </div>
      </section>

      {pageMessage ? (
        <div
          className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
          role="status"
        >
          {pageMessage}
        </div>
      ) : null}

      {pageError ? (
        <div
          className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
          role="alert"
        >
          {pageError}
        </div>
      ) : null}

      {/* Summary */}
      <section className="grid gap-4 md:grid-cols-3">
        <SummaryCard label="Total invitations" value={invitations.length} />
        <SummaryCard label="Pending" value={pendingCount} />
        <SummaryCard label="Accepted" value={acceptedCount} />
      </section>

      {/* Invitation list */}
      <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
        <div className="mb-4 flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-base font-semibold text-slate-800">
              Invitation records
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              All invitations created for the current tenant.
            </p>
          </div>

          <div className="relative w-full max-w-sm">
            <HiOutlineMagnifyingGlass className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="Search by email or inviter"
              className="h-11 w-full rounded-xl border border-slate-300 bg-white pl-10 pr-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
          </div>
        </div>

        {loading ? (
          <div className="text-sm text-slate-500">Loading invitations...</div>
        ) : filteredInvitations.length === 0 ? (
          <div className="rounded-2xl border border-slate-200 bg-slate-50 px-4 py-10 text-center text-sm text-slate-500">
            No invitations found.
          </div>
        ) : (
          <div className="space-y-4">
            {filteredInvitations.map((item) => (
              <div
                key={item.invitationPublicId}
                className="flex flex-col gap-4 rounded-2xl border border-slate-200 bg-slate-50 p-5 xl:flex-row xl:items-center xl:justify-between"
              >
                <div className="flex min-w-0 items-start gap-4">
                  <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">
                    {getInitials(item.email)}
                  </div>

                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <div className="truncate text-sm font-semibold text-slate-800">
                        {item.email}
                      </div>

                      <Badge>{item.role}</Badge>

                      <Badge tone={item.status === "Accepted" ? "success" : "warning"}>
                        {item.status}
                      </Badge>
                    </div>

                    <div className="mt-2 flex flex-wrap items-center gap-2 text-xs text-slate-500">
                      <span>Invited by {item.createdByUserEmail}</span>
                      <span>•</span>
                      <span>Created {formatDate(item.createdAt)}</span>
                      <span>•</span>
                      <span>Expires {formatDate(item.expiredAt)}</span>
                    </div>

                    {item.acceptedAt ? (
                      <div className="mt-2 text-sm text-emerald-700">
                        Accepted at {formatDate(item.acceptedAt)}
                      </div>
                    ) : null}

                    <div className="mt-2 font-mono text-xs text-slate-500 break-all">
                      {item.invitationPublicId}
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap items-center gap-3 xl:justify-end">
                  {item.status === "Pending" ? (
                    <button
                      type="button"
                      disabled={resendingInvitationId === item.invitationPublicId}
                      className="inline-flex items-center gap-1 text-sm font-medium text-indigo-600 transition hover:text-indigo-500 disabled:cursor-not-allowed disabled:opacity-50"
                      onClick={() => void handleResend(item)}
                    >
                      <HiOutlineArrowPath className="h-4 w-4" />
                      {resendingInvitationId === item.invitationPublicId
                        ? "Resending..."
                        : "Resend"}
                    </button>
                  ) : null}

                  <button
                    type="button"
                    className="text-sm text-slate-700 transition hover:text-indigo-600"
                    onClick={() => handleViewLater(item)}
                  >
                    Details
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}