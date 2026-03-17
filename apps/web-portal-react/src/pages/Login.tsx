import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { getCurrentUser, login } from "../api/account"
import { authStore } from "../lib/authStore"
import "./Login.css"

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
        navigate("/waiting-membership")
        return
      }

      if (memberships.length === 1) {
        await authStore.activateSingleTenantIfPossible()

        if (authStore.hasTenantContext) {
          navigate("/home")
          return
        }

        navigate("/waiting-membership")
        return
      }

      navigate("/waiting-membership")
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
    <div className="portal-page">
      <div className="portal-shell">
        <div className="portal-brand">
          <div className="portal-badge">FinTrack Portal</div>
          <h1 className="portal-title">Manage finance operations with clarity.</h1>
          <p className="portal-description">
            A multi-tenant finance platform for organizations, administrators,
            and members.
          </p>
        </div>

        <div className="portal-card">
          <div className="portal-card-header">
            <h2 className="portal-card-title">Sign in</h2>
            <p className="portal-card-subtitle">
              Access your account and continue to your workspace.
            </p>
          </div>

          <form
            onSubmit={(e) => {
              e.preventDefault()
              void onLogin()
            }}
          >
            <div className="portal-form-item">
              <label htmlFor="email">Email</label>
              <input
                id="email"
                type="email"
                value={form.email}
                onChange={(e) => updateField("email", e.target.value)}
                placeholder="you@example.com"
              />
            </div>

            <div className="portal-form-item">
              <label htmlFor="password">Password</label>
              <input
                id="password"
                type="password"
                value={form.password}
                onChange={(e) => updateField("password", e.target.value)}
                placeholder="Enter your password"
              />
            </div>

            <button
              type="submit"
              className="portal-primary-btn"
              disabled={loading}
            >
              {loading ? "Signing in..." : "Sign in"}
            </button>

            {errorMessage ? (
              <div className="portal-alert" role="alert">
                {errorMessage}
              </div>
            ) : null}
          </form>

          <div className="portal-divider"></div>

          <div className="portal-links">
            <Link to="/register-tenant">Create organization</Link>
            <Link to="/register-user">Register as individual user</Link>
          </div>
        </div>
      </div>
    </div>
  )
}