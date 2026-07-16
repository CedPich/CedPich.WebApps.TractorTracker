<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useMachineStore } from '@/stores/machineStore'
import TrackerMap from '@/components/TrackerMap.vue'
import WorkHoursChart from '@/components/WorkHoursChart.vue'
import MapFilters from '@/components/MapFilters.vue'
import { registerPushNotifications } from '@/composables/usePushNotifications'
import SettingsPanel from '@/components/SettingsPanel.vue'
import type { AppSettings } from '@/components/SettingsPanel.vue'
import DatePicker from 'primevue/datepicker'

const store = useMachineStore()

const mapFrom = ref('')
const mapTo = ref('')
const mapLive = ref(false)

const workFrom = ref(new Date(Date.now() - 86400000 * 30))
const workTo = ref(new Date())
const pauseThresholdMinutes = ref(15)
function toIso(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

// Remplit les jours sans données avec 0 heures
const filledWorkHours = computed(() => {
  const map = new Map(store.workHours.map(d => [d.date, d.hours]))
  const result: { date: string; hours: number }[] = []
  const cur = new Date(workFrom.value); cur.setHours(0, 0, 0, 0)
  const end = new Date(workTo.value); end.setHours(0, 0, 0, 0)
  while (cur <= end) {
    const key = toIso(cur)
    result.push({ date: key, hours: map.get(key) ?? 0 })
    cur.setDate(cur.getDate() + 1)
  }
  return result
})

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

    <div class="bottom-row">
      <SettingsPanel @change="onSettingsChange" />

      <section class="chart-section">
        <div class="chart-header">
          <h2>Heures travaillées par jour</h2>
          <div class="chart-filters">
            <label>Du
              <DatePicker v-model="workFrom" date-format="dd/mm/yy" :max-date="workTo" show-button-bar :manual-input="false" class="dp-compact" @update:model-value="refreshWorkHours" />
            </label>
            <label>Au
              <DatePicker v-model="workTo" date-format="dd/mm/yy" :min-date="workFrom" :max-date="new Date()" show-button-bar :manual-input="false" class="dp-compact" @update:model-value="refreshWorkHours" />
            </label>
          </div>
        </div>
        <div class="chart-wrapper">
          <WorkHoursChart v-if="filledWorkHours.length" :data="filledWorkHours" />
          <p v-else-if="!store.loading">Aucune donnée sur la période.</p>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 0.75rem;
  height: 100%;
  overflow: hidden;
}

.dashboard-header { border-bottom: 1px solid #374151; padding-bottom: 0.5rem; flex-shrink: 0; }
.dashboard-header h1 { margin: 0; font-size: 1.25rem; color: #f9fafb; }

.map-section { display: flex; flex-direction: column; gap: 0.5rem; flex: 1; min-height: 0; }
.map-wrapper { position: relative; flex: 1; min-height: 0; border-radius: 8px; overflow: hidden; border: 1px solid #374151; }

.position-info {
  position: absolute; top: 0.75rem; right: 0.75rem;
  background: rgba(17,24,39,0.88); color: #f3f4f6;
  padding: 0.5rem 0.75rem; border-radius: 6px;
  display: flex; flex-direction: column; gap: 2px;
  font-size: 0.85rem; border: 1px solid #374151;
}

/* Desktop : settings + graphique côte à côte */
.bottom-row {
  display: flex;
  gap: 0.75rem;
  align-items: stretch;
  flex-shrink: 0;
}
.bottom-row > :first-child { flex-shrink: 0; width: 220px; }
.bottom-row > .chart-section { flex: 1; min-width: 0; }

.chart-section { padding: 0.75rem 1rem; background: #1f2937; border-radius: 8px; border: 1px solid #374151; display: flex; flex-direction: column; }
.chart-header { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; margin-bottom: 0.4rem; flex-shrink: 0; }
.chart-header h2 { font-size: 0.9rem; color: #d1d5db; margin: 0; }
.chart-filters { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.chart-filters label { display: flex; align-items: center; gap: 0.4rem; font-size: 0.8rem; color: #9ca3af; }
:deep(.dp-compact .p-datepicker-input) {
  background: #111827; color: #f3f4f6;
  border: 1px solid #374151; border-radius: 4px;
  padding: 0.2rem 0.5rem; font-size: 0.8rem;
  width: 7rem;
}
.chart-wrapper { flex: 1; min-height: 120px; }

.chart-section p { color: #6b7280; font-size: 0.85rem; }
.error { color: #fca5a5; padding: 0.5rem; background: #450a0a; border-radius: 4px; flex-shrink: 0; }

/* Mobile : settings et graphique empilés */
@media (max-width: 640px) {
  .dashboard { overflow: auto; height: auto; }
  .map-wrapper { height: 300px; flex: none; }
  .bottom-row { flex-direction: column; }
  .bottom-row > :first-child { width: 100%; }
  .chart-wrapper { min-height: 200px; }
}
</style>
