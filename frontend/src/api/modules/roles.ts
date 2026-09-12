import request from '../request'
import type { RoleItem, RoleMenuAssignRequest, RoleMenusResponse } from '@/types'

export const rolesApi = {
  getAll() { return request.get<RoleItem[]>('/Roles') },
  getById(id: number) { return request.get<RoleItem>(`/Roles/${id}`) },
  create(data: { name: string; description?: string }) { return request.post('/Roles', data) },
  update(id: number, data: { name: string; description?: string }) { return request.put(`/Roles/${id}`, data) },
  delete(id: number) { return request.delete(`/Roles/${id}`) },
  getMenus(roleId: number) { return request.get<RoleMenusResponse>(`/RoleMenus/${roleId}`) },
  assignMenus(data: RoleMenuAssignRequest) { return request.post('/RoleMenus', data) }
}
