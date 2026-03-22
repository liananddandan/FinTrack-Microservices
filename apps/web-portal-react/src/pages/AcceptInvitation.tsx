import { useEffect, useMemo, useState } from "react"
import { Link, useLocation, useNavigate } from "react-router-dom"
import {
  acceptTenantInvitation,
  resolveTenantInvitation,
  type ResolveTenantInvitationResult,
} from "../api/invitation"
import {
  HiOutlineEnvelopeOpen,
  HiOutlineArrowUpRight,
  HiOutlineCheckCircle,
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
}: {
  label: string
  value: React.ReactNode
}) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm text-slate-500">{label}</span>
      <span className="text-right text-sm font-medium text-slate-800">
        {value}
      </span>
    </div>
  )
}

export default function AcceptInvitation() {
  const location = useLocation()
  const navigate = useNavigate()

  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")
  const [invitation, setInvitation] =
    useState<ResolveTenantInvitationResult | null>(null)

  const token = useMemo(() => {
    const params = new URLSearchParams(location.search)
    return params.get("token") ?? ""
  }, [location.search])

  const canAccept =
    !!invitation &&
    invitation.status === "Pending" &&
    !successMessage

  useEffect(() => {
    async function loadInvitation() {
      setLoading(true)
      setErrorMessage("")
      setSuccessMessage("")

      if (!token) {
        setErrorMessage("Invitation token is missing.")
        setLoading(false)
        return
      }

      try {
        const result = await resolveTenantInvitation(token)
        setInvitation(result)
      } catch (error) {
        const message =
          typeof error === "object" &&
            error !== null &&
            "response" in error &&
            typeof (error as any).response?.data?.message === "string"
            ? (error as any).response.data.message
            : error instanceof Error
              ? error.message
              : "Failed to load invitation."

        setErrorMessage(message)
      } finally {
        setLoading(false)
      }
    }

    void loadInvitation()
  }, [token])

  async function handleAccept() {
    if (!token) {
      setErrorMessage("Invitation token is missing.")
      return
    }

    setSubmitting(true)
    setErrorMessage("")
    setSuccessMessage("")

    try {
      await acceptTenantInvitation(token)

      setSuccessMessage(
        "Invitation accepted successfully. You can now sign in."
      )

      setInvitation((prev) =>
        prev
          ? {
            ...prev,
            status: "Accepted",
          }
          : prev
      )
    } catch (error) {
      const message =
        typeof error === "object" &&
          error !== null &&
          "response" in error &&
          typeof (error as any).response?.data?.message === "string"
          ? (error as any).response.data.message
          : error instanceof Error
            ? error.message
            : "Failed to accept invitation."

      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  function goLogin() {
    navigate("/portal/login", { replace: true })
  }

  function formatDate(value: string) {
    if (!value) return "-"

    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value

    return date.toLocaleString()
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto max-w-3xl">
        <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
          <div className="flex items-center gap-4">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineEnvelopeOpen className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">
                Transaction & Workflow Platform
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Workspace invitation
              </p>
            </div>
          </div>

          <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800">
            Accept your invitation
          </h1>

          <p className="mt-4 max-w-2xl text-base leading-7 text-slate-600">
            Review the invitation details and confirm whether you want to join this organization workspace.
          </p>

          {loading ? (
            <div className="mt-8 space-y-3">
              <div className="h-12 rounded-xl bg-slate-100" />
              <div className="h-12 rounded-xl bg-slate-100" />
              <div className="h-12 rounded-xl bg-slate-100" />
              <div className="h-12 rounded-xl bg-slate-100" />
              <div className="h-12 rounded-xl bg-slate-100" />
            </div>
          ) : errorMessage ? (
            <div
              className="mt-8 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              role="alert"
            >
              {errorMessage}
            </div>
          ) : invitation ? (
            <>
              <div className="mt-8 rounded-2xl border border-slate-200 bg-slate-50 px-5">
                <InfoRow
                  label="Organization"
                  value={invitation.tenantName}
                />
                <InfoRow
                  label="Email"
                  value={invitation.email}
                />
                <InfoRow
                  label="Role"
                  value={<Badge>{invitation.role}</Badge>}
                />
                <InfoRow
                  label="Status"
                  value={
                    <Badge
                      tone={
                        invitation.status === "Pending" ? "warning" : "success"
                      }
                    >
                      {invitation.status}
                    </Badge>
                  }
                />
                <InfoRow
                  label="Expires"
                  value={formatDate(invitation.expiredAt)}
                />
              </div>

              <div className="mt-8 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-4">
                <div className="flex items-start gap-3">
                  <div className="mt-0.5 text-indigo-600">
                    <HiOutlineCheckCircle className="h-5 w-5" />
                  </div>
                  <p className="text-sm leading-6 text-slate-600">
                    Accepting this invitation will add your account to the organization workspace with the role shown above.
                  </p>
                </div>
              </div>

              {successMessage ? (
                <div
                  className="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
                  role="status"
                >
                  {successMessage}
                </div>
              ) : null}

              <div className="mt-8 flex flex-wrap gap-3">
                <div >
                  <button
                    onClick={goLogin}
                    className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
                  >
                    <span>Back to login</span>
                    <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
                  </button>
                </div>

                <button
                  type="button"
                  disabled={!canAccept || submitting}
                  onClick={handleAccept}
                  className="inline-flex items-center rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {submitting ? "Accepting..." : "Accept invitation"}
                </button>
              </div>
            </>
          ) : (
            <div
              className="mt-8 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              role="alert"
            >
              Invitation detail is unavailable.
            </div>
          )}

        </section>
      </div>
    </div>
  )
}