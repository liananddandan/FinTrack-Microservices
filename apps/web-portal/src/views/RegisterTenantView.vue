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
            This creates a tenant and the first administrator account.
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
            v-if="success"
            title="Organization registered successfully. Redirecting to sign in..."
            type="success"
            show-icon
            class="portal-alert"
          />

          <el-alert
            v-if="error"
            :title="error"
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
const success = ref(false);
const error = ref("");

const form = reactive({
  tenantName: "",
  adminName: "",
  adminEmail: "",
  adminPassword: "",
});

async function onSubmit() {
  error.value = "";
  success.value = false;

  if (!form.tenantName || !form.adminName || !form.adminEmail || !form.adminPassword) {
    error.value = "All fields are required.";
    return;
  }

  if (form.adminPassword.length < 8) {
    error.value = "Password must be at least 8 characters.";
    return;
  }

  loading.value = true;

  try {
    await registerTenant({
      tenantName: form.tenantName,
      adminName: form.adminName,
      adminEmail: form.adminEmail,
      adminPassword: form.adminPassword,
    });

    success.value = true;

    setTimeout(() => {
      router.push("/login");
    }, 1200);
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? "Failed to register organization.";
  } finally {
    loading.value = false;
  }
}
</script>