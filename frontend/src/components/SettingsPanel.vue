<script setup lang="ts">
import { ref } from 'vue'

const emit = defineEmits<{ change: [settings: AppSettings] }>()

export interface AppSettings {
  pauseThresholdMinutes: number
}

const pauseThreshold = ref(15)

function apply() {
  emit('change', { pauseThresholdMinutes: pauseThreshold.value })
}
</script>

<template>
  <div class="settings-panel">
    <h3>Paramètres</h3>
    <div class="setting-row">
      <label for="pause-threshold">Seuil de pause (heures travaillées)</label>
      <div class="setting-control">
        <input
          id="pause-threshold"
          type="number"
          v-model.number="pauseThreshold"
          min="1"
          max="120"
          @change="apply"
        />
        <span class="unit">min</span>
      </div>
      <p class="hint">
        Un écart supérieur à {{ pauseThreshold }} min entre deux points est considéré comme une pause et exclu du calcul.
      </p>
    </div>
  </div>
</template>

<style scoped>
.settings-panel {
  padding: 1rem;
  background: #1f2937;
  border: 1px solid #374151;
  border-radius: 8px;
}
.settings-panel h3 {
  margin: 0 0 0.75rem;
  font-size: 0.9rem;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.setting-row { display: flex; flex-direction: column; gap: 0.35rem; }
label { font-size: 0.85rem; color: #d1d5db; }
.setting-control { display: flex; align-items: center; gap: 0.4rem; }
input[type="number"] {
  width: 5rem;
  background: #111827;
  color: #f3f4f6;
  border: 1px solid #374151;
  border-radius: 4px;
  padding: 0.25rem 0.5rem;
  font-size: 0.85rem;
  text-align: right;
}
input[type="number"]:focus { outline: none; border-color: #3b82f6; }
.unit { font-size: 0.8rem; color: #6b7280; }
.hint { font-size: 0.75rem; color: #6b7280; margin: 0; }
</style>
