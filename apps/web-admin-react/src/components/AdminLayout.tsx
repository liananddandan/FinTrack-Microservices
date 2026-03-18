import { NavLink, Outlet, useLocation, useNavigate } from "react-router-dom"
import { authStore } from "../lib/authStore"
import { useAuth } from "../hooks/useAuth"
import "./AdminLayout.css"

function getPageTitle(path: string): string {
  if (path.startsWith("/admin/members")) return "Members"
  if (path.startsWith("/admin/transactions")) return "Transactions"
  if (path.startsWith("/admin/invitations")) return "Invitations"
  if (path.startsWith("/admin/audit-logs")) return "Audit Logs"
  return "Dashboard"
}

export default function AdminLayout() {
  const location = useLocation()
  const navigate = useNavigate()
  const auth = useAuth()

  const pageTitle = getPageTitle(location.pathname)

  function logout() {
    authStore.logout()
    navigate("/login")
  }

  return (
    <div className="admin-shell">
      <aside className="admin-aside">
        <div className="admin-brand">
          <div className="admin-brand-title">FinTrack Admin</div>
          <div className="admin-brand-subtitle">
            {auth.currentTenantName || "No tenant"}
          </div>
        </div>

        <nav className="admin-menu">
          <NavLink
            to="/admin/overview"
            className={({ isActive }) =>
              `admin-menu-item ${isActive ? "active" : ""}`
            }
          >
            Overview
          </NavLink>

          <NavLink
            to="/admin/transactions"
            className={({ isActive }) =>
              `admin-menu-item ${isActive ? "active" : ""}`
            }
          >
            Transactions
          </NavLink>

          <NavLink
            to="/admin/members"
            className={({ isActive }) =>
              `admin-menu-item ${isActive ? "active" : ""}`
            }
          >
            Members
          </NavLink>

          <NavLink
            to="/admin/invitations"
            className={({ isActive }) =>
              `admin-menu-item ${isActive ? "active" : ""}`
            }
          >
            Invitations
          </NavLink>

          <NavLink
            to="/admin/audit-logs"
            className={({ isActive }) =>
              `admin-menu-item ${isActive ? "active" : ""}`
            }
          >
            Audit Logs
          </NavLink>
        </nav>
      </aside>

      <div className="admin-content">
        <header className="admin-header">
          <div className="admin-header-left">
            <div className="admin-page-title">{pageTitle}</div>
            <div className="admin-page-meta">
              Tenant: {auth.currentTenantName || "Unknown tenant"}
            </div>
          </div>

          <div className="admin-header-right">
            <div className="admin-user">
              <div className="admin-user-name">
                {auth.userName || auth.userEmail || "Unknown user"}
              </div>
              <div className="admin-user-email">
                {auth.userEmail || "No email"}
              </div>
            </div>

            <button className="admin-logout-btn" onClick={logout}>
              Logout
            </button>
          </div>
        </header>

        <main className="admin-main">
          <Outlet />
        </main>
      </div>
    </div>
  )
}