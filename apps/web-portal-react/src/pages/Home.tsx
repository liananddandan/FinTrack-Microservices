import { useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { useAuth } from "../hooks/useAuth"
import {
  HiOutlineArrowsRightLeft,
  HiOutlineArrowRight,
  HiOutlineBanknotes,
  HiOutlineDocumentText,
  HiOutlineClipboardDocumentList,
  HiOutlineUserCircle,
  HiOutlineBell,
  HiOutlineChartBar,
  HiOutlineClock,
  HiOutlineArrowLeftOnRectangle,
} from "react-icons/hi2"

function ActionCard({
  title,
  description,
  icon,
  onClick,
}: {
  title: string
  description: string
  icon: React.ReactNode
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="group rounded-2xl border border-slate-200 bg-white p-5 text-left transition hover:border-indigo-400 hover:bg-slate-50"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600 shadow-sm">
          {icon}
        </div>

        <HiOutlineArrowRight className="mt-1 h-5 w-5 text-slate-400 transition group-hover:translate-x-1 group-hover:text-indigo-600" />
      </div>

      <div className="mt-4">
        <p className="text-sm font-semibold text-slate-800">{title}</p>
        <p className="mt-2 text-sm leading-6 text-slate-600">{description}</p>
      </div>
    </button>
  )
}

function OverviewCard({
  label,
  value,
  description,
  icon,
}: {
  label: string
  value: string
  description: string
  icon: React.ReactNode
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5">
      <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
        {icon}
      </div>
      <p className="mt-4 text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-slate-800">{value}</p>
      <p className="mt-2 text-sm leading-6 text-slate-500">{description}</p>
    </div>
  )
}

function PlannedItem({
  icon,
  title,
  description,
}: {
  icon: React.ReactNode
  title: string
  description: string
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5">
      <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
        {icon}
      </div>
      <p className="mt-4 text-sm font-semibold text-slate-800">{title}</p>
      <p className="mt-2 text-sm leading-6 text-slate-600">{description}</p>
    </div>
  )
}

export default function Home() {
  const navigate = useNavigate()
  const auth = useAuth()
  const [initializing, setInitializing] = useState(true)

  useEffect(() => {
    async function init() {
      try {
        await auth.initialize()
      } finally {
        setInitializing(false)
      }
    }

    void init()
  }, [])

  function goRecordIncome() {
    navigate("/portal/record-income")
  }

  function goProcurement() {
    navigate("/portal/procurements/new")
  }

  function goMyTransactions() {
    navigate("/portal/my-transactions")
  }

  function goProfile() {
    navigate("/portal/profile")
  }

  function logout() {
    auth.logout()
    navigate("/portal/login", { replace: true })
  }

  if (initializing) {
    return (
      <div className="min-h-screen bg-slate-50 px-6 py-10">
        <div className="mx-auto max-w-6xl text-sm text-slate-500">
          Loading workspace...
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-slate-50 px-6 py-10">
      <div className="mx-auto flex max-w-6xl flex-col gap-8">
        <section className="rounded-3xl border border-slate-200 bg-white p-8 sm:p-10">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
            <div className="max-w-2xl">
              <div className="inline-flex items-center gap-3 rounded-full border border-slate-200 bg-slate-50 px-4 py-2">
                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
                  <HiOutlineArrowsRightLeft className="h-5 w-5" />
                </div>
                <div>
                  <p className="text-sm font-medium text-slate-800">
                    Transaction & Workflow Platform
                  </p>
                  <p className="text-xs text-slate-500">Portal workspace</p>
                </div>
              </div>

              <h1 className="mt-8 text-3xl font-semibold tracking-tight text-slate-800 sm:text-4xl">
                {auth.currentTenantName || "Workspace"}
              </h1>

              <p className="mt-4 max-w-2xl text-base leading-7 text-slate-600">
                Work inside your current tenant context. Record income, submit procurement requests,
                and review the transaction records you created in this workspace.
              </p>

              <div className="mt-6 flex flex-wrap items-center gap-3">
                <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                  Role: {auth.currentMembership?.role || "Unknown"}
                </span>

                <span className="inline-flex items-center rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-700">
                  User: {auth.userName || auth.userEmail || "Unknown user"}
                </span>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-3">
              <button
                type="button"
                onClick={goProfile}
                className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
              >
                <HiOutlineUserCircle className="h-5 w-5" />
                Profile
              </button>

              <button
                type="button"
                onClick={logout}
                className="inline-flex items-center gap-2 rounded-full border border-rose-200 bg-rose-50 px-4 py-2 text-sm text-rose-700 transition hover:border-rose-300 hover:bg-rose-100"
              >
                <HiOutlineArrowLeftOnRectangle className="h-5 w-5" />
                Sign out
              </button>
            </div>
          </div>
        </section>

        <section>
          <div className="mb-4">
            <h2 className="text-2xl font-semibold text-slate-800">Quick actions</h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Start a new record or continue working in this tenant.
            </p>
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            <ActionCard
              title="Record income"
              description="Add an income entry to the current tenant ledger."
              icon={<HiOutlineBanknotes className="h-5 w-5" />}
              onClick={goRecordIncome}
            />

            <ActionCard
              title="New procurement request"
              description="Create a request that can move through an approval workflow."
              icon={<HiOutlineDocumentText className="h-5 w-5" />}
              onClick={goProcurement}
            />

            <ActionCard
              title="My transactions"
              description="Review the records you created or submitted in this workspace."
              icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
              onClick={goMyTransactions}
            />
          </div>
        </section>

        <section>
          <div className="mb-4">
            <h2 className="text-2xl font-semibold text-slate-800">Your workspace</h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              A quick view of your current access and activity.
            </p>
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            <OverviewCard
              label="My transactions"
              value="—"
              description="Recent income entries and procurement requests you created can appear here."
              icon={<HiOutlineClipboardDocumentList className="h-5 w-5" />}
            />

            <OverviewCard
              label="My requests"
              value="—"
              description="Track requests you submitted and their current workflow status."
              icon={<HiOutlineClock className="h-5 w-5" />}
            />

            <OverviewCard
              label="Workspace access"
              value={auth.currentMembership?.role || "—"}
              description="Your active role inside the current tenant workspace."
              icon={<HiOutlineUserCircle className="h-5 w-5" />}
            />
          </div>
        </section>

        <section>
          <div className="mb-4">
            <h2 className="text-2xl font-semibold text-slate-800">Planned capabilities</h2>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Additional features for future iterations of the user portal.
            </p>
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            <PlannedItem
              icon={<HiOutlineChartBar className="h-5 w-5" />}
              title="Reports"
              description="User-facing summaries and exportable views across your submitted records."
            />

            <PlannedItem
              icon={<HiOutlineBell className="h-5 w-5" />}
              title="Notifications"
              description="Updates for request status changes, approvals, and workspace events."
            />

            <PlannedItem
              icon={<HiOutlineClock className="h-5 w-5" />}
              title="Request timeline"
              description="A future view for tracking workflow stages and historical updates."
            />
          </div>
        </section>
      </div>
    </div>
  )
}