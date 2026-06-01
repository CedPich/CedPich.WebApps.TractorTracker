<script setup lang="ts">
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js'
import type { DailyWorkHoursDto } from '@/api/machineApi'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

const props = defineProps<{ data: DailyWorkHoursDto[] }>()

const chartData = computed(() => ({
  labels: props.data.map(d => d.date),
  datasets: [{
    label: 'Heures travaillées',
    data: props.data.map(d => d.hours),
    backgroundColor: '#3b82f6',
    borderRadius: 4,
  }],
}))

const options = {
  responsive: true,
  plugins: { legend: { display: false } },
  scales: { y: { beginAtZero: true, title: { display: true, text: 'Heures' } } },
}
</script>

<template>
  <Bar :data="chartData" :options="options" />
</template>
