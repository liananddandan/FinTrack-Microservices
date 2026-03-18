import { useEffect, useMemo, useState } from "react"
import {
  changeTenantMemberRole,
  getTenantMembers,
  removeTenantMember,
  type TenantMemberDto,
} from "../api/tenant"
import { createTenantInvitation } from "../api/invitation"
import "./Members.css"

type InviteForm = {
  email: string
  role: string
}

type RoleForm = {
  role: string
}

export default function Members() {
  const [loading, setLoading] = useState(false)
  const [keyword, setKeyword] = useState("")
  const [members, setMembers] = useState<TenantMemberDto[]>([])

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

    try {
      const result = await getTenantMembers()
      setMembers(result)
    } catch (error: unknown) {
      console.error("Failed to load members:", error)
      if (error instanceof Error) {
        window.alert(error.message || "Failed to load tenant members.")
      } else {
        window.alert("Failed to load tenant members.")
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

      window.alert("Invitation created successfully.")
    } catch (error: unknown) {
      console.error("Failed to create invitation:", error)
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
    console.log("clicked member:", member)
    window.alert(`TODO: view details for ${member.email}`)
  }

  async function handleRemove(member: TenantMemberDto) {
    const confirmed = window.confirm(
      `Remove ${member.email} from this organization?`
    )

    if (!confirmed) return

    try {
      await removeTenantMember(member.membershipPublicId)
      window.alert("Member removed successfully.")
      await loadMembers()
    } catch (error: unknown) {
      console.error("Failed to remove member:", error)

      if (error instanceof Error) {
        window.alert(error.message || "Failed to remove member.")
      } else {
        window.alert("Failed to remove member.")
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

      window.alert("Member role updated successfully.")
      closeRoleDialog()
      await loadMembers()
    } catch (error: unknown) {
      console.error("Failed to change member role:", error)

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
    <div className="members-page">
      <div className="members-topbar">
        <div>
          <h2 className="members-title">Members</h2>
          <p className="members-subtitle">
            Manage users who belong to the current organization.
          </p>
        </div>

        <div className="members-actions">
          <button className="primary-btn" onClick={openInviteDialog}>
            Invite member
          </button>
        </div>
      </div>

      <div className="members-summary-grid">
        <div className="summary-card">
          <div className="summary-label">Total members</div>
          <div className="summary-value">{members.length}</div>
        </div>

        <div className="summary-card">
          <div className="summary-label">Admins</div>
          <div className="summary-value">{adminCount}</div>
        </div>

        <div className="summary-card">
          <div className="summary-label">Active members</div>
          <div className="summary-value">{activeCount}</div>
        </div>
      </div>

      <div className="members-card">
        <div className="members-card-header">
          <div>
            <div className="members-card-title">Organization members</div>
            <div className="members-card-subtitle">
              Showing all users currently associated with this tenant.
            </div>
          </div>

          <div className="members-toolbar">
            <input
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="Search by email or name"
              className="members-search"
            />
          </div>
        </div>

        {loading ? (
          <div className="loading-block">Loading members...</div>
        ) : filteredMembers.length === 0 ? (
          <div className="empty-block">No members found.</div>
        ) : (
          <div className="member-list">
            {filteredMembers.map((member) => (
              <div key={member.membershipPublicId} className="member-item">
                <div className="member-main">
                  <div className="member-avatar">
                    {getInitials(member.userName || member.email)}
                  </div>

                  <div className="member-info">
                    <div className="member-name-row">
                      <div className="member-name">
                        {member.userName || "Unnamed user"}
                      </div>

                      <span
                        className={
                          member.role === "Admin"
                            ? "tag tag-danger"
                            : "tag tag-primary"
                        }
                      >
                        {member.role}
                      </span>

                      {member.isActive ? (
                        <span className="tag tag-success">Active</span>
                      ) : (
                        <span className="tag tag-info">Inactive</span>
                      )}
                    </div>

                    <div className="member-email">{member.email}</div>

                    <div className="member-meta">
                      <span className="mono">{member.userPublicId}</span>
                      <span className="dot">•</span>
                      <span>Joined {formatDate(member.joinedAt)}</span>
                    </div>
                  </div>
                </div>

                <div className="member-side">
                  <button
                    className="link-btn"
                    onClick={() => handleViewLater(member)}
                  >
                    Details
                  </button>

                  <button
                    className="link-btn primary-text"
                    onClick={() => openRoleDialog(member)}
                  >
                    Change role
                  </button>

                  <button
                    className="link-btn danger-text"
                    disabled={member.role === "Admin" || !member.isActive}
                    onClick={() => void handleRemove(member)}
                  >
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {inviteDialogVisible ? (
        <div className="dialog-backdrop">
          <div className="dialog-card">
            <div className="invite-dialog-header">
              <div className="invite-dialog-title">Invite member</div>
              <div className="invite-dialog-subtitle">
                Send an invitation to an already registered user.
              </div>
            </div>

            <div className="dialog-form">
              <div className="form-item">
                <label>Email</label>
                <input
                  value={inviteForm.email}
                  onChange={(e) =>
                    setInviteForm((prev) => ({
                      ...prev,
                      email: e.target.value,
                    }))
                  }
                  placeholder="user@example.com"
                />
              </div>

              <div className="form-item">
                <label>Role</label>
                <select
                  value={inviteForm.role}
                  onChange={(e) =>
                    setInviteForm((prev) => ({
                      ...prev,
                      role: e.target.value,
                    }))
                  }
                >
                  <option value="Member">Member</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>

              {inviteErrorMessage ? (
                <div className="alert error">{inviteErrorMessage}</div>
              ) : null}

              {inviteSuccessMessage ? (
                <div className="alert success">{inviteSuccessMessage}</div>
              ) : null}
            </div>

            <div className="invite-dialog-footer">
              <button className="secondary-btn" onClick={closeInviteDialog}>
                Cancel
              </button>
              <button
                className="primary-btn"
                disabled={inviteSubmitting}
                onClick={() => void submitInvitation()}
              >
                {inviteSubmitting ? "Sending..." : "Send invitation"}
              </button>
            </div>
          </div>
        </div>
      ) : null}

      {roleDialogVisible ? (
        <div className="dialog-backdrop">
          <div className="dialog-card">
            <div className="invite-dialog-header">
              <div className="invite-dialog-title">Change member role</div>
              <div className="invite-dialog-subtitle">
                Update the role for the selected organization member.
              </div>
            </div>

            <div className="dialog-form">
              <div className="form-item">
                <label>Member</label>
                <input value={selectedMember?.email || ""} disabled />
              </div>

              <div className="form-item">
                <label>Role</label>
                <select
                  value={roleForm.role}
                  onChange={(e) =>
                    setRoleForm({
                      role: e.target.value,
                    })
                  }
                >
                  <option value="Member">Member</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>

              {roleErrorMessage ? (
                <div className="alert error">{roleErrorMessage}</div>
              ) : null}
            </div>

            <div className="invite-dialog-footer">
              <button className="secondary-btn" onClick={closeRoleDialog}>
                Cancel
              </button>
              <button
                className="primary-btn"
                disabled={roleSubmitting}
                onClick={() => void submitRoleChange()}
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