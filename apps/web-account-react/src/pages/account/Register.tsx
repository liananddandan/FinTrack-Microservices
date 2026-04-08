import { useState } from "react"
import { useNavigate, Link } from "react-router-dom"
import { HiOutlineUserPlus, HiOutlineArrowUpRight } from "react-icons/hi2"
import { accountApi } from "../../lib/accountApi"
import { Turnstile } from "@marsidev/react-turnstile"

export default function RegisterUser() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")
  const [turnstileToken, setTurnstileToken] = useState("")

  const [form, setForm] = useState({
    userName: "",
    email: "",
    password: "",
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

  async function onRegister() {
    setErrorMessage("")
    setSuccessMessage("")

    if (!form.userName.trim()) {
      setErrorMessage("User name is required.")
      return
    }

    if (!form.email.trim()) {
      setErrorMessage("Email is required.")
      return
    }

    if (!form.password.trim()) {
      setErrorMessage("Password is required.")
      return
    }

    if (!form.confirmPassword.trim()) {
      setErrorMessage("Please confirm your password.")
      return
    }

    if (form.password !== form.confirmPassword) {
      setErrorMessage("Passwords do not match.")
      return
    }

    if (!turnstileToken) {
      setErrorMessage("Please complete the verification challenge.")
      return
    }

    setLoading(true)

    try {
      await accountApi.registerUser({
        userName: form.userName.trim(),
        email: form.email.trim(),
        password: form.password,
        turnstileToken,
      })

      setSuccessMessage("User registered successfully. Redirecting to sign in...")

      setTimeout(() => {
        navigate("/account/login", { replace: true })
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
      setTurnstileToken("")
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
              <HiOutlineUserPlus className="h-7 w-7" />
            </div>

            <div>
              <p className="text-lg font-semibold text-slate-800">FinTrack</p>
              <p className="mt-1 text-sm text-slate-500">Account registration</p>
            </div>
          </div>

          <h1 className="mt-3 text-3xl font-semibold tracking-tight text-slate-800">
            Create your user account
          </h1>

          <p className="mt-4 text-base leading-7 text-slate-600">
            Set up your personal account first. Organization membership can be added later through invitation or workspace setup.
          </p>
        </section>

        <section className="rounded-3xl border border-slate-200 border-t-4 border-t-indigo-200 bg-white p-8 shadow-sm sm:p-10">
          <div>
            <h2 className="text-2xl font-semibold text-slate-800">
              User account setup
            </h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              This creates a user account only. It does not create or join any organization yet.
            </p>
          </div>

          <div className="mt-8 space-y-5">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                User name
              </label>
              <input
                type="text"
                placeholder="e.g. Chen Li"
                value={form.userName}
                onChange={(e) => updateField("userName", e.target.value)}
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Email
              </label>
              <input
                type="email"
                placeholder="you@example.com"
                value={form.email}
                onChange={(e) => updateField("email", e.target.value)}
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Password
              </label>
              <input
                type="password"
                placeholder="Enter your password"
                value={form.password}
                onChange={(e) => updateField("password", e.target.value)}
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700">
                Confirm password
              </label>
              <input
                type="password"
                placeholder="Re-enter your password"
                value={form.confirmPassword}
                onChange={(e) => updateField("confirmPassword", e.target.value)}
                className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
              />
            </div>

            <div className="pt-2">
              <Turnstile
                siteKey={import.meta.env.VITE_TURNSTILE_SITE_KEY}
                onSuccess={(token) => {
                  setTurnstileToken(token)
                  setErrorMessage("")
                }}
                onExpire={() => {
                  setTurnstileToken("")
                }}
                onError={() => {
                  setTurnstileToken("")
                  setErrorMessage("Verification failed. Please try again.")
                }}
              />
            </div>

            <button
              type="button"
              onClick={onRegister}
              disabled={loading || !turnstileToken}
              className="inline-flex h-11 w-full items-center justify-center rounded-xl bg-indigo-600 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {loading ? "Creating..." : "Create account"}
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

          <div className="my-8 h-px bg-slate-200" />

          <div className="flex flex-wrap gap-3">
            <Link
              to="/account/login"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Back to sign in</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>

            <Link
              to="/account/register-tenant"
              className="group inline-flex items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
            >
              <span>Create organization instead</span>
              <HiOutlineArrowUpRight className="h-4 w-4 text-slate-400 transition group-hover:text-indigo-600" />
            </Link>
          </div>
        </section>
      </div>
    </div>
  )
}