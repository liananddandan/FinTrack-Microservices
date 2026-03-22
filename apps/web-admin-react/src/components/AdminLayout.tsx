import { NavLink, Outlet, useNavigate } from "react-router-dom"
import {
  HiOutlineHome,
  HiOutlineClipboardDocumentList,
  HiOutlineUsers,
  HiOutlineEnvelope,
  HiOutlineDocumentMagnifyingGlass,
  HiOutlineArrowLeftOnRectangle,
  HiOutlineArrowsRightLeft,
} from "react-icons/hi2"
import { authStore } from "../lib/authStore"
import { useAuth } from "../hooks/useAuth"

function AdminNavItem({
  to,
  label,
  icon,
}: {
  to: string
  label: string
  icon: React.ReactNode
}) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        [
          "group flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition",
          isActive
            ? "bg-indigo-50 text-indigo-700"
            : "text-slate-600 hover:bg-slate-100 hover:text-slate-900",
        ].join(" ")
      }
    >
      <span className="flex h-5 w-5 items-center justify-center">{icon}</span>
      <span>{label}</span>
    </NavLink>
  )
}

export default function AdminLayout() {
  const navigate = useNavigate()
  const auth = useAuth()

  function logout() {
    authStore.logout()
    navigate("/login", { replace: true })
  }

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <div className="grid min-h-screen lg:grid-cols-[280px_minmax(0,1fr)]">
        <aside className="flex min-h-screen flex-col border-r border-slate-200 bg-white px-5 py-6">
          <div className="flex items-start gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineArrowsRightLeft className="h-6 w-6" />
            </div>

            <div className="min-w-0">
              <div className="text-xs leading-5 text-slate-500">
                Transaction & Workflow Platform
              </div>
              <div className="mt-1 text-[11px] font-medium text-indigo-600">
                Admin
              </div>
            </div>
          </div>

          <div className="mt-6 rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
            <div className="text-xs uppercase tracking-wide text-slate-500">
              Workspace
            </div>

            <div className="mt-2 truncate text-sm font-medium text-slate-800">
              {auth.currentTenantName || "No tenant"}
            </div>

            <div className="mt-2 inline-flex items-center rounded-full bg-indigo-50 px-2.5 py-1 text-xs font-medium text-indigo-700">
              {auth.currentMembership?.role || "Unknown role"}
            </div>
          </div>

           <div className="pt-6">
            <div className="rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3">
              <div className="truncate text-sm font-medium text-slate-900">
                {auth.userName || auth.userEmail || "Unknown user"}
              </div>
              <div className="mt-1 truncate text-xs text-slate-500">
                {auth.userEmail || "No email"}
              </div>

              <button
                type="button"
                onClick={logout}
                className="mt-4 inline-flex w-full items-center justify-center gap-2 rounded-xl border border-rose-200 bg-rose-50 px-4 py-2 text-sm font-medium text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
              >
                <HiOutlineArrowLeftOnRectangle className="h-4 w-4" />
                Logout
              </button>
            </div>
          </div>

          <nav className="mt-8 space-y-2">
            <AdminNavItem
              to="/overview"
              label="Overview"
              icon={<HiOutlineHome className="h-5 w-5" />}
            />

            <AdminNavItem
              to="/transactions"
              label="Transactions"
              icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
            />

            <AdminNavItem
              to="/members"
              label="Members"
              icon={<HiOutlineUsers className="h-5 w-5" />}
            />

            <AdminNavItem
              to="/invitations"
              label="Invitations"
              icon={<HiOutlineEnvelope className="h-5 w-5" />}
            />

            <AdminNavItem
              to="/audit-logs"
              label="Audit Logs"
              icon={<HiOutlineDocumentMagnifyingGlass className="h-5 w-5" />}
            />
          </nav>

        </aside>

        <main className="min-w-0 px-6 py-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}