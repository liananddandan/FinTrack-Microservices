<script setup lang="ts">
import { onMounted, ref } from "vue";
import { ElMessage } from "element-plus";
import { getTenantTransactions } from "../api/transaction-admin";

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

const loading = ref(false);
const items = ref<TransactionItem[]>([]);
const totalCount = ref(0);

const query = ref({
  type: "",
  status: "",
  paymentStatus: "",
  pageNumber: 1,
  pageSize: 10,
});

async function load() {
  loading.value = true;

  try {
    const result = await getTenantTransactions({
      type: query.value.type || undefined,
      status: query.value.status || undefined,
      paymentStatus: query.value.paymentStatus || undefined,
      pageNumber: query.value.pageNumber,
      pageSize: query.value.pageSize,
    });

    items.value = result.items;
    totalCount.value = result.totalCount;
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to load transactions.");
  } finally {
    loading.value = false;
  }
}

function resetFilters() {
  query.value.type = "";
  query.value.status = "";
  query.value.paymentStatus = "";
  query.value.pageNumber = 1;
  load();
}

function handlePageChange(page: number) {
  query.value.pageNumber = page;
  load();
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

onMounted(load);
</script>

<template>
  <div class="transactions-page">
    <div class="page-header">
      <div>
        <h1 class="page-title">Transactions</h1>
        <p class="page-subtitle">
          Review all transactions within the current tenant.
        </p>
      </div>
    </div>

    <el-card class="filter-card" shadow="never">
      <div class="filter-row">
        <el-select v-model="query.type" placeholder="Type" clearable class="filter-item">
          <el-option label="Donation" value="Donation" />
          <el-option label="Procurement" value="Procurement" />
        </el-select>

        <el-select v-model="query.status" placeholder="Status" clearable class="filter-item">
          <el-option label="Draft" value="Draft" />
          <el-option label="Submitted" value="Submitted" />
          <el-option label="Completed" value="Completed" />
          <el-option label="Failed" value="Failed" />
          <el-option label="Cancelled" value="Cancelled" />
          <el-option label="Rejected" value="Rejected" />
        </el-select>

        <el-select
          v-model="query.paymentStatus"
          placeholder="Payment"
          clearable
          class="filter-item"
        >
          <el-option label="NotStarted" value="NotStarted" />
          <el-option label="Processing" value="Processing" />
          <el-option label="Succeeded" value="Succeeded" />
          <el-option label="Failed" value="Failed" />
        </el-select>

        <el-button type="primary" @click="load">Search</el-button>
        <el-button @click="resetFilters">Reset</el-button>
      </div>
    </el-card>

    <el-card class="table-card" shadow="never">
      <el-table v-loading="loading" :data="items" empty-text="No transactions found.">
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

      <div class="pagination-wrap">
        <el-pagination
          background
          layout="prev, pager, next, total"
          :current-page="query.pageNumber"
          :page-size="query.pageSize"
          :total="totalCount"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.transactions-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.page-title {
  margin: 0;
  font-size: 30px;
  font-weight: 800;
  color: #111827;
}

.page-subtitle {
  margin-top: 8px;
  color: #6b7280;
  font-size: 14px;
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
}

.filter-item {
  width: 180px;
}

.strong {
  font-weight: 800;
  color: #111827;
}

.pagination-wrap {
  margin-top: 18px;
  display: flex;
  justify-content: flex-end;
}
</style>