import { useEffect, useMemo, useState } from "react"
import { Link, useLocation, useNavigate } from "react-router-dom"
import {
  acceptTenantInvitation,
  resolveTenantInvitation,
  type ResolveTenantInvitationResult,
} from "../api/invitation"
import "./AcceptInvitation.css"

export default function AcceptInvitation() {
  const location = useLocation()
  const navigate = useNavigate()

  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState("")
  const [successMessage, setSuccessMessage] = useState("")
  const [invitation, setInvitation] =
    useState<ResolveTenantInvitationResult | null>(null)

  const token = useMemo(() => {
    const params = new URLSearchParams(location.search)
    return params.get("token") ?? ""
  }, [location.search])

  const canAccept = !!invitation &&
    invitation.status === "Pending" &&
    !successMessage

  useEffect(() => {
    async function loadInvitation() {
      setLoading(true)
      setErrorMessage("")
      setSuccessMessage("")

      if (!token) {
        setErrorMessage("Invitation token is missing.")
        setLoading(false)
        return
      }

      try {
        const result = await resolveTenantInvitation(token)
        setInvitation(result)
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "Failed to load invitation."
        setErrorMessage(message)
      } finally {
        setLoading(false)
      }
    }

    void loadInvitation()
  }, [token])

  async function handleAccept() {
    if (!token) {
      setErrorMessage("Invitation token is missing.")
      return
    }

    setSubmitting(true)
    setErrorMessage("")
    setSuccessMessage("")

    try {
      await acceptTenantInvitation(token)

      setSuccessMessage(
        "Invitation accepted successfully. You can now sign in."
      )

      setInvitation((prev) =>
        prev
          ? {
              ...prev,
              status: "Accepted",
            }
          : prev
      )
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Failed to accept invitation."
      setErrorMessage(message)
    } finally {
      setSubmitting(false)
    }
  }

  function goLogin() {
    navigate("/portal/login")
  }

  function formatDate(value: string) {
    if (!value) return "-"

    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value

    return date.toLocaleString()
  }

  return (
    <div className="portal-page">
      <div className="portal-shell">
        <div className="portal-brand">
          <div className="portal-badge">FinTrack Portal</div>
          <h1 className="portal-title">Accept your invitation</h1>
          <p className="portal-description">
            Review the invitation details and confirm whether you want to join
            this organization.
          </p>
        </div>

        <div className="portal-card">
          {loading ? (
            <div className="skeleton-block">
              <div className="skeleton-row"></div>
              <div className="skeleton-row"></div>
              <div className="skeleton-row"></div>
              <div className="skeleton-row"></div>
              <div className="skeleton-row"></div>
              <div className="skeleton-row"></div>
            </div>
          ) : errorMessage ? (
            <div className="portal-alert error">{errorMessage}</div>
          ) : invitation ? (
            <>
              <div className="portal-card-header">
                <h2 className="portal-card-title">Invitation details</h2>
                <p className="portal-card-subtitle">
                  Please confirm the information before accepting.
                </p>
              </div>

              <div className="invitation-summary">
                <div className="summary-row">
                  <span className="summary-label">Organization</span>
                  <span className="summary-value">{invitation.tenantName}</span>
                </div>

                <div className="summary-row">
                  <span className="summary-label">Email</span>
                  <span className="summary-value">{invitation.email}</span>
                </div>

                <div className="summary-row">
                  <span className="summary-label">Role</span>
                  <span className="summary-value">
                    <span className="tag tag-info">{invitation.role}</span>
                  </span>
                </div>

                <div className="summary-row">
                  <span className="summary-label">Status</span>
                  <span className="summary-value">
                    <span
                      className={`tag ${
                        invitation.status === "Pending"
                          ? "tag-warning"
                          : "tag-success"
                      }`}
                    >
                      {invitation.status}
                    </span>
                  </span>
                </div>

                <div className="summary-row">
                  <span className="summary-label">Expires</span>
                  <span className="summary-value">
                    {formatDate(invitation.expiredAt)}
                  </span>
                </div>
              </div>

              {successMessage ? (
                <div className="portal-alert success">{successMessage}</div>
              ) : null}

              <div className="portal-actions">
                <button type="button" className="secondary-btn" onClick={goLogin}>
                  Back to login
                </button>

                <button
                  type="button"
                  className="primary-btn"
                  disabled={!canAccept || submitting}
                  onClick={handleAccept}
                >
                  {submitting ? "Accepting..." : "Accept invitation"}
                </button>
              </div>
            </>
          ) : (
            <div className="portal-alert error">
              Invitation detail is unavailable.
            </div>
          )}

          <div className="portal-footer-link">
            <Link to="/portal/login">Back to login</Link>
          </div>
        </div>
      </div>
    </div>
  )
}