<template>
  <div class="members-page">
    <div class="members-topbar">
      <div>
        <h2 class="members-title">Members</h2>
        <p class="members-subtitle">
          Manage users who belong to the current organization.
        </p>
      </div>

      <div class="members-actions">
        <el-button type="primary" @click="handleInviteLater">
          Invite member
        </el-button>
      </div>
    </div>

    <div class="members-summary-grid">
      <el-card class="summary-card" shadow="never">
        <div class="summary-label">Total members</div>
        <div class="summary-value">{{ members.length }}</div>
      </el-card>

      <el-card class="summary-card" shadow="never">
        <div class="summary-label">Admins</div>
        <div class="summary-value">{{ adminCount }}</div>
      </el-card>

      <el-card class="summary-card" shadow="never">
        <div class="summary-label">Active members</div>
        <div class="summary-value">{{ activeCount }}</div>
      </el-card>
    </div>

    <el-card class="members-card" shadow="never">
      <template #header>
        <div class="members-card-header">
          <div>
            <div class="members-card-title">Organization members</div>
            <div class="members-card-subtitle">
              Showing all users currently associated with this tenant.
            </div>
          </div>

          <div class="members-toolbar">
            <el-input
              v-model="keyword"
              placeholder="Search by email or name"
              clearable
              class="members-search"
            />
          </div>
        </div>
      </template>

      <el-skeleton :rows="6" animated v-if="loading" />

      <template v-else>
        <el-empty
          v-if="filteredMembers.length === 0"
          description="No members found."
        />

        <div v-else class="member-list">
          <div
            v-for="member in filteredMembers"
            :key="member.userPublicId"
            class="member-item"
          >
            <div class="member-main">
              <div class="member-avatar">
                {{ getInitials(member.userName || member.email) }}
              </div>

              <div class="member-info">
                <div class="member-name-row">
                  <div class="member-name">
                    {{ member.userName || "Unnamed user" }}
                  </div>

                  <el-tag
                    :type="member.role === 'Admin' ? 'danger' : 'primary'"
                    effect="light"
                    round
                  >
                    {{ member.role }}
                  </el-tag>

                  <el-tag
                    v-if="member.isActive"
                    type="success"
                    effect="light"
                    round
                  >
                    Active
                  </el-tag>
                  <el-tag v-else type="info" effect="light" round>
                    Inactive
                  </el-tag>
                </div>

                <div class="member-email">
                  {{ member.email }}
                </div>

                <div class="member-meta">
                  <span class="mono">{{ member.userPublicId }}</span>
                  <span class="dot">•</span>
                  <span>Joined {{ formatDate(member.joinedAt) }}</span>
                </div>
              </div>
            </div>

            <div class="member-side">
              <el-button text @click="handleViewLater(member)">
                Details
              </el-button>
            </div>
          </div>
        </div>
      </template>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { ElMessage } from "element-plus";
import { getTenantMembers, type TenantMemberDto } from "../api/tenant";

const loading = ref(false);
const keyword = ref("");
const members = ref<TenantMemberDto[]>([]);

const filteredMembers = computed(() => {
  const q = keyword.value.trim().toLowerCase();

  if (!q) return members.value;

  return members.value.filter((member) => {
    const email = member.email?.toLowerCase() ?? "";
    const userName = member.userName?.toLowerCase() ?? "";

    return email.includes(q) || userName.includes(q);
  });
});

const adminCount = computed(
  () => members.value.filter((x) => x.role === "Admin").length
);

const activeCount = computed(
  () => members.value.filter((x) => x.isActive).length
);

onMounted(async () => {
  await loadMembers();
});

async function loadMembers() {
  loading.value = true;

  try {
    members.value = await getTenantMembers();
  } catch (error: any) {
    console.error("Failed to load members:", error);
    ElMessage.error(
      error?.response?.data?.message ??
        error?.message ??
        "Failed to load tenant members."
    );
  } finally {
    loading.value = false;
  }
}

function getInitials(value: string) {
  const trimmed = value.trim();
  if (!trimmed) return "U";

  const parts = trimmed.split(/\s+/);
  if (parts.length === 1) {
    return parts[0].slice(0, 1).toUpperCase();
  }

  return `${parts[0][0] ?? ""}${parts[1][0] ?? ""}`.toUpperCase();
}

function formatDate(value: string) {
  if (!value) return "-";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return date.toLocaleDateString();
}

function handleInviteLater() {
  ElMessage.info("TODO: invite member flow.");
}

function handleViewLater(member: TenantMemberDto) {
  ElMessage.info(`TODO: view details for ${member.email}`);
}
</script>

<style scoped>
.members-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.members-topbar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.members-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: #111827;
}

.members-subtitle {
  margin: 6px 0 0;
  color: #6b7280;
  font-size: 14px;
}

.members-actions {
  flex-shrink: 0;
}

.members-summary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
}

.summary-card {
  border-radius: 18px;
}

.summary-label {
  font-size: 13px;
  color: #6b7280;
  margin-bottom: 10px;
}

.summary-value {
  font-size: 30px;
  font-weight: 700;
  color: #111827;
}

.members-card {
  border-radius: 20px;
}

.members-card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.members-card-title {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
}

.members-card-subtitle {
  margin-top: 6px;
  font-size: 13px;
  color: #6b7280;
}

.members-toolbar {
  flex-shrink: 0;
}

.members-search {
  width: 280px;
}

.member-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.member-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 18px;
  border: 1px solid #ebeef5;
  border-radius: 18px;
  background: linear-gradient(180deg, #ffffff 0%, #fafbfc 100%);
  transition: all 0.2s ease;
}

.member-item:hover {
  border-color: #dbe4f0;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.06);
}

.member-main {
  display: flex;
  align-items: center;
  gap: 16px;
  min-width: 0;
}

.member-avatar {
  width: 48px;
  height: 48px;
  border-radius: 999px;
  background: #e8f0ff;
  color: #1d4ed8;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  flex-shrink: 0;
}

.member-info {
  min-width: 0;
}

.member-name-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.member-name {
  font-size: 16px;
  font-weight: 700;
  color: #111827;
}

.member-email {
  margin-top: 6px;
  color: #374151;
  font-size: 14px;
  word-break: break-all;
}

.member-meta {
  margin-top: 8px;
  color: #6b7280;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

.dot {
  color: #9ca3af;
}

.member-side {
  flex-shrink: 0;
}

@media (max-width: 900px) {
  .members-topbar,
  .members-card-header,
  .member-item {
    flex-direction: column;
    align-items: flex-start;
  }

  .members-summary-grid {
    grid-template-columns: 1fr;
  }

  .members-search {
    width: 100%;
  }

  .members-toolbar {
    width: 100%;
  }

  .member-side {
    width: 100%;
  }
}
</style>