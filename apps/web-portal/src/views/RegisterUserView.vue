<template>
  <div class="portal-page">
    <div class="portal-shell single">
      <el-card class="portal-card" shadow="never">
        <div class="portal-card-header">
          <h2 class="portal-card-title">Register user</h2>
          <p class="portal-card-subtitle">
            This entry is reserved for the next iteration of the system.
          </p>
        </div>

        <el-alert
          title="V1 currently focuses on organization registration and invitation-based membership."
          type="info"
          show-icon
        />

        <div class="portal-info-box">
          In V2, users will be able to register independently and join one or more organizations later.
        </div>

        <div class="portal-footer-link">
          <router-link to="/login">Back to login</router-link>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts"></script>