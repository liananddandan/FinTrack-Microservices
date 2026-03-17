import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { getCurrentUser } from "../api/account"
import { authStore } from "../lib/authStore"
import { useAuth } from "../hooks/useAuth"
import "./WaitingMembership.css"

export default function WaitingMembership() {
  const navigate = useNavigate()
  const auth = useAuth()

  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState("")
  const [errorMessage, setErrorMessage] = useState("")

  async function redirectIfTenantReady() {
    if (authStore.hasTenantContext) {
      navigate("/home", { replace: true })
      return
    }

    const memberships = authStore.resolvedMemberships

    if (memberships.length === 1) {
      try {
        await authStore.activateSingleTenantIfPossible()

        if (authStore.hasTenantContext) {
          navigate("/home", { replace: true })
        }
      } catch {
        // stay on waiting page
      }
    }
  }

  useEffect(() => {
    void redirectIfTenantReady()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function refreshProfile() {
    setLoading(true)
    setMessage("")
    setErrorMessage("")

    try {
      const profile = await getCurrentUser()
      authStore.setProfile(profile)

      if ((profile.memberships?.length ?? 0) > 0) {
        await authStore.activateSingleTenantIfPossible()
      }

      if (authStore.hasTenantContext) {
        navigate("/home", { replace: true })
        return
      }

      setMessage("Status refreshed.")
    } catch (err) {
      const msg =
        err instanceof Error
          ? err.message
          : "Failed to refresh account status."
      setErrorMessage(msg)
    } finally {
      setLoading(false)
    }
  }

  function logout() {
    authStore.logout()
    navigate("/login")
  }

  return (
    <div className="portal-page">
      <div className="portal-shell">
        <div className="portal-brand">
          <div className="portal-badge">FinTrack Portal</div>
          <h1 className="portal-title">Waiting for organization access</h1>
          <p className="portal-description">
            Your account is ready, but you are not connected to any active
            organization yet.
          </p>
        </div>

        <div className="portal-card">
          <div className="portal-card-header">
            <h2 className="portal-card-title">Account status</h2>
            <p className="portal-card-subtitle">
              You can sign in, but tenant features become available only after
              your membership is active.
            </p>
          </div>

          <div className="summary-list">
            <div className="summary-row">
              <span className="summary-label">Email</span>
              <span className="summary-value">{auth.userEmail || "-"}</span>
            </div>

            <div className="summary-row">
              <span className="summary-label">User name</span>
              <span className="summary-value">{auth.userName || "-"}</span>
            </div>

            <div className="summary-row">
              <span className="summary-label">Membership count</span>
              <span className="summary-value">
                {auth.resolvedMemberships.length}
              </span>
            </div>
          </div>

          <div className="portal-alert" role="note">
            If an administrator has invited you, please open the invitation
            email and accept the invitation first.
          </div>

          {message ? (
            <div className="portal-message success" role="status">
              {message}
            </div>
          ) : null}

          {errorMessage ? (
            <div className="portal-message error" role="alert">
              {errorMessage}
            </div>
          ) : null}

          <div className="portal-actions">
            <button onClick={refreshProfile} disabled={loading}>
              {loading ? "Refreshing..." : "Refresh status"}
            </button>

            <button className="danger" onClick={logout}>
              Sign out
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}