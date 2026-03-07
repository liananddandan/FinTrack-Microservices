<template>
  <div class="portal-page">
    <div class="portal-shell">
      <div class="portal-brand">
        <div class="portal-badge">FinTrack Portal</div>
        <h1 class="portal-title">Waiting for organization access</h1>
        <p class="portal-description">
          Your account is ready, but you are not connected to any active organization yet.
        </p>
      </div>

      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Account status</h2>
          <p class="portal-card-subtitle">
            You can sign in, but tenant features become available only after your membership is active.
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
            <span class="summary-label">Membership count</span>
            <span class="summary-value">{{ auth.resolvedMemberships.length }}</span>
          </div>
        </div>

        <el-alert
          title="If an administrator has invited you, please open the invitation email and accept the invitation first."
          type="info"
          show-icon
          class="portal-alert"
        />

        <div class="portal-actions">
          <el-button @click="refreshProfile" :loading="loading">
            Refresh status
          </el-button>
          <el-button type="danger" plain @click="logout">
            Sign out
          </el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import { getCurrentUser } from "../api/account";
import { useAuthStore } from "../stores/auth";

const auth = useAuthStore();
const router = useRouter();
const loading = ref(false);

onMounted(async () => {
  await redirectIfTenantReady();
});

async function redirectIfTenantReady() {
  if (auth.hasTenantContext) {
    await router.replace("/home");
    return;
  }

  const memberships = auth.resolvedMemberships;

  if (memberships.length === 1) {
    try {
      await auth.activateSingleTenantIfPossible();
      if (auth.hasTenantContext) {
        await router.replace("/home");
      }
    } catch {
      // stay on waiting page
    }
  }
}

async function refreshProfile() {
  loading.value = true;

  try {
    const profile = await getCurrentUser();
    auth.setProfile(profile);

    if ((profile.memberships?.length ?? 0) > 0) {
      await auth.activateSingleTenantIfPossible();
    }

    if (auth.hasTenantContext) {
      await router.replace("/home");
      return;
    }

    ElMessage.success("Status refreshed.");
  } catch (error: any) {
    ElMessage.error(
      error?.response?.data?.message ??
        error?.message ??
        "Failed to refresh account status."
    );
  } finally {
    loading.value = false;
  }
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