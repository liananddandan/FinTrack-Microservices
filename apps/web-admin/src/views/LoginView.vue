<template>
  <div style="max-width: 360px; margin: 80px auto;">
    <el-card>
      <h3>Admin Login</h3>
      <el-form label-position="top" @submit.prevent>
        <el-form-item label="Email">
          <el-input v-model="email" placeholder="you@example.com" />
        </el-form-item>
        <el-form-item label="Password">
          <el-input v-model="password" type="password" placeholder="******" show-password />
        </el-form-item>
        <el-button type="primary" style="width: 100%;" :loading="loading" @click="onLogin">
          Sign in
        </el-button>
        <el-alert v-if="error" :title="error" type="error" show-icon style="margin-top:12px;" />
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { login } from "../api/identity";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const auth = useAuthStore();

const email = ref("");
const password = ref("");
const loading = ref(false);
const error = ref<string | null>(null);

async function onLogin() {
  error.value = null;
  loading.value = true;
  try {
    const res = await login({ email: email.value, password: password.value });
    auth.setAccessToken(res.accessToken);
    if (res.tenantId) auth.setTenantId(res.tenantId);
    await router.push("/transactions");
  } catch (e: any) {
    error.value = e?.response?.data?.message ?? "Login failed";
  } finally {
    loading.value = false;
  }
}
</script>