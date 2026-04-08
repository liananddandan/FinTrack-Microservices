import { useEffect, useMemo, useState } from "react"
import { HiOutlineBuildingOffice2, HiOutlineCheckCircle } from "react-icons/hi2"
import { tenantApi } from "../lib/tenantApi"
import { usePlatformAuthStore } from "../lib/usePlatformAuthStore.ts"
import type { TenantSummaryDto } from "@fintrack/web-shared"

function StatCard({
  title,
  value,
  hint,
}: {
  title: string
  value: string | number
  hint: string
}) {
  return (
    <div className="rounded-3xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="text-sm font-medium text-slate-500">{title}</div>
      <div className="mt-3 text-3xl font-semibold tracking-tight text-slate-800">
        {value}
      </div>
      <div className="mt-2 text-sm text-slate-500">{hint}</div>
    </div>
  )
}

export default function Overview() {
  const authState = usePlatformAuthStore()

  const [tenants, setTenants] = useState<TenantSummaryDto[]>([])
  const [loading, setLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState("")

  useEffect(() => {
    console.log("[Overview] render/auth snapshot", {
      platformRole: authState.platformRole,
      profile: authState.profile,
      userEmail: authState.profile?.email ?? "",
    })
  }, [authState])

  useEffect(() => {

    async function load() {
      setLoading(true)
      setErrorMessage("")

      try {
        const result = await tenantApi.getPlatformTenants()
        setTenants(result)
      } catch (err: unknown) {
        if (err instanceof Error) {
          setErrorMessage(err.message || "Failed to load overview.")
        } else {
          setErrorMessage("Failed to load overview.")
        }
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [])

  const totalTenants = tenants.length
  const activeTenants = useMemo(
    () => tenants.filter((x) => x.isActive).length,
    [tenants]
  )

  const latestTenants = useMemo(
    () =>
      [...tenants]
        .sort(
          (a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        )
        .slice(0, 5),
    [tenants]
  )

  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-slate-200 bg-white px-6 py-6 shadow-sm sm:px-8">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Platform dashboard
            </div>

            <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-800">
              Overview
            </h1>

            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">
              This dashboard gives you a quick view of platform tenants and the current platform context.
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
            <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
              Platform role
            </div>
            <div className="mt-1 text-sm font-semibold text-slate-800">
              {authState.platformRole || "Unknown"}
            </div>
          </div>
        </div>
      </section>

      {errorMessage ? (
        <div
          className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
          role="alert"
        >
          {errorMessage}
        </div>
      ) : null}

      <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <StatCard
          title="Total tenants"
          value={loading ? "..." : totalTenants}
          hint="All tenants currently registered in the system."
        />

        <StatCard
          title="Active tenants"
          value={loading ? "..." : activeTenants}
          hint="Tenants currently marked as active."
        />

        <StatCard
          title="Current platform user"
          value={authState.profile?.email || "Unknown"}
          hint="Signed-in platform administrator account."
        />
      </section>

      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex items-center gap-2">
          <HiOutlineBuildingOffice2 className="h-5 w-5 text-slate-500" />
          <h2 className="text-lg font-semibold text-slate-800">
            Recently created tenants
          </h2>
        </div>

        {loading ? (
          <div className="mt-4 text-sm text-slate-500">Loading tenants...</div>
        ) : latestTenants.length === 0 ? (
          <div className="mt-4 rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
            No tenants found yet.
          </div>
        ) : (
          <div className="mt-5 space-y-3">
            {latestTenants.map((tenant) => (
              <div
                key={tenant.tenantPublicId}
                className="flex flex-col gap-3 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-4 sm:flex-row sm:items-center sm:justify-between"
              >
                <div className="min-w-0">
                  <div className="truncate text-sm font-semibold text-slate-800">
                    {tenant.tenantName}
                  </div>
                  <div className="mt-1 text-xs text-slate-500 break-all">
                    {tenant.tenantPublicId}
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <span
                    className={[
                      "inline-flex items-center gap-1 rounded-full px-3 py-1 text-xs font-medium",
                      tenant.isActive
                        ? "bg-emerald-100 text-emerald-700"
                        : "bg-slate-200 text-slate-700",
                    ].join(" ")}
                  >
                    <HiOutlineCheckCircle className="h-4 w-4" />
                    {tenant.isActive ? "Active" : "Inactive"}
                  </span>

                  <span className="text-xs text-slate-500">
                    {new Date(tenant.createdAt).toLocaleString()}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}