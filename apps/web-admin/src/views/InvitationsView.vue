<template>
  <div class="invitations-page">
    <div class="invitations-topbar">
      <div>
        <h2 class="invitations-title">Invitations</h2>
        <p class="invitations-subtitle">
          Review invitation history and monitor acceptance status.
        </p>
      </div>
    </div>

    <div class="invitations-summary-grid">
      <el-card class="summary-card" shadow="never">
        <div class="summary-label">Total invitations</div>
        <div class="summary-value">{{ invitations.length }}</div>
      </el-card>

      <el-card class="summary-card" shadow="never">
        <div class="summary-label">Pending</div>
        <div class="summary-value">{{ pendingCount }}</div>
      </el-card>

      <el-card class="summary-card" shadow="never">
        <div class="summary-label">Accepted</div>
        <div class="summary-value">{{ acceptedCount }}</div>
      </el-card>
    </div>

    <el-card class="invitations-card" shadow="never">
      <template #header>
        <div class="invitations-card-header">
          <div>
            <div class="invitations-card-title">Invitation records</div>
            <div class="invitations-card-subtitle">
              All invitations created for the current tenant.
            </div>
          </div>

          <div class="invitations-toolbar">
            <el-input
              v-model="keyword"
              placeholder="Search by email or inviter"
              clearable
              class="invitations-search"
            />
          </div>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <template v-else>
        <el-empty
          v-if="filteredInvitations.length === 0"
          description="No invitations found."
        />

        <div v-else class="invitation-list">
          <div
            v-for="item in filteredInvitations"
            :key="item.invitationPublicId"
            class="invitation-item"
          >
            <div class="invitation-main">
              <div class="invitation-avatar">
                {{ getInitials(item.email) }}
              </div>

              <div class="invitation-info">
                <div class="invitation-row invitation-row-top">
                  <div class="invitation-email">{{ item.email }}</div>

                  <el-tag type="primary" effect="light" round>
                    {{ item.role }}
                  </el-tag>

                  <el-tag
                    :type="statusTagType(item.status)"
                    effect="light"
                    round
                  >
                    {{ item.status }}
                  </el-tag>
                </div>

                <div class="invitation-meta">
                  <span>Invited by {{ item.createdByUserEmail }}</span>
                  <span class="dot">•</span>
                  <span>Created {{ formatDate(item.createdAt) }}</span>
                  <span class="dot">•</span>
                  <span>Expires {{ formatDate(item.expiredAt) }}</span>
                </div>

                <div v-if="item.acceptedAt" class="invitation-accepted">
                  Accepted at {{ formatDate(item.acceptedAt) }}
                </div>

                <div class="invitation-id mono">
                  {{ item.invitationPublicId }}
                </div>
              </div>
            </div>

            <div class="invitation-side">
              <el-button text @click="handleViewLater(item)">
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
import {
  getTenantInvitations,
  type TenantInvitationDto,
} from "../api/invitation";

const loading = ref(false);
const keyword = ref("");
const invitations = ref<TenantInvitationDto[]>([]);

const filteredInvitations = computed(() => {
  const q = keyword.value.trim().toLowerCase();

  if (!q) return invitations.value;

  return invitations.value.filter((item) => {
    const email = item.email?.toLowerCase() ?? "";
    const inviter = item.createdByUserEmail?.toLowerCase() ?? "";
    return email.includes(q) || inviter.includes(q);
  });
});

const pendingCount = computed(
  () => invitations.value.filter((x) => x.status === "Pending").length
);

const acceptedCount = computed(
  () => invitations.value.filter((x) => x.status === "Accepted").length
);

onMounted(async () => {
  await loadInvitations();
});

async function loadInvitations() {
  loading.value = true;

  try {
    invitations.value = await getTenantInvitations();
  } catch (error: any) {
    console.error("Failed to load invitations:", error);
    ElMessage.error(
      error?.response?.data?.message ??
        error?.message ??
        "Failed to load invitations."
    );
  } finally {
    loading.value = false;
  }
}

function getInitials(email: string) {
  const value = email.trim();
  if (!value) return "I";
  return value.slice(0, 1).toUpperCase();
}

function formatDate(value?: string | null) {
  if (!value) return "-";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return date.toLocaleString();
}

function statusTagType(status: string) {
  if (status === "Accepted") return "success";
  if (status === "Pending") return "warning";
  return "info";
}

function handleViewLater(item: TenantInvitationDto) {
  ElMessage.info(`TODO: view invitation ${item.invitationPublicId}`);
}
</script>

<style scoped>
.invitations-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.invitations-topbar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.invitations-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: #111827;
}

.invitations-subtitle {
  margin: 6px 0 0;
  color: #6b7280;
  font-size: 14px;
}

.invitations-summary-grid {
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

.invitations-card {
  border-radius: 20px;
}

.invitations-card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.invitations-card-title {
  font-size: 18px;
  font-weight: 700;
  color: #111827;
}

.invitations-card-subtitle {
  margin-top: 6px;
  font-size: 13px;
  color: #6b7280;
}

.invitations-toolbar {
  flex-shrink: 0;
}

.invitations-search {
  width: 280px;
}

.invitation-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.invitation-item {
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

.invitation-item:hover {
  border-color: #dbe4f0;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.06);
}

.invitation-main {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  min-width: 0;
}

.invitation-avatar {
  width: 48px;
  height: 48px;
  border-radius: 999px;
  background: #f3f4f6;
  color: #374151;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  flex-shrink: 0;
}

.invitation-info {
  min-width: 0;
}

.invitation-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.invitation-row-top {
  margin-bottom: 8px;
}

.invitation-email {
  font-size: 16px;
  font-weight: 700;
  color: #111827;
  word-break: break-all;
}

.invitation-meta {
  color: #6b7280;
  font-size: 13px;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.invitation-accepted {
  margin-top: 8px;
  color: #059669;
  font-size: 13px;
  font-weight: 600;
}

.invitation-id {
  margin-top: 10px;
  font-size: 12px;
  color: #9ca3af;
  word-break: break-all;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

.dot {
  color: #9ca3af;
}

.invitation-side {
  flex-shrink: 0;
}

@media (max-width: 900px) {
  .invitations-card-header,
  .invitation-item,
  .invitations-topbar {
    flex-direction: column;
    align-items: flex-start;
  }

  .invitations-summary-grid {
    grid-template-columns: 1fr;
  }

  .invitations-search {
    width: 100%;
  }

  .invitations-toolbar {
    width: 100%;
  }

  .invitation-side {
    width: 100%;
  }
}
</style>