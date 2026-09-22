<template>
  <el-container class="layout">
    <!-- 侧边栏 -->
    <el-aside width="260px" class="sidebar">
      <div class="sidebar-header">
        <div class="sidebar-logo">
          <span class="material-symbols-outlined logo-icon">neurology</span>
          <div>
            <div class="logo-text">架构师</div>
            <div class="logo-sub">管理后台</div>
          </div>
        </div>
      </div>
      <nav class="sidebar-nav">
        <template v-for="item in menuTree" :key="item.id">
          <!-- 有子菜单的父项 -->
          <template v-if="item.children && item.children.length > 0">
            <button class="nav-group-toggle" :class="{ expanded: expandedMenus.has(item.id) }" @click="toggleMenu(item.id)">
              <span class="material-symbols-outlined nav-icon">{{ item.icon || 'folder' }}</span>
              <span>{{ item.name }}</span>
              <span class="material-symbols-outlined expand-icon">expand_more</span>
            </button>
            <div v-show="expandedMenus.has(item.id)" class="nav-sub-items">
              <router-link v-for="child in item.children" :key="child.id" :to="child.path" class="nav-item nav-sub-item" active-class="nav-item-active">
                <span class="material-symbols-outlined nav-icon">{{ child.icon || 'circle' }}</span>
                <span>{{ child.name }}</span>
              </router-link>
            </div>
          </template>
          <!-- 无子菜单的直接路由 -->
          <router-link v-else :to="item.path" class="nav-item" active-class="nav-item-active">
            <span class="material-symbols-outlined nav-icon">{{ item.icon || 'circle' }}</span>
            <span>{{ item.name }}</span>
          </router-link>
        </template>
      </nav>
      <div class="sidebar-footer">
        <div class="user-info">
          <div class="user-avatar">{{ username.charAt(0).toUpperCase() }}</div>
          <div>
            <div class="user-name">{{ username }}</div>
            <div class="user-role">{{ authStore.roleName || '管理员' }}</div>
          </div>
        </div>
        <div class="footer-actions">
          <button class="action-btn" @click="showPwdDialog = true" title="修改密码">
            <span class="material-symbols-outlined">lock</span>
          </button>
          <button class="action-btn logout-btn" @click="handleLogout" title="退出登录">
            <span class="material-symbols-outlined">logout</span>
          </button>
        </div>
      </div>

      <!-- 修改密码弹窗 -->
      <el-dialog v-model="showPwdDialog" title="修改密码" width="420px" :close-on-click-modal="false" custom-class="pwd-dialog">
        <el-form ref="pwdFormRef" :model="pwdForm" :rules="pwdRules" label-position="top" @submit.prevent="handleChangePwd">
          <el-form-item label="旧密码" prop="oldPassword">
            <el-input v-model="pwdForm.oldPassword" type="password" show-password placeholder="请输入旧密码" />
          </el-form-item>
          <el-form-item label="新密码" prop="newPassword">
            <el-input v-model="pwdForm.newPassword" type="password" show-password placeholder="至少6位" />
          </el-form-item>
          <el-form-item label="确认新密码" prop="confirmPassword">
            <el-input v-model="pwdForm.confirmPassword" type="password" show-password placeholder="再次输入新密码" />
          </el-form-item>
        </el-form>
        <template #footer>
          <el-button @click="showPwdDialog = false" :loading="pwdLoading">取消</el-button>
          <el-button type="primary" @click="handleChangePwd" :loading="pwdLoading">确认修改</el-button>
        </template>
      </el-dialog>
    </el-aside>

    <!-- 主内容区 -->
    <el-container>
      <el-header class="topbar">
        <div class="topbar-left">
          <span class="topbar-breadcrumb">/ {{ currentTitle }}</span>
        </div>
        <div class="topbar-right">
          <span class="topbar-status">
            <span class="status-dot"></span>
            系统运行中
          </span>
          <span class="topbar-time">{{ timeStr }}</span>
        </div>
      </el-header>
      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, reactive, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api/modules/auth'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import type { MenuTree } from '@/types'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const username = computed(() => authStore.username || '未登录')

// 根据当前路由路径，从菜单树中构建完整面包屑路径
const currentTitle = computed(() => {
  const path = route.path
  const breadcrumb = getBreadcrumb(menuTree.value, path)
  return breadcrumb || (route.meta?.title as string) || ''
})

function getBreadcrumb(tree: MenuTree[], targetPath: string): string | null {
  for (const item of tree) {
    // 先检查当前层级
    if (item.path === targetPath) {
      return item.name
    }
    // 如果有子菜单，递归查找
    if (item.children && item.children.length > 0) {
      const childResult = getBreadcrumb(item.children, targetPath)
      if (childResult) {
        return `${item.name} / ${childResult}`
      }
    }
  }
  return null
}

// 从 store 获取动态菜单树
const menuTree = computed(() => authStore.menus || [])

// 展开/折叠管理
const expandedMenus = ref(new Set<number>())

// 根据当前路由自动展开父菜单
function autoExpandParent() {
  if (menuTree.value.length > 0) {
    menuTree.value.forEach(item => {
      if (item.children && item.children.length > 0) {
        const hasActiveChild = item.children.some(child => child.path === route.path)
        if (hasActiveChild) {
          expandedMenus.value.add(item.id)
        }
      }
    })
  }
}

// 监听路由变化自动展开父菜单
watch(() => route.path, () => {
  autoExpandParent()
})

function toggleMenu(id: number) {
  if (expandedMenus.value.has(id)) {
    expandedMenus.value.delete(id)
  } else {
    expandedMenus.value.add(id)
  }
  expandedMenus.value = new Set(expandedMenus.value)
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}

const timeStr = ref('')
let timer: number
onMounted(() => {
  autoExpandParent()
  const update = () => {
    timeStr.value = new Date().toLocaleString('zh-CN', { hour12: false })
  }
  update()
  timer = window.setInterval(update, 1000)
})
onUnmounted(() => clearInterval(timer))

// 修改密码
const showPwdDialog = ref(false)
const pwdLoading = ref(false)
const pwdFormRef = ref<FormInstance>()

const pwdForm = reactive({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const validateConfirmPwd = (_rule: unknown, value: string, callback: (error?: Error) => void) => {
  if (value !== pwdForm.newPassword) {
    callback(new Error('两次输入的密码不一致'))
  } else {
    callback()
  }
}

const pwdRules: FormRules = {
  oldPassword: [{ required: true, message: '请输入旧密码', trigger: 'blur' }],
  newPassword: [
    { required: true, message: '请输入新密码', trigger: 'blur' },
    { min: 6, message: '密码长度不能少于6位', trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: '请确认新密码', trigger: 'blur' },
    { validator: validateConfirmPwd, trigger: 'blur' }
  ]
}

async function handleChangePwd() {
  const valid = await pwdFormRef.value?.validate().catch(() => false)
  if (!valid) return

  pwdLoading.value = true
  try {
    await authApi.changePassword({
      username: authStore.username,
      oldPassword: pwdForm.oldPassword,
      newPassword: pwdForm.newPassword
    })
    ElMessage.success('密码修改成功，请重新登录')
    pwdForm.oldPassword = ''
    pwdForm.newPassword = ''
    pwdForm.confirmPassword = ''
    showPwdDialog.value = false
    // 退出重新登录
    authStore.logout()
    router.push('/login')
  } catch {
    // 错误已在拦截器处理
  } finally {
    pwdLoading.value = false
  }
}

</script>

<style scoped>
.layout { height: 100vh; background: #051424; }

/* 侧边栏 */
.sidebar {
  background: #061829 !important;
  border-right: 1px solid rgba(6, 182, 212, 0.08) !important;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.sidebar-header {
  padding: 24px 20px;
  border-bottom: 1px solid rgba(6, 182, 212, 0.08);
}
.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 12px;
}
.logo-icon {
  font-size: 32px;
  color: #06b6d4;
}
.logo-text {
  font-size: 18px;
  font-weight: 700;
  color: #e2e8f0;
  letter-spacing: 0.05em;
}
.logo-sub {
  font-size: 11px;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

/* 导航 */
.sidebar-nav {
  flex: 1;
  padding: 16px 12px;
  overflow-y: auto;
}
.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 8px;
  color: #94a3b8;
  text-decoration: none;
  font-size: 14px;
  margin-bottom: 4px;
  transition: all 0.2s;
}
.nav-item:hover {
  background: rgba(6, 182, 212, 0.08);
  color: #c8d6e5;
}
.nav-item-active {
  background: rgba(6, 182, 212, 0.12) !important;
  color: #06b6d4 !important;
  border: 1px solid rgba(6, 182, 212, 0.15);
}
.nav-icon { font-size: 20px; }

/* 分组菜单 */
.nav-group-toggle {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 10px 14px;
  border-radius: 8px;
  color: #94a3b8;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 14px;
  text-align: left;
  margin-bottom: 4px;
  transition: all 0.2s;
}
.nav-group-toggle:hover { background: rgba(6, 182, 212, 0.08); color: #c8d6e5; }
.nav-group-toggle .expand-icon {
  margin-left: auto;
  font-size: 18px;
  transition: transform 0.2s;
}
.nav-group-toggle.expanded .expand-icon { transform: rotate(180deg); }

.nav-sub-items { padding-left: 12px; margin-bottom: 8px; }
.nav-sub-item { font-size: 13px !important; }

/* 底部用户区 */
.sidebar-footer {
  padding: 16px 20px;
  border-top: 1px solid rgba(6, 182, 212, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}
.user-avatar {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: rgba(6, 182, 212, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 600;
  color: #06b6d4;
}
.user-name { font-size: 13px; color: #c8d6e5; }
.user-role { font-size: 11px; color: #64748b; }
.logout-btn {
  background: none;
  border: 1px solid rgba(6, 182, 212, 0.12);
  border-radius: 8px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  cursor: pointer;
  transition: all 0.2s;
}
.logout-btn:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; border-color: rgba(239, 68, 68, 0.2); }

.footer-actions {
  display: flex;
  gap: 8px;
}
.action-btn {
  background: none;
  border: 1px solid rgba(6, 182, 212, 0.12);
  border-radius: 8px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  cursor: pointer;
  transition: all 0.2s;
}
.action-btn:hover { background: rgba(6, 182, 212, 0.1); color: #06b6d4; border-color: rgba(6, 182, 212, 0.25); }

/* 顶栏 */
.topbar {
  height: 56px !important;
  background: rgba(10, 30, 50, 0.8) !important;
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(6, 182, 212, 0.08);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px !important;
}
.topbar-breadcrumb { font-size: 13px; color: #64748b; font-family: 'JetBrains Mono', monospace; }
.topbar-right { display: flex; align-items: center; gap: 20px; }
.topbar-status { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #64748b; }
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: #22c55e; animation: pulse 2s infinite; }
@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}
.topbar-time { font-size: 13px; color: #64748b; font-family: 'JetBrains Mono', monospace; }

/* 内容区 */
.main-content {
  background: transparent !important;
  padding: 32px !important;
  overflow-y: auto;
}

/* 弹窗暗色主题 */
:deep(.pwd-dialog) {
  background: #0b1e33 !important;
  border: 1px solid rgba(6, 182, 212, 0.12) !important;
  border-radius: 12px !important;
}
:deep(.pwd-dialog .el-dialog__header) {
  border-bottom: 1px solid rgba(6, 182, 212, 0.08);
}
:deep(.pwd-dialog .el-dialog__title) {
  color: #e2e8f0;
}
:deep(.pwd-dialog .el-dialog__body) {
  padding: 20px 24px;
}
:deep(.pwd-dialog .el-form-item__label) {
  color: #94a3b8;
}
:deep(.pwd-dialog .el-input__wrapper) {
  background: rgba(6, 182, 212, 0.04) !important;
  border: 1px solid rgba(6, 182, 212, 0.12) !important;
  box-shadow: none !important;
}
:deep(.pwd-dialog .el-input__inner) {
  color: #e2e8f0;
}
:deep(.pwd-dialog .el-dialog__footer) {
  border-top: 1px solid rgba(6, 182, 212, 0.08);
}
</style>
