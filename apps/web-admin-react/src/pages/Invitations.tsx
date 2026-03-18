import { useEffect, useMemo, useState } from "react"
import {
  getTenantInvitations,
  resendTenantInvitation,
  type TenantInvitationDto,
} from "../api/invitation"
import "./Invitations.css"

export default function Invitations() {
  const [loading, setLoading] = useState(false)
  const [keyword, setKeyword] = useState("")
  const [invitations, setInvitations] = useState<TenantInvitationDto[]>([])
  const [resendingInvitationId, setResendingInvitationId] = useState("")

  const filteredInvitations = useMemo(() => {
    const q = keyword.trim().toLowerCase()

    if (!q) return invitations

    return invitations.filter((item) => {
      const email = item.email?.toLowerCase() ?? ""
      const inviter = item.createdByUserEmail?.toLowerCase() ?? ""
      return email.includes(q) || inviter.includes(q)
    })
  }, [keyword, invitations])

  const pendingCount = useMemo(
    () => invitations.filter((x) => x.status === "Pending").length,
    [invitations]
  )

  const acceptedCount = useMemo(
    () => invitations.filter((x) => x.status === "Accepted").length,
    [invitations]
  )

  useEffect(() => {
    void loadInvitations()
  }, [])

  async function loadInvitations() {
    setLoading(true)

    try {
      const result = await getTenantInvitations()
      setInvitations(result)
    } catch (error: unknown) {
      console.error("Failed to load invitations:", error)

      if (error instanceof Error) {
        window.alert(error.message || "Failed to load invitations.")
      } else {
        window.alert("Failed to load invitations.")
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleResend(item: TenantInvitationDto) {
    setResendingInvitationId(item.invitationPublicId)

    try {
      await resendTenantInvitation(item.invitationPublicId)
      window.alert("Invitation email resent successfully.")
    } catch (error: unknown) {
      console.error("Failed to resend invitation:", error)

      if (error instanceof Error) {
        window.alert(error.message || "Failed to resend invitation.")
      } else {
        window.alert("Failed to resend invitation.")
      }
    } finally {
      setResendingInvitationId("")
    }
  }

  function getInitials(email: string) {
    const value = email.trim()
    if (!value) return "I"
    return value.slice(0, 1).toUpperCase()
  }

  function formatDate(value?: string | null) {
    if (!value) return "-"

    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value

    return date.toLocaleString()
  }

  function statusClass(status: string) {
    if (status === "Accepted") return "tag tag-success"
    if (status === "Pending") return "tag tag-warning"
    return "tag tag-info"
  }

  function handleViewLater(item: TenantInvitationDto) {
    window.alert(`TODO: view invitation ${item.invitationPublicId}`)
  }

  return (
    <div className="invitations-page">
      <div className="invitations-topbar">
        <div>
          <h2 className="invitations-title">Invitations</h2>
          <p className="invitations-subtitle">
            Review invitation history and monitor acceptance status.
          </p>
        </div>
      </div>

      <div className="invitations-summary-grid">
        <div className="summary-card">
          <div className="summary-label">Total invitations</div>
          <div className="summary-value">{invitations.length}</div>
        </div>

        <div className="summary-card">
          <div className="summary-label">Pending</div>
          <div className="summary-value">{pendingCount}</div>
        </div>

        <div className="summary-card">
          <div className="summary-label">Accepted</div>
          <div className="summary-value">{acceptedCount}</div>
        </div>
      </div>

      <div className="invitations-card">
        <div className="invitations-card-header">
          <div>
            <div className="invitations-card-title">Invitation records</div>
            <div className="invitations-card-subtitle">
              All invitations created for the current tenant.
            </div>
          </div>

          <div className="invitations-toolbar">
            <input
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="Search by email or inviter"
              className="invitations-search"
            />
          </div>
        </div>

        {loading ? (
          <div className="loading-block">Loading invitations...</div>
        ) : filteredInvitations.length === 0 ? (
          <div className="empty-block">No invitations found.</div>
        ) : (
          <div className="invitation-list">
            {filteredInvitations.map((item) => (
              <div key={item.invitationPublicId} className="invitation-item">
                <div className="invitation-main">
                  <div className="invitation-avatar">
                    {getInitials(item.email)}
                  </div>

                  <div className="invitation-info">
                    <div className="invitation-row invitation-row-top">
                      <div className="invitation-email">{item.email}</div>

                      <span className="tag tag-primary">{item.role}</span>

                      <span className={statusClass(item.status)}>
                        {item.status}
                      </span>
                    </div>

                    <div className="invitation-meta">
                      <span>Invited by {item.createdByUserEmail}</span>
                      <span className="dot">•</span>
                      <span>Created {formatDate(item.createdAt)}</span>
                      <span className="dot">•</span>
                      <span>Expires {formatDate(item.expiredAt)}</span>
                    </div>

                    {item.acceptedAt ? (
                      <div className="invitation-accepted">
                        Accepted at {formatDate(item.acceptedAt)}
                      </div>
                    ) : null}

                    <div className="invitation-id mono">
                      {item.invitationPublicId}
                    </div>
                  </div>
                </div>

                <div className="invitation-side">
                  {item.status === "Pending" ? (
                    <button
                      className="link-btn primary-text"
                      disabled={resendingInvitationId === item.invitationPublicId}
                      onClick={() => void handleResend(item)}
                    >
                      {resendingInvitationId === item.invitationPublicId
                        ? "Resending..."
                        : "Resend"}
                    </button>
                  ) : null}

                  <button
                    className="link-btn"
                    onClick={() => handleViewLater(item)}
                  >
                    Details
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}