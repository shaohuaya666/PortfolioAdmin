import request from '../request'
import type { UserDto } from '@/types'

export const usersApi = {
  getAll() { return request.get<UserDto[]>('/Users') },
  getById(id: number) { return request.get<UserDto>(`/Users/${id}`) },
  create(data: { username: string; password: string; roleId: number }) { return request.post('/Users', data) },
  update(id: number, data: { username: string; roleId: number }) { return request.put(`/Users/${id}`, data) },
  resetPassword(id: number, data: { newPassword: string }) { return request.post(`/Users/${id}/reset-password`, data) },
  delete(id: number) { return request.delete(`/Users/${id}`) }
}
