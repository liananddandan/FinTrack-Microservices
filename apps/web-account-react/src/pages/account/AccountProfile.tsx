import { useNavigate } from "react-router-dom"
import { HiOutlineUserCircle, HiOutlineArrowLeft } from "react-icons/hi2"
import { useAuth } from "../../hooks/useAuth"

function InfoRow({
  label,
  value,
  mono = false,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="flex items-start justify-between gap-4 border-b border-slate-100 py-3 last:border-b-0">
      <span className="text-sm text-slate-500">{label}</span>
      <span
        className={`text-right text-sm font-medium text-slate-800 ${
          mono ? "font-mono break-all" : ""
        }`}
      >
        {value}
      </span>
    </div>
  )
}

export default function AccountProfile() {
  const navigate = useNavigate()
  const auth = useAuth()

  function goBack() {
    navigate("/account/home")
  }

  function logout() {
    auth.logout()
    navigate("/account/login", { replace: true })
  }

  const memberships = auth.resolvedMemberships ?? []

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto flex max-w-5xl flex-col gap-8">
        <div className="flex items-center justify-between">
          <button
            type="button"
            onClick={goBack}
            className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
          >
            <HiOutlineArrowLeft className="h-4 w-4" />
            Back to account home
          </button>
        </div>

        <section className="rounded-3xl border border-slate-200 bg-white p-8 sm:p-10">
          <div className="inline-flex items-center gap-3 rounded-full border border-slate-200 bg-slate-50 px-4 py-2">
            <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
              <HiOutlineUserCircle className="h-5 w-5" />
            </div>
            <div>
              <p className="text-sm font-medium text-slate-800">FinTrack</p>
              <p className="text-xs text-slate-500">Account profile</p>
            </div>
          </div>

          <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800">
            Account details
          </h1>

          <p className="mt-4 max-w-2xl text-base leading-7 text-slate-600">
            Review your account information and organization memberships.
            Editing can be added later if needed.
          </p>

          <div className="mt-8 grid gap-6 lg:grid-cols-[1fr_1fr]">
            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-6">
              <h2 className="text-lg font-semibold text-slate-800">User</h2>

              <div className="mt-4">
                <InfoRow label="Email" value={auth.userEmail || "-"} />
                <InfoRow label="User name" value={auth.userName || "-"} />
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-slate-50 p-6">
              <h2 className="text-lg font-semibold text-slate-800">
                Membership summary
              </h2>

              <div className="mt-4">
                <InfoRow
                  label="Organizations"
                  value={String(memberships.length)}
                />
                <InfoRow
                  label="Admin memberships"
                  value={String(auth.adminMemberships?.length ?? 0)}
                />
              </div>
            </div>
          </div>

          <div className="mt-8 rounded-2xl border border-slate-200 bg-slate-50 p-6">
            <h2 className="text-lg font-semibold text-slate-800">
              Organizations
            </h2>

            {memberships.length === 0 ? (
              <p className="mt-4 text-sm text-slate-500">
                You do not belong to any organizations yet.
              </p>
            ) : (
              <div className="mt-4 overflow-hidden rounded-2xl border border-slate-200 bg-white">
                <div className="grid grid-cols-[minmax(0,1fr)_120px] border-b border-slate-200 bg-slate-50 px-4 py-3 text-xs font-semibold uppercase tracking-wide text-slate-500">
                  <div>Organization</div>
                  <div>Role</div>
                </div>

                {memberships.map((membership) => (
                  <div
                    key={membership.tenantPublicId}
                    className="grid grid-cols-[minmax(0,1fr)_120px] border-b border-slate-100 px-4 py-3 text-sm last:border-b-0"
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

          <div className="mt-8 flex flex-wrap gap-3">
            <button
              type="button"
              onClick={logout}
              className="inline-flex items-center rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
            >
              Sign out
            </button>
          </div>
        </section>
      </div>
    </div>
  )
}