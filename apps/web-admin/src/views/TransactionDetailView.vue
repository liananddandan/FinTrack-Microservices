<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import {
  getTransactionDetail,
  approveProcurement,
  rejectProcurement,
} from "../api/transaction-admin";

type TransactionDetail = {
  transactionPublicId: string;
  tenantPublicId: string;
  tenantName: string;
  type: string;
  title: string;
  description?: string;
  amount: number;
  currency: string;
  status: string;
  paymentStatus: string;
  riskStatus: string;
  createdByUserPublicId: string;
  createdAtUtc: string;
  approvedByUserPublicId?: string;
  approvedAtUtc?: string;
  paidByUserPublicId?: string;
  paidAtUtc?: string;
  paymentReference?: string;
  failureReason?: string;
  refundedByUserPublicId?: string;
  refundedAtUtc?: string;
};

const route = useRoute();
const router = useRouter();

const loading = ref(false);
const actionLoading = ref(false);
const rejectReason = ref("");
const rejectDialogVisible = ref(false);
const detail = ref<TransactionDetail | null>(null);

const isProcurement = computed(() => detail.value?.type === "Procurement");
const canReview = computed(
  () =>
    detail.value &&
    detail.value.type === "Procurement" &&
    detail.value.status === "Submitted"
);

function formatDateTime(value?: string) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

function formatAmount(amount: number, currency: string) {
  return `${amount} ${currency}`;
}

function statusTagType(status: string) {
  switch (status) {
    case "Completed":
    case "Approved":
      return "success";
    case "Failed":
    case "Rejected":
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

async function load() {
  const transactionPublicId = route.params.transactionPublicId as string;

  if (!transactionPublicId) {
    ElMessage.error("Transaction id is missing.");
    router.push("/admin/transactions");
    return;
  }

  loading.value = true;

  try {
    detail.value = await getTransactionDetail(transactionPublicId);
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to load transaction detail.");
    router.push("/admin/transactions");
  } finally {
    loading.value = false;
  }
}

async function handleApprove() {
  if (!detail.value) return;

  actionLoading.value = true;

  try {
    await approveProcurement(detail.value.transactionPublicId);
    ElMessage.success("Procurement approved.");
    await load();
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to approve procurement.");
  } finally {
    actionLoading.value = false;
  }
}

function openRejectDialog() {
  rejectReason.value = "";
  rejectDialogVisible.value = true;
}

async function confirmReject() {
  if (!detail.value) return;

  if (!rejectReason.value.trim()) {
    ElMessage.error("Reject reason is required.");
    return;
  }

  actionLoading.value = true;

  try {
    await rejectProcurement(detail.value.transactionPublicId, {
      reason: rejectReason.value.trim(),
    });

    ElMessage.success("Procurement rejected.");
    rejectDialogVisible.value = false;
    await load();
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to reject procurement.");
  } finally {
    actionLoading.value = false;
  }
}

onMounted(load);
</script>

<template>
  <div class="detail-page">
    <div class="page-header">
      <div>
        <h1 class="page-title">Transaction Detail</h1>
        <p class="page-subtitle">
          Review full transaction information within the current tenant.
        </p>
      </div>

      <el-button @click="router.push('/admin/transactions')">
        Back to transactions
      </el-button>
    </div>

    <el-card class="summary-card" shadow="never" v-loading="loading">
      <template v-if="detail">
        <div class="summary-top">
          <div>
            <div class="summary-type">{{ detail.type }}</div>
            <h2 class="summary-title">{{ detail.title }}</h2>
            <div class="summary-tenant">{{ detail.tenantName }}</div>
          </div>

          <div class="summary-right">
            <div class="summary-amount">
              {{ formatAmount(detail.amount, detail.currency) }}
            </div>

            <div class="summary-tags">
              <el-tag :type="statusTagType(detail.status)" round effect="light">
                {{ detail.status }}
              </el-tag>

              <el-tag
                :type="paymentTagType(detail.paymentStatus)"
                round
                effect="light"
              >
                {{ detail.paymentStatus }}
              </el-tag>
            </div>
          </div>
        </div>

        <div v-if="canReview" class="review-actions">
          <el-button
            type="success"
            :loading="actionLoading"
            @click="handleApprove"
          >
            Approve
          </el-button>

          <el-button
            type="danger"
            plain
            :loading="actionLoading"
            @click="openRejectDialog"
          >
            Reject
          </el-button>
        </div>
      </template>
    </el-card>

    <template v-if="detail">
      <el-card class="content-card" shadow="never">
        <template #header>
          <div class="card-title">
            {{ isProcurement ? "Procurement Information" : "Transaction Information" }}
          </div>
        </template>

        <div class="info-list">
          <div class="info-row">
            <span class="info-label">Title</span>
            <span class="info-value">{{ detail.title }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Description</span>
            <span class="info-value">{{ detail.description || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Amount</span>
            <span class="info-value strong">
              {{ formatAmount(detail.amount, detail.currency) }}
            </span>
          </div>

          <div class="info-row">
            <span class="info-label">Type</span>
            <span class="info-value">{{ detail.type }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Status</span>
            <span class="info-value">{{ detail.status }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Payment status</span>
            <span class="info-value">{{ detail.paymentStatus }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Risk status</span>
            <span class="info-value">{{ detail.riskStatus }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Payment reference</span>
            <span class="info-value">{{ detail.paymentReference || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Failure reason</span>
            <span class="info-value">{{ detail.failureReason || "-" }}</span>
          </div>
        </div>
      </el-card>

      <el-card class="content-card" shadow="never">
        <template #header>
          <div class="card-title">Timeline & Metadata</div>
        </template>

        <div class="info-list">
          <div class="info-row">
            <span class="info-label">Transaction ID</span>
            <span class="info-value mono">{{ detail.transactionPublicId }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Tenant ID</span>
            <span class="info-value mono">{{ detail.tenantPublicId }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Created by</span>
            <span class="info-value mono">{{ detail.createdByUserPublicId }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Created at</span>
            <span class="info-value">{{ formatDateTime(detail.createdAtUtc) }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Approved by</span>
            <span class="info-value mono">{{ detail.approvedByUserPublicId || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Approved at</span>
            <span class="info-value">{{ formatDateTime(detail.approvedAtUtc) }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Paid by</span>
            <span class="info-value mono">{{ detail.paidByUserPublicId || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Paid at</span>
            <span class="info-value">{{ formatDateTime(detail.paidAtUtc) }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Refunded by</span>
            <span class="info-value mono">{{ detail.refundedByUserPublicId || "-" }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Refunded at</span>
            <span class="info-value">{{ formatDateTime(detail.refundedAtUtc) }}</span>
          </div>
        </div>
      </el-card>
    </template>

    <el-dialog
      v-model="rejectDialogVisible"
      title="Reject Procurement"
      width="520px"
    >
      <el-form label-position="top">
        <el-form-item label="Reject reason">
          <el-input
            v-model="rejectReason"
            type="textarea"
            :rows="4"
            placeholder="Please enter reject reason"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="rejectDialogVisible = false">Cancel</el-button>
        <el-button
          type="danger"
          :loading="actionLoading"
          @click="confirmReject"
        >
          Confirm Reject
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.detail-page {
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
  margin-top: 8px;
  color: #6b7280;
  font-size: 14px;
  line-height: 1.6;
}

.summary-card,
.content-card {
  border: none;
  border-radius: 22px;
  box-shadow: 0 10px 28px rgba(15, 23, 42, 0.06);
}

.summary-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.summary-type {
  font-size: 13px;
  font-weight: 700;
  color: #4f46e5;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.summary-title {
  margin: 8px 0 6px;
  font-size: 28px;
  font-weight: 800;
  color: #111827;
  line-height: 1.2;
}

.summary-tenant {
  color: #6b7280;
  font-size: 14px;
}

.summary-right {
  text-align: right;
  display: flex;
  flex-direction: column;
  gap: 12px;
  align-items: flex-end;
}

.summary-amount {
  font-size: 26px;
  font-weight: 800;
  color: #111827;
  white-space: nowrap;
}

.summary-tags {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.review-actions {
  margin-top: 18px;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.card-title {
  font-size: 18px;
  font-weight: 800;
  color: #111827;
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
  max-width: 60%;
}

.info-value.strong {
  font-weight: 800;
}

.mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas,
    "Liberation Mono", "Courier New", monospace;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-top {
    flex-direction: column;
  }

  .summary-right {
    text-align: left;
    align-items: flex-start;
  }

  .summary-tags {
    justify-content: flex-start;
  }

  .info-row {
    flex-direction: column;
  }

  .info-value {
    text-align: left;
    max-width: 100%;
  }

  .review-actions {
    justify-content: stretch;
    flex-direction: column;
  }
}
</style>