import request from '../request'
import type { SkillCategory, TagInfo } from '@/types'

export const skillCategoriesApi = {
  getAll() { return request.get<SkillCategory[]>('/SkillCategories') },
  create(data: { title: string; themeColor: string }) { return request.post('/SkillCategories', data) },
  update(id: number, data: { title: string; themeColor: string }) { return request.put(`/SkillCategories/${id}`, data) },
  delete(id: number) { return request.delete(`/SkillCategories/${id}`) }
}

export const tagsApi = {
  getAll(categoryId?: number) { return request.get<TagInfo[]>('/Tags', { params: { categoryId } }) },
  create(data: { name: string; isCore: boolean; skillCategoryId: number }) { return request.post('/Tags', data) },
  update(id: number, data: { name: string; isCore: boolean; skillCategoryId: number }) { return request.put(`/Tags/${id}`, data) },
  delete(id: number) { return request.delete(`/Tags/${id}`) }
}
