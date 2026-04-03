import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import {
  HiOutlineArrowUpRight,
  HiOutlineUserCircle,
  HiOutlineBeaker,
} from "react-icons/hi2"
import { accountApi } from "../../lib/accountApi"
import { authStore } from "../../lib/authStore"
import { devApi } from "../../lib/devApi"
import type { DevSeedResult } from "@fintrack/web-shared"

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

  function fillDemoAccount(email: string, password: string) {
    setForm({
      email,
      password,
    })

    setErrorMessage("")
  }

  async function onSeedDemoData() {
    setSeedMessage("")
    setSeedErrorMessage("")
    setSeedLoading(true)

    try {
      const result = await devApi.seedDemoData()
      setDemoSeedResult(result)

      const firstTenant = result.tenants?.[0]
      if (firstTenant) {
        setForm({
          email: firstTenant.memberEmail,
          password: firstTenant.memberPassword,
        })
      }

      setSeedMessage(
        "Demo data seeded. Choose an account below to autofill the sign-in form."
      )
    } catch (err: unknown) {
      setSeedErrorMessage(
        err instanceof Error ? err.message : "Seed demo data failed."
      )
    } finally {
      setSeedLoading(false)
    }
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
      const result = await accountApi.login({
        email: form.email.trim(),
        password: form.password,
      })

      authStore.setAccountTokens(
        result.tokens.accessToken,
        result.tokens.refreshToken
      )

      authStore.setMemberships(result.memberships ?? [])

      const profile = await accountApi.getCurrentUser()
      authStore.setProfile(profile)

      navigate("/account/home", { replace: true })
    } catch (err: unknown) {
      const message =
        typeof err === "object" &&
        err !== null &&
        "response" in err &&
        typeof (err as any).response?.data?.message === "string"
          ? (err as any).response.data.message
          : err instanceof Error
            ? err.message
            : "Login failed."

      setErrorMessage(message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto grid max-w-6xl gap-10 lg:grid-cols-[1fr_1fr]">
        <section className="px-2 py-2 sm:px-4">
          <div className="flex items-center gap-4">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineUserCircle className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">FinTrack</p>
              <p className="mt-1 text-sm text-slate-500">
                Account center
              </p>
            </div>
          </div>

          <div className="mt-8">
            <h1 className="text-2xl font-semibold text-slate-800">
              Sign in to your account
            </h1>

            <p className="mt-3 text-sm leading-6 text-slate-500">
              Access your profile, invitations, organization memberships,
              and account settings from one place.
            </p>
          </div>

          <div className="mt-8 flex items-center gap-2">
            <HiOutlineBeaker className="h-5 w-5 text-amber-600" />
            <h2 className="text-2xl font-semibold text-slate-800">
              Try demo
            </h2>
          </div>

          <p className="mt-3 text-sm leading-6 text-slate-500">
            Seed demo tenants and use sample accounts to explore the system quickly.
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
            <div className="mt-4 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
              {seedMessage}
            </div>
          ) : null}

          {seedErrorMessage ? (
            <div className="mt-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
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
                  <h3 className="text-base font-semibold text-slate-800">
                    {tenant.tenantName}
                  </h3>

                  <div className="mt-4 space-y-3">
                    <div className="rounded-xl border border-slate-200 bg-white p-3">
                      <div className="text-sm font-semibold text-slate-800">
                        Member account
                      </div>

                      <div className="mt-2 text-xs text-slate-500">
                        {tenant.memberEmail}
                      </div>

                      <div className="mt-1 text-xs text-slate-500">
                        {tenant.memberPassword}
                      </div>

                      <button
                        type="button"
                        onClick={() =>
                          fillDemoAccount(tenant.memberEmail, tenant.memberPassword)
                        }
                        className="mt-3 inline-flex h-8 items-center justify-center rounded-lg bg-indigo-600 px-3 text-xs font-medium text-white transition hover:bg-indigo-500"
                      >
                        Use member account
                      </button>
                    </div>

                    <div className="rounded-xl border border-slate-200 bg-white p-3">
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
                          fillDemoAccount(tenant.adminEmail, tenant.adminPassword)
                        }
                        className="mt-3 inline-flex h-8 items-center justify-center rounded-lg bg-slate-800 px-3 text-xs font-medium text-white transition hover:bg-slate-700"
                      >
                        Use admin account
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : null}
        </section>

        <section className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm sm:p-10">
          <div>
            <h2 className="text-2xl font-semibold text-slate-800">Sign in</h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Enter your account details to continue.
            </p>
          </div>

          <form className="mt-8 space-y-5" onSubmit={onLogin} autoComplete="off">
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
                placeholder="you@example.com"
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
                placeholder="Enter your password"
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <button
              type="submit"
              className="inline-flex h-11 w-full items-center justify-center rounded-xl bg-indigo-600 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={loading || seedLoading}
            >
              {loading ? "Signing in..." : "Sign in"}
            </button>

            {errorMessage ? (
              <div
                role="alert"
                className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
              >
                {errorMessage}
              </div>
            ) : null}
          </form>

          <div className="my-8 h-px bg-slate-200" />

          <div className="flex flex-wrap gap-3">
            <Link
              to="/account/register-tenant"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Create organization</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>

            <Link
              to="/account/register-user"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Register individual user</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>
          </div>
        </section>
      </div>
    </div>
  )
}