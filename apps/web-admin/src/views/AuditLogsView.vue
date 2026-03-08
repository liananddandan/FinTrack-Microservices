<template>
  <div class="audit-page">
    <div class="audit-topbar">
      <div>
        <h2 class="audit-title">Audit Logs</h2>
        <p class="audit-subtitle">
          Review administrative actions and membership-related activities in the current organization.
        </p>
      </div>
    </div>

    <el-card class="audit-filter-card" shadow="never">
      <div class="audit-filters">
        <el-select
          v-model="filters.actionType"
          placeholder="Action type"
          clearable
          class="filter-item"
        >
          <el-option label="Membership.Invited" value="Membership.Invited" />
          <el-option label="Membership.InvitationResent" value="Membership.InvitationResent" />
          <el-option label="Membership.Accepted" value="Membership.Accepted" />
          <el-option label="Membership.Removed" value="Membership.Removed" />
          <el-option label="Membership.RoleChanged" value="Membership.RoleChanged" />
        </el-select>

        <el-date-picker
          v-model="dateRange"
          type="datetimerange"
          start-placeholder="From"
          end-placeholder="To"
          range-separator="To"
          format="YYYY-MM-DD HH:mm"
          value-format="YYYY-MM-DDTHH:mm:ss[Z]"
          class="filter-item filter-date"
        />

        <el-button type="primary" @click="loadLogs(1)">Search</el-button>
        <el-button @click="resetFilters">Reset</el-button>
      </div>
    </el-card>

    <el-card class="audit-table-card" shadow="never">
      <el-table
        v-loading="loading"
        :data="logs"
        width="100%"
        empty-text="No audit logs found."
      >
        <el-table-column prop="occurredAtUtc" label="Time" min-width="180">
          <template #default="{ row }">
            {{ formatDateTime(row.occurredAtUtc) }}
          </template>
        </el-table-column>

        <el-table-column prop="actorDisplayName" label="Actor" min-width="140">
          <template #default="{ row }">
            {{ row.actorDisplayName || "-" }}
          </template>
        </el-table-column>

        <el-table-column prop="actionType" label="Action" min-width="220" />

        <el-table-column prop="targetDisplay" label="Target" min-width="180">
          <template #default="{ row }">
            {{ row.targetDisplay || "-" }}
          </template>
        </el-table-column>

        <el-table-column prop="summary" label="Summary" min-width="320" />

        <el-table-column label="Details" width="100" fixed="right">
          <template #default="{ row }">
            <el-button text @click="openDetails(row)">View</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="audit-pagination">
        <el-pagination
          background
          layout="prev, pager, next, total"
          :total="totalCount"
          :page-size="pageSize"
          :current-page="pageNumber"
          @current-change="loadLogs"
        />
      </div>
    </el-card>

    <el-dialog
      v-model="detailsVisible"
      title="Audit Log Details"
      width="680px"
      destroy-on-close
    >
      <template v-if="selectedLog">
        <div class="detail-row"><strong>Time:</strong> {{ formatDateTime(selectedLog.occurredAtUtc) }}</div>
        <div class="detail-row"><strong>Actor:</strong> {{ selectedLog.actorDisplayName || "-" }}</div>
        <div class="detail-row"><strong>Action:</strong> {{ selectedLog.actionType }}</div>
        <div class="detail-row"><strong>Target:</strong> {{ selectedLog.targetDisplay || "-" }}</div>
        <div class="detail-row"><strong>Source:</strong> {{ selectedLog.source || "-" }}</div>
        <div class="detail-row"><strong>Correlation Id:</strong> {{ selectedLog.correlationId || "-" }}</div>

        <div class="detail-json-title">Metadata</div>
        <pre class="detail-json">{{ formatJson(selectedLog.metadataJson) }}</pre>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from "vue";
import { ElMessage } from "element-plus";
import {
  getAuditLogs,
  type AuditLogItem,
} from "../api/audit-log";

const loading = ref(false);
const logs = ref<AuditLogItem[]>([]);
const totalCount = ref(0);
const pageNumber = ref(1);
const pageSize = ref(10);

const dateRange = ref<string[] | null>(null);

const filters = reactive({
  actionType: "",
});

const detailsVisible = ref(false);
const selectedLog = ref<AuditLogItem | null>(null);

onMounted(async () => {
  await loadLogs(1);
});

async function loadLogs(page = 1) {
  loading.value = true;
  pageNumber.value = page;

  try {
    const result = await getAuditLogs({
      actionType: filters.actionType || undefined,
      fromUtc: dateRange.value?.[0],
      toUtc: dateRange.value?.[1],
      pageNumber: pageNumber.value,
      pageSize: pageSize.value,
    });

    logs.value = result.items;
    totalCount.value = result.totalCount;
  } catch (error: any) {
    ElMessage.error(
      error?.response?.data?.message ??
        error?.message ??
        "Failed to load audit logs."
    );
  } finally {
    loading.value = false;
  }
}

function resetFilters() {
  filters.actionType = "";
  dateRange.value = null;
  loadLogs(1);
}

function openDetails(log: AuditLogItem) {
  selectedLog.value = log;
  detailsVisible.value = true;
}

function formatDateTime(value: string) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

function formatJson(value: string) {
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value || "{}";
  }
}
</script>

<style scoped>
.audit-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.audit-topbar {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.audit-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: #111827;
}

.audit-subtitle {
  margin: 6px 0 0;
  color: #6b7280;
  font-size: 14px;
}

.audit-filter-card,
.audit-table-card {
  border-radius: 20px;
}

.audit-filters {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.filter-item {
  width: 220px;
}

.filter-date {
  width: 360px;
}

.audit-pagination {
  margin-top: 20px;
  display: flex;
  justify-content: flex-end;
}

.detail-row {
  margin-bottom: 10px;
  color: #374151;
}

.detail-json-title {
  margin-top: 20px;
  margin-bottom: 10px;
  font-weight: 700;
  color: #111827;
}

.detail-json {
  margin: 0;
  padding: 16px;
  background: #f8fafc;
  border-radius: 12px;
  overflow-x: auto;
  font-size: 12px;
  line-height: 1.5;
}
</style>