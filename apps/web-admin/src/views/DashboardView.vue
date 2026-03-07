<template>
  <el-card shadow="never">
    <template #header>
      <div style="font-weight: 700;">Dashboard</div>
    </template>

    <p>TODO: admin dashboard overview</p>
    <p>Current tenant: {{ auth.currentTenantName || "Unknown tenant" }}</p>
    <p>Current role: {{ auth.currentMembership?.role || "Unknown role" }}</p>
  </el-card>
</template>

<script setup lang="ts">
import { useAuthStore } from "../stores/auth";

const auth = useAuthStore();
</script>