<template>
  <el-container class="layout">
    <!-- 侧边栏 -->
    <el-aside width="260px" class="sidebar">
      <div class="sidebar-header">
        <div class="sidebar-logo">
          <span class="material-symbols-outlined logo-icon">neurology</span>
          <div>
            <div class="logo-text">ARCHITECT</div>
            <div class="logo-sub">管理后台</div>
          </div>
        </div>
      </div>
      <nav class="sidebar-nav">
        <template v-for="item in menuItems" :key="item.path">
          <router-link :to="item.path" class="nav-item" active-class="nav-item-active">
            <span class="material-symbols-outlined nav-icon">{{ item.icon }}</span>
            <span>{{ item.title }}</span>
          </router-link>
        </template>
      </nav>
      <div class="sidebar-footer">
        <div class="user-info">
          <div class="user-avatar">{{ username.charAt(0).toUpperCase() }}</div>
          <div>
            <div class="user-name">{{ username }}</div>
            <div class="user-role">管理员</div>
          </div>
        </div>
        <button class="logout-btn" @click="handleLogout" title="退出登录">
          <span class="material-symbols-outlined">logout</span>
        </button>
      </div>
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
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const username = computed(() => authStore.username || 'Admin')
const currentTitle = computed(() => route.meta?.title as string || '')

const timeStr = ref('')
let timer: number
onMounted(() => {
  const update = () => {
    timeStr.value = new Date().toLocaleString('zh-CN', { hour12: false })
  }
  update()
  timer = window.setInterval(update, 1000)
})
onUnmounted(() => clearInterval(timer))

const menuItems = [
  { path: '/dashboard', title: '仪表盘概览', icon: 'dashboard' },
  { path: '/advantages', title: '核心优势', icon: 'stars' },
  { path: '/skills', title: '技术栈矩阵', icon: 'code' },
  { path: '/projects', title: '项目管理', icon: 'deployed_code' },
  { path: '/work-history', title: '工作经历', icon: 'work' },
  { path: '/diagnostics', title: '技能诊断', icon: 'monitoring' },
]

function handleLogout() {
  authStore.logout()
  router.push('/login')
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
</style>
