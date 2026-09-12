import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { LoginResponse, MenuTree } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const username = ref<string>(localStorage.getItem('username') || '')
  const roleName = ref<string>(localStorage.getItem('roleName') || '')
  const menus = ref<MenuTree[]>(JSON.parse(localStorage.getItem('menus') || '[]'))
  const permissions = ref<string[]>(JSON.parse(localStorage.getItem('permissions') || '[]'))

  function setAuth(res: LoginResponse) {
    token.value = res.token
    username.value = res.username
    roleName.value = res.roleName
    menus.value = res.menus
    permissions.value = res.permissions || []
    localStorage.setItem('token', res.token)
    localStorage.setItem('username', res.username)
    localStorage.setItem('roleName', res.roleName)
    localStorage.setItem('menus', JSON.stringify(res.menus))
    localStorage.setItem('permissions', JSON.stringify(res.permissions || []))
  }

  function hasPermission(code: string): boolean {
    return permissions.value.includes(code)
  }

  function logout() {
    token.value = ''
    username.value = ''
    roleName.value = ''
    menus.value = []
    permissions.value = []
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('roleName')
    localStorage.removeItem('menus')
    localStorage.removeItem('permissions')
  }

  return { token, username, roleName, menus, permissions, setAuth, hasPermission, logout }
})
