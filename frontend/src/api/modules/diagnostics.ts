import request from '../request'
import type { SkillDiagnostic } from '@/types'

export const diagnosticsApi = {
  getAll() { return request.get<SkillDiagnostic[]>('/SkillDiagnostics') },
  create(data: { tagName: string; desc: string; stat: string; status: string }) { return request.post('/SkillDiagnostics', data) },
  update(id: number, data: { tagName: string; desc: string; stat: string; status: string }) { return request.put(`/SkillDiagnostics/${id}`, data) },
  delete(id: number) { return request.delete(`/SkillDiagnostics/${id}`) }
}
