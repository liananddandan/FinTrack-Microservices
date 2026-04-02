import { useEffect, useMemo, useState } from "react"
import { HiOutlineBuildingOffice2 } from "react-icons/hi2"
import { getPlatformTenants, type TenantSummaryDto } from "../api/tenant"

export default function Tenants() {
  const [tenants, setTenants] = useState<TenantSummaryDto[]>([])
  const [loading, setLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState("")
  const [search, setSearch] = useState("")

  useEffect(() => {
    async function load() {
      setLoading(true)
      setErrorMessage("")

      try {
        const result = await getPlatformTenants()
        setTenants(result)
      } catch (err: unknown) {
        if (err instanceof Error) {
          setErrorMessage(err.message || "Failed to load tenants.")
        } else {
          setErrorMessage("Failed to load tenants.")
        }
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [])

  const filteredTenants = useMemo(() => {
    const keyword = search.trim().toLowerCase()

    if (!keyword) {
      return tenants
    }

    return tenants.filter(
      (tenant) =>
        tenant.tenantName.toLowerCase().includes(keyword) ||
        tenant.tenantPublicId.toLowerCase().includes(keyword)
    )
  }, [search, tenants])

  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-slate-200 bg-white px-6 py-6 shadow-sm sm:px-8">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Platform management
            </div>

            <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-800">
              Tenants
            </h1>

            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-500">
              View all tenants currently registered in the system. Tenant creation and domain mapping can be added next.
            </p>
          </div>

          <div className="w-full max-w-sm">
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Search tenants
            </label>
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name or public id"
              className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
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

      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex items-center gap-2">
          <HiOutlineBuildingOffice2 className="h-5 w-5 text-slate-500" />
          <h2 className="text-lg font-semibold text-slate-800">
            Tenant directory
          </h2>
        </div>

        {loading ? (
          <div className="mt-4 text-sm text-slate-500">Loading tenants...</div>
        ) : filteredTenants.length === 0 ? (
          <div className="mt-5 rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
            No tenants matched your search.
          </div>
        ) : (
          <div className="mt-5 overflow-hidden rounded-2xl border border-slate-200">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-slate-200">
                <thead className="bg-slate-50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                      Tenant
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                      Public Id
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                      Status
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                      Created At
                    </th>
                  </tr>
                </thead>

                <tbody className="divide-y divide-slate-200 bg-white">
                  {filteredTenants.map((tenant) => (
                    <tr key={tenant.tenantPublicId}>
                      <td className="px-4 py-4 text-sm font-medium text-slate-800">
                        {tenant.tenantName}
                      </td>

                      <td className="px-4 py-4 text-sm text-slate-500 break-all">
                        {tenant.tenantPublicId}
                      </td>

                      <td className="px-4 py-4 text-sm">
                        <span
                          className={[
                            "inline-flex rounded-full px-3 py-1 text-xs font-medium",
                            tenant.isActive
                              ? "bg-emerald-100 text-emerald-700"
                              : "bg-slate-200 text-slate-700",
                          ].join(" ")}
                        >
                          {tenant.isActive ? "Active" : "Inactive"}
                        </span>
                      </td>

                      <td className="px-4 py-4 text-sm text-slate-500">
                        {new Date(tenant.createdAt).toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </section>
    </div>
  )
}