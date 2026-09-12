import request from '../request'
import type { MenuItem } from '@/types'

export interface MenuCreateRequest {
  name: string
  path: string
  icon?: string
  parentId: number
  sort: number
  type?: string
  permissionCode?: string
}

export const menusApi = {
  getTree() { return request.get<MenuItem[]>('/Menus/tree') },
  getAll() { return request.get<MenuItem[]>('/Menus') },
  create(data: MenuCreateRequest) {
    return request.post('/Menus', data)
  },
  update(id: number, data: MenuCreateRequest) {
    return request.put(`/Menus/${id}`, data)
  },
  delete(id: number) { return request.delete(`/Menus/${id}`) }
}
