<template>
  <div class="portal-page">
    <div class="portal-shell">
      <div class="portal-brand">
        <div class="portal-badge">FinTrack Portal</div>
        <h1 class="portal-title">Create your personal account.</h1>
        <p class="portal-description">
          Register as an individual user first. Tenant membership can be added later by invitation.
        </p>
      </div>

      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Register user</h2>
          <p class="portal-card-subtitle">
            This creates a user account only. It does not create or join any organization yet.
          </p>
        </div>

        <el-form label-position="top" @submit.prevent>
          <el-form-item label="User name">
            <el-input
              v-model="form.userName"
              placeholder="e.g. Chen Li"
              size="large"
            />
          </el-form-item>

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

          <el-form-item label="Confirm password">
            <el-input
              v-model="form.confirmPassword"
              type="password"
              show-password
              placeholder="Re-enter your password"
              size="large"
            />
          </el-form-item>

          <el-button
            type="primary"
            size="large"
            class="portal-primary-btn"
            :loading="loading"
            @click="onRegister"
          >
            Create account
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

        <div class="portal-divider"></div>

        <div class="portal-links">
          <router-link to="/login">Back to login</router-link>
          <router-link to="/register-tenant">Create organization instead</router-link>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { registerUser } from "../api/account";

const router = useRouter();

const loading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");

const form = reactive({
  userName: "",
  email: "",
  password: "",
  confirmPassword: "",
});

async function onRegister() {
  errorMessage.value = "";
  successMessage.value = "";

  if (!form.userName.trim()) {
    errorMessage.value = "User name is required.";
    return;
  }

  if (!form.email.trim()) {
    errorMessage.value = "Email is required.";
    return;
  }

  if (!form.password.trim()) {
    errorMessage.value = "Password is required.";
    return;
  }

  if (!form.confirmPassword.trim()) {
    errorMessage.value = "Please confirm your password.";
    return;
  }

  if (form.password !== form.confirmPassword) {
    errorMessage.value = "Passwords do not match.";
    return;
  }

  loading.value = true;

  try {
    await registerUser({
      userName: form.userName.trim(),
      email: form.email.trim(),
      password: form.password,
    });

    successMessage.value = "User registered successfully. Redirecting to login...";

    setTimeout(() => {
      router.push("/login");
    }, 1200);
  } catch (err: any) {
    errorMessage.value =
      err?.response?.data?.message ??
      err?.message ??
      "User registration failed.";
  } finally {
    loading.value = false;
  }
}
</script>