import request from '../request'
import type { MenuTreeNode } from '@/types'

export const menusApi = {
  getTree() { return request.get<MenuTreeNode[]>('/Menus/tree') },
  getAll() { return request.get<MenuTreeNode[]>('/Menus') },
  create(data: { name: string; path: string; icon?: string; parentId: number; sort: number }) {
    return request.post('/Menus', data)
  },
  update(id: number, data: { name: string; path: string; icon?: string; parentId: number; sort: number }) {
    return request.put(`/Menus/${id}`, data)
  },
  delete(id: number) { return request.delete(`/Menus/${id}`) }
}
