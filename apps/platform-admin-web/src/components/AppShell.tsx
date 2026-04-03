import { NavLink, Outlet, useNavigate } from "react-router-dom"
import {
  HiOutlineBuildingOffice2,
  HiOutlineHome,
  HiOutlineArrowLeftOnRectangle,
  HiOutlineShieldCheck,
} from "react-icons/hi2"
import { usePlatformAuth } from "../lib/usePlatformAuth"

function SidebarLink({
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
      className={({ isActive }: { isActive: boolean }) =>
        [
          "flex items-center gap-3 rounded-2xl px-4 py-3 text-sm font-medium transition",
          isActive
            ? "bg-indigo-50 text-indigo-700"
            : "text-slate-600 hover:bg-slate-100 hover:text-slate-800",
        ].join(" ")
      }
    >
      {icon}
      <span>{label}</span>
    </NavLink>
  )
}

export default function AppShell() {
  const navigate = useNavigate()
  const auth = usePlatformAuth()

  function logout() {
    auth.logout()
    navigate("/login", { replace: true })
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <div className="grid min-h-screen lg:grid-cols-[260px_minmax(0,1fr)]">
        <aside className="border-r border-slate-200 bg-white px-5 py-6">
          <div className="flex items-center gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
              <HiOutlineShieldCheck className="h-6 w-6" />
            </div>

            <div>
              <div className="text-sm font-semibold text-slate-800">
                Multi-Tenant Retail Operations Platform
              </div>
              <div className="text-xs text-slate-500">Platform admin workspace</div>
            </div>
          </div>

          <div className="mt-8">
            <div className="mb-3 px-2 text-xs font-semibold uppercase tracking-wide text-slate-400">
              Navigation
            </div>

            <nav className="space-y-2">
              <SidebarLink
                to="/overview"
                label="Overview"
                icon={<HiOutlineHome className="h-5 w-5" />}
              />

              <SidebarLink
                to="/tenants"
                label="Tenants"
                icon={<HiOutlineBuildingOffice2 className="h-5 w-5" />}
              />
            </nav>
          </div>

          <div className="mt-10 rounded-2xl border border-slate-200 bg-slate-50 p-4">
            <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
              Signed in as
            </div>

            <div className="mt-2 text-sm font-medium text-slate-800">
              {auth.userName || "Platform Admin"}
            </div>

            <div className="mt-1 break-all text-xs text-slate-500">
              {auth.userEmail || "Unknown user"}
            </div>

            <div className="mt-3 inline-flex rounded-full bg-white px-3 py-1 text-xs font-medium text-slate-700">
              {auth.platformRole || "Unknown role"}
            </div>
          </div>

          <button
            type="button"
            onClick={logout}
            className="mt-6 inline-flex h-11 w-full items-center justify-center gap-2 rounded-2xl border border-slate-300 bg-white px-4 text-sm font-medium text-slate-700 transition hover:border-slate-400 hover:bg-slate-50"
          >
            <HiOutlineArrowLeftOnRectangle className="h-5 w-5" />
            Sign out
          </button>
        </aside>

        <main className="min-w-0 px-6 py-6 sm:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}