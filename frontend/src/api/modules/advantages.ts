import request from '../request'
import type { Advantage } from '@/types'

export const advantagesApi = {
  getAll() { return request.get<Advantage[]>('/Advantages') },
  getById(id: string) { return request.get<Advantage>(`/Advantages/${id}`) },
  create(data: Omit<Advantage, 'id'> & { id: string }) { return request.post('/Advantages', data) },
  update(id: string, data: Omit<Advantage, 'id'>) { return request.put(`/Advantages/${id}`, data) },
  delete(id: string) { return request.delete(`/Advantages/${id}`) }
}
