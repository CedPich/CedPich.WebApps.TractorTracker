import { defineStore } from 'pinia'
import { ref } from 'vue'
import { machineApi, type PositionDto, type DailyWorkHoursDto } from '@/api/machineApi'

export const useMachineStore = defineStore('machine', () => {
  const currentPosition = ref<PositionDto | null>(null)
  const history = ref<PositionDto[]>([])
  const workHours = ref<DailyWorkHoursDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchCurrentPosition() {
    try {
      currentPosition.value = await machineApi.getCurrentPosition()
    } catch {
      error.value = 'Impossible de récupérer la position actuelle.'
    }
  }

  async function fetchHistory(from: string, to: string) {
    loading.value = true
    try {
      history.value = await machineApi.getHistory(from, to)
    } catch {
      error.value = "Impossible de récupérer l'historique."
    } finally {
      loading.value = false
    }
  }

  async function fetchWorkHours(from: string, to: string, pauseThresholdMinutes = 15) {
    loading.value = true
    try {
      workHours.value = await machineApi.getWorkHours(from, to, pauseThresholdMinutes)
    } catch {
      error.value = 'Impossible de récupérer les heures travaillées.'
    } finally {
      loading.value = false
    }
  }

  return { currentPosition, history, workHours, loading, error, fetchCurrentPosition, fetchHistory, fetchWorkHours }
})
