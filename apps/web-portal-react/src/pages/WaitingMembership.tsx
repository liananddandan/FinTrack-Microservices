import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { authStore } from "../lib/authStore"
import { useAuth } from "../hooks/useAuth"
import { authService } from "../lib/authService"
import { accountApi } from "../lib/accountApi"
import {
  HiOutlineClock,
  HiOutlineEnvelope,
  HiOutlineArrowPath,
  HiOutlineArrowLeftOnRectangle,
  HiOutlineCheckCircle,
  HiOutlineUserPlus,
} from "react-icons/hi2"

function InfoRow({
  label,
  value,
}: {
  label: string
  value: string
}) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm text-slate-500">{label}</span>
      <span className="break-all text-right text-sm font-medium text-slate-800">
        {value}
      </span>
    </div>
  )
}

function StepRow({
  icon,
  text,
}: {
  icon: React.ReactNode
  text: string
}) {
  return (
    <div className="flex items-start gap-3">
      <div className="mt-0.5 text-indigo-600">{icon}</div>
      <p className="text-sm leading-6 text-slate-600">{text}</p>
    </div>
  )
}

export default function WaitingMembership() {
  const navigate = useNavigate()
  const auth = useAuth()

  const [loading, setLoading] = useState(false)
  const [initializing, setInitializing] = useState(true)
  const [message, setMessage] = useState("")
  const [errorMessage, setErrorMessage] = useState("")

  async function redirectIfTenantReady() {
    if (authStore.hasTenantContext) {
      navigate("/portal/home", { replace: true })
      return
    }

    const memberships = authStore.resolvedMemberships

    if (memberships.length === 1) {
      try {
        await authService.activateTenantForCurrentHost()

        if (authStore.hasTenantContext) {
          navigate("/portal/home", { replace: true })
        }
      } catch {
        // stay on page
      }
    }
  }

  useEffect(() => {
    async function init() {
      try {
        await auth.initializeProfile()
        await redirectIfTenantReady()
      } finally {
        setInitializing(false)
      }
    }

    void init()
  }, [])

  async function refreshProfile() {
    setLoading(true)
    setMessage("")
    setErrorMessage("")

    try {
      const profile = await accountApi.getCurrentUser()
      authStore.setProfile(profile)

      if ((profile.memberships?.length ?? 0) > 0) {
        await authService.activateTenantForCurrentHost()
      }

      if (authStore.hasTenantContext) {
        navigate("/portal/home", { replace: true })
        return
      }

      setMessage("Status refreshed.")
    } catch (err: unknown) {
      const msg =
        typeof err === "object" &&
        err !== null &&
        "response" in err &&
        typeof (err as any).response?.data?.message === "string"
          ? (err as any).response.data.message
          : err instanceof Error
          ? err.message
          : "Failed to refresh account status."

      setErrorMessage(msg)
    } finally {
      setLoading(false)
    }
  }

  function logout() {
    authStore.logout()
    navigate("/portal/login", { replace: true })
  }

  if (initializing) {
    return (
      <div className="min-h-screen bg-slate-50 px-6 py-10">
        <div className="mx-auto max-w-3xl text-sm text-slate-500">
          Loading account status...
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto max-w-3xl">
        <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
          <div className="flex items-center gap-4">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineClock className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">
                Retail Operations Platform
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Pending workspace access
              </p>
            </div>
          </div>

          <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800">
            Workspace access required
          </h1>

          <p className="mt-4 text-base leading-7 text-slate-600">
            Your account is ready, but you are not connected to any active workspace yet.
          </p>

          <div className="mt-8 rounded-2xl border border-slate-200 bg-slate-50 px-5">
            <InfoRow label="Email" value={auth.userEmail || "-"} />
            <InfoRow label="User name" value={auth.userName || "-"} />
            <InfoRow label="Status" value="Pending workspace access" />
          </div>

          <div className="mt-8">
            <h2 className="text-lg font-semibold text-slate-800">Next steps</h2>

            <div className="mt-4 space-y-4">
              <StepRow
                icon={<HiOutlineUserPlus className="h-5 w-5" />}
                text="Ask an administrator to add your account to a workspace."
              />

              <StepRow
                icon={<HiOutlineEnvelope className="h-5 w-5" />}
                text="Check your email and accept the invitation if one was sent."
              />

              <StepRow
                icon={<HiOutlineCheckCircle className="h-5 w-5" />}
                text="Refresh your status after joining."
              />
            </div>
          </div>

          {message ? (
            <div
              className="mt-6 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
              role="status"
            >
              {message}
            </div>
          ) : null}

          {errorMessage ? (
            <div
              className="mt-6 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              role="alert"
            >
              {errorMessage}
            </div>
          ) : null}

          <div className="mt-8 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={refreshProfile}
              disabled={loading}
              className="inline-flex items-center gap-2 rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <HiOutlineArrowPath className="h-4 w-4" />
              {loading ? "Refreshing..." : "Refresh status"}
            </button>

            <button
              type="button"
              onClick={logout}
              className="inline-flex items-center gap-2 rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
            >
              <HiOutlineArrowLeftOnRectangle className="h-4 w-4" />
              Sign out
            </button>
          </div>
        </section>
      </div>
    </div>
  )
}