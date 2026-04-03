import { useMemo } from "react"
import { Link, useNavigate } from "react-router-dom"
import {
  HiOutlineUserCircle,
  HiOutlineBuildingOffice2,
  HiOutlineEnvelope,
  HiOutlineArrowRight,
  HiOutlineArrowLeftOnRectangle,
} from "react-icons/hi2"
import { useAuth } from "../../hooks/useAuth"

function StatCard({
  title,
  value,
  description,
}: {
  title: string
  value: string
  description: string
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-sm font-medium text-slate-500">{title}</p>
      <p className="mt-3 text-3xl font-semibold tracking-tight text-slate-800">
        {value}
      </p>
      <p className="mt-2 text-sm leading-6 text-slate-500">{description}</p>
    </div>
  )
}

export default function Home() {
  const navigate = useNavigate()
  const auth = useAuth()

  const memberships = auth.resolvedMemberships ?? []
  const adminMemberships = auth.adminMemberships ?? []

  const displayName = useMemo(() => {
    return auth.userName || auth.userEmail || "Account user"
  }, [auth.userEmail, auth.userName])

  function logout() {
    auth.logout()
    navigate("/account/login", { replace: true })
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto max-w-6xl">
        <section className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm sm:p-10">
          <div className="flex flex-col gap-6 sm:flex-row sm:items-start sm:justify-between">
            <div className="flex items-start gap-4">
              <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-100 text-indigo-600">
                <HiOutlineUserCircle className="h-7 w-7" />
              </div>

              <div>
                <p className="text-sm font-medium uppercase tracking-wide text-slate-400">
                  Account center
                </p>
                <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-800">
                  Welcome, {displayName}
                </h1>
                <p className="mt-3 max-w-2xl text-base leading-7 text-slate-600">
                  Manage your account, review organization memberships, and continue
                  into the parts of the system you have access to.
                </p>
              </div>
            </div>

            <button
              type="button"
              onClick={logout}
              className="inline-flex items-center gap-2 rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
            >
              <HiOutlineArrowLeftOnRectangle className="h-4 w-4" />
              Sign out
            </button>
          </div>
        </section>

        <section className="mt-8 grid gap-4 md:grid-cols-3">
          <StatCard
            title="Organizations"
            value={String(memberships.length)}
            description="Organizations your account currently belongs to."
          />
          <StatCard
            title="Admin roles"
            value={String(adminMemberships.length)}
            description="Memberships where you currently have admin access."
          />
          <StatCard
            title="Invitations"
            value="Via email"
            description="Invitation acceptance is supported through invitation links."
          />
        </section>

        <section className="mt-8 grid gap-6 lg:grid-cols-[1.2fr_0.8fr]">
          <div className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm">
            <div className="flex items-center gap-3">
              <HiOutlineBuildingOffice2 className="h-5 w-5 text-indigo-600" />
              <h2 className="text-xl font-semibold text-slate-800">
                Your organizations
              </h2>
            </div>

            {memberships.length === 0 ? (
              <div className="mt-6 rounded-2xl border border-amber-200 bg-amber-50 p-5">
                <p className="text-base font-semibold text-slate-800">
                  No organization access yet
                </p>
                <p className="mt-2 text-sm leading-6 text-slate-600">
                  Your account is ready, but you have not joined any organization yet.
                  Check your invitation email or ask an administrator to invite you.
                </p>
              </div>
            ) : (
              <div className="mt-6 overflow-hidden rounded-2xl border border-slate-200">
                <div className="grid grid-cols-[minmax(0,1fr)_120px] border-b border-slate-200 bg-slate-50 px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <div>Organization</div>
                  <div>Role</div>
                </div>

                {memberships.map((membership) => (
                  <div
                    key={membership.tenantPublicId}
                    className="grid grid-cols-[minmax(0,1fr)_120px] border-b border-slate-100 px-4 py-4 text-sm last:border-b-0"
                  >
                    <div className="min-w-0">
                      <div className="font-medium text-slate-800">
                        {membership.tenantName}
                      </div>
                      <div className="mt-1 break-all font-mono text-xs text-slate-400">
                        {membership.tenantPublicId}
                      </div>
                    </div>
                    <div className="text-slate-700">{membership.role}</div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="rounded-3xl border border-slate-200 bg-white p-8 shadow-sm">
            <div className="flex items-center gap-3">
              <HiOutlineUserCircle className="h-5 w-5 text-indigo-600" />
              <h2 className="text-xl font-semibold text-slate-800">
                Quick actions
              </h2>
            </div>

            <div className="mt-6 space-y-4">
              <Link
                to="/account/profile"
                className="group block rounded-2xl border border-slate-200 bg-slate-50 p-4 transition hover:border-indigo-300 hover:bg-indigo-50"
              >
                <div className="flex items-center justify-between gap-4">
                  <div>
                    <h3 className="text-sm font-semibold text-slate-800 group-hover:text-indigo-700">
                      View profile
                    </h3>
                    <p className="mt-1 text-sm leading-6 text-slate-500">
                      Review your account details and membership summary.
                    </p>
                  </div>
                  <HiOutlineArrowRight className="h-4 w-4 shrink-0 text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-600" />
                </div>
              </Link>


              <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                <div className="flex items-start gap-3">
                  <HiOutlineEnvelope className="mt-0.5 h-5 w-5 text-indigo-600" />
                  <div>
                    <h3 className="text-sm font-semibold text-slate-800">
                      Invitation access
                    </h3>
                    <p className="mt-1 text-sm leading-6 text-slate-500">
                      Invitation acceptance is supported through your invitation email link.
                      A dedicated invitation list can be added here later.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  )
}