import request from '../request'
import type { ProjectDeliverable } from '@/types'

export interface DeliverablePayload {
  id: string
  name: string
  version: string
  description: string
  status: string
  url: string
  coverImage: string
  techTags: string
  completedAt: string
  owner: string
  remark: string
  sortOrder: number
  isPublic: boolean
  projectId: string | null
}

export const deliverablesApi = {
  getAll(params?: { projectId?: string; status?: string }) {
    return request.get<ProjectDeliverable[]>('/ProjectDeliverables', { params })
  },
  getById(id: string) {
    return request.get<ProjectDeliverable>(`/ProjectDeliverables/${id}`)
  },
  create(data: DeliverablePayload) {
    return request.post('/ProjectDeliverables', data)
  },
  update(id: string, data: DeliverablePayload) {
    return request.put(`/ProjectDeliverables/${id}`, data)
  },
  delete(id: string) {
    return request.delete(`/ProjectDeliverables/${id}`)
  }
}
