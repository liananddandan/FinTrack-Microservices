<template>
  <div class="portal-page">
    <div class="portal-shell waiting-shell">
      <div class="portal-brand waiting-brand">
        <div class="portal-badge">FinTrack Portal</div>
        <h1 class="portal-title">Your account is ready.</h1>
        <p class="portal-description">
          You have signed in successfully. Review your current account and tenant status below.
        </p>
      </div>

      <div class="waiting-grid">
        <el-card class="portal-card waiting-card" shadow="never">
          <template #header>
            <div class="card-header-row">
              <div>
                <h2 class="portal-card-title">Account overview</h2>
                <p class="portal-card-subtitle">
                  Basic information about the currently signed-in account.
                </p>
              </div>
              <el-tag type="success" effect="light" round>
                Signed in
              </el-tag>
            </div>
          </template>

          <div class="info-list">
            <div class="info-row">
              <span class="info-label">Email</span>
              <span class="info-value">{{ auth.userEmail || "Unknown user" }}</span>
            </div>

            <div class="info-row">
              <span class="info-label">User Public ID</span>
              <span class="info-value mono">{{ auth.profile?.userPublicId || "TODO" }}</span>
            </div>

            <div class="info-row">
              <span class="info-label">User name</span>
              <span class="info-value">{{ auth.profile?.userName || "TODO" }}</span>
            </div>

            <div class="info-row">
              <span class="info-label">Access token</span>
              <span class="info-value">
                <el-tag v-if="auth.accessToken" type="success" effect="light">Present</el-tag>
                <el-tag v-else type="danger" effect="light">Missing</el-tag>
              </span>
            </div>

            <div class="info-row">
              <span class="info-label">Refresh token</span>
              <span class="info-value">
                <el-tag v-if="auth.refreshToken" type="success" effect="light">Present</el-tag>
                <el-tag v-else type="warning" effect="light">Missing</el-tag>
              </span>
            </div>
          </div>
        </el-card>

        <el-card class="portal-card waiting-card" shadow="never">
          <template #header>
            <div class="card-header-row">
              <div>
                <h2 class="portal-card-title">Memberships</h2>
                <p class="portal-card-subtitle">
                  Organizations and roles currently associated with this account.
                </p>
              </div>
              <el-tag type="info" effect="light" round>
                {{ memberships.length }} item<span v-if="memberships.length !== 1">s</span>
              </el-tag>
            </div>
          </template>

          <div v-if="memberships.length > 0" class="membership-list">
            <div
              v-for="membership in memberships"
              :key="`${membership.tenantPublicId}-${membership.role}`"
              class="membership-item"
            >
              <div class="membership-main">
                <div class="membership-name">{{ membership.tenantName }}</div>
                <div class="membership-meta mono">{{ membership.tenantPublicId }}</div>
              </div>

              <div class="membership-side">
                <el-tag type="primary" effect="light" round>
                  {{ membership.role }}
                </el-tag>
              </div>
            </div>
          </div>

          <el-empty
            v-else
            description="No active tenant memberships found yet."
          />
        </el-card>

        <el-card class="portal-card waiting-card wide-card" shadow="never">
          <template #header>
            <div class="card-header-row">
              <div>
                <h2 class="portal-card-title">Workspace status</h2>
                <p class="portal-card-subtitle">
                  Current routing state based on your tenant memberships.
                </p>
              </div>
            </div>
          </template>

          <div class="workspace-status-panel">
            <div class="status-block">
              <div class="status-icon">
                <el-tag
                  v-if="memberships.length === 0"
                  type="warning"
                  effect="dark"
                  round
                >
                  Pending
                </el-tag>

                <el-tag
                  v-else-if="memberships.length === 1"
                  type="success"
                  effect="dark"
                  round
                >
                  Ready
                </el-tag>

                <el-tag
                  v-else
                  type="primary"
                  effect="dark"
                  round
                >
                  Select Tenant
                </el-tag>
              </div>

              <div class="status-content">
                <div class="status-title">
                  {{ workspaceStatusTitle }}
                </div>
                <div class="status-text">
                  {{ workspaceStatusText }}
                </div>
              </div>
            </div>

            <div class="workspace-actions">
              <el-button
                v-if="memberships.length === 1"
                type="primary"
                size="large"
                @click="enterSingleWorkspace"
              >
                Enter workspace
              </el-button>

              <el-button
                v-else-if="memberships.length > 1"
                type="primary"
                size="large"
                @click="openTenantSelector"
              >
                Choose workspace
              </el-button>

              <el-button
                v-else
                type="primary"
                plain
                size="large"
                @click="requestInvitation"
              >
                TODO: Join organization
              </el-button>
            </div>
          </div>

          <div class="todo-list">
            <div class="todo-item">
              <div class="todo-title">Single membership routing</div>
              <div class="todo-text">
                TODO: Route directly to the correct workspace when the account has exactly one active membership.
              </div>
            </div>

            <div class="todo-item">
              <div class="todo-title">Tenant selector</div>
              <div class="todo-text">
                TODO: Provide a dedicated tenant selection page when the account belongs to multiple organizations.
              </div>
            </div>

            <div class="todo-item">
              <div class="todo-title">Role-based navigation</div>
              <div class="todo-text">
                TODO: Route Admin users to admin portal and non-admin users to member workspace.
              </div>
            </div>

            <div class="todo-item">
              <div class="todo-title">Invitation workflow</div>
              <div class="todo-text">
                TODO: Surface pending invitations and allow the user to accept or reject them from the portal.
              </div>
            </div>
          </div>

          <el-alert
            class="status-alert"
            title="V1 note"
            type="info"
            show-icon
            :closable="false"
            description="This page currently acts as a transitional account status view while workspace routing and tenant-specific flows are being finalized."
          />
        </el-card>
      </div>

      <div class="portal-actions waiting-actions">
        <el-button :loading="loading" @click="refreshProfile">Refresh profile</el-button>
        <el-button @click="goLogin">Back to login</el-button>
        <el-button type="danger" plain @click="logout">Logout</el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import { useAuthStore } from "../stores/auth";
import { getCurrentUser } from "../api/account";

const router = useRouter();
const auth = useAuthStore();
const loading = ref(false);

const memberships = computed(() => auth.memberships ?? []);

const workspaceStatusTitle = computed(() => {
  if (memberships.value.length === 0) return "No active workspace yet";
  if (memberships.value.length === 1) return "One workspace is available";
  return "Multiple workspaces available";
});

const workspaceStatusText = computed(() => {
  if (memberships.value.length === 0) {
    return "Your account is signed in, but it is not yet connected to an active tenant membership.";
  }

  if (memberships.value.length === 1) {
    return `Your account has one active tenant membership in ${memberships.value[0].tenantName}.`;
  }

  return "Your account belongs to multiple tenants. A tenant selection flow will be added next.";
});

async function refreshProfile() {
  if (!auth.accessToken) return;

  loading.value = true;
  try {
    const profile = await getCurrentUser();
    auth.setProfile(profile);
    ElMessage.success("Profile refreshed.");
  } catch (error) {
    console.error("Failed to refresh profile:", error);
    ElMessage.error("Failed to refresh profile.");
  } finally {
    loading.value = false;
  }
}

function enterSingleWorkspace() {
  ElMessage.info("TODO: route to the single workspace.");
}

function openTenantSelector() {
  ElMessage.info("TODO: open tenant selector.");
}

function requestInvitation() {
  ElMessage.info("TODO: implement invitation / join organization flow.");
}

function logout() {
  auth.logout();
  router.push("/login");
}

function goLogin() {
  router.push("/login");
}
</script>

<style scoped>
.waiting-shell {
  max-width: 1100px;
}

.waiting-brand {
  margin-bottom: 24px;
}

.waiting-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.waiting-card {
  border-radius: 20px;
}

.wide-card {
  grid-column: 1 / -1;
}

.card-header-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.info-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  padding: 10px 0;
  border-bottom: 1px solid #f0f2f5;
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  color: #606266;
  font-size: 14px;
  min-width: 120px;
}

.info-value {
  color: #303133;
  font-size: 14px;
  text-align: right;
  word-break: break-word;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
  font-size: 13px;
}

.membership-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.membership-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px;
  border: 1px solid #ebeef5;
  border-radius: 16px;
  background: #fafafa;
}

.membership-main {
  min-width: 0;
}

.membership-name {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 6px;
}

.membership-meta {
  color: #909399;
  word-break: break-all;
}

.membership-side {
  flex-shrink: 0;
}

.workspace-status-panel {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  padding: 18px;
  margin-bottom: 18px;
  border-radius: 18px;
  background: linear-gradient(135deg, #f8fbff 0%, #f4f6fb 100%);
  border: 1px solid #ebeef5;
}

.status-block {
  display: flex;
  align-items: flex-start;
  gap: 16px;
}

.status-title {
  font-size: 18px;
  font-weight: 700;
  color: #303133;
  margin-bottom: 8px;
}

.status-text {
  font-size: 14px;
  color: #606266;
  line-height: 1.6;
  max-width: 640px;
}

.workspace-actions {
  flex-shrink: 0;
}

.todo-list {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
  margin-bottom: 18px;
}

.todo-item {
  padding: 16px;
  border-radius: 16px;
  background: #f8fafc;
  border: 1px solid #ebeef5;
}

.todo-title {
  font-size: 15px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 8px;
}

.todo-text {
  font-size: 14px;
  color: #606266;
  line-height: 1.6;
}

.status-alert {
  margin-top: 8px;
}

.waiting-actions {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

@media (max-width: 900px) {
  .waiting-grid {
    grid-template-columns: 1fr;
  }

  .wide-card {
    grid-column: auto;
  }

  .todo-list {
    grid-template-columns: 1fr;
  }

  .workspace-status-panel,
  .card-header-row,
  .membership-item,
  .info-row {
    flex-direction: column;
    align-items: flex-start;
  }

  .info-value {
    text-align: left;
  }

  .workspace-actions {
    width: 100%;
  }

  .workspace-actions .el-button {
    width: 100%;
  }

  .waiting-actions {
    justify-content: stretch;
    flex-direction: column;
  }
}
</style>