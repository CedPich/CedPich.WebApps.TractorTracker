<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import DatePicker from 'primevue/datepicker'

type Mode = 'day' | 'range'

const emit = defineEmits<{ change: [from: string, to: string]; liveChange: [live: boolean] }>()

const mode = ref<Mode>('day')

// --- Mode journée ---
const today = new Date()
function isoDate(d: Date): string { return d.toISOString().substring(0, 10) }
function dateAgo(n: number): Date { const d = new Date(today); d.setDate(d.getDate() - n); return d }

const todayStr = isoDate(today)

const selectedDateObj = ref<Date | null>(new Date(today))
const selectedDateStr = computed(() => selectedDateObj.value ? isoDate(selectedDateObj.value) : todayStr)

// Minutes depuis minuit, pas de 15, max 1440 (= 00:00 lendemain)
const startMinutes = ref(0)
const endMinutes = ref(1440)

const quickDays = [
  { label: "Aujourd'hui", date: new Date(today), str: todayStr },
  { label: 'Hier', date: dateAgo(1), str: isoDate(dateAgo(1)) },
  { label: 'Avant-hier', date: dateAgo(2), str: isoDate(dateAgo(2)) },
]

// --- Mode live ---
const liveMode = ref(false)
const isToday = computed(() => mode.value === 'day' && selectedDateStr.value === todayStr)
let liveInterval: ReturnType<typeof setInterval> | null = null

function stopLive() {
  if (liveInterval !== null) {
    clearInterval(liveInterval)
    liveInterval = null
  }
}

function toggleLive() {
  liveMode.value = !liveMode.value
  emit('liveChange', liveMode.value)
  if (liveMode.value) {
    liveInterval = setInterval(() => emit('change', from.value, new Date().toISOString()), 30_000)
  } else {
    stopLive()
  }
}

// Désactiver le live si on quitte aujourd'hui ou le mode journée
watch(isToday, (val) => {
  if (!val && liveMode.value) {
    liveMode.value = false
    stopLive()
    emit('liveChange', false)
  }
})

onUnmounted(stopLive)

// --- Mode intervalle ---
const rangeFromObj = ref<Date | null>(dateAgo(7))
const rangeToObj = ref<Date | null>(new Date(today))
const rangeFromStr = computed(() => rangeFromObj.value ? isoDate(rangeFromObj.value) : isoDate(dateAgo(7)))
const rangeToStr = computed(() => rangeToObj.value ? isoDate(rangeToObj.value) : todayStr)

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
    return localToIsoMinutes(selectedDateStr.value, startMinutes.value, false)
  return localToIsoMinutes(rangeFromStr.value, 0, false)
})

const to = computed(() => {
  if (liveMode.value) return new Date().toISOString()
  if (mode.value === 'day')
    return localToIsoMinutes(selectedDateStr.value, endMinutes.value, true)
  return localToIsoMinutes(rangeToStr.value, 1440, true)
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
          :key="q.str"
          :class="{ active: selectedDateStr === q.str }"
          @click="selectedDateObj = q.date"
        >{{ q.label }}</button>
        <DatePicker
          v-model="selectedDateObj"
          date-format="dd/mm/yy"
          :max-date="today"
          show-button-bar
          :manual-input="false"
          class="dp-compact"
        />

        <button v-if="isToday" class="live-btn" :class="{ active: liveMode }" @click="toggleLive">
          <span class="live-dot" :class="{ pulse: liveMode }" />
          Live
        </button>
      </div>

      <div class="sliders" :class="{ dimmed: liveMode }">
        <div class="slider-row">
          <span class="slider-label">Début</span>
          <input type="range" v-model.number="startMinutes" min="0" :max="endMinutes" step="15" :disabled="liveMode" />
          <span class="slider-value">{{ formatMinutes(startMinutes) }}</span>
        </div>
        <div class="slider-row">
          <span class="slider-label">Fin</span>
          <input type="range" v-model.number="endMinutes" :min="startMinutes" max="1440" step="15" :disabled="liveMode" />
          <span class="slider-value">{{ liveMode ? 'maintenant' : formatMinutes(endMinutes) }}</span>
        </div>
      </div>
    </template>

    <!-- Mode intervalle -->
    <template v-else>
      <div class="range-inputs">
        <label>Du
          <DatePicker
            v-model="rangeFromObj"
            date-format="dd/mm/yy"
            :max-date="rangeToObj ?? today"
            show-button-bar
            :manual-input="false"
            class="dp-compact"
          />
        </label>
        <label>Au
          <DatePicker
            v-model="rangeToObj"
            date-format="dd/mm/yy"
            :min-date="rangeFromObj ?? undefined"
            :max-date="today"
            show-button-bar
            :manual-input="false"
            class="dp-compact"
          />
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

:deep(.dp-compact .p-datepicker-input) {
  background: #111827;
  color: #f3f4f6;
  border: 1px solid #374151;
  border-radius: 4px;
  padding: 0.2rem 0.5rem;
  font-size: 0.8rem;
  width: 7rem;
}

.live-btn {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  margin-left: auto;
  padding: 0.25rem 0.65rem;
  border: 1px solid #374151;
  border-radius: 4px;
  background: transparent;
  color: #9ca3af;
  cursor: pointer;
  font-size: 0.8rem;
  transition: all 0.15s;
}
.live-btn.active {
  background: #052e16;
  border-color: #16a34a;
  color: #4ade80;
}

.live-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #6b7280;
  flex-shrink: 0;
}
.live-dot.pulse {
  background: #4ade80;
  animation: pulse 1.5s ease-in-out infinite;
}
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

.sliders { display: flex; flex-direction: column; gap: 0.5rem; transition: opacity 0.2s; }
.sliders.dimmed { opacity: 0.4; pointer-events: none; }
.slider-row { display: flex; align-items: center; gap: 0.6rem; }
.slider-label { font-size: 0.8rem; color: #9ca3af; width: 2.5rem; }
.slider-value { font-size: 0.8rem; color: #f3f4f6; width: 4.5rem; text-align: right; font-variant-numeric: tabular-nums; }
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
</style>
