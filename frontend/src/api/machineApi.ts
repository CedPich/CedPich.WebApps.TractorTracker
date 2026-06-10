import axios from 'axios'

// En production VITE_API_URL est vide → baseURL undefined → URLs relatives, nginx proxifie /api/
// En dev VITE_API_URL=http://localhost:5000
const http = axios.create({ baseURL: import.meta.env.VITE_API_URL || undefined })

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
  getCurrentPosition: () =>
    http.get<PositionDto>('/api/machine/position').then(r => r.data),

  getHistory: (from: string, to: string) =>
    http.get<PositionDto[]>('/api/machine/history', { params: { from, to } }).then(r => r.data),

  getWorkHours: (from: string, to: string) =>
    http.get<DailyWorkHoursDto[]>('/api/machine/work-hours', { params: { from, to } }).then(r => r.data),
}
