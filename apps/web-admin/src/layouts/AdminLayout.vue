<template>
  <el-container style="min-height: 100vh;">
    <el-aside width="220px">
      <div style="padding: 16px; font-weight: 700;">Web Admin</div>
      <el-menu :default-active="active" router>
        <el-menu-item index="/transactions">Transactions</el-menu-item>
        <el-menu-item index="/members">Members</el-menu-item>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header style="display:flex; align-items:center; justify-content:space-between;">
        <div>Tenant: (later)</div>
        <el-button text type="danger">Logout (later)</el-button>
      </el-header>

      <el-main>
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
const route = useRoute();
const active = computed(() => route.path);
</script>