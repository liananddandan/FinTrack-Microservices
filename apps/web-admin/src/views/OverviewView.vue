<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import { getTenantTransactionSummary, getTenantTransactions } from "../api/transaction-admin";
import { useAuthStore } from "../stores/auth";

type SummaryDto = {
  tenantPublicId: string;
  tenantName: string;
  currentBalance: number;
  totalDonationAmount: number;
  totalProcurementAmount: number;
  totalTransactionCount: number;
};

type TransactionListItem = {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  title: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  riskStatus: string;
  createdAtUtc: string;
};

const router = useRouter();
const auth = useAuthStore();

const loading = ref(false);
const summary = ref<SummaryDto | null>(null);
const recentTransactions = ref<TransactionListItem[]>([]);

async function load() {
  loading.value = true;

  try {
    const transactionResult = await getTenantTransactions({
      pageNumber: 1,
      pageSize: 5,
    });

    recentTransactions.value = transactionResult.items;
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to load recent transactions.");
  }

  try {
    const summaryResult = await getTenantTransactionSummary();
    summary.value = summaryResult;
  } catch {
    // summary 接口还没做好时，先静默降级
    summary.value = {
      tenantPublicId: auth.currentTenantPublicId || "",
      tenantName: auth.currentTenantName || "",
      currentBalance: 0,
      totalDonationAmount: 0,
      totalProcurementAmount: 0,
      totalTransactionCount: recentTransactions.value.length,
    };
  } finally {
    loading.value = false;
  }
}

function formatAmount(value: number) {
  return new Intl.NumberFormat("en-NZ", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function formatDateTime(value: string) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

function statusTagType(status: string) {
  switch (status) {
    case "Completed":
      return "success";
    case "Failed":
      return "danger";
    case "Submitted":
      return "warning";
    case "Cancelled":
      return "info";
    default:
      return "info";
  }
}

function paymentTagType(status: string) {
  switch (status) {
    case "Succeeded":
      return "success";
    case "Failed":
      return "danger";
    case "Processing":
      return "warning";
    default:
      return "info";
  }
}

function goTransactions() {
  router.push("/admin/transactions");
}

function goAuditLogs() {
  router.push("/admin/audit-logs");
}

function goMembers() {
  router.push("/admin/members");
}

function goInvitations() {
  router.push("/admin/invitations");
}

function goTransactionDetail(row: TransactionListItem) {
  router.push(`/admin/transactions/${row.transactionPublicId}`);
}

onMounted(load);
</script>

<template>
  <div class="overview-page" v-loading="loading">
    <div class="page-header">
      <div>
        <h1 class="page-title">Overview</h1>
        <p class="page-subtitle">
          Monitor your tenant’s balance, transactions, and activity in one place.
        </p>
      </div>
    </div>

    <div class="stats-grid">
      <el-card class="stat-card" shadow="never">
        <div class="stat-label">Current Balance</div>
        <div class="stat-value">
          {{ summary ? formatAmount(summary.currentBalance) : "-" }}
        </div>
      </el-card>

      <el-card class="stat-card" shadow="never">
        <div class="stat-label">Total Donations</div>
        <div class="stat-value">
          {{ summary ? formatAmount(summary.totalDonationAmount) : "-" }}
        </div>
      </el-card>

      <el-card class="stat-card" shadow="never">
        <div class="stat-label">Total Procurements</div>
        <div class="stat-value">
          {{ summary ? formatAmount(summary.totalProcurementAmount) : "-" }}
        </div>
      </el-card>

      <el-card class="stat-card" shadow="never">
        <div class="stat-label">Total Transactions</div>
        <div class="stat-value">
          {{ summary?.totalTransactionCount ?? "-" }}
        </div>
      </el-card>
    </div>

    <div class="content-grid">
      <el-card class="panel-card" shadow="never">
        <template #header>
          <div class="panel-header">
            <div>
              <div class="panel-title">Tenant Information</div>
              <div class="panel-subtitle">Current tenant context and administrator details.</div>
            </div>
          </div>
        </template>

        <div class="info-list">
          <div class="info-row">
            <span class="info-label">Tenant Name</span>
            <span class="info-value">{{ summary?.tenantName || auth.currentTenantName || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Tenant ID</span>
            <span class="info-value mono">{{ summary?.tenantPublicId || auth.currentTenantPublicId || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Current User</span>
            <span class="info-value">{{ auth.userName || auth.userEmail || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Role</span>
            <span class="info-value">
              <el-tag type="primary" round effect="light">
                {{ auth.currentMembership?.role || "-" }}
              </el-tag>
            </span>
          </div>
        </div>
      </el-card>

      <el-card class="panel-card" shadow="never">
        <template #header>
          <div class="panel-header">
            <div>
              <div class="panel-title">Quick Actions</div>
              <div class="panel-subtitle">Go to the main administration areas.</div>
            </div>
          </div>
        </template>

        <div class="action-grid">
          <button class="action-tile" @click="goTransactions">
            <div class="action-title">Transactions</div>
            <div class="action-text">View and manage tenant-wide transactions.</div>
          </button>

          <button class="action-tile" @click="goAuditLogs">
            <div class="action-title">Audit Logs</div>
            <div class="action-text">Review important tenant activities and actions.</div>
          </button>

          <button class="action-tile" @click="goMembers">
            <div class="action-title">Members</div>
            <div class="action-text">View and manage tenant members.</div>
          </button>

          <button class="action-tile" @click="goInvitations">
            <div class="action-title">Invitations</div>
            <div class="action-text">Manage pending and sent tenant invitations.</div>
          </button>
        </div>
      </el-card>
    </div>

    <el-card class="panel-card" shadow="never">
      <template #header>
        <div class="panel-header">
          <div>
            <div class="panel-title">Recent Transactions</div>
            <div class="panel-subtitle">Latest transactions in this tenant.</div>
          </div>

          <el-button link type="primary" @click="goTransactions">
            View all
          </el-button>
        </div>
      </template>

      <el-table
        :data="recentTransactions"
        empty-text="No transactions found."
        class="recent-table"
        @row-click="goTransactionDetail"
      >
        <el-table-column prop="title" label="Title" min-width="180" />
        <el-table-column prop="type" label="Type" width="130">
          <template #default="{ row }">
            <el-tag round effect="light">{{ row.type }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column label="Amount" width="140">
          <template #default="{ row }">
            <span class="strong">{{ row.amount }} {{ row.currency }}</span>
          </template>
        </el-table-column>

        <el-table-column prop="status" label="Status" width="130">
          <template #default="{ row }">
            <el-tag :type="statusTagType(row.status)" round effect="light">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="paymentStatus" label="Payment" width="140">
          <template #default="{ row }">
            <el-tag :type="paymentTagType(row.paymentStatus)" round effect="light">
              {{ row.paymentStatus }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column prop="createdAtUtc" label="Created At" min-width="180">
          <template #default="{ row }">
            {{ formatDateTime(row.createdAtUtc) }}
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<style scoped>
.overview-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.page-title {
  margin: 0;
  font-size: 30px;
  font-weight: 800;
  color: #111827;
  letter-spacing: -0.02em;
}

.page-subtitle {
  margin: 8px 0 0;
  color: #6b7280;
  font-size: 14px;
  line-height: 1.6;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.stat-card,
.panel-card {
  border: none;
  border-radius: 22px;
  box-shadow: 0 10px 28px rgba(15, 23, 42, 0.06);
}

.stat-label {
  font-size: 13px;
  color: #6b7280;
}

.stat-value {
  margin-top: 12px;
  font-size: 30px;
  font-weight: 800;
  color: #111827;
  letter-spacing: -0.02em;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.panel-title {
  font-size: 18px;
  font-weight: 800;
  color: #111827;
}

.panel-subtitle {
  margin-top: 6px;
  color: #6b7280;
  font-size: 13px;
  line-height: 1.5;
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
  align-items: flex-start;
}

.info-label {
  color: #6b7280;
  font-size: 14px;
}

.info-value {
  color: #111827;
  font-size: 14px;
  font-weight: 600;
  text-align: right;
  word-break: break-word;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

.action-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.action-tile {
  text-align: left;
  border: 1px solid #e5e7eb;
  border-radius: 18px;
  padding: 16px;
  background: linear-gradient(180deg, #ffffff 0%, #f9fafb 100%);
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-tile:hover {
  transform: translateY(-2px);
  border-color: #cbd5e1;
  box-shadow: 0 10px 22px rgba(15, 23, 42, 0.06);
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

.strong {
  font-weight: 800;
  color: #111827;
}

.recent-table :deep(.el-table__row) {
  cursor: pointer;
}

@media (max-width: 1100px) {
  .stats-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .content-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }

  .action-grid {
    grid-template-columns: 1fr;
  }

  .info-row {
    flex-direction: column;
  }

  .info-value {
    text-align: left;
  }
}
</style>