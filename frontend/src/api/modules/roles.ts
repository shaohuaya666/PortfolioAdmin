import request from '../request'
import type { Role, RoleMenuAssign, RoleMenusResponse } from '@/types'

export const rolesApi = {
  getAll() { return request.get<Role[]>('/Roles') },
  getById(id: number) { return request.get<Role>(`/Roles/${id}`) },
  create(data: { name: string; description?: string }) { return request.post('/Roles', data) },
  update(id: number, data: { name: string; description?: string }) { return request.put(`/Roles/${id}`, data) },
  delete(id: number) { return request.delete(`/Roles/${id}`) },
  getMenus(roleId: number) { return request.get<RoleMenusResponse>(`/RoleMenus/${roleId}`) },
  assignMenus(data: RoleMenuAssign) { return request.post('/RoleMenus', data) }
}
