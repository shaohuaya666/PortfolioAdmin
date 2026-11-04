import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '@/router'
import { useAuthStore } from '@/stores/auth'

const request = axios.create({
  baseURL: '/api',
  timeout: 10000
})

// 防止重复跳转的标志
let isRedirectingToLogin = false

request.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

request.interceptors.response.use(
  response => response,
  error => {
    // 处理 401 未授权错误
    if (error.response?.status === 401) {
      // 防止多个并发请求同时触发跳转
      if (!isRedirectingToLogin) {
        isRedirectingToLogin = true

        // 清理所有认证信息
        const authStore = useAuthStore()
        authStore.logout()

        // 跳转到登录页
        router.push('/login').catch(() => {
          // 捕获重复导航错误，避免控制台报错
        }).finally(() => {
          // 延迟重置标志，避免页面刷新时立即触发
          setTimeout(() => {
            isRedirectingToLogin = false
          }, 1000)
        })

        // 只显示一次提示消息
        ElMessage.warning('登录已过期，请重新登录')
      }

      // 返回一个被拒绝的 Promise，但不显示额外的错误消息
      return Promise.reject(new Error('Unauthorized'))
    }

    // 处理 403 无权限错误
    if (error.response?.status === 403) {
      const msg = error.response?.data?.message || '无操作权限'
      ElMessage.error(msg)
      return Promise.reject(error)
    }

    // 处理其他错误
    const msg = error.response?.data?.message || '请求失败'
    ElMessage.error(msg)
    return Promise.reject(error)
  }
)

export default request
