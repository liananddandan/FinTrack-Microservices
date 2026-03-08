<template>
  <div class="portal-page">
    <div class="portal-shell">
      <section class="hero-card">
        <div class="hero-left">
          <div class="portal-badge">FinTrack Portal</div>
          <h1 class="portal-title">
            Welcome back, {{ auth.userName || "member" }}
          </h1>
          <p class="portal-description">
            You are connected to your organization workspace. You can make a
            donation or review your transaction history here.
          </p>

          <div class="hero-actions">
            <el-button type="primary" size="large" @click="goDonate">
              Make a donation
            </el-button>
            <el-button size="large" @click="goMyTransactions">
              My transactions
            </el-button>
          </div>
        </div>

        <div class="hero-right">
          <div class="tenant-card">
            <div class="tenant-label">Workspace</div>
            <div class="tenant-name">
              {{ auth.currentTenantName || "-" }}
            </div>
            <div class="tenant-role">
              <el-tag type="primary" effect="light" round>
                {{ auth.currentMembership?.role || "-" }}
              </el-tag>
            </div>
          </div>
        </div>
      </section>

      <section class="content-grid">
        <el-card class="info-card" shadow="never">
          <template #header>
            <div class="card-header">
              <div>
                <div class="card-title">Workspace Summary</div>
                <div class="card-subtitle">
                  Your current account and tenant context.
                </div>
              </div>
            </div>
          </template>

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
              <span class="summary-value mono">
                {{ auth.currentTenantPublicId || "-" }}
              </span>
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
        </el-card>

        <el-card class="action-card" shadow="never">
          <template #header>
            <div class="card-header">
              <div>
                <div class="card-title">Quick Actions</div>
                <div class="card-subtitle">
                  Choose what you want to do next.
                </div>
              </div>
            </div>
          </template>

          <div class="action-stack">
            <button class="action-item primary" @click="goDonate">
              <div class="action-icon">❤</div>
              <div class="action-content">
                <div class="action-title">Make a donation</div>
                <div class="action-text">
                  Create a donation transaction for your current tenant.
                </div>
              </div>
            </button>

            <button class="action-item" @click="goMyTransactions">
              <div class="action-icon">📘</div>
              <div class="action-content">
                <div class="action-title">My transactions</div>
                <div class="action-text">
                  Review your donation and procurement history.
                </div>
              </div>
            </button>
          </div>

          <div class="signout-wrap">
            <el-button type="danger" plain @click="logout">Sign out</el-button>
          </div>
        </el-card>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const auth = useAuthStore();

function goDonate() {
  router.push("/donate");
}

function goMyTransactions() {
  router.push("/my-transactions");
}

function logout() {
  auth.logout();
  router.push("/login");
}
</script>

<style scoped>
.portal-page {
  min-height: 100%;
  padding: 24px;
  background:
    radial-gradient(circle at top left, rgba(59, 130, 246, 0.06), transparent 28%),
    linear-gradient(180deg, #f8fbff 0%, #f5f7fb 100%);
}

.portal-shell {
  max-width: 1080px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.hero-card {
  display: grid;
  grid-template-columns: 1.7fr 0.5fr;
  gap: 16px;
  border-radius: 24px;
  padding: 24px;
  background: linear-gradient(135deg, #f0f7ff 0%, #eef2ff 100%);
  border: 1px solid #dbeafe;
  box-shadow: 0 16px 40px rgba(15, 23, 42, 0.06);
}

.hero-left,
.hero-right {
  min-width: 0;
}

.portal-badge {
  display: inline-flex;
  align-items: center;
  padding: 8px 14px;
  border-radius: 999px;
  background: #e0ecff;
  color: #1d4ed8;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.portal-title {
  margin: 18px 0 10px;
  font-size: 36px;
  line-height: 1.1;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: #0f172a;
}

.portal-description {
  margin: 0;
  max-width: 620px;
  color: #475569;
  font-size: 15px;
  line-height: 1.7;
}

.hero-actions {
  margin-top: 24px;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.tenant-card {
  height: auto;
  min-height: 120px;
  border-radius: 16px;
  padding: 14px 16px;
  background: rgba(255, 255, 255, 0.65);
  border: 1px solid #e8eef7;
  box-shadow: none;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.tenant-label {
  font-size: 12px;
  color: #64748b;
}

.tenant-name {
  margin-top: 8px;
  font-size: 18px;
  line-height: 1.3;
  font-weight: 700;
  color: #111827;
  word-break: break-word;
}

.tenant-role {
  margin-top: 10px;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 0.95fr;
  gap: 20px;
}

.info-card,
.action-card {
  border: none;
  border-radius: 24px;
  box-shadow: 0 10px 28px rgba(15, 23, 42, 0.06);
  background: rgba(255, 255, 255, 0.96);
}

.card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.card-title {
  font-size: 18px;
  font-weight: 800;
  color: #111827;
  letter-spacing: -0.01em;
}

.card-subtitle {
  margin-top: 6px;
  font-size: 13px;
  color: #6b7280;
  line-height: 1.5;
}

.summary-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
  padding: 18px;
  border-radius: 20px;
  background: linear-gradient(180deg, #fafcff 0%, #f8fafc 100%);
  border: 1px solid #edf2f7;
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
  font-weight: 700;
  color: #111827;
  text-align: right;
  word-break: break-word;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

.action-stack {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.action-item {
  width: 100%;
  border: 1px solid #e5e7eb;
  background: linear-gradient(180deg, #ffffff 0%, #f9fafb 100%);
  border-radius: 20px;
  padding: 18px;
  text-align: left;
  display: flex;
  align-items: flex-start;
  gap: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-item:hover {
  transform: translateY(-2px);
  border-color: #cbd5e1;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.06);
}

.action-item.primary {
  border-color: #bfdbfe;
  background: linear-gradient(135deg, #eff6ff 0%, #eef2ff 100%);
}

.action-icon {
  width: 46px;
  height: 46px;
  flex-shrink: 0;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #eef2ff;
  font-size: 20px;
}

.action-content {
  min-width: 0;
}

.action-title {
  font-size: 15px;
  font-weight: 800;
  color: #111827;
}

.action-text {
  margin-top: 6px;
  font-size: 13px;
  line-height: 1.6;
  color: #6b7280;
}

.signout-wrap {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 960px) {
  .hero-card,
  .content-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .portal-page {
    padding: 16px;
  }

  .hero-card {
    padding: 22px;
  }

  .portal-title {
    font-size: 30px;
  }

  .hero-actions,
  .signout-wrap {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-row {
    flex-direction: column;
    align-items: flex-start;
  }

  .summary-value {
    text-align: left;
  }
}
</style>