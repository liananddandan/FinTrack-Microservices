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
            You are connected to your organization workspace. From here, you can
            make a donation, create a procurement request, and review your own
            transaction history.
          </p>

          <div class="hero-inline-meta">
            <span class="hero-meta-item">
              <span class="hero-meta-label">Tenant</span>
              <span class="hero-meta-value">{{ auth.currentTenantName || "-" }}</span>
            </span>

            <span class="hero-meta-divider"></span>

            <span class="hero-meta-item">
              <span class="hero-meta-label">Role</span>
              <el-tag type="primary" effect="light" round>
                {{ auth.currentMembership?.role || "-" }}
              </el-tag>
            </span>
          </div>
        </div>

        <div class="hero-right">
          <div class="tenant-card">
            <div class="tenant-card-label">Current workspace</div>
            <div class="tenant-card-name">
              {{ auth.currentTenantName || "-" }}
            </div>
            <div class="tenant-card-id mono">
              {{ auth.currentTenantPublicId || "-" }}
            </div>
          </div>
        </div>
      </section>

      <section class="actions-section">
        <div class="section-heading">
          <h2 class="section-title">Quick Actions</h2>
          <p class="section-subtitle">
            Choose the next action you want to take in this workspace.
          </p>
        </div>

        <div class="action-grid">
          <button class="action-card primary" @click="goDonate">
            <div class="action-icon">❤</div>
            <div class="action-content">
              <div class="action-title">Make a donation</div>
              <div class="action-text">
                Contribute funds directly to support your tenant.
              </div>
            </div>
          </button>

          <button class="action-card" @click="goProcurement">
            <div class="action-icon">🧾</div>
            <div class="action-content">
              <div class="action-title">New procurement</div>
              <div class="action-text">
                Create a procurement draft and submit it for approval.
              </div>
            </div>
          </button>

          <button class="action-card" @click="goMyTransactions">
            <div class="action-icon">📘</div>
            <div class="action-content">
              <div class="action-title">My transactions</div>
              <div class="action-text">
                Review your donations and procurement requests.
              </div>
            </div>
          </button>
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

        <el-card class="side-card" shadow="never">
          <template #header>
            <div class="card-header">
              <div>
                <div class="card-title">Account</div>
                <div class="card-subtitle">
                  Session and workspace actions.
                </div>
              </div>
            </div>
          </template>

          <div class="side-card-body">
            <div class="account-note">
              Your tenant context is active. Use the quick actions above to
              create or review transactions in this workspace.
            </div>

            <div class="account-actions">
              <el-button @click="goMyTransactions">My transactions</el-button>
              <el-button type="danger" plain @click="logout">Sign out</el-button>
            </div>
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

function goProcurement() {
  router.push("/procurements/new");
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
    radial-gradient(circle at top left, rgba(59, 130, 246, 0.06), transparent 30%),
    linear-gradient(180deg, #f8fbff 0%, #f5f7fb 100%);
}

.portal-shell {
  max-width: 1120px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.hero-card {
  display: grid;
  grid-template-columns: 1.5fr 0.7fr;
  gap: 18px;
  border-radius: 26px;
  padding: 28px;
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
  line-height: 1.08;
  font-weight: 800;
  letter-spacing: -0.02em;
  color: #0f172a;
}

.portal-description {
  margin: 0;
  max-width: 680px;
  color: #475569;
  font-size: 15px;
  line-height: 1.7;
}

.hero-inline-meta {
  margin-top: 22px;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 14px;
}

.hero-meta-item {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.hero-meta-label {
  font-size: 13px;
  color: #64748b;
}

.hero-meta-value {
  font-size: 14px;
  font-weight: 700;
  color: #111827;
}

.hero-meta-divider {
  width: 1px;
  height: 18px;
  background: #dbe4f0;
}

.tenant-card {
  height: auto;
  min-height: 118px;
  border-radius: 18px;
  padding: 16px 18px;
  background: rgba(255, 255, 255, 0.68);
  border: 1px solid #e8eef7;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.tenant-card-label {
  font-size: 12px;
  color: #64748b;
}

.tenant-card-name {
  margin-top: 8px;
  font-size: 20px;
  line-height: 1.25;
  font-weight: 700;
  color: #111827;
  word-break: break-word;
}

.tenant-card-id {
  margin-top: 10px;
  font-size: 12px;
  color: #64748b;
  word-break: break-all;
}

.actions-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.section-heading {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.section-title {
  margin: 0;
  font-size: 22px;
  font-weight: 800;
  color: #111827;
  letter-spacing: -0.01em;
}

.section-subtitle {
  margin: 0;
  font-size: 14px;
  color: #6b7280;
  line-height: 1.6;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
}

.action-card {
  width: 100%;
  border: 1px solid #e5e7eb;
  background: linear-gradient(180deg, #ffffff 0%, #f9fafb 100%);
  border-radius: 22px;
  padding: 18px;
  text-align: left;
  display: flex;
  align-items: flex-start;
  gap: 14px;
  cursor: pointer;
  transition: all 0.22s ease;
}

.action-card:hover {
  transform: translateY(-2px);
  border-color: #cbd5e1;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.06);
}

.action-card.primary {
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

.content-grid {
  display: grid;
  grid-template-columns: 1fr 0.85fr;
  gap: 18px;
}

.info-card,
.side-card {
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

.side-card-body {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.account-note {
  padding: 18px;
  border-radius: 18px;
  background: linear-gradient(180deg, #fafcff 0%, #f8fafc 100%);
  border: 1px solid #edf2f7;
  color: #4b5563;
  font-size: 14px;
  line-height: 1.7;
}

.account-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

@media (max-width: 1024px) {
  .hero-card,
  .content-grid {
    grid-template-columns: 1fr;
  }

  .action-grid {
    grid-template-columns: 1fr 1fr;
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

  .action-grid {
    grid-template-columns: 1fr;
  }

  .summary-row {
    flex-direction: column;
    align-items: flex-start;
  }

  .summary-value {
    text-align: left;
  }

  .account-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .hero-inline-meta {
    flex-direction: column;
    align-items: flex-start;
  }

  .hero-meta-divider {
    display: none;
  }
}
</style>