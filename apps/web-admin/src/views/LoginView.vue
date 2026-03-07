<template>
  <div class="login-page">
    <el-card class="login-card" shadow="never">
      <div class="login-header">
        <h2>Admin Sign In</h2>
        <p>Sign in with an administrator account.</p>
      </div>

      <el-form label-position="top" @submit.prevent>
        <el-form-item label="Email">
          <el-input v-model="form.email" size="large" />
        </el-form-item>

        <el-form-item label="Password">
          <el-input
            v-model="form.password"
            type="password"
            show-password
            size="large"
          />
        </el-form-item>

        <el-button
          type="primary"
          size="large"
          style="width: 100%;"
          :loading="loading"
          @click="onLogin"
        >
          Sign in
        </el-button>

        <el-alert
          v-if="errorMessage"
          :title="errorMessage"
          type="error"
          show-icon
          style="margin-top: 16px;"
        />
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { login, getCurrentUser } from "../api/account";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const auth = useAuthStore();

const loading = ref(false);
const errorMessage = ref("");

const form = reactive({
  email: "",
  password: "",
});

async function onLogin() {
  errorMessage.value = "";

  if (!form.email.trim()) {
    errorMessage.value = "Email is required.";
    return;
  }

  if (!form.password.trim()) {
    errorMessage.value = "Password is required.";
    return;
  }

  loading.value = true;

  try {
    const result = await login({
      email: form.email.trim(),
      password: form.password,
    });

    auth.setTokens(
      result.tokens.accessToken,
      result.tokens.refreshToken
    );

    auth.setMemberships(result.memberships);

    const profile = await getCurrentUser();
    auth.setProfile(profile);

    if (!auth.isAdmin) {
      auth.logout();
      errorMessage.value = "This account is not an administrator.";
      return;
    }

    await router.push("/dashboard");
  } catch (err: any) {
    errorMessage.value =
      err?.response?.data?.message ??
      err?.message ??
      "Login failed.";
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f7fb;
  padding: 24px;
}

.login-card {
  width: 100%;
  max-width: 420px;
  border-radius: 20px;
}

.login-header {
  margin-bottom: 16px;
}

.login-header h2 {
  margin: 0 0 8px;
}

.login-header p {
  margin: 0;
  color: #6b7280;
}
</style>