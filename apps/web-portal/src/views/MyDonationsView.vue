<script setup lang="ts">
import { onMounted, ref } from "vue"
import { getMyTransactions } from "../api/transactions"

const items = ref<any[]>([])
const loading = ref(true)

async function load() {
    loading.value = true

    const res = await getMyTransactions()

    items.value = res.items;

    loading.value = false
}

onMounted(load)
</script>

<template>

    <div style="max-width:900px;margin:auto">

        <h2>My Donations</h2>

        <div v-if="loading">Loading...</div>

        <table v-else border="1" cellpadding="6">

            <thead>
                <tr>
                    <th>Tenant</th>
                    <th>Amount</th>
                    <th>Status</th>
                    <th>Payment</th>
                    <th>Time</th>
                </tr>
            </thead>

            <tbody>
                <tr v-for="item in items" :key="item.publicId">
                    <td>{{ item.tenantName }}</td>
                    <td>{{ item.amount }} {{ item.currency }}</td>
                    <td>{{ item.status }}</td>
                    <td>{{ item.paymentStatus }}</td>
                    <td>{{ item.createdAtUtc }}</td>
                </tr>
            </tbody>

        </table>

    </div>

</template>