<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useMachineStore } from '@/stores/machineStore'
import TrackerMap from '@/components/TrackerMap.vue'
import WorkHoursChart from '@/components/WorkHoursChart.vue'
import MapFilters from '@/components/MapFilters.vue'
import { registerPushNotifications } from '@/composables/usePushNotifications'
import SettingsPanel from '@/components/SettingsPanel.vue'
import type { AppSettings } from '@/components/SettingsPanel.vue'

const store = useMachineStore()

const mapFrom = ref('')
const mapTo = ref('')
const mapLive = ref(false)

const workFrom = ref(new Date(Date.now() - 86400000 * 30))
const workTo = ref(new Date())
const pauseThresholdMinutes = ref(15)
function toIso(d: Date): string { return d.toISOString().substring(0, 10) }

async function onMapFilterChange(from: string, to: string) {
  mapFrom.value = from
  mapTo.value = to
  await Promise.all([
    store.fetchCurrentPosition(),
    store.fetchHistory(from, to),
  ])
}

async function refreshWorkHours() {
  await store.fetchWorkHours(toIso(workFrom.value), toIso(workTo.value), pauseThresholdMinutes.value)
}

function onSettingsChange(s: AppSettings) {
  pauseThresholdMinutes.value = s.pauseThresholdMinutes
  refreshWorkHours()
}

onMounted(() => {
  refreshWorkHours()
  registerPushNotifications()
})
</script>

<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <h1>TractorTracker</h1>
    </header>

    <div v-if="store.error" class="error">{{ store.error }}</div>

    <section class="map-section">
      <MapFilters @change="onMapFilterChange" @live-change="live => mapLive = live" />
      <div class="map-wrapper">
        <TrackerMap :current-position="store.currentPosition" :history="store.history" :live="mapLive" />
        <div v-if="store.currentPosition" class="position-info">
          <strong>Position actuelle</strong>
          <span>{{ store.currentPosition.latitude.toFixed(5) }}, {{ store.currentPosition.longitude.toFixed(5) }}</span>
          <span>{{ new Date(store.currentPosition.recordedAt).toLocaleString('fr-FR') }}</span>
        </div>
      </div>
    </section>

    <SettingsPanel @change="onSettingsChange" />

    <section class="chart-section">
      <div class="chart-header">
        <h2>Heures travaillées par jour</h2>
        <div class="chart-filters">
          <label>Du <input type="date" :value="toIso(workFrom)" @change="e => { const v = (e.target as HTMLInputElement).value; if (v) { workFrom = new Date(v); refreshWorkHours() } }" /></label>
          <label>Au <input type="date" :value="toIso(workTo)" @change="e => { const v = (e.target as HTMLInputElement).value; if (v) { workTo = new Date(v); refreshWorkHours() } }" /></label>
        </div>
      </div>
      <WorkHoursChart v-if="store.workHours.length" :data="store.workHours" />
      <p v-else-if="!store.loading">Aucune donnée sur la période.</p>
    </section>
  </div>
</template>

<style scoped>
.dashboard { display: flex; flex-direction: column; gap: 1.5rem; padding: 1rem; }

.dashboard-header { border-bottom: 1px solid #374151; padding-bottom: 0.75rem; }
.dashboard-header h1 { margin: 0; font-size: 1.5rem; color: #f9fafb; }

.map-section { display: flex; flex-direction: column; gap: 0.75rem; }
.map-wrapper { position: relative; height: 450px; border-radius: 8px; overflow: hidden; border: 1px solid #374151; }

.position-info {
  position: absolute; top: 0.75rem; right: 0.75rem;
  background: rgba(17,24,39,0.88); color: #f3f4f6;
  padding: 0.5rem 0.75rem; border-radius: 6px;
  display: flex; flex-direction: column; gap: 2px;
  font-size: 0.85rem; border: 1px solid #374151;
}

.chart-section { padding: 1rem; background: #1f2937; border-radius: 8px; border: 1px solid #374151; }
.chart-header { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; margin-bottom: 0.75rem; }
.chart-header h2 { font-size: 1rem; color: #d1d5db; margin: 0; }
.chart-filters { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.chart-filters label { display: flex; align-items: center; gap: 0.4rem; font-size: 0.8rem; color: #9ca3af; }
.chart-filters input[type="date"] {
  background: #111827; color: #f3f4f6;
  border: 1px solid #374151; border-radius: 4px;
  padding: 0.2rem 0.4rem; font-size: 0.8rem;
}

.chart-section p { color: #6b7280; }
.error { color: #fca5a5; padding: 0.5rem; background: #450a0a; border-radius: 4px; }
</style>
