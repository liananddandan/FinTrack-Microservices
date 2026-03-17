import { useNavigate } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import "./Home.css"

export default function Home() {
  const navigate = useNavigate()
  const auth = useAuth()

  function goDonate() {
    navigate("/donate")
  }

  function goProcurement() {
    navigate("/procurements/new")
  }

  function goMyTransactions() {
    navigate("/my-transactions")
  }

  function logout() {
    auth.logout()
    navigate("/login")
  }

  return (
    <div className="portal-page">
      <div className="portal-shell">
        <section className="hero-card">
          <div className="hero-left">
            <div className="portal-badge">FinTrack Portal</div>
            <h1 className="portal-title">
              Welcome back, {auth.userName || "member"}
            </h1>
            <p className="portal-description">
              You are connected to your organization workspace. From here, you can
              make a donation, create a procurement request, and review your own
              transaction history.
            </p>

            <div className="hero-inline-meta">
              <span className="hero-meta-item">
                <span className="hero-meta-label">Tenant</span>
                <span className="hero-meta-value">
                  {auth.currentTenantName || "-"}
                </span>
              </span>

              <span className="hero-meta-divider"></span>

              <span className="hero-meta-item">
                <span className="hero-meta-label">Role</span>
                <span className="role-tag">
                  {auth.currentMembership?.role || "-"}
                </span>
              </span>
            </div>
          </div>

          <div className="hero-right">
            <div className="tenant-card">
              <div className="tenant-card-label">Current workspace</div>
              <div className="tenant-card-name">
                {auth.currentTenantName || "-"}
              </div>
              <div className="tenant-card-id mono">
                {auth.currentTenantPublicId || "-"}
              </div>
            </div>
          </div>
        </section>

        <section className="actions-section">
          <div className="section-heading">
            <h2 className="section-title">Quick Actions</h2>
            <p className="section-subtitle">
              Choose the next action you want to take in this workspace.
            </p>
          </div>

          <div className="action-grid">
            <button className="action-card primary" onClick={goDonate}>
              <div className="action-icon">❤</div>
              <div className="action-content">
                <div className="action-title">Make a donation</div>
                <div className="action-text">
                  Contribute funds directly to support your tenant.
                </div>
              </div>
            </button>

            <button className="action-card" onClick={goProcurement}>
              <div className="action-icon">🧾</div>
              <div className="action-content">
                <div className="action-title">New procurement</div>
                <div className="action-text">
                  Create a procurement draft and submit it for approval.
                </div>
              </div>
            </button>

            <button className="action-card" onClick={goMyTransactions}>
              <div className="action-icon">📘</div>
              <div className="action-content">
                <div className="action-title">My transactions</div>
                <div className="action-text">
                  Review your donations and procurement requests.
                </div>
              </div>
            </button>
          </div>
        </section>

        <section className="content-grid">
          <div className="info-card">
            <div className="card-header">
              <div>
                <div className="card-title">Workspace Summary</div>
                <div className="card-subtitle">
                  Your current account and tenant context.
                </div>
              </div>
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
                <span className="summary-label">Tenant</span>
                <span className="summary-value">
                  {auth.currentTenantName || "-"}
                </span>
              </div>

              <div className="summary-row">
                <span className="summary-label">Tenant ID</span>
                <span className="summary-value mono">
                  {auth.currentTenantPublicId || "-"}
                </span>
              </div>

              <div className="summary-row">
                <span className="summary-label">Role</span>
                <span className="summary-value">
                  <span className="role-tag">
                    {auth.currentMembership?.role || "-"}
                  </span>
                </span>
              </div>
            </div>
          </div>

          <div className="side-card">
            <div className="card-header">
              <div>
                <div className="card-title">Account</div>
                <div className="card-subtitle">
                  Session and workspace actions.
                </div>
              </div>
            </div>

            <div className="side-card-body">
              <div className="account-note">
                Your tenant context is active. Use the quick actions above to
                create or review transactions in this workspace.
              </div>

              <div className="account-actions">
                <button className="secondary-btn" onClick={goMyTransactions}>
                  My transactions
                </button>
                <button className="danger-btn" onClick={logout}>
                  Sign out
                </button>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}