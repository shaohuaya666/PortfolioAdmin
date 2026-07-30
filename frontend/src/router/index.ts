import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('@/views/login/Index.vue'),
      meta: { title: '登录' }
    },
    {
      path: '/',
      component: () => import('@/layout/MainLayout.vue'),
      redirect: '/dashboard',
      children: [
        {
          path: 'dashboard',
          name: 'Dashboard',
          component: () => import('@/views/dashboard/Index.vue'),
          meta: { title: '仪表盘', icon: 'dashboard' }
        },
        {
          path: 'advantages',
          name: 'Advantages',
          component: () => import('@/views/advantages/Index.vue'),
          meta: { title: '核心优势', icon: 'stars' }
        },
        {
          path: 'skills',
          name: 'Skills',
          component: () => import('@/views/skills/Index.vue'),
          meta: { title: '技术栈矩阵', icon: 'code' }
        },
        {
          path: 'projects',
          name: 'Projects',
          component: () => import('@/views/projects/Index.vue'),
          meta: { title: '项目管理', icon: 'deployed_code' }
        },
        {
          path: 'work-history',
          name: 'WorkHistory',
          component: () => import('@/views/workHistory/Index.vue'),
          meta: { title: '工作经历', icon: 'work' }
        },
        {
          path: 'diagnostics',
          name: 'Diagnostics',
          component: () => import('@/views/diagnostics/Index.vue'),
          meta: { title: '技能诊断', icon: 'monitoring' }
        }
      ]
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('token')
  if (to.path !== '/login' && !token) {
    next('/login')
  } else {
    next()
  }
})

export default router
