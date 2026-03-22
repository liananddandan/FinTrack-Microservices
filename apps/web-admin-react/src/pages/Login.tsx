import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { getCurrentUser, login } from "../api/account"
import { seedDemoData, type DevSeedResult } from "../api/dev"
import { authStore } from "../lib/authStore"
import {
  HiOutlineArrowsRightLeft,
  HiOutlineBeaker,
} from "react-icons/hi2"

type LoginForm = {
  email: string
  password: string
}

export default function Login() {
  const navigate = useNavigate()

  const [form, setForm] = useState<LoginForm>({
    email: "",
    password: "",
  })

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")

  const [seedLoading, setSeedLoading] = useState(false)
  const [seedMessage, setSeedMessage] = useState("")
  const [seedErrorMessage, setSeedErrorMessage] = useState("")
  const [demoSeedResult, setDemoSeedResult] = useState<DevSeedResult | null>(null)

  function updateField<K extends keyof LoginForm>(key: K, value: LoginForm[K]) {
    setForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  async function onLogin(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage("")

    if (!form.email.trim()) {
      setErrorMessage("Email is required.")
      return
    }

    if (!form.password.trim()) {
      setErrorMessage("Password is required.")
      return
    }

    setLoading(true)

    try {
      const result = await login({
        email: form.email.trim(),
        password: form.password,
      })

      authStore.setAccountTokens(
        result.tokens.accessToken,
        result.tokens.refreshToken
      )

      authStore.clearTenantAccessToken()
      authStore.setMemberships(result.memberships ?? [])

      const profile = await getCurrentUser()
      authStore.setProfile(profile)

      if (!authStore.isAdmin) {
        authStore.logout()
        setErrorMessage("This account does not have admin access.")
        return
      }

      const activated = await authStore.activateSingleAdminTenantIfPossible()

      if (!activated) {
        authStore.clearTenantAccessToken()
        setErrorMessage("Multiple admin tenants are not supported in V1.")
        return
      }

      navigate("/overview", { replace: true })
    } catch (err: unknown) {
      if (typeof err === "object" && err !== null && "response" in err) {
        const maybeAxiosError = err as {
          response?: {
            data?: {
              message?: string
            }
          }
          message?: string
        }

        setErrorMessage(
          maybeAxiosError.response?.data?.message ??
            maybeAxiosError.message ??
            "Login failed."
        )
      } else if (err instanceof Error) {
        setErrorMessage(err.message)
      } else {
        setErrorMessage("Login failed.")
      }
    } finally {
      setLoading(false)
    }
  }

  async function onSeedDemoData() {
    setSeedMessage("")
    setSeedErrorMessage("")
    setSeedLoading(true)

    try {
      const result = await seedDemoData()
      setDemoSeedResult(result)

      setForm({
        email: result.adminEmail,
        password: result.adminPassword,
      })

      setSeedMessage("Demo data seeded. Admin credentials are ready to use.")
    } catch (err: unknown) {
      if (typeof err === "object" && err !== null && "response" in err) {
        const maybeAxiosError = err as {
          response?: {
            data?: {
              message?: string
            }
          }
          message?: string
        }

        setSeedErrorMessage(
          maybeAxiosError.response?.data?.message ??
            maybeAxiosError.message ??
            "Seed demo data failed."
        )
      } else if (err instanceof Error) {
        setSeedErrorMessage(err.message)
      } else {
        setSeedErrorMessage("Seed demo data failed.")
      }
    } finally {
      setSeedLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto grid max-w-5xl items-center gap-12 lg:grid-cols-[0.9fr_1.1fr]">
        <section className="max-w-md">
          <div className="flex items-center gap-4">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineArrowsRightLeft className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">
                Transaction & Workflow Platform
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Admin workspace access
              </p>
            </div>
          </div>

          <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800">
            Admin sign in
          </h1>

          <p className="mt-4 text-base leading-7 text-slate-600">
            Sign in with an administrator account to access tenant-level
            management, transaction review, member administration, and audit
            activity.
          </p>
        </section>

        <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
          <div>
            <h2 className="text-2xl font-semibold text-slate-800">
              Sign in
            </h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Use your admin credentials to continue.
            </p>
          </div>

          <form className="mt-8 space-y-5" onSubmit={onLogin}>
            <div>
              <label
                htmlFor="email"
                className="mb-2 block text-sm font-medium text-slate-700"
              >
                Email
              </label>
              <input
                id="email"
                type="email"
                value={form.email}
                onChange={(e) => updateField("email", e.target.value)}
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div>
              <label
                htmlFor="password"
                className="mb-2 block text-sm font-medium text-slate-700"
              >
                Password
              </label>
              <input
                id="password"
                type="password"
                value={form.password}
                onChange={(e) => updateField("password", e.target.value)}
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="inline-flex h-11 w-full items-center justify-center rounded-xl bg-indigo-600 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? "Signing in..." : "Sign in"}
            </button>

            {errorMessage ? (
              <div
                className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
                role="alert"
              >
                {errorMessage}
              </div>
            ) : null}
          </form>

          <div className="my-8 h-px bg-slate-200" />

          <div className="flex items-center gap-2">
            <HiOutlineBeaker className="h-5 w-5 text-amber-600" />
            <p className="text-sm font-medium text-slate-700">Demo setup</p>
          </div>

          <p className="mt-2 text-sm leading-6 text-slate-500">
            Seed a demo tenant and test accounts for local development.
          </p>

          <div className="mt-4">
            <button
              type="button"
              disabled={seedLoading}
              onClick={onSeedDemoData}
              className="inline-flex h-11 w-full items-center justify-center rounded-xl border border-amber-200 bg-amber-50 text-sm font-semibold text-amber-700 transition hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {seedLoading ? "Seeding..." : "Seed demo data"}
            </button>
          </div>

          {seedMessage ? (
            <div
              className="mt-4 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
              role="status"
            >
              {seedMessage}
            </div>
          ) : null}

          {seedErrorMessage ? (
            <div
              className="mt-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              role="alert"
            >
              {seedErrorMessage}
            </div>
          ) : null}

          {demoSeedResult ? (
            <div className="mt-6 rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <h3 className="text-sm font-semibold text-slate-800">
                Demo credentials
              </h3>

              <div className="mt-4 space-y-3 text-sm text-slate-600">
                <div>
                  <span className="font-medium text-slate-700">Tenant:</span>{" "}
                  {demoSeedResult.tenantName}
                </div>

                <div>
                  <span className="font-medium text-slate-700">Admin:</span>{" "}
                  {demoSeedResult.adminEmail} / {demoSeedResult.adminPassword}
                </div>

                <div>
                  <span className="font-medium text-slate-700">Member:</span>{" "}
                  {demoSeedResult.memberEmail} / {demoSeedResult.memberPassword}
                </div>
              </div>
            </div>
          ) : null}
        </section>
      </div>
    </div>
  )
}