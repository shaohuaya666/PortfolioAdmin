<template>
  <div class="login-page">
    <div class="login-card">
      <!-- 装饰线条 -->
      <div class="login-decor top-left"></div>
      <div class="login-decor bottom-right"></div>

      <div class="login-header">
        <span class="material-symbols-outlined login-logo">neurology</span>
        <h1>ARCHITECT</h1>
        <p>Portfolio 管理后台</p>
      </div>

      <el-form ref="formRef" :model="form" :rules="rules" @submit.prevent="handleLogin">
        <el-form-item prop="username">
          <el-input
            v-model="form.username"
            placeholder="用户名"
            :prefix-icon="markRaw(User)"
            size="large"
          />
        </el-form-item>
        <el-form-item prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="密码"
            :prefix-icon="markRaw(Lock)"
            size="large"
            show-password
            @keyup.enter="handleLogin"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" size="large" :loading="loading" class="login-btn" @click="handleLogin">
            登 录
          </el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, markRaw } from 'vue'
import { useRouter } from 'vue-router'
import { User, Lock } from '@element-plus/icons-vue'
import { authApi } from '@/api/modules/auth'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

const router = useRouter()
const authStore = useAuthStore()
const loading = ref(false)
const formRef = ref<FormInstance>()

const form = reactive({
  username: '',
  password: ''
})

const rules: FormRules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

async function handleLogin() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    const res = await authApi.login(form)
    authStore.setAuth(res.data)
    ElMessage.success('登录成功')
    router.push('/')
  } catch {
    // 错误已在拦截器处理
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #051424;
  position: relative;
  overflow: hidden;
}
.login-page::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: radial-gradient(ellipse at 20% 50%, rgba(6, 182, 212, 0.06) 0%, transparent 60%),
              radial-gradient(ellipse at 80% 20%, rgba(6, 182, 212, 0.04) 0%, transparent 50%);
}

.login-card {
  width: 400px;
  background: #0b1e33;
  border: 1px solid rgba(6, 182, 212, 0.12);
  border-radius: 16px;
  padding: 48px 40px;
  position: relative;
}
.login-decor {
  position: absolute;
  width: 40px;
  height: 40px;
  border-color: rgba(6, 182, 212, 0.2);
  border-style: solid;
}
.login-decor.top-left { top: -1px; left: -1px; border-width: 2px 0 0 2px; border-radius: 16px 0 0 0; }
.login-decor.bottom-right { bottom: -1px; right: -1px; border-width: 0 2px 2px 0; border-radius: 0 0 16px 0; }

.login-header { text-align: center; margin-bottom: 36px; }
.login-logo {
  font-size: 48px;
  color: #06b6d4;
  margin-bottom: 12px;
}
.login-header h1 {
  font-size: 24px;
  font-weight: 700;
  color: #e2e8f0;
  letter-spacing: 0.05em;
}
.login-header p {
  font-size: 13px;
  color: #64748b;
  margin-top: 6px;
}

.login-btn {
  width: 100%;
  --el-button-bg-color: #06b6d4;
  --el-button-border-color: #06b6d4;
}
</style>
