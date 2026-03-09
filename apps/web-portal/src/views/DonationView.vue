<template>
    <div class="donation-page">
        <div class="donation-shell">

            <div class="page-header">
                <h1>Make a donation</h1>
                <p>Support your organization by making a donation.</p>
            </div>

            <el-card class="donation-card" shadow="never">

                <el-form :model="form" :rules="rules" ref="formRef" label-position="top">

                    <el-form-item label="Amount" prop="amount">
                        <el-input-number v-model="form.amount" :min="1" :step="10" controls-position="right"
                            style="width: 200px" />
                    </el-form-item>

                    <el-form-item label="Currency">
                        <el-input v-model="form.currency" disabled />
                    </el-form-item>

                    <el-form-item label="Description">
                        <el-input v-model="form.description" type="textarea" placeholder="Optional message" />
                    </el-form-item>

                    <div class="actions">
                        <el-button @click="goBack">
                            Cancel
                        </el-button>

                        <el-button type="primary" :loading="loading" @click="submitDonation">
                            Donate
                        </el-button>
                    </div>

                </el-form>

            </el-card>

        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { useRouter } from "vue-router"
import { ElMessage } from "element-plus"
import { createDonation } from "../api/transactions";

const router = useRouter()

const loading = ref(false)

const formRef = ref()

const form = ref({
    amount: 10,
    currency: "NZD",
    description: ""
})

const rules = {
    amount: [
        { required: true, message: "Amount is required", trigger: "blur" }
    ]
}

async function submitDonation() {
    await formRef.value.validate()

    loading.value = true

    try {

        await createDonation({
            title: "Donation",
            description: form.value.description,
            amount: form.value.amount,
            currency: form.value.currency,
        });

        ElMessage.success("Donation successful");

        router.push("/home");

    } catch (err) {
        ElMessage.error("Donation request failed")
    } finally {
        loading.value = false
    }
}

function goBack() {
    router.push("/")
}
</script>

<style scoped>
.donation-page {
    padding: 24px;
}

.donation-shell {
    max-width: 640px;
    margin: 0 auto;
}

.page-header {
    margin-bottom: 20px;
}

.page-header h1 {
    font-size: 28px;
    font-weight: 800;
}

.page-header p {
    margin-top: 6px;
    color: #6b7280;
}

.donation-card {
    border-radius: 20px;
}

.actions {
    margin-top: 20px;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}
</style>