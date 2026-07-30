import request from '../request'
import type { WorkHistory, Achievement } from '@/types'

export const workHistoriesApi = {
  getAll() { return request.get<WorkHistory[]>('/WorkHistories') },
  create(data: { id: string; company: string; role: string; period: string; desc: string; isCurrent: boolean }) { return request.post('/WorkHistories', data) },
  update(id: string, data: { company: string; role: string; period: string; desc: string; isCurrent: boolean }) { return request.put(`/WorkHistories/${id}`, data) },
  delete(id: string) { return request.delete(`/WorkHistories/${id}`) }
}

export const achievementsApi = {
  getAll(workHistoryId?: string) { return request.get<Achievement[]>('/Achievements', { params: { workHistoryId } }) },
  create(data: { description: string; workHistoryId: string }) { return request.post('/Achievements', data) },
  update(id: number, data: { description: string; workHistoryId: string }) { return request.put(`/Achievements/${id}`, data) },
  delete(id: number) { return request.delete(`/Achievements/${id}`) }
}
