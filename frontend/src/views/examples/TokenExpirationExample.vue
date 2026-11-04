<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { getProjectList, deleteProject } from '@/api/modules/projects'
import { safeRequest, batchRequest } from '@/utils/request-handler'
import type { Project } from '@/types'

// ========================================
// 示例 1: 使用 try-catch 处理单个请求
// ========================================
const projects1 = ref<Project[]>([])
const loading1 = ref(false)

const loadProjects1 = async () => {
  loading1.value = true
  try {
    const res = await getProjectList()
    projects1.value = res.data
  } catch (error: any) {
    // token 过期时，拦截器已经处理跳转，这里只需要静默返回
    if (error.message === 'Unauthorized') {
      return
    }
    // 其他错误可以在这里特殊处理
    console.error('加载项目失败:', error)
  } finally {
    loading1.value = false
  }
}

// ========================================
// 示例 2: 使用 safeRequest 工具（推荐）
// ========================================
const projects2 = ref<Project[]>([])
const loading2 = ref(false)

const loadProjects2 = async () => {
  loading2.value = true

  const res = await safeRequest(
    () => getProjectList(),
    {
      defaultValue: { data: [] }, // 错误时返回空数组
      silent: false // 显示错误提示（401 除外）
    }
  )

  projects2.value = res?.data || []
  loading2.value = false
}

// ========================================
// 示例 3: 使用 batchRequest 批量加载（页面初始化推荐）
// ========================================
const projects3 = ref<Project[]>([])
const stats = ref<any>(null)
const loading3 = ref(false)

const loadPageData = async () => {
  loading3.value = true

  const results = await batchRequest([
    {
      name: 'projects',
      request: () => getProjectList()
    },
    {
      name: 'stats',
      request: () => getProjectList(), // 假设这是统计接口
      required: false // 非必需，失败不影响其他请求
    }
  ])

  projects3.value = results.projects?.data || []
  stats.value = results.stats?.data || null

  loading3.value = false
}

// ========================================
// 示例 4: 删除操作的错误处理
// ========================================
const handleDelete = async (id: number) => {
  try {
    await deleteProject(id)
    ElMessage.success('删除成功')
    // 重新加载列表
    await loadProjects1()
  } catch (error: any) {
    // token 过期时，拦截器已处理跳转
    if (error.message === 'Unauthorized') {
      return
    }

    // 403 权限错误，拦截器已显示提示
    if (error.response?.status === 403) {
      return
    }

    // 其他错误
    ElMessage.error('删除失败')
  }
}

// ========================================
// 示例 5: 使用 safeRequest 处理删除（更简洁）
// ========================================
const handleDeleteSafe = async (id: number) => {
  const result = await safeRequest(
    () => deleteProject(id),
    {
      onError: () => {
        ElMessage.error('删除失败')
      }
    }
  )

  if (result) {
    ElMessage.success('删除成功')
    await loadProjects1()
  }
}

// 页面挂载时加载数据
onMounted(() => {
  // 根据实际需求选择其中一种方式
  loadProjects1()
  // 或 loadProjects2()
  // 或 loadPageData()
})
</script>

<template>
  <div class="example-page">
    <h2>Token 过期处理示例</h2>

    <div class="section">
      <h3>示例 1: 使用 try-catch</h3>
      <el-button @click="loadProjects1" :loading="loading1">加载数据</el-button>
      <div v-if="projects1.length">共 {{ projects1.length }} 条数据</div>
    </div>

    <div class="section">
      <h3>示例 2: 使用 safeRequest</h3>
      <el-button @click="loadProjects2" :loading="loading2">加载数据</el-button>
      <div v-if="projects2.length">共 {{ projects2.length }} 条数据</div>
    </div>

    <div class="section">
      <h3>示例 3: 批量加载</h3>
      <el-button @click="loadPageData" :loading="loading3">加载数据</el-button>
      <div v-if="projects3.length">
        <div>项目: {{ projects3.length }} 条</div>
        <div v-if="stats">统计数据已加载</div>
      </div>
    </div>

    <div class="section">
      <h3>示例 4 & 5: 删除操作</h3>
      <el-button @click="handleDelete(1)">删除 (try-catch)</el-button>
      <el-button @click="handleDeleteSafe(1)">删除 (safeRequest)</el-button>
    </div>
  </div>
</template>

<style scoped>
.example-page {
  padding: 20px;
}

.section {
  margin-bottom: 30px;
  padding: 20px;
  border: 1px solid #eee;
  border-radius: 8px;
}

.section h3 {
  margin-top: 0;
  color: #06b6d4;
}
</style>
