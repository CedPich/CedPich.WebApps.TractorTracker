<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useMachineStore } from '@/stores/machineStore'
import TrackerMap from '@/components/TrackerMap.vue'
import WorkHoursChart from '@/components/WorkHoursChart.vue'

const store = useMachineStore()

const dateFrom = ref(new Date(Date.now() - 86400000 * 7))
const dateTo = ref(new Date())

function toIso(d: Date): string { return d.toISOString().substring(0, 10) }

async function refresh() {
  const from = toIso(dateFrom.value)
  const to = toIso(dateTo.value)
  await Promise.all([
    store.fetchCurrentPosition(),
    store.fetchHistory(dateFrom.value.toISOString(), dateTo.value.toISOString()),
    store.fetchWorkHours(from, to),
  ])
}

onMounted(refresh)
</script>

<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <h1>TractorTracker</h1>
      <div class="filters">
        <label>Du
          <input type="date" :value="toIso(dateFrom)" @change="e => { const v = (e.target as HTMLInputElement).value; if (v) { dateFrom = new Date(v); refresh() } }" />
        </label>
        <label>Au
          <input type="date" :value="toIso(dateTo)" @change="e => { const v = (e.target as HTMLInputElement).value; if (v) { dateTo = new Date(v); refresh() } }" />
        </label>
        <button @click="refresh">Actualiser</button>
      </div>
    </header>

    <div v-if="store.error" class="error">{{ store.error }}</div>

    <section class="map-section">
      <TrackerMap :current-position="store.currentPosition" :history="store.history" />
      <div v-if="store.currentPosition" class="position-info">
        <strong>Position actuelle</strong>
        <span>{{ store.currentPosition.latitude.toFixed(5) }}, {{ store.currentPosition.longitude.toFixed(5) }}</span>
        <span v-if="store.currentPosition.speedKmh != null">{{ store.currentPosition.speedKmh.toFixed(1) }} km/h</span>
        <span>{{ new Date(store.currentPosition.recordedAt).toLocaleString('fr-FR') }}</span>
      </div>
    </section>

    <section class="chart-section">
      <h2>Heures travaillées par jour</h2>
      <WorkHoursChart v-if="store.workHours.length" :data="store.workHours" />
      <p v-else-if="!store.loading">Aucune donnée sur la période.</p>
    </section>
  </div>
</template>

<style scoped>
.dashboard { display: flex; flex-direction: column; gap: 1.5rem; padding: 1rem; font-family: sans-serif; }
.dashboard-header { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; }
.dashboard-header h1 { margin: 0; font-size: 1.5rem; }
.filters { display: flex; gap: 0.75rem; align-items: center; flex-wrap: wrap; }
.filters label { display: flex; flex-direction: column; font-size: 0.85rem; }
.map-section { position: relative; height: 450px; border-radius: 8px; overflow: hidden; border: 1px solid #e5e7eb; }
.position-info { position: absolute; top: 0.75rem; right: 0.75rem; background: rgba(255,255,255,0.9); padding: 0.5rem 0.75rem; border-radius: 6px; display: flex; flex-direction: column; gap: 2px; font-size: 0.85rem; }
.chart-section { padding: 1rem; background: #f9fafb; border-radius: 8px; }
.error { color: #dc2626; padding: 0.5rem; background: #fee2e2; border-radius: 4px; }
</style>
