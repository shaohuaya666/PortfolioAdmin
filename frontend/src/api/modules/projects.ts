import request from '../request'
import type { CompactProject, ProjectSkill } from '@/types'

export const projectsApi = {
  getAll() { return request.get<CompactProject[]>('/CompactProjects') },
  getById(id: string) { return request.get<CompactProject>(`/CompactProjects/${id}`) },
  create(data: { id: string; title: string; type: string; year: string; desc: string }) { return request.post('/CompactProjects', data) },
  update(id: string, data: { title: string; type: string; year: string; desc: string }) { return request.put(`/CompactProjects/${id}`, data) },
  delete(id: string) { return request.delete(`/CompactProjects/${id}`) }
}

export const projectSkillsApi = {
  getAll(projectId?: string) { return request.get<ProjectSkill[]>('/ProjectSkills', { params: { projectId } }) },
  create(data: { name: string; projectId: string }) { return request.post('/ProjectSkills', data) },
  update(id: number, data: { name: string; projectId: string }) { return request.put(`/ProjectSkills/${id}`, data) },
  delete(id: number) { return request.delete(`/ProjectSkills/${id}`) }
}
