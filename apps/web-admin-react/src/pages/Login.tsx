import { useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import { getCurrentUser, login } from "../api/account"
import { seedDemoData, type DevSeedResult } from "../api/dev"
import { authStore } from "../lib/authStore"
import {
  HiOutlineArrowsRightLeft,
  HiOutlineBeaker,
  HiOutlineCube,
} from "react-icons/hi2"

type LoginForm = {
  email: string
  password: string
}

type TenantKey = "aucklandCoffee" | "sushiBar"

const TENANT_CONFIG = {
  aucklandCoffee: {
    displayName: "Auckland Coffee",
    subtitle: "Coffee shop admin workspace",
    icon: HiOutlineBeaker,
    themeClass: "bg-amber-100 text-amber-700",
    matcher: "coffee",
  },
  sushiBar: {
    displayName: "Sushi Bar",
    subtitle: "Restaurant admin workspace",
    icon: HiOutlineCube,
    themeClass: "bg-rose-100 text-rose-700",
    matcher: "sushi",
  },
} satisfies Record<
  TenantKey,
  {
    displayName: string
    subtitle: string
    icon: typeof HiOutlineBeaker
    themeClass: string
    matcher: string
  }
>

function getTenantKeyFromHost(): TenantKey | null {
  const host = window.location.hostname.toLowerCase()

  if (host.includes("coffee.")) {
    return "aucklandCoffee"
  }

  if (host.includes("sushi.")) {
    return "sushiBar"
  }

  return null
}

export default function Login() {
  const navigate = useNavigate()

  const tenantKey = useMemo(() => getTenantKeyFromHost(), [])
  const tenantConfig = tenantKey ? TENANT_CONFIG[tenantKey] : null
  const TenantIcon = tenantConfig?.icon ?? HiOutlineArrowsRightLeft

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

  function fillDemoAdmin(email: string, password: string) {
    setForm({
      email,
      password,
    })

    setErrorMessage("")
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

      const filteredTenants = result.tenants.filter((tenant) => {
        if (!tenantKey || !tenantConfig) {
          return true
        }

        return tenant.tenantName
          .toLowerCase()
          .includes(tenantConfig.matcher)
      })

      const filteredResult = {
        ...result,
        tenants: filteredTenants,
      }

      setDemoSeedResult(filteredResult)

      const firstTenant = filteredTenants?.[0]

      if (firstTenant) {
        setForm({
          email: firstTenant.adminEmail,
          password: firstTenant.adminPassword,
        })
      }

      setSeedMessage(
        "Demo data seeded. Choose an administrator account below to autofill the sign-in form."
      )
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
      <div className="mx-auto grid max-w-6xl gap-10 lg:grid-cols-[1fr_1fr]">
        <section className="px-2 py-2 sm:px-4">
          <div className="flex items-center gap-4">
            <div
              className={`flex h-14 w-14 items-center justify-center rounded-2xl ${
                tenantConfig?.themeClass ?? "bg-indigo-100 text-indigo-600"
              }`}
            >
              <TenantIcon className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">
                {tenantConfig?.displayName ?? "Multi-tenant retail management"}
              </p>
              <p className="mt-1 text-sm text-slate-500">
                {tenantConfig?.subtitle ?? "Admin workspace access"}
              </p>
            </div>
          </div>

          <div className="mt-8 flex items-center gap-2">
            <HiOutlineBeaker className="h-5 w-5 text-amber-600" />
            <h1 className="text-2xl font-semibold text-slate-800">
              Use demo data
            </h1>
          </div>

          <p className="mt-3 text-sm leading-6 text-slate-500">
            Seed demo tenants and choose an administrator account to enter the admin workspace.
          </p>

          <button
            type="button"
            disabled={seedLoading}
            onClick={() => void onSeedDemoData()}
            className="mt-6 inline-flex h-11 w-full items-center justify-center rounded-xl border border-amber-200 bg-amber-50 text-sm font-semibold text-amber-700 transition hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {seedLoading ? "Seeding..." : "Seed demo data"}
          </button>

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

          {demoSeedResult?.tenants?.length ? (
            <div className="mt-6 grid gap-4 md:grid-cols-2">
              {demoSeedResult.tenants.map((tenant) => (
                <div
                  key={tenant.tenantPublicId}
                  className="rounded-2xl border border-slate-200 bg-slate-50 p-4"
                >
                  <h2 className="text-base font-semibold text-slate-800">
                    {tenant.tenantName}
                  </h2>

                  <div className="mt-4 rounded-xl border border-slate-200 bg-white p-3">
                    <div className="text-sm font-semibold text-slate-800">
                      Admin account
                    </div>

                    <div className="mt-2 text-xs text-slate-500">
                      {tenant.adminEmail}
                    </div>

                    <div className="mt-1 text-xs text-slate-500">
                      {tenant.adminPassword}
                    </div>

                    <button
                      type="button"
                      onClick={() =>
                        fillDemoAdmin(tenant.adminEmail, tenant.adminPassword)
                      }
                      className="mt-3 inline-flex h-8 items-center justify-center rounded-lg bg-indigo-600 px-3 text-xs font-medium text-white transition hover:bg-indigo-500"
                    >
                      Use demo admin
                    </button>
                  </div>
                </div>
              ))}
            </div>
          ) : null}
        </section>

        <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
          <div>
            <h2 className="text-2xl font-semibold text-slate-800">
              Admin sign in
            </h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Use your administrator credentials to continue.
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
        </section>
      </div>
    </div>
  )
}