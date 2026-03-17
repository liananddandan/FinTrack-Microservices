import { useState } from "react"
import { useNavigate, Link } from "react-router-dom"
import { registerUser } from "../api/account"
import "./RegisterUser.css"

export default function RegisterUser() {
  const navigate = useNavigate()

  const [loading, setLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")

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

    setLoading(true)

    try {
      await registerUser({
        userName: form.userName.trim(),
        email: form.email.trim(),
        password: form.password,
      })

      setSuccessMessage(
        "User registered successfully. Redirecting to login..."
      )

      setTimeout(() => {
        navigate("/login")
      }, 1200)
    } catch (err) {
      const msg =
        err instanceof Error
          ? err.message
          : "User registration failed."
      setErrorMessage(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="portal-page">
      <div className="portal-shell">
        <div className="portal-brand">
          <div className="portal-badge">FinTrack Portal</div>
          <h1 className="portal-title">
            Create your personal account.
          </h1>
          <p className="portal-description">
            Register as an individual user first. Tenant membership can be added later by invitation.
          </p>
        </div>

        <div className="portal-card">
          <div className="portal-card-header">
            <h2 className="portal-card-title">Register user</h2>
            <p className="portal-card-subtitle">
              This creates a user account only. It does not create or join any organization yet.
            </p>
          </div>

          <div className="form">
            <div className="form-item">
              <label>User name</label>
              <input
                type="text"
                placeholder="e.g. Chen Li"
                value={form.userName}
                onChange={(e) =>
                  updateField("userName", e.target.value)
                }
              />
            </div>

            <div className="form-item">
              <label>Email</label>
              <input
                type="email"
                placeholder="you@example.com"
                value={form.email}
                onChange={(e) =>
                  updateField("email", e.target.value)
                }
              />
            </div>

            <div className="form-item">
              <label>Password</label>
              <input
                type="password"
                placeholder="Enter your password"
                value={form.password}
                onChange={(e) =>
                  updateField("password", e.target.value)
                }
              />
            </div>

            <div className="form-item">
              <label>Confirm password</label>
              <input
                type="password"
                placeholder="Re-enter your password"
                value={form.confirmPassword}
                onChange={(e) =>
                  updateField("confirmPassword", e.target.value)
                }
              />
            </div>

            <button
              className="portal-primary-btn"
              disabled={loading}
              onClick={onRegister}
            >
              {loading ? "Creating..." : "Create account"}
            </button>

            {successMessage && (
              <div className="portal-alert success">
                {successMessage}
              </div>
            )}

            {errorMessage && (
              <div className="portal-alert error">
                {errorMessage}
              </div>
            )}
          </div>

          <div className="portal-divider"></div>

          <div className="portal-links">
            <Link to="/login">Back to login</Link>
            <Link to="/register-tenant">
              Create organization instead
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}