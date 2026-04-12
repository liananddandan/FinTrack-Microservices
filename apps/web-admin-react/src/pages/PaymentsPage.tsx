import { useEffect, useState } from "react"
import { HiOutlineCreditCard, HiOutlineArrowTopRightOnSquare } from "react-icons/hi2"
import type { TenantStripeConnectStatusDto } from "@fintrack/web-shared"
import { paymentApi } from "../lib/paymentApi"

export default function PaymentsPage() {
  const [loading, setLoading] = useState(true)
  const [actionLoading, setActionLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [status, setStatus] = useState<TenantStripeConnectStatusDto | null>(null)

  useEffect(() => {
    void loadStatus()
  }, [])

  async function loadStatus() {
    setLoading(true)
    setErrorMessage("")

    try {
      const result = await paymentApi.getStripeConnectStatus()
      setStatus(result)
    } catch (err) {
      const msg =
        err instanceof Error
          ? err.message
          : "Failed to load payment settings."

      setErrorMessage(msg)
    } finally {
      setLoading(false)
    }
  }

  async function handleConnectStripe() {
    setActionLoading(true)
    setErrorMessage("")

    try {
      const result = await paymentApi.createOrResumeStripeOnboardingLink()

      if (!result.url) {
        setErrorMessage("Failed to create Stripe onboarding link.")
        return
      }

      window.location.href = result.url
    } catch (err) {
      const msg =
        err instanceof Error
          ? err.message
          : "Failed to create Stripe onboarding link."

      setErrorMessage(msg)
    } finally {
      setActionLoading(false)
    }
  }

  function getPrimaryActionLabel() {
    if (!status?.isConnected) {
      return "Connect Stripe"
    }

    if (status.onboardingRequired || !status.chargesEnabled) {
      return "Resume Stripe Onboarding"
    }

    return "Reconnect Stripe"
  }

  return (
    <div className="space-y-6">
      <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex items-start justify-between gap-4">
          <div>
            <div className="inline-flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineCreditCard className="h-6 w-6" />
            </div>

            <h1 className="mt-4 text-2xl font-semibold text-slate-900">
              Payment Settings
            </h1>

            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">
              Connect Stripe for this workspace to enable tenant-level payment processing.
            </p>
          </div>

          <button
            type="button"
            onClick={handleConnectStripe}
            disabled={loading || actionLoading}
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <HiOutlineArrowTopRightOnSquare className="h-4 w-4" />
            {actionLoading ? "Opening..." : getPrimaryActionLabel()}
          </button>
        </div>
      </div>

      {errorMessage ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {errorMessage}
        </div>
      ) : null}

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <StatusCard
          label="Stripe Connected"
          value={loading ? "Loading..." : status?.isConnected ? "Yes" : "No"}
        />

        <StatusCard
          label="Charges Enabled"
          value={loading ? "Loading..." : status?.chargesEnabled ? "Yes" : "No"}
        />

        <StatusCard
          label="Payouts Enabled"
          value={loading ? "Loading..." : status?.payoutsEnabled ? "Yes" : "No"}
        />

        <StatusCard
          label="Onboarding Required"
          value={
            loading
              ? "Loading..."
              : status?.onboardingRequired
                ? "Yes"
                : "No"
          }
        />
      </div>

      <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold text-slate-900">
          Connected Account
        </h2>

        <div className="mt-4 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-4">
          <div className="text-xs uppercase tracking-wide text-slate-500">
            Stripe Connected Account ID
          </div>

          <div className="mt-2 break-all text-sm font-medium text-slate-800">
            {loading
              ? "Loading..."
              : status?.connectedAccountId || "Not connected yet"}
          </div>
        </div>

        <p className="mt-4 text-sm leading-6 text-slate-500">
          Complete Stripe onboarding before enabling payment methods for this tenant.
        </p>
      </div>
    </div>
  )
}

function StatusCard({
  label,
  value,
}: {
  label: string
  value: string
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="text-xs uppercase tracking-wide text-slate-500">
        {label}
      </div>
      <div className="mt-3 text-lg font-semibold text-slate-900">{value}</div>
    </div>
  )
}