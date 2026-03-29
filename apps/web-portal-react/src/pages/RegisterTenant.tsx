import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { HiOutlineBuildingOffice2, HiOutlineArrowUpRight } from "react-icons/hi2"
import { registerTenant } from "../api/tenant"

export default function RegisterTenant() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [successMessage, setSuccessMessage] = useState("")
  const [errorMessage, setErrorMessage] = useState("")

  const [form, setForm] = useState({
    tenantName: "",
    adminName: "",
    adminEmail: "",
    adminPassword: "",
    confirmPassword: "",
  })

  function updateField<K extends keyof typeof form>(
    key: K,
    value: (typeof form)[K]
  ) {
    setForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  async function onSubmit() {
    setErrorMessage("")
    setSuccessMessage("")

    if (!form.tenantName.trim()) {
      setErrorMessage("Organization name is required.")
      return
    }

    if (!form.adminName.trim()) {
      setErrorMessage("Administrator name is required.")
      return
    }

    if (!form.adminEmail.trim()) {
      setErrorMessage("Administrator email is required.")
      return
    }

    if (!form.adminPassword.trim()) {
      setErrorMessage("Password is required.")
      return
    }

    if (!form.confirmPassword.trim()) {
      setErrorMessage("Please confirm your password.")
      return
    }

    if (form.adminPassword !== form.confirmPassword) {
      setErrorMessage("Passwords do not match.")
      return
    }

    setLoading(true)

    try {
      await registerTenant({
        tenantName: form.tenantName.trim(),
        adminName: form.adminName.trim(),
        adminEmail: form.adminEmail.trim(),
        adminPassword: form.adminPassword,
      })

      setSuccessMessage(
        "Organization registered successfully. Redirecting to sign in..."
      )

      setTimeout(() => {
        navigate("/portal/login", { replace: true })
      }, 1200)
    } catch (err) {
      const msg =
        typeof err === "object" &&
          err !== null &&
          "response" in err &&
          typeof (err as any).response?.data?.message === "string"
          ? (err as any).response.data.message
          : err instanceof Error
            ? err.message
            : "User registration failed."
      setErrorMessage(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto grid max-w-5xl items-center gap-12 lg:grid-cols-[0.85fr_1.15fr]">
        <section className="max-w-md">
          <div className="flex items-center gap-4">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineBuildingOffice2 className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">
                Retail Operations Platform
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Organization workspace setup
              </p>
            </div>
          </div>

          <p className="mt-6 text-sm font-medium uppercase tracking-wide text-indigo-600">
            Workspace setup
          </p>

          <h1 className="mt-3 text-3xl font-semibold tracking-tight text-slate-800">
            Create your organization workspace
          </h1>

          <p className="mt-4 text-base leading-7 text-slate-600">
            Register a tenant and create its first administrator account in one step.
          </p>
        </section>

        <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
          <div>
            <h2 className="text-2xl font-semibold text-slate-800">
              Register organization
            </h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              This creates a tenant and its first administrator account.
            </p>
          </div>

          <div className="mt-8 space-y-5">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Organization name
              </label>
              <input
                type="text"
                placeholder="e.g. Acme Corp"
                value={form.tenantName}
                onChange={(e) => updateField("tenantName", e.target.value)}
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Administrator name
              </label>
              <input
                type="text"
                placeholder="e.g. Emily"
                value={form.adminName}
                onChange={(e) => updateField("adminName", e.target.value)}
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Administrator email
              </label>
              <input
                type="email"
                placeholder="admin@example.com"
                value={form.adminEmail}
                onChange={(e) => updateField("adminEmail", e.target.value)}
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Password
              </label>
              <input
                type="password"
                placeholder="At least 8 characters"
                value={form.adminPassword}
                onChange={(e) => updateField("adminPassword", e.target.value)}
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Confirm password
              </label>
              <input
                type="password"
                placeholder="Re-enter password"
                value={form.confirmPassword}
                onChange={(e) => updateField("confirmPassword", e.target.value)}
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <button
              type="button"
              disabled={loading}
              onClick={onSubmit}
              className="!inline-flex !h-11 !w-full !items-center !justify-center !rounded-xl !bg-indigo-600 !text-sm !font-semibold !text-white transition hover:!bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? "Creating..." : "Create organization"}
            </button>

            {successMessage ? (
              <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                {successMessage}
              </div>
            ) : null}

            {errorMessage ? (
              <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {errorMessage}
              </div>
            ) : null}
          </div>

          <div className="mt-8 flex flex-wrap gap-3">
            <Link
              to="/portal/login"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Back to login</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>

            <Link
              to="/portal/register-user"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Create user account instead</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>
          </div>
        </section>
      </div>
    </div>
  )
}