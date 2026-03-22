import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { HiOutlineArrowsRightLeft, HiOutlineArrowUpRight } from "react-icons/hi2"
import { getCurrentUser, login } from "../api/account"
import { authStore } from "../lib/authStore"

type LoginForm = {
  email: string
  password: string
}

export default function Login() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [form, setForm] = useState<LoginForm>({
    email: "",
    password: "",
  })

  function updateField<K extends keyof LoginForm>(key: K, value: LoginForm[K]) {
    setForm((prev) => ({
      ...prev,
      [key]: value,
    }))
  }

  async function onLogin() {
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

      const memberships = profile.memberships ?? []

      if (memberships.length === 0) {
        navigate("/portal/waiting-membership", { replace: true })
        return
      }

      if (memberships.length === 1) {
        await authStore.activateSingleTenantIfPossible()

        if (authStore.hasTenantContext) {
          navigate("/portal/home", { replace: true })
          return
        }

        navigate("/portal/waiting-membership", { replace: true })
        return
      }

      navigate("/portal/waiting-membership", { replace: true })
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
      <div className="mx-auto grid max-w-5xl items-center gap-12 lg:grid-cols-[0.85fr_1.15fr]">
        <section className="max-w-md">
          <div className="flex items-center gap-4">
            <div className="text-indigo-600">
              <HiOutlineArrowsRightLeft className="h-12 w-12" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">
                Transaction & Workflow Platform
              </p>
              <p className="mt-1 text-sm text-slate-500">
                Multi-tenant portal access
              </p>
            </div>
          </div>

          <p className="mt-6 text-base leading-7 text-slate-600">
            Sign in to access your workspace and continue your transaction flow.
          </p>
        </section>

        <section className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm sm:p-10">
          <div>
            <h1 className="text-2xl font-semibold text-slate-800">Sign in</h1>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Enter your account details to continue.
            </p>
          </div>

          <form
            className="mt-8 space-y-5"
            onSubmit={(e) => {
              e.preventDefault()
              void onLogin()
            }}
          >
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
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
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
                className="!block !h-11 !w-full !rounded-xl !border !border-slate-300 !bg-white !px-3 !text-sm !text-slate-800 !opacity-100 outline-none placeholder:!text-slate-400 focus:!border-indigo-500 focus:!ring-2 focus:!ring-indigo-100"
              />
            </div>

            <button
              type="submit"
              className="!inline-flex !h-11 !w-full !items-center !justify-center !rounded-xl !bg-indigo-600 !text-sm !font-semibold !text-white transition hover:!bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={loading}
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
              to="/portal/register-tenant"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Create organization</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>

            <Link
              to="/portal/register-user"
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