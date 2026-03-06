<template>
  <div class="portal-page">
    <div class="portal-shell">
      <div class="portal-brand">
        <div class="portal-badge">FinTrack Portal</div>
        <h1 class="portal-title">Manage finance operations with clarity.</h1>
        <p class="portal-description">
          A multi-tenant finance platform for organizations, administrators, and members.
        </p>
      </div>

      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Sign in</h2>
          <p class="portal-card-subtitle">Access your account and continue to your workspace.</p>
        </div>

        <el-form label-position="top" @submit.prevent>
          <el-form-item label="Email">
            <el-input
              v-model="form.email"
              placeholder="you@example.com"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Password">
            <el-input
              v-model="form.password"
              type="password"
              show-password
              placeholder="Enter your password"
              size="large"
            />
          </el-form-item>

          <el-button
            type="primary"
            size="large"
            class="portal-primary-btn"
            :loading="loading"
            @click="onLogin"
          >
            Sign in
          </el-button>

          <el-alert
            v-if="error"
            :title="error"
            type="error"
            show-icon
            class="portal-alert"
          />
        </el-form>

        <div class="portal-divider"></div>

        <div class="portal-links">
          <router-link to="/register-tenant">Create organization</router-link>
          <router-link to="/register-user">Register as individual user</router-link>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { login } from "../api/account";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const auth = useAuthStore();

const loading = ref(false);
const error = ref("");

const form = reactive({
  email: "",
  password: "",
});

async function onLogin() {
  error.value = "";

  if (!form.email || !form.password) {
    error.value = "Email and password are required.";
    return;
  }

  loading.value = true;

  try {
    const result = await login({
      email: form.email,
      password: form.password,
    });

    auth.setAuth({
      accessToken: result.accessToken,
      refreshToken: result.refreshToken,
      userEmail: form.email,
    });

    await router.push("/waiting-membership");
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? "Login failed.";
  } finally {
    loading.value = false;
  }
}
</script>