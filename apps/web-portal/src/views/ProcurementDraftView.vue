<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import { createProcurement } from "../api/transactions";

const router = useRouter();

const loading = ref(false);
const form = ref({
  title: "",
  description: "",
  amount: 0,
  currency: "NZD",
});

async function submit() {
  loading.value = true;

  try {
    const result = await createProcurement({
      title: form.value.title,
      description: form.value.description,
      amount: form.value.amount,
      currency: form.value.currency,
    });

    ElMessage.success("Procurement draft created.");
    router.push(`/transactions/${result.transactionPublicId}`);
  } catch (err: any) {
    ElMessage.error(err.message || "Failed to create procurement.");
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="page">
    <el-card class="card" shadow="never">
      <template #header>
        <div class="title">New Procurement Draft</div>
      </template>

      <el-form label-position="top">
        <el-form-item label="Title">
          <el-input v-model="form.title" />
        </el-form-item>

        <el-form-item label="Description">
          <el-input v-model="form.description" type="textarea" />
        </el-form-item>

        <el-form-item label="Amount">
          <el-input-number v-model="form.amount" :min="0" />
        </el-form-item>

        <el-form-item label="Currency">
          <el-input v-model="form.currency" />
        </el-form-item>

        <el-button type="primary" :loading="loading" @click="submit">
          Save Draft
        </el-button>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.page { padding: 24px; }
.card { max-width: 720px; margin: 0 auto; border-radius: 20px; }
.title { font-size: 20px; font-weight: 800; }
</style>