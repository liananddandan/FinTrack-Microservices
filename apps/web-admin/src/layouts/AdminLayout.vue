<template>
  <el-container class="admin-shell">
    <el-aside width="240px" class="admin-aside">
      <div class="admin-brand">
        <div class="admin-brand-title">FinTrack Admin</div>
        <div class="admin-brand-subtitle">
          {{ auth.currentTenantName || "No tenant" }}
        </div>
      </div>

      <el-menu :default-active="active" router class="admin-menu">
        <el-menu-item index="/dashboard">
          <span>Dashboard</span>
        </el-menu-item>

        <el-menu-item index="/transactions">
          <span>Transactions</span>
        </el-menu-item>

        <el-menu-item index="/members">
          <span>Members</span>
        </el-menu-item>

        <el-menu-item index="/invitations">
          <span>Invitations</span>
        </el-menu-item>

        <el-menu-item index="/audit-logs">
          <span>Audit Logs</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header class="admin-header">
        <div class="admin-header-left">
          <div class="admin-page-title">{{ pageTitle }}</div>
          <div class="admin-page-meta">
            Tenant: {{ auth.currentTenantName || "Unknown tenant" }}
          </div>
        </div>

        <div class="admin-header-right">
          <div class="admin-user">
            <div class="admin-user-name">
              {{ auth.userName || auth.userEmail || "Unknown user" }}
            </div>
            <div class="admin-user-email">
              {{ auth.userEmail || "No email" }}
            </div>
          </div>

          <el-button type="danger" plain @click="logout">
            Logout
          </el-button>
        </div>
      </el-header>

      <el-main class="admin-main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();

const active = computed(() => route.path);

const pageTitle = computed(() => {
  if (route.path.startsWith("/members")) return "Members";
  if (route.path.startsWith("/transactions")) return "Transactions";
  return "Dashboard";
});

function logout() {
  auth.logout();
  router.push("/login");
}
</script>

<style scoped>
.admin-shell {
  min-height: 100vh;
  background: #f5f7fb;
}

.admin-aside {
  background: #111827;
  color: #fff;
  border-right: 1px solid rgba(255, 255, 255, 0.06);
}

.admin-brand {
  padding: 20px 18px 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.admin-brand-title {
  font-size: 18px;
  font-weight: 700;
  color: #fff;
}

.admin-brand-subtitle {
  margin-top: 6px;
  font-size: 13px;
  color: rgba(255, 255, 255, 0.7);
  word-break: break-word;
}

.admin-menu {
  border-right: none;
  background: transparent;
}

.admin-header {
  height: 72px;
  background: #fff;
  border-bottom: 1px solid #ebeef5;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
}

.admin-header-left {
  min-width: 0;
}

.admin-page-title {
  font-size: 20px;
  font-weight: 700;
  color: #111827;
}

.admin-page-meta {
  margin-top: 4px;
  font-size: 13px;
  color: #6b7280;
}

.admin-header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.admin-user {
  text-align: right;
}

.admin-user-name {
  font-size: 14px;
  font-weight: 600;
  color: #111827;
}

.admin-user-email {
  font-size: 12px;
  color: #6b7280;
}

.admin-main {
  padding: 24px;
}
</style>