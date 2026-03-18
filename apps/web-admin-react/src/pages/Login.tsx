import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { getCurrentUser, login } from "../api/account"
import { seedDemoData, type DevSeedResult } from "../api/dev"
import { authStore } from "../lib/authStore"
import "./Login.css"

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

      navigate("/admin/overview")
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
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <h2>Admin Sign In</h2>
          <p>Sign in with an administrator account.</p>
        </div>

        <form onSubmit={onLogin} className="login-form">
          <div className="form-item">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={form.email}
              onChange={(e) => updateField("email", e.target.value)}
            />
          </div>

          <div className="form-item">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={form.password}
              onChange={(e) => updateField("password", e.target.value)}
            />
          </div>

          <button
            type="submit"
            className="primary-btn full-width"
            disabled={loading}
          >
            {loading ? "Signing in..." : "Sign in"}
          </button>

          {errorMessage ? (
            <div className="alert error" role="alert">
              {errorMessage}
            </div>
          ) : null}
        </form>

        <div className="divider" />

        <button
          type="button"
          className="warning-btn full-width"
          disabled={seedLoading}
          onClick={onSeedDemoData}
        >
          {seedLoading ? "Seeding..." : "Seed Demo Data"}
        </button>

        {seedMessage ? (
          <div className="alert success" role="status">
            {seedMessage}
          </div>
        ) : null}

        {seedErrorMessage ? (
          <div className="alert error" role="alert">
            {seedErrorMessage}
          </div>
        ) : null}

        {demoSeedResult ? (
          <div className="demo-credentials">
            <h3>Demo Accounts</h3>
            <p>Tenant: {demoSeedResult.tenantName}</p>
            <p>
              Admin: {demoSeedResult.adminEmail} / {demoSeedResult.adminPassword}
            </p>
            <p>
              Member: {demoSeedResult.memberEmail} / {demoSeedResult.memberPassword}
            </p>
          </div>
        ) : null}
      </div>
    </div>
  )
}