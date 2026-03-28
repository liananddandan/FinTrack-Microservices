import { useEffect, useMemo, useState } from "react"
import {
  changeTenantMemberRole,
  getTenantMembers,
  removeTenantMember,
  type TenantMemberDto,
} from "../api/tenant"
import { createTenantInvitation } from "../api/invitation"
import {
  HiOutlineUsers,
  HiOutlineMagnifyingGlass,
  HiOutlineEnvelope,
  HiOutlineUserPlus,
} from "react-icons/hi2"

type InviteForm = {
  email: string
  role: string
}

type RoleForm = {
  role: string
}

function Badge({
  children,
  tone = "default",
}: {
  children: React.ReactNode
  tone?: "default" | "success" | "warning" | "danger"
}) {
  const className =
    tone === "success"
      ? "bg-emerald-50 text-emerald-700 border-emerald-200"
      : tone === "warning"
      ? "bg-amber-50 text-amber-700 border-amber-200"
      : tone === "danger"
      ? "bg-rose-50 text-rose-700 border-rose-200"
      : "bg-slate-100 text-slate-700 border-slate-200"

  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-medium ${className}`}
    >
      {children}
    </span>
  )
}

function SummaryCard({
  label,
  value,
}: {
  label: string
  value: string | number
}) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-5">
      <p className="text-sm text-slate-500">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-slate-800">{value}</p>
    </div>
  )
}

export default function Members() {
  const [loading, setLoading] = useState(false)
  const [keyword, setKeyword] = useState("")
  const [members, setMembers] = useState<TenantMemberDto[]>([])

  const [pageMessage, setPageMessage] = useState("")
  const [pageError, setPageError] = useState("")

  const [inviteDialogVisible, setInviteDialogVisible] = useState(false)
  const [inviteSubmitting, setInviteSubmitting] = useState(false)
  const [inviteErrorMessage, setInviteErrorMessage] = useState("")
  const [inviteSuccessMessage, setInviteSuccessMessage] = useState("")

  const [roleDialogVisible, setRoleDialogVisible] = useState(false)
  const [roleSubmitting, setRoleSubmitting] = useState(false)
  const [roleErrorMessage, setRoleErrorMessage] = useState("")
  const [selectedMember, setSelectedMember] = useState<TenantMemberDto | null>(
    null
  )

  const [inviteForm, setInviteForm] = useState<InviteForm>({
    email: "",
    role: "Member",
  })

  const [roleForm, setRoleForm] = useState<RoleForm>({
    role: "Member",
  })

  const filteredMembers = useMemo(() => {
    const q = keyword.trim().toLowerCase()

    if (!q) return members

    return members.filter((member) => {
      const email = member.email?.toLowerCase() ?? ""
      const userName = member.userName?.toLowerCase() ?? ""
      return email.includes(q) || userName.includes(q)
    })
  }, [keyword, members])

  const adminCount = useMemo(
    () => members.filter((x) => x.role === "Admin").length,
    [members]
  )

  const activeCount = useMemo(
    () => members.filter((x) => x.isActive).length,
    [members]
  )

  useEffect(() => {
    void loadMembers()
  }, [])

  async function loadMembers() {
    setLoading(true)
    setPageError("")
    setPageMessage("")

    try {
      const result = await getTenantMembers()
      setMembers(result)
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPageError(error.message || "Failed to load tenant members.")
      } else {
        setPageError("Failed to load tenant members.")
      }
    } finally {
      setLoading(false)
    }
  }

  function openInviteDialog() {
    setInviteDialogVisible(true)
    setInviteErrorMessage("")
    setInviteSuccessMessage("")
  }

  function closeInviteDialog() {
    setInviteDialogVisible(false)
    setInviteSubmitting(false)
    setInviteErrorMessage("")
    setInviteSuccessMessage("")
    setInviteForm({
      email: "",
      role: "Member",
    })
  }

  async function submitInvitation() {
    setInviteErrorMessage("")
    setInviteSuccessMessage("")

    if (!inviteForm.email.trim()) {
      setInviteErrorMessage("Email is required.")
      return
    }

    if (!inviteForm.role.trim()) {
      setInviteErrorMessage("Role is required.")
      return
    }

    setInviteSubmitting(true)

    try {
      await createTenantInvitation({
        email: inviteForm.email.trim(),
        role: inviteForm.role,
      })

      setInviteSuccessMessage(
        "Invitation created successfully. The email has been queued for delivery."
      )

      setPageMessage("Invitation created successfully.")
    } catch (error: unknown) {
      if (error instanceof Error) {
        setInviteErrorMessage(error.message || "Failed to create invitation.")
      } else {
        setInviteErrorMessage("Failed to create invitation.")
      }
    } finally {
      setInviteSubmitting(false)
    }
  }

  function getInitials(value: string) {
    const trimmed = value.trim()
    if (!trimmed) return "U"

    const parts = trimmed.split(/\s+/)

    if (parts.length === 1) {
      return parts[0]?.slice(0, 1).toUpperCase() || "U"
    }

    const first = parts[0]?.[0] ?? ""
    const second = parts[1]?.[0] ?? ""

    return `${first}${second}`.toUpperCase() || "U"
  }

  function formatDate(value: string) {
    if (!value) return "-"

    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return value

    return date.toLocaleDateString()
  }

  function handleViewLater(member: TenantMemberDto) {
    setPageMessage(`Details page is not implemented yet for ${member.email}.`)
  }

  async function handleRemove(member: TenantMemberDto) {
    const confirmed = window.confirm(
      `Remove ${member.email} from this organization?`
    )

    if (!confirmed) return

    setPageMessage("")
    setPageError("")

    try {
      await removeTenantMember(member.membershipPublicId)
      setPageMessage("Member removed successfully.")
      await loadMembers()
    } catch (error: unknown) {
      if (error instanceof Error) {
        setPageError(error.message || "Failed to remove member.")
      } else {
        setPageError("Failed to remove member.")
      }
    }
  }

  function openRoleDialog(member: TenantMemberDto) {
    setSelectedMember(member)
    setRoleForm({ role: member.role })
    setRoleErrorMessage("")
    setRoleDialogVisible(true)
  }

  function closeRoleDialog() {
    setRoleDialogVisible(false)
    setRoleSubmitting(false)
    setRoleErrorMessage("")
    setSelectedMember(null)
    setRoleForm({ role: "Member" })
  }

  async function submitRoleChange() {
    setRoleErrorMessage("")

    if (!selectedMember) {
      setRoleErrorMessage("No member selected.")
      return
    }

    if (!roleForm.role.trim()) {
      setRoleErrorMessage("Role is required.")
      return
    }

    setRoleSubmitting(true)

    try {
      await changeTenantMemberRole(
        selectedMember.membershipPublicId,
        roleForm.role
      )

      closeRoleDialog()
      setPageMessage("Member role updated successfully.")
      await loadMembers()
    } catch (error: unknown) {
      if (error instanceof Error) {
        setRoleErrorMessage(error.message || "Failed to change member role.")
      } else {
        setRoleErrorMessage("Failed to change member role.")
      }
    } finally {
      setRoleSubmitting(false)
    }
  }

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6">
      {/* Header */}
      <section className="rounded-3xl border border-slate-200 bg-white px-8 py-7 sm:px-10 sm:py-8">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
              <HiOutlineUsers className="h-6 w-6" />
            </div>

            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-slate-800">
                Members
              </h1>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
                Manage users who belong to the current organization workspace.
              </p>
            </div>
          </div>

          <button
            type="button"
            onClick={openInviteDialog}
            className="inline-flex items-center gap-2 rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
          >
            <HiOutlineUserPlus className="h-4 w-4" />
            Invite member
          </button>
        </div>
      </section>

      {pageMessage ? (
        <div
          className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700"
          role="status"
        >
          {pageMessage}
        </div>
      ) : null}

      {pageError ? (
        <div
          className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
          role="alert"
        >
          {pageError}
        </div>
      ) : null}

      {/* Summary */}
      <section className="grid gap-4 md:grid-cols-3">
        <SummaryCard label="Total members" value={members.length} />
        <SummaryCard label="Admins" value={adminCount} />
        <SummaryCard label="Active members" value={activeCount} />
      </section>

      {/* Member list */}
      <section className="rounded-3xl border border-slate-200 bg-white p-5 sm:p-6">
        <div className="mb-4 flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-base font-semibold text-slate-800">
              Organization members
            </h2>
            <p className="mt-1 text-sm text-slate-500">
              Showing all users currently associated with this tenant.
            </p>
          </div>

          <div className="relative w-full max-w-sm">
            <HiOutlineMagnifyingGlass className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="Search by email or name"
              className="h-11 w-full rounded-xl border border-slate-300 bg-white pl-10 pr-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
            />
          </div>
        </div>

        {loading ? (
          <div className="text-sm text-slate-500">Loading members...</div>
        ) : filteredMembers.length === 0 ? (
          <div className="rounded-2xl border border-slate-200 bg-slate-50 px-4 py-10 text-center text-sm text-slate-500">
            No members found.
          </div>
        ) : (
          <div className="space-y-4">
            {filteredMembers.map((member) => (
              <div
                key={member.membershipPublicId}
                className="flex flex-col gap-4 rounded-2xl border border-slate-200 bg-slate-50 p-5 xl:flex-row xl:items-center xl:justify-between"
              >
                <div className="flex min-w-0 items-start gap-4">
                  <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-sm font-semibold text-indigo-700">
                    {getInitials(member.userName || member.email)}
                  </div>

                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <div className="truncate text-sm font-semibold text-slate-800">
                        {member.userName || "Unnamed user"}
                      </div>

                      <Badge tone={member.role === "Admin" ? "danger" : "default"}>
                        {member.role}
                      </Badge>

                      <Badge tone={member.isActive ? "success" : "default"}>
                        {member.isActive ? "Active" : "Inactive"}
                      </Badge>
                    </div>

                    <div className="mt-2 text-sm text-slate-600">
                      {member.email}
                    </div>

                    <div className="mt-2 flex flex-wrap items-center gap-2 text-xs text-slate-500">
                      <span className="font-mono">{member.userPublicId}</span>
                      <span>•</span>
                      <span>Joined {formatDate(member.joinedAt)}</span>
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap items-center gap-3 xl:justify-end">
                  <button
                    type="button"
                    className="text-sm text-slate-700 transition hover:text-indigo-600"
                    onClick={() => handleViewLater(member)}
                  >
                    Details
                  </button>

                  <button
                    type="button"
                    className="text-sm font-medium text-indigo-600 transition hover:text-indigo-500"
                    onClick={() => openRoleDialog(member)}
                  >
                    Change role
                  </button>

                  <button
                    type="button"
                    disabled={member.role === "Admin" || !member.isActive}
                    className="text-sm font-medium text-rose-700 transition hover:text-rose-600 disabled:cursor-not-allowed disabled:opacity-40"
                    onClick={() => void handleRemove(member)}
                  >
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Invite dialog */}
      {inviteDialogVisible ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4">
          <div className="w-full max-w-lg rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
            <div className="flex items-start gap-3">
              <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
                <HiOutlineEnvelope className="h-5 w-5" />
              </div>

              <div>
                <h2 className="text-xl font-semibold text-slate-800">
                  Invite member
                </h2>
                <p className="mt-1 text-sm leading-6 text-slate-500">
                  Send an invitation to an already registered user.
                </p>
              </div>
            </div>

            <div className="mt-6 space-y-5">
              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Email
                </label>
                <input
                  value={inviteForm.email}
                  onChange={(e) =>
                    setInviteForm((prev) => ({
                      ...prev,
                      email: e.target.value,
                    }))
                  }
                  placeholder="user@example.com"
                  className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                />
              </div>

              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Role
                </label>
                <select
                  value={inviteForm.role}
                  onChange={(e) =>
                    setInviteForm((prev) => ({
                      ...prev,
                      role: e.target.value,
                    }))
                  }
                  className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                >
                  <option value="Member">Member</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>

              {inviteErrorMessage ? (
                <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                  {inviteErrorMessage}
                </div>
              ) : null}

              {inviteSuccessMessage ? (
                <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
                  {inviteSuccessMessage}
                </div>
              ) : null}
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={closeInviteDialog}
                className="inline-flex items-center rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
              >
                Cancel
              </button>

              <button
                type="button"
                disabled={inviteSubmitting}
                onClick={() => void submitInvitation()}
                className="inline-flex items-center rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {inviteSubmitting ? "Sending..." : "Send invitation"}
              </button>
            </div>
          </div>
        </div>
      ) : null}

      {/* Role dialog */}
      {roleDialogVisible ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 px-4">
          <div className="w-full max-w-lg rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
            <h2 className="text-xl font-semibold text-slate-800">
              Change member role
            </h2>
            <p className="mt-1 text-sm leading-6 text-slate-500">
              Update the role for the selected organization member.
            </p>

            <div className="mt-6 space-y-5">
              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Member
                </label>
                <input
                  value={selectedMember?.email || ""}
                  disabled
                  className="block h-11 w-full rounded-xl border border-slate-300 bg-slate-100 px-3 text-sm text-slate-600 outline-none"
                />
              </div>

              <div>
                <label className="mb-2 block text-sm font-medium text-slate-700">
                  Role
                </label>
                <select
                  value={roleForm.role}
                  onChange={(e) =>
                    setRoleForm({
                      role: e.target.value,
                    })
                  }
                  className="block h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-sm text-slate-800 outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                >
                  <option value="Member">Member</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>

              {roleErrorMessage ? (
                <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                  {roleErrorMessage}
                </div>
              ) : null}
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={closeRoleDialog}
                className="inline-flex items-center rounded-full border border-slate-300 bg-white px-4 py-2 text-sm text-slate-700 transition hover:border-indigo-500 hover:text-indigo-600"
              >
                Cancel
              </button>

              <button
                type="button"
                disabled={roleSubmitting}
                onClick={() => void submitRoleChange()}
                className="inline-flex items-center rounded-full bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {roleSubmitting ? "Saving..." : "Save"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}