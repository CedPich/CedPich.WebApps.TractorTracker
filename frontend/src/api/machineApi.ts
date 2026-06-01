import axios from 'axios'

const http = axios.create({ baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000' })

export interface PositionDto {
  latitude: number
  longitude: number
  recordedAt: string
  speedKmh: number | null
  headingDegrees: number | null
}

export interface DailyWorkHoursDto {
  date: string
  hours: number
  positionCount: number
}

export const machineApi = {
  getCurrentPosition: (machineId: string) =>
    http.get<PositionDto>(`/api/machines/${machineId}/position`).then(r => r.data),

  getHistory: (machineId: string, from: string, to: string) =>
    http.get<PositionDto[]>(`/api/machines/${machineId}/history`, { params: { from, to } }).then(r => r.data),

  getWorkHours: (machineId: string, from: string, to: string) =>
    http.get<DailyWorkHoursDto[]>(`/api/machines/${machineId}/work-hours`, { params: { from, to } }).then(r => r.data),

  sync: (machineId: string) =>
    http.post(`/api/machines/${machineId}/sync`),
}
