<template>
  <div class="portal-page">
    <div class="portal-shell single">
      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Membership status</h2>
          <p class="portal-card-subtitle">
            Your account is signed in successfully.
          </p>
        </div>

        <div style="margin-bottom: 12px;">
          <strong>Signed in as:</strong> {{ auth.userEmail || "Unknown user" }}
        </div>

        <el-alert
          title="In V1, users without an active tenant workflow remain in the portal until organization-specific flow is completed."
          type="info"
          show-icon
        />

        <div class="portal-actions">
          <el-button @click="goLogin">Back to login</el-button>
          <el-button type="danger" plain @click="logout">Logout</el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";

const router = useRouter();
const auth = useAuthStore();

function logout() {
  auth.logout();
  router.push("/login");
}

function goLogin() {
  router.push("/login");
}
</script>