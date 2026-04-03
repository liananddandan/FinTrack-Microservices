import { useEffect, useMemo, useState } from "react"
import { useNavigate, useParams } from "react-router-dom"
import {
  HiOutlineArrowLeft,
  HiOutlineBuildingOffice2,
  HiOutlineGlobeAlt,
  HiOutlinePencilSquare,
  HiOutlinePlus,
  HiOutlineTrash,
} from "react-icons/hi2"
import { tenantApi } from "../lib/tenantApi"
import type {
  TenantSummaryDto,
  TenantDomainMappingDto,
} from "@fintrack/web-shared"

type DomainForm = {
  host: string
  domainType: string
  isPrimary: boolean
  isActive: boolean
}

const defaultForm: DomainForm = {
  host: "",
  domainType: "TenantPortal",
  isPrimary: false,
  isActive: true,
}

export default function TenantConfig() {
  const navigate = useNavigate()
  const { tenantPublicId = "" } = useParams()

  const [tenant, setTenant] = useState<TenantSummaryDto | null>(null)
  const [domains, setDomains] = useState<TenantDomainMappingDto[]>([])
  const [loading, setLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState("")

  const [showCreateForm, setShowCreateForm] = useState(false)
  const [createForm, setCreateForm] = useState<DomainForm>(defaultForm)
  const [submitting, setSubmitting] = useState(false)

  const [editingDomainId, setEditingDomainId] = useState<string>("")
  const [editForm, setEditForm] = useState<DomainForm>(defaultForm)

  useEffect(() => {
    async function load() {
      setLoading(true)
      setErrorMessage("")

      try {
        const [tenantsResult, domainsResult] = await Promise.all([
          tenantApi.getPlatformTenants(),
          tenantApi.getTenantDomains(tenantPublicId),
        ])

        const currentTenant =
          tenantsResult.find((x) => x.tenantPublicId === tenantPublicId) ?? null

        setTenant(currentTenant)
        setDomains(domainsResult)
      } catch (err: unknown) {
        if (err instanceof Error) {
          setErrorMessage(err.message || "Failed to load tenant configuration.")
        } else {
          setErrorMessage("Failed to load tenant configuration.")
        }
      } finally {
        setLoading(false)
      }
    }

    if (tenantPublicId) {
      void load()
    }
  }, [tenantPublicId])

  const sortedDomains = useMemo(() => {
    return [...domains].sort((a, b) => {
      if (a.domainType !== b.domainType) {
        return a.domainType.localeCompare(b.domainType)
      }

      if (a.isPrimary !== b.isPrimary) {
        return a.isPrimary ? -1 : 1
      }

      return a.host.localeCompare(b.host)
    })
  }, [domains])

  function updateCreateField<K extends keyof DomainForm>(
    key: K,
    value: DomainForm[K]
  ) {
    setCreateForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  function updateEditField<K extends keyof DomainForm>(
    key: K,
    value: DomainForm[K]
  ) {
    setEditForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  function beginEdit(domain: TenantDomainMappingDto) {
    setEditingDomainId(domain.domainPublicId)
    setEditForm({
      host: domain.host,
      domainType: domain.domainType,
      isPrimary: domain.isPrimary,
      isActive: domain.isActive,
    })
  }

  function cancelEdit() {
    setEditingDomainId("")
    setEditForm(defaultForm)
  }

  async function reloadDomains() {
    const result = await tenantApi.getTenantDomains(tenantPublicId)
    setDomains(result)
  }

  async function onCreateDomain(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage("")

    if (!createForm.host.trim()) {
      setErrorMessage("Host is required.")
      return
    }

    setSubmitting(true)

    try {
      await tenantApi.createTenantDomain({
        tenantPublicId,
        host: createForm.host.trim(),
        domainType: createForm.domainType,
        isPrimary: createForm.isPrimary,
        isActive: createForm.isActive,
      })

      setCreateForm(defaultForm)
      setShowCreateForm(false)
      await reloadDomains()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to create domain mapping.")
      } else {
        setErrorMessage("Failed to create domain mapping.")
      }
    } finally {
      setSubmitting(false)
    }
  }

  async function onUpdateDomain(domainPublicId: string) {
    setErrorMessage("")
    setSubmitting(true)

    try {
      await tenantApi.updateTenantDomain(domainPublicId, {
        host: editForm.host.trim(),
        domainType: editForm.domainType,
        isPrimary: editForm.isPrimary,
        isActive: editForm.isActive,
      })

      setEditingDomainId("")
      setEditForm(defaultForm)
      await reloadDomains()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to update domain mapping.")
      } else {
        setErrorMessage("Failed to update domain mapping.")
      }
    } finally {
      setSubmitting(false)
    }
  }

  async function onToggleActive(domain: TenantDomainMappingDto) {
    setErrorMessage("")

    try {
      await tenantApi.setTenantDomainActive(domain.domainPublicId, !domain.isActive)
      await reloadDomains()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to update domain status.")
      } else {
        setErrorMessage("Failed to update domain status.")
      }
    }
  }

  async function onDeleteDomain(domainPublicId: string) {
    const confirmed = window.confirm(
      "Delete this domain mapping? This action cannot be undone."
    )

    if (!confirmed) {
      return
    }

    setErrorMessage("")

    try {
      await tenantApi.deleteTenantDomain(domainPublicId)
      await reloadDomains()
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErrorMessage(err.message || "Failed to delete domain mapping.")
      } else {
        setErrorMessage("Failed to delete domain mapping.")
      }
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-slate-200 bg-white px-6 py-6 shadow-sm sm:px-8">
        <button
          type="button"
          onClick={() => navigate("/tenants")}
          className="inline-flex items-center gap-2 rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
        >
          <HiOutlineArrowLeft className="h-4 w-4" />
          Back to tenants
        </button>

        <div className="mt-5 flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Tenant configuration
            </div>

            <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-800">
              {tenant?.tenantName ?? "Tenant"}
            </h1>

            <p className="mt-2 text-sm leading-6 text-slate-500 break-all">
              {tenantPublicId}
            </p>
          </div>

          <div className="rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
            <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
              Status
            </div>
            <div className="mt-1 text-sm font-semibold text-slate-800">
              {tenant?.isActive ? "Active" : "Inactive"}
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

      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex items-center gap-2">
          <HiOutlineBuildingOffice2 className="h-5 w-5 text-slate-500" />
          <h2 className="text-lg font-semibold text-slate-800">Overview</h2>
        </div>

        {loading ? (
          <div className="mt-4 text-sm text-slate-500">Loading tenant...</div>
        ) : (
          <div className="mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Tenant name
              </div>
              <div className="mt-2 text-sm font-semibold text-slate-800">
                {tenant?.tenantName ?? "-"}
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Tenant id
              </div>
              <div className="mt-2 break-all text-sm text-slate-700">
                {tenantPublicId}
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Status
              </div>
              <div className="mt-2 text-sm font-semibold text-slate-800">
                {tenant?.isActive ? "Active" : "Inactive"}
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Created at
              </div>
              <div className="mt-2 text-sm text-slate-700">
                {tenant?.createdAt
                  ? new Date(tenant.createdAt).toLocaleString()
                  : "-"}
              </div>
            </div>
          </div>
        )}
      </section>

      <section className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-2">
            <HiOutlineGlobeAlt className="h-5 w-5 text-slate-500" />
            <h2 className="text-lg font-semibold text-slate-800">Domains</h2>
          </div>

          <button
            type="button"
            onClick={() => setShowCreateForm((prev) => !prev)}
            className="inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
          >
            <HiOutlinePlus className="h-4 w-4" />
            Add domain
          </button>
        </div>

        {showCreateForm ? (
          <form
            className="mt-5 grid gap-4 rounded-2xl border border-slate-200 bg-slate-50 p-4 lg:grid-cols-2"
            onSubmit={onCreateDomain}
          >
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Host
              </label>
              <input
                type="text"
                value={createForm.host}
                onChange={(e) => updateCreateField("host", e.target.value)}
                placeholder="coffee.chenlis.com"
                className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Domain type
              </label>
              <select
                value={createForm.domainType}
                onChange={(e) => updateCreateField("domainType", e.target.value)}
                className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              >
                <option value="TenantPortal">TenantPortal</option>
                <option value="TenantAdmin">TenantAdmin</option>
              </select>
            </div>

            <label className="inline-flex items-center gap-2 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={createForm.isPrimary}
                onChange={(e) => updateCreateField("isPrimary", e.target.checked)}
              />
              Primary domain
            </label>

            <label className="inline-flex items-center gap-2 text-sm text-slate-700">
              <input
                type="checkbox"
                checked={createForm.isActive}
                onChange={(e) => updateCreateField("isActive", e.target.checked)}
              />
              Active
            </label>

            <div className="lg:col-span-2 flex items-center gap-3">
              <button
                type="submit"
                disabled={submitting}
                className="inline-flex h-10 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:opacity-60"
              >
                {submitting ? "Saving..." : "Save domain"}
              </button>

              <button
                type="button"
                onClick={() => {
                  setShowCreateForm(false)
                  setCreateForm(defaultForm)
                }}
                className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
              >
                Cancel
              </button>
            </div>
          </form>
        ) : null}

        {loading ? (
          <div className="mt-4 text-sm text-slate-500">Loading domains...</div>
        ) : sortedDomains.length === 0 ? (
          <div className="mt-5 rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center text-sm text-slate-500">
            No domain mappings configured yet.
          </div>
        ) : (
          <div className="mt-5 space-y-4">
            {sortedDomains.map((domain) => {
              const isEditing = editingDomainId === domain.domainPublicId

              return (
                <div
                  key={domain.domainPublicId}
                  className="rounded-2xl border border-slate-200 bg-slate-50 p-4"
                >
                  {isEditing ? (
                    <div className="grid gap-4 lg:grid-cols-2">
                      <div>
                        <label className="mb-2 block text-sm font-medium text-slate-700">
                          Host
                        </label>
                        <input
                          type="text"
                          value={editForm.host}
                          onChange={(e) => updateEditField("host", e.target.value)}
                          className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                        />
                      </div>

                      <div>
                        <label className="mb-2 block text-sm font-medium text-slate-700">
                          Domain type
                        </label>
                        <select
                          value={editForm.domainType}
                          onChange={(e) =>
                            updateEditField("domainType", e.target.value)
                          }
                          className="h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                        >
                          <option value="TenantPortal">TenantPortal</option>
                          <option value="TenantAdmin">TenantAdmin</option>
                        </select>
                      </div>

                      <label className="inline-flex items-center gap-2 text-sm text-slate-700">
                        <input
                          type="checkbox"
                          checked={editForm.isPrimary}
                          onChange={(e) =>
                            updateEditField("isPrimary", e.target.checked)
                          }
                        />
                        Primary domain
                      </label>

                      <label className="inline-flex items-center gap-2 text-sm text-slate-700">
                        <input
                          type="checkbox"
                          checked={editForm.isActive}
                          onChange={(e) =>
                            updateEditField("isActive", e.target.checked)
                          }
                        />
                        Active
                      </label>

                      <div className="lg:col-span-2 flex items-center gap-3">
                        <button
                          type="button"
                          disabled={submitting}
                          onClick={() => void onUpdateDomain(domain.domainPublicId)}
                          className="inline-flex h-10 items-center justify-center rounded-xl bg-indigo-600 px-4 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:opacity-60"
                        >
                          {submitting ? "Saving..." : "Save changes"}
                        </button>

                        <button
                          type="button"
                          onClick={cancelEdit}
                          className="inline-flex h-10 items-center justify-center rounded-xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:bg-slate-50"
                        >
                          Cancel
                        </button>
                      </div>
                    </div>
                  ) : (
                    <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                      <div className="min-w-0">
                        <div className="break-all text-sm font-semibold text-slate-800">
                          {domain.host}
                        </div>

                        <div className="mt-2 flex flex-wrap items-center gap-2">
                          <span className="inline-flex rounded-full bg-indigo-100 px-3 py-1 text-xs font-medium text-indigo-700">
                            {domain.domainType}
                          </span>

                          <span
                            className={[
                              "inline-flex rounded-full px-3 py-1 text-xs font-medium",
                              domain.isPrimary
                                ? "bg-amber-100 text-amber-700"
                                : "bg-slate-200 text-slate-700",
                            ].join(" ")}
                          >
                            {domain.isPrimary ? "Primary" : "Secondary"}
                          </span>

                          <span
                            className={[
                              "inline-flex rounded-full px-3 py-1 text-xs font-medium",
                              domain.isActive
                                ? "bg-emerald-100 text-emerald-700"
                                : "bg-slate-200 text-slate-700",
                            ].join(" ")}
                          >
                            {domain.isActive ? "Active" : "Inactive"}
                          </span>
                        </div>

                        <div className="mt-3 text-xs text-slate-500 break-all">
                          {domain.domainPublicId}
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-2">
                        <button
                          type="button"
                          onClick={() => beginEdit(domain)}
                          className="inline-flex items-center gap-2 rounded-xl border border-slate-300 bg-white px-3 py-2 text-xs font-medium text-slate-700 transition hover:bg-slate-50"
                        >
                          <HiOutlinePencilSquare className="h-4 w-4" />
                          Edit
                        </button>

                        <button
                          type="button"
                          onClick={() => void onToggleActive(domain)}
                          className="inline-flex rounded-xl border border-slate-300 bg-white px-3 py-2 text-xs font-medium text-slate-700 transition hover:bg-slate-50"
                        >
                          {domain.isActive ? "Disable" : "Enable"}
                        </button>

                        <button
                          type="button"
                          onClick={() => void onDeleteDomain(domain.domainPublicId)}
                          className="inline-flex items-center gap-2 rounded-xl border border-rose-300 bg-white px-3 py-2 text-xs font-medium text-rose-700 transition hover:bg-rose-50"
                        >
                          <HiOutlineTrash className="h-4 w-4" />
                          Delete
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        )}
      </section>
    </div>
  )
}