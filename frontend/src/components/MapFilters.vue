<script setup lang="ts">
import { ref, computed, watch } from 'vue'

type Mode = 'day' | 'range'

const emit = defineEmits<{ change: [from: string, to: string] }>()

const mode = ref<Mode>('day')

// --- Mode journée ---
const today = new Date()
function isoDate(d: Date): string { return d.toISOString().substring(0, 10) }
function daysAgo(n: number): string { const d = new Date(today); d.setDate(d.getDate() - n); return isoDate(d) }

const selectedDate = ref(isoDate(today))
// Minutes depuis minuit, pas de 15, max 1440 (= 00:00 lendemain)
const startMinutes = ref(0)
const endMinutes = ref(1440)

const quickDays = [
  { label: "Aujourd'hui", value: isoDate(today) },
  { label: 'Hier', value: daysAgo(1) },
  { label: 'Avant-hier', value: daysAgo(2) },
]

// --- Mode intervalle ---
const rangeFrom = ref(daysAgo(7))
const rangeTo = ref(isoDate(today))

function localToIsoMinutes(dateStr: string, totalMinutes: number, isEnd: boolean): string {
  const [y, m, d] = dateStr.split('-').map(Number)
  if (totalMinutes >= 1440)
    return new Date(y, m - 1, d + 1, 0, 0, 0).toISOString()
  const h = Math.floor(totalMinutes / 60)
  const min = totalMinutes % 60
  return new Date(y, m - 1, d, h, min, isEnd ? 59 : 0).toISOString()
}

// --- Calcul from/to ---
const from = computed(() => {
  if (mode.value === 'day')
    return localToIsoMinutes(selectedDate.value, startMinutes.value, false)
  return localToIsoMinutes(rangeFrom.value, 0, false)
})

const to = computed(() => {
  if (mode.value === 'day')
    return localToIsoMinutes(selectedDate.value, endMinutes.value, true)
  return localToIsoMinutes(rangeTo.value, 1440, true)
})

watch([from, to], () => emit('change', from.value, to.value), { immediate: true })

function formatMinutes(m: number): string {
  if (m >= 1440) return '00:00'
  return `${String(Math.floor(m / 60)).padStart(2, '0')}:${String(m % 60).padStart(2, '0')}`
}
</script>

<template>
  <div class="map-filters">

    <div class="mode-tabs">
      <button :class="{ active: mode === 'day' }" @click="mode = 'day'">Journée</button>
      <button :class="{ active: mode === 'range' }" @click="mode = 'range'">Intervalle</button>
    </div>

    <!-- Mode journée -->
    <template v-if="mode === 'day'">
      <div class="quick-days">
        <button
          v-for="q in quickDays"
          :key="q.value"
          :class="{ active: selectedDate === q.value }"
          @click="selectedDate = q.value"
        >{{ q.label }}</button>
        <input type="date" v-model="selectedDate" :max="isoDate(today)" />
      </div>

      <div class="sliders">
        <div class="slider-row">
          <span class="slider-label">Début</span>
          <input type="range" v-model.number="startMinutes" min="0" :max="endMinutes" step="15" />
          <span class="slider-value">{{ formatMinutes(startMinutes) }}</span>
        </div>
        <div class="slider-row">
          <span class="slider-label">Fin</span>
          <input type="range" v-model.number="endMinutes" :min="startMinutes" max="1440" step="15" />
          <span class="slider-value">{{ formatMinutes(endMinutes) }}</span>
        </div>
      </div>
    </template>

    <!-- Mode intervalle -->
    <template v-else>
      <div class="range-inputs">
        <label>
          Du
          <input type="date" v-model="rangeFrom" :max="rangeTo" />
        </label>
        <label>
          Au
          <input type="date" v-model="rangeTo" :min="rangeFrom" :max="isoDate(today)" />
        </label>
      </div>
    </template>

  </div>
</template>

<style scoped>
.map-filters {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  padding: 0.75rem;
  background: #1f2937;
  border: 1px solid #374151;
  border-radius: 8px;
}

.mode-tabs {
  display: flex;
  gap: 0.25rem;
}
.mode-tabs button {
  flex: 1;
  padding: 0.3rem 0.75rem;
  border: 1px solid #374151;
  border-radius: 4px;
  background: transparent;
  color: #9ca3af;
  cursor: pointer;
  font-size: 0.85rem;
  transition: all 0.15s;
}
.mode-tabs button.active {
  background: #3b82f6;
  border-color: #3b82f6;
  color: #fff;
}

.quick-days {
  display: flex;
  gap: 0.35rem;
  flex-wrap: wrap;
  align-items: center;
}
.quick-days button {
  padding: 0.25rem 0.6rem;
  border: 1px solid #374151;
  border-radius: 4px;
  background: transparent;
  color: #d1d5db;
  cursor: pointer;
  font-size: 0.8rem;
  transition: all 0.15s;
}
.quick-days button.active {
  background: #374151;
  border-color: #6b7280;
  color: #f9fafb;
}
.quick-days button:hover:not(.active) { border-color: #6b7280; }

.quick-days input[type="date"] {
  background: #111827;
  color: #f3f4f6;
  border: 1px solid #374151;
  border-radius: 4px;
  padding: 0.2rem 0.4rem;
  font-size: 0.8rem;
}

.sliders { display: flex; flex-direction: column; gap: 0.5rem; }
.slider-row { display: flex; align-items: center; gap: 0.6rem; }
.slider-label { font-size: 0.8rem; color: #9ca3af; width: 2.5rem; }
.slider-value { font-size: 0.8rem; color: #f3f4f6; width: 3rem; text-align: right; font-variant-numeric: tabular-nums; }
input[type="range"] {
  flex: 1;
  accent-color: #3b82f6;
  cursor: pointer;
}

.range-inputs { display: flex; gap: 0.75rem; flex-wrap: wrap; }
.range-inputs label {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  font-size: 0.8rem;
  color: #9ca3af;
}
.range-inputs input[type="date"] {
  background: #111827;
  color: #f3f4f6;
  border: 1px solid #374151;
  border-radius: 4px;
  padding: 0.25rem 0.5rem;
  font-size: 0.85rem;
}
</style>
