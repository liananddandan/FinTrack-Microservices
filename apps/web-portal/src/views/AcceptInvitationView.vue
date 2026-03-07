<template>
  <div class="portal-page">
    <div class="portal-shell">
      <div class="portal-brand">
        <div class="portal-badge">FinTrack Portal</div>
        <h1 class="portal-title">Accept your invitation</h1>
        <p class="portal-description">
          Review the invitation details and confirm whether you want to join this organization.
        </p>
      </div>

      <el-card class="portal-card" shadow="never">
        <el-skeleton v-if="loading" :rows="6" animated />

        <template v-else>
          <el-alert
            v-if="errorMessage"
            :title="errorMessage"
            type="error"
            show-icon
            class="portal-alert"
          />

          <template v-else-if="invitation">
            <div class="portal-card-header">
              <h2 class="portal-card-title">Invitation details</h2>
              <p class="portal-card-subtitle">
                Please confirm the information before accepting.
              </p>
            </div>

            <div class="invitation-summary">
              <div class="summary-row">
                <span class="summary-label">Organization</span>
                <span class="summary-value">{{ invitation.tenantName }}</span>
              </div>

              <div class="summary-row">
                <span class="summary-label">Email</span>
                <span class="summary-value">{{ invitation.email }}</span>
              </div>

              <div class="summary-row">
                <span class="summary-label">Role</span>
                <span class="summary-value">
                  <el-tag type="primary" effect="light" round>
                    {{ invitation.role }}
                  </el-tag>
                </span>
              </div>

              <div class="summary-row">
                <span class="summary-label">Status</span>
                <span class="summary-value">
                  <el-tag
                    :type="invitation.status === 'Pending' ? 'warning' : 'success'"
                    effect="light"
                    round
                  >
                    {{ invitation.status }}
                  </el-tag>
                </span>
              </div>

              <div class="summary-row">
                <span class="summary-label">Expires</span>
                <span class="summary-value">{{ formatDate(invitation.expiredAt) }}</span>
              </div>
            </div>

            <el-alert
              v-if="successMessage"
              :title="successMessage"
              type="success"
              show-icon
              class="portal-alert"
            />

            <div class="portal-actions">
              <el-button @click="goLogin">Back to login</el-button>
              <el-button
                type="primary"
                :loading="submitting"
                :disabled="!canAccept"
                @click="handleAccept"
              >
                Accept invitation
              </el-button>
            </div>
          </template>
        </template>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  acceptTenantInvitation,
  resolveTenantInvitation,
  type ResolveTenantInvitationResult,
} from "../api/invitation";

const route = useRoute();
const router = useRouter();

const loading = ref(true);
const submitting = ref(false);
const errorMessage = ref("");
const successMessage = ref("");
const invitation = ref<ResolveTenantInvitationResult | null>(null);

const token = computed(() => {
  const value = route.query.token;
  return typeof value === "string" ? value : "";
});

const canAccept = computed(() => {
  return !!invitation.value &&
    invitation.value.status === "Pending" &&
    !successMessage.value;
});

onMounted(async () => {
  await loadInvitation();
});

async function loadInvitation() {
  loading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  if (!token.value) {
    errorMessage.value = "Invitation token is missing.";
    loading.value = false;
    return;
  }

  try {
    invitation.value = await resolveTenantInvitation(token.value);
  } catch (error: any) {
    console.error("Failed to resolve invitation:", error);
    errorMessage.value =
      error?.response?.data?.message ??
      error?.message ??
      "Failed to load invitation.";
  } finally {
    loading.value = false;
  }
}

async function handleAccept() {
  if (!token.value) {
    errorMessage.value = "Invitation token is missing.";
    return;
  }

  submitting.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    await acceptTenantInvitation(token.value);

    successMessage.value =
      "Invitation accepted successfully. You can now sign in.";

    if (invitation.value) {
      invitation.value = {
        ...invitation.value,
        status: "Accepted",
      };
    }
  } catch (error: any) {
    console.error("Failed to accept invitation:", error);
    errorMessage.value =
      error?.response?.data?.message ??
      error?.message ??
      "Failed to accept invitation.";
  } finally {
    submitting.value = false;
  }
}

function goLogin() {
  router.push("/login");
}

function formatDate(value: string) {
  if (!value) return "-";

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return date.toLocaleString();
}
</script>

<style scoped>
.invitation-summary {
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