<template>
  <div class="portal-page">
    <div class="portal-shell">
      <div class="portal-brand">
        <div class="portal-badge">Organization Setup</div>
        <h1 class="portal-title">Create your organization workspace.</h1>
        <p class="portal-description">
          Register a tenant and create its first administrator account in one step.
        </p>
      </div>

      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Register organization</h2>
          <p class="portal-card-subtitle">
            This creates a tenant and its first administrator account.
          </p>
        </div>

        <el-form label-position="top" @submit.prevent>
          <el-form-item label="Organization name">
            <el-input
              v-model="form.tenantName"
              placeholder="e.g. Demo Church"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Administrator name">
            <el-input
              v-model="form.adminName"
              placeholder="e.g. Emily"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Administrator email">
            <el-input
              v-model="form.adminEmail"
              placeholder="admin@example.com"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Password">
            <el-input
              v-model="form.adminPassword"
              type="password"
              show-password
              placeholder="At least 8 characters"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Confirm password">
            <el-input
              v-model="form.confirmPassword"
              type="password"
              show-password
              placeholder="Re-enter password"
              size="large"
            />
          </el-form-item>

          <el-button
            type="primary"
            size="large"
            class="portal-primary-btn"
            :loading="loading"
            @click="onSubmit"
          >
            Create organization
          </el-button>

          <el-alert
            v-if="successMessage"
            :title="successMessage"
            type="success"
            show-icon
            class="portal-alert"
          />

          <el-alert
            v-if="errorMessage"
            :title="errorMessage"
            type="error"
            show-icon
            class="portal-alert"
          />
        </el-form>

        <div class="portal-info-box">
          In V1, registering an organization creates the tenant and its first administrator directly.
        </div>

        <div class="portal-footer-link">
          <router-link to="/login">Back to login</router-link>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { registerTenant } from "../api/tenant";

const router = useRouter();

const loading = ref(false);
const successMessage = ref("");
const errorMessage = ref("");

const form = reactive({
  tenantName: "",
  adminName: "",
  adminEmail: "",
  adminPassword: "",
  confirmPassword: "",
});

async function onSubmit() {
  errorMessage.value = "";
  successMessage.value = "";

  if (!form.tenantName.trim()) {
    errorMessage.value = "Organization name is required.";
    return;
  }

  if (!form.adminName.trim()) {
    errorMessage.value = "Administrator name is required.";
    return;
  }

  if (!form.adminEmail.trim()) {
    errorMessage.value = "Administrator email is required.";
    return;
  }

  if (!form.adminPassword.trim()) {
    errorMessage.value = "Password is required.";
    return;
  }

  if (!form.confirmPassword.trim()) {
    errorMessage.value = "Please confirm your password.";
    return;
  }

  if (form.adminPassword !== form.confirmPassword) {
    errorMessage.value = "Passwords do not match.";
    return;
  }

  loading.value = true;

  try {
    await registerTenant({
      tenantName: form.tenantName.trim(),
      adminName: form.adminName.trim(),
      adminEmail: form.adminEmail.trim(),
      adminPassword: form.adminPassword,
    });

    successMessage.value = "Organization registered successfully. Redirecting to sign in...";

    setTimeout(() => {
      router.push("/login");
    }, 1200);
  } catch (err: any) {
    errorMessage.value =
      err?.response?.data?.message ??
      err?.message ??
      "Failed to register organization.";
  } finally {
    loading.value = false;
  }
}
</script>