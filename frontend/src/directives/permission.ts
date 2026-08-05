import type { DirectiveBinding } from 'vue'
import { useAuthStore } from '@/stores/auth'

/**
 * v-permission 指令：根据权限码控制元素显隐
 *
 * 用法：
 *   <el-button v-permission="'projects:create'">新增</el-button>
 *   <el-button v-permission="['projects:create', 'projects:edit']">操作</el-button>
 *
 * 注意：元素在 mounted 时如果无权限会被移除。
 * 权限变更后需要整体刷新页面（登录/登出会触发页面跳转，自然刷新）。
 */
function checkPermission(el: HTMLElement, binding: DirectiveBinding): boolean {
  const codes: string[] = Array.isArray(binding.value) ? binding.value : [binding.value]
  if (codes.length === 0 || codes[0] == null) return true // 无权限码默认显示

  const auth = useAuthStore()
  const hasAny = codes.some(code => auth.hasPermission(code))
  console.log(
    `[v-permission] ${hasAny ? '显示' : '隐藏'}: codes=${codes.join(',')}, permissions=[${auth.permissions.join(',')}]`
  )
  return hasAny
}

export default {
  mounted(el: HTMLElement, binding: DirectiveBinding) {
    if (!checkPermission(el, binding)) {
      el.parentNode?.removeChild(el)
    }
  },
  updated(el: HTMLElement, binding: DirectiveBinding) {
    // 权限变更时重新检查（权限列表变更后通过路由切换触发）
    if (!checkPermission(el, binding)) {
      el.style.display = 'none'
    } else {
      el.style.display = ''
    }
  }
}
