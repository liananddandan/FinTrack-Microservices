<template>
  <div class="portal-page">
    <div class="portal-shell">
      <div class="portal-brand">
        <div class="portal-badge">FinTrack Portal</div>
        <h1 class="portal-title">Welcome back</h1>
        <p class="portal-description">
          You are now connected to your organization workspace.
        </p>
      </div>

      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Workspace</h2>
          <p class="portal-card-subtitle">
            Your current tenant context has been activated successfully.
          </p>
        </div>

        <div class="summary-list">
          <div class="summary-row">
            <span class="summary-label">Email</span>
            <span class="summary-value">{{ auth.userEmail || "-" }}</span>
          </div>

          <div class="summary-row">
            <span class="summary-label">User name</span>
            <span class="summary-value">{{ auth.userName || "-" }}</span>
          </div>

          <div class="summary-row">
            <span class="summary-label">Tenant</span>
            <span class="summary-value">{{ auth.currentTenantName || "-" }}</span>
          </div>

          <div class="summary-row">
            <span class="summary-label">Tenant ID</span>
            <span class="summary-value mono">{{ auth.currentTenantPublicId || "-" }}</span>
          </div>

          <div class="summary-row">
            <span class="summary-label">Role</span>
            <span class="summary-value">
              <el-tag type="primary" effect="light" round>
                {{ auth.currentMembership?.role || "-" }}
              </el-tag>
            </span>
          </div>
        </div>

        <div class="portal-actions">
          <el-button @click="goWaiting">View waiting page</el-button>
          <el-button type="danger" plain @click="logout">Sign out</el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const auth = useAuthStore();

function goWaiting() {
  router.push("/waiting-membership");
}

function logout() {
  auth.logout();
  router.push("/login");
}
</script>

<style scoped>
.summary-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
  padding: 18px;
  border-radius: 18px;
  background: #f9fafb;
  border: 1px solid #eef2f7;
}

.summary-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.summary-label {
  font-size: 14px;
  color: #6b7280;
}

.summary-value {
  font-size: 14px;
  font-weight: 600;
  color: #111827;
  text-align: right;
  word-break: break-word;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

.portal-actions {
  margin-top: 24px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

@media (max-width: 640px) {
  .summary-row {
    flex-direction: column;
    align-items: flex-start;
  }

  .summary-value {
    text-align: left;
  }

  .portal-actions {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>