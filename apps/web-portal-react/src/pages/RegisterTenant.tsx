import { useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { registerTenant } from "../api/tenant"
import "./RegisterTenant.css"

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
        navigate("/portal/login")
      }, 1200)
    } catch (err) {
      const msg =
        err instanceof Error
          ? err.message
          : "Failed to register organization."
      setErrorMessage(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="portal-page">
      <div className="portal-shell">
        <div className="portal-brand">
          <div className="portal-badge">Organization Setup</div>
          <h1 className="portal-title">Create your organization workspace.</h1>
          <p className="portal-description">
            Register a tenant and create its first administrator account in one
            step.
          </p>
        </div>

        <div className="portal-card">
          <div className="portal-card-header">
            <h2 className="portal-card-title">Register organization</h2>
            <p className="portal-card-subtitle">
              This creates a tenant and its first administrator account.
            </p>
          </div>

          <div className="form">
            <div className="form-item">
              <label>Organization name</label>
              <input
                type="text"
                placeholder="e.g. Demo Church"
                value={form.tenantName}
                onChange={(e) => updateField("tenantName", e.target.value)}
              />
            </div>

            <div className="form-item">
              <label>Administrator name</label>
              <input
                type="text"
                placeholder="e.g. Emily"
                value={form.adminName}
                onChange={(e) => updateField("adminName", e.target.value)}
              />
            </div>

            <div className="form-item">
              <label>Administrator email</label>
              <input
                type="email"
                placeholder="admin@example.com"
                value={form.adminEmail}
                onChange={(e) => updateField("adminEmail", e.target.value)}
              />
            </div>

            <div className="form-item">
              <label>Password</label>
              <input
                type="password"
                placeholder="At least 8 characters"
                value={form.adminPassword}
                onChange={(e) => updateField("adminPassword", e.target.value)}
              />
            </div>

            <div className="form-item">
              <label>Confirm password</label>
              <input
                type="password"
                placeholder="Re-enter password"
                value={form.confirmPassword}
                onChange={(e) =>
                  updateField("confirmPassword", e.target.value)
                }
              />
            </div>

            <button
              className="portal-primary-btn"
              disabled={loading}
              onClick={onSubmit}
            >
              {loading ? "Creating..." : "Create organization"}
            </button>

            {successMessage ? (
              <div className="portal-alert success">{successMessage}</div>
            ) : null}

            {errorMessage ? (
              <div className="portal-alert error">{errorMessage}</div>
            ) : null}
          </div>

          <div className="portal-info-box">
            In V1, registering an organization creates the tenant and its first
            administrator directly.
          </div>

          <div className="portal-footer-link">
            <Link to="/portal/login">Back to login</Link>
          </div>
        </div>
      </div>
    </div>
  )
}