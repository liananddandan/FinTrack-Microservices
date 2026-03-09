<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { ElMessage } from "element-plus";
import { getMyTransactions } from "../api/transactions";

type TransactionItem = {
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

const items = ref<TransactionItem[]>([]);
const loading = ref(false);

const filters = ref({
  keyword: "",
  type: "",
  status: "",
  paymentStatus: "",
});

async function load() {
  loading.value = true;

  try {
    const res = await getMyTransactions();
    items.value = res.items;
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to load transactions");
  } finally {
    loading.value = false;
  }
}

onMounted(load);

const filteredItems = computed(() => {
  const keyword = filters.value.keyword.trim().toLowerCase();

  return items.value.filter((item) => {
    const matchesKeyword =
      !keyword ||
      item.tenantName.toLowerCase().includes(keyword) ||
      item.title.toLowerCase().includes(keyword) ||
      item.type.toLowerCase().includes(keyword);

    const matchesType =
      !filters.value.type || item.type === filters.value.type;

    const matchesStatus =
      !filters.value.status || item.status === filters.value.status;

    const matchesPaymentStatus =
      !filters.value.paymentStatus ||
      item.paymentStatus === filters.value.paymentStatus;

    return (
      matchesKeyword &&
      matchesType &&
      matchesStatus &&
      matchesPaymentStatus
    );
  });
});

function resetFilters() {
  filters.value = {
    keyword: "",
    type: "",
    status: "",
    paymentStatus: "",
  };
}

function formatDateTime(value: string) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

function amountText(row: TransactionItem) {
  return `${row.amount} ${row.currency}`;
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
</script>

<template>
  <div class="transactions-page">
    <div class="page-header">
      <div>
        <h1 class="page-title">My Transactions</h1>
        <p class="page-subtitle">
          Review your donations and procurement requests in the current tenant.
        </p>
      </div>
    </div>

    <el-card class="filter-card" shadow="never">
      <div class="filter-row">
        <el-input
          v-model="filters.keyword"
          placeholder="Search by tenant, title, or type"
          clearable
          class="filter-item keyword-input"
        />

        <el-select
          v-model="filters.type"
          placeholder="Type"
          clearable
          class="filter-item"
        >
          <el-option label="Donation" value="Donation" />
          <el-option label="Procurement" value="Procurement" />
        </el-select>

        <el-select
          v-model="filters.status"
          placeholder="Status"
          clearable
          class="filter-item"
        >
          <el-option label="Draft" value="Draft" />
          <el-option label="Submitted" value="Submitted" />
          <el-option label="Completed" value="Completed" />
          <el-option label="Failed" value="Failed" />
          <el-option label="Cancelled" value="Cancelled" />
          <el-option label="Rejected" value="Rejected" />
        </el-select>

        <el-select
          v-model="filters.paymentStatus"
          placeholder="Payment"
          clearable
          class="filter-item"
        >
          <el-option label="NotStarted" value="NotStarted" />
          <el-option label="Processing" value="Processing" />
          <el-option label="Succeeded" value="Succeeded" />
          <el-option label="Failed" value="Failed" />
        </el-select>

        <el-button @click="resetFilters">Reset</el-button>
      </div>
    </el-card>

    <el-card class="table-card" shadow="never">
      <template #header>
        <div class="table-header">
          <div class="table-title">Transaction List</div>
          <div class="table-count">
            {{ filteredItems.length }} item<span v-if="filteredItems.length !== 1">s</span>
          </div>
        </div>
      </template>

      <el-table
        v-loading="loading"
        :data="filteredItems"
        empty-text="No transactions found."
        class="transaction-table"
      >
        <el-table-column prop="tenantName" label="Tenant" min-width="180" />
        <el-table-column prop="title" label="Title" min-width="180" />
        <el-table-column prop="type" label="Type" width="130">
          <template #default="{ row }">
            <el-tag round effect="light">
              {{ row.type }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="Amount" width="140">
          <template #default="{ row }">
            <span class="amount-text">{{ amountText(row) }}</span>
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
.transactions-page {
  max-width: 1180px;
  margin: 0 auto;
  padding: 24px;
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
  letter-spacing: -0.02em;
  color: #111827;
}

.page-subtitle {
  margin: 8px 0 0;
  color: #6b7280;
  font-size: 14px;
  line-height: 1.6;
}

.filter-card,
.table-card {
  border: none;
  border-radius: 22px;
  box-shadow: 0 10px 28px rgba(15, 23, 42, 0.06);
}

.filter-row {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: center;
}

.filter-item {
  width: 160px;
}

.keyword-input {
  width: 280px;
}

.table-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.table-title {
  font-size: 18px;
  font-weight: 800;
  color: #111827;
}

.table-count {
  font-size: 13px;
  color: #6b7280;
}

.amount-text {
  font-weight: 700;
  color: #111827;
}

.transaction-table :deep(.el-table__cell) {
  padding-top: 14px;
  padding-bottom: 14px;
}

@media (max-width: 768px) {
  .transactions-page {
    padding: 16px;
  }

  .filter-item,
  .keyword-input {
    width: 100%;
  }

  .table-header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>