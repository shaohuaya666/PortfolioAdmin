<template>
  <div>
    <h2 class="page-title">仪表盘概览</h2>
    <p class="page-subtitle">Portfolio 数据总览</p>

    <!-- 统计卡片 -->
    <div class="grid grid-cols-4 gap-5 mb-8">
      <div class="card-panel stat-card">
        <div class="stat-icon">
          <span class="material-symbols-outlined">deployed_code</span>
        </div>
        <div>
          <div class="stat-number">{{ stats.projectCount }}</div>
          <div class="stat-label">项目经历</div>
        </div>
      </div>
      <div class="card-panel stat-card">
        <div class="stat-icon">
          <span class="material-symbols-outlined">code</span>
        </div>
        <div>
          <div class="stat-number">{{ stats.skillCount }}</div>
          <div class="stat-label">技术标签</div>
        </div>
      </div>
      <div class="card-panel stat-card">
        <div class="stat-icon">
          <span class="material-symbols-outlined">work</span>
        </div>
        <div>
          <div class="stat-number">{{ stats.workYearCount }}</div>
          <div class="stat-label">工作经历</div>
        </div>
      </div>
      <div class="card-panel stat-card">
        <div class="stat-icon">
          <span class="material-symbols-outlined">monitoring</span>
        </div>
        <div>
          <div class="stat-number">{{ stats.diagnosticCount }}</div>
          <div class="stat-label">技能诊断</div>
        </div>
      </div>
    </div>

    <!-- 快捷入口 -->
    <div class="card-panel">
      <h3 class="text-[#c8d6e5] text-base font-semibold mb-4">快捷管理</h3>
      <div class="grid grid-cols-3 gap-4">
        <router-link to="/advantages" class="quick-link">
          <span class="material-symbols-outlined">stars</span>
          <div>
            <div class="quick-title">核心优势</div>
            <div class="quick-desc">管理个人核心竞争力</div>
          </div>
        </router-link>
        <router-link to="/skills" class="quick-link">
          <span class="material-symbols-outlined">code</span>
          <div>
            <div class="quick-title">技术栈矩阵</div>
            <div class="quick-desc">管理技能分类与标签</div>
          </div>
        </router-link>
        <router-link to="/projects" class="quick-link">
          <span class="material-symbols-outlined">deployed_code</span>
          <div>
            <div class="quick-title">项目管理</div>
            <div class="quick-desc">维护项目经历与技能</div>
          </div>
        </router-link>
        <router-link to="/work-history" class="quick-link">
          <span class="material-symbols-outlined">work</span>
          <div>
            <div class="quick-title">工作经历</div>
            <div class="quick-desc">管理工作履历与成就</div>
          </div>
        </router-link>
        <router-link to="/diagnostics" class="quick-link">
          <span class="material-symbols-outlined">monitoring</span>
          <div>
            <div class="quick-title">技能诊断</div>
            <div class="quick-desc">管理技能详细诊断</div>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { dashboardApi } from '@/api/modules/dashboard'
import type { DashboardStats } from '@/types'

const stats = ref<DashboardStats>({ projectCount: 0, skillCount: 0, workYearCount: 0, diagnosticCount: 0 })

onMounted(async () => {
  try {
    const res = await dashboardApi.getStats()
    stats.value = res.data
  } catch {}
})
</script>

<style scoped>
.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
}
.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  background: rgba(6, 182, 212, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #06b6d4;
  flex-shrink: 0;
}
.stat-icon .material-symbols-outlined { font-size: 24px; }

.quick-link {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px;
  border: 1px solid rgba(6, 182, 212, 0.08);
  border-radius: 10px;
  text-decoration: none;
  color: #94a3b8;
  transition: all 0.2s;
}
.quick-link:hover {
  border-color: rgba(6, 182, 212, 0.2);
  background: rgba(6, 182, 212, 0.05);
  color: #c8d6e5;
}
.quick-link .material-symbols-outlined {
  font-size: 28px;
  color: #06b6d4;
  flex-shrink: 0;
}
.quick-title { font-size: 14px; font-weight: 600; }
.quick-desc { font-size: 12px; color: #64748b; margin-top: 2px; }
</style>
