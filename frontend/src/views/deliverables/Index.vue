<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">项目成品集合</h2>
        <p class="page-subtitle !mb-0">集中管理与展示各项目交付成果</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500" v-permission="'deliverables:create'">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
        新增成品
      </el-button>
    </div>

    <!-- 工具栏：筛选 + 视图切换 -->
    <div class="card-panel flex flex-wrap items-center gap-3 mb-4 !p-3">
      <el-select v-model="filterStatus" clearable placeholder="全部状态" style="width:140px">
        <el-option v-for="(v, k) in STATUS_MAP" :key="k" :label="v.label" :value="k" />
      </el-select>
      <el-select v-model="filterProjectId" clearable placeholder="全部项目" style="width:180px">
        <el-option v-for="p in projects" :key="p.id" :label="p.title" :value="p.id" />
      </el-select>
      <el-input v-model="keyword" placeholder="搜索名称 / 描述" clearable style="width:200px">
        <template #prefix><span class="material-symbols-outlined" style="font-size:16px">search</span></template>
      </el-input>
      <span class="text-xs text-[#64748b] ml-1">{{ filtered.length }} 条成果</span>
      <div class="ml-auto flex items-center gap-1 p-1 rounded-lg" style="background:rgba(6,182,212,0.06)">
        <button
          v-for="m in (['card', 'list'] as const)"
          :key="m"
          class="view-toggle-btn"
          :class="{ active: viewMode === m }"
          @click="viewMode = m"
        >
          <span class="material-symbols-outlined" style="font-size:18px">{{ m === 'card' ? 'grid_view' : 'view_list' }}</span>
        </button>
      </div>
    </div>

    <!-- 卡片视图 -->
    <div v-if="viewMode === 'card'" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
      <div v-for="item in filtered" :key="item.id" class="card-panel overflow-hidden group !p-0 flex flex-col">
        <div class="cover-box">
          <img v-if="item.coverImage" :src="item.coverImage" :alt="item.name" class="w-full h-full object-cover" />
          <div v-else class="w-full h-full flex items-center justify-center">
            <span class="material-symbols-outlined cover-placeholder">inventory_2</span>
          </div>
          <span class="status-chip" :style="{ background: statusColor(item.status) }">{{ statusLabel(item.status) }}</span>
          <span v-if="!item.isPublic" class="private-chip" title="未公开">
            <span class="material-symbols-outlined" style="font-size:12px">lock</span>
          </span>
        </div>
        <div class="p-4 flex-1 flex flex-col gap-2">
          <div class="flex items-center gap-2">
            <a
              v-if="item.url"
              :href="item.url"
              target="_blank"
              rel="noopener noreferrer"
              class="text-sm text-[#e2e8f0] font-semibold hover:text-cyan-400 truncate flex items-center gap-1"
              :title="item.url"
            >
              {{ item.name }}
              <span class="material-symbols-outlined" style="font-size:13px;color:#06b6d4">open_in_new</span>
            </a>
            <span v-else class="text-sm text-[#e2e8f0] font-semibold truncate">{{ item.name }}</span>
            <span v-if="item.version" class="ver-tag">v{{ item.version }}</span>
          </div>
          <p class="text-xs text-[#64748b] line-clamp-2 min-h-[2rem]">{{ item.description || '暂无描述' }}</p>
          <div class="flex flex-wrap gap-1">
            <span v-for="t in splitTags(item.techTags)" :key="t" class="skill-tag">{{ t }}</span>
          </div>
          <div class="mt-auto pt-2 flex items-center justify-between text-xs text-[#64748b] border-t" style="border-color:rgba(6,182,212,0.08)">
            <span class="flex items-center gap-2 min-w-0">
              <span v-if="item.owner" class="flex items-center gap-1 truncate">
                <span class="material-symbols-outlined" style="font-size:13px">person</span>{{ item.owner }}
              </span>
              <span v-if="item.completedAt" class="font-mono">{{ item.completedAt }}</span>
            </span>
            <span class="flex items-center gap-1">
              <span v-if="projectName(item.projectId)" class="tag-cyan !text-[10px] truncate max-w-[100px]">{{ projectName(item.projectId) }}</span>
              <span class="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <el-button size="small" text @click="openDialog(item)" v-permission="'deliverables:edit'">
                  <span class="material-symbols-outlined text-sm">edit</span>
                </el-button>
                <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)" v-permission="'deliverables:delete'">
                  <span class="material-symbols-outlined text-sm">delete</span>
                </el-button>
              </span>
            </span>
          </div>
        </div>
      </div>
      <div v-if="!filtered.length" class="col-span-full card-panel text-center text-sm text-[#64748b] py-10">暂无数据</div>
    </div>

    <!-- 列表视图 -->
    <div v-else class="card-panel !p-0 overflow-hidden">
      <el-table :data="filtered" class="deliverable-table">
        <el-table-column label="封面" width="72">
          <template #default="{ row }">
            <img v-if="row.coverImage" :src="row.coverImage" class="thumb" :alt="row.name" />
            <div v-else class="thumb thumb-empty flex items-center justify-center">
              <span class="material-symbols-outlined" style="font-size:14px;color:#475569">inventory_2</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="名称 / 版本" min-width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <a v-if="row.url" :href="row.url" target="_blank" rel="noopener noreferrer" class="text-[#c8d6e5] hover:text-cyan-400 text-sm">{{ row.name }}</a>
            <span v-else class="text-[#c8d6e5] text-sm">{{ row.name }}</span>
            <span v-if="row.version" class="ver-tag ml-2">v{{ row.version }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="90">
          <template #default="{ row }">
            <span class="inline-flex items-center gap-1.5 text-xs">
              <span class="w-1.5 h-1.5 rounded-full" :style="{ background: statusColor(row.status) }"></span>
              {{ statusLabel(row.status) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="所属项目" width="130" show-overflow-tooltip>
          <template #default="{ row }">{{ projectName(row.projectId) || '-' }}</template>
        </el-table-column>
        <el-table-column label="技术栈" min-width="160">
          <template #default="{ row }">
            <span v-for="t in splitTags(row.techTags)" :key="t" class="skill-tag mr-1">{{ t }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="owner" label="负责人" width="90" show-overflow-tooltip />
        <el-table-column prop="completedAt" label="完成时间" width="100">
          <template #default="{ row }">{{ row.completedAt || '-' }}</template>
        </el-table-column>
        <el-table-column prop="sortOrder" label="权重" width="60" />
        <el-table-column label="公开" width="60">
          <template #default="{ row }">
            <span class="material-symbols-outlined text-sm" :style="{ color: row.isPublic ? '#22c55e' : '#64748b' }">{{ row.isPublic ? 'visibility' : 'visibility_off' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button v-if="row.url" size="small" text tag="a" :href="row.url" target="_blank" rel="noopener noreferrer">
              <span class="material-symbols-outlined text-sm">open_in_new</span>
            </el-button>
            <el-button size="small" text @click="openDialog(row)" v-permission="'deliverables:edit'">
              <span class="material-symbols-outlined text-sm">edit</span>
            </el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDelete(row.id)" v-permission="'deliverables:delete'">
              <span class="material-symbols-outlined text-sm">delete</span>
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑成品' : '新增成品'" width="640px" top="8vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <div class="grid grid-cols-2 gap-x-4">
          <el-form-item label="编号" prop="id">
            <el-input v-model="form.id" :disabled="isEdit" placeholder="如: erp-release-v2" />
          </el-form-item>
          <el-form-item label="名称" prop="name">
            <el-input v-model="form.name" placeholder="项目成品名称" />
          </el-form-item>
          <el-form-item label="版本号">
            <el-input v-model="form.version" placeholder="如: 2.3.1" />
          </el-form-item>
          <el-form-item label="状态" prop="status">
            <el-select v-model="form.status" class="!w-full">
              <el-option v-for="(v, k) in STATUS_MAP" :key="k" :label="v.label" :value="k" />
            </el-select>
          </el-form-item>
          <el-form-item label="项目地址">
            <el-input v-model="form.url" placeholder="https://..." />
          </el-form-item>
          <el-form-item label="封面图">
            <el-input v-model="form.coverImage" placeholder="封面图 URL" />
          </el-form-item>
          <el-form-item label="完成时间">
            <el-date-picker v-model="form.completedAt" type="date" value-format="YYYY-MM-DD" placeholder="选择日期" class="!w-full" />
          </el-form-item>
          <el-form-item label="负责人">
            <el-input v-model="form.owner" placeholder="负责人姓名" />
          </el-form-item>
          <el-form-item label="排序权重">
            <el-input-number v-model="form.sortOrder" :min="0" :max="9999" class="!w-full" />
          </el-form-item>
          <el-form-item label="是否公开">
            <el-switch v-model="form.isPublic" active-text="公开" inactive-text="隐藏" />
          </el-form-item>
        </div>
        <el-form-item label="所属项目">
          <el-select v-model="form.projectId" clearable placeholder="不关联任何项目" class="!w-full">
            <el-option v-for="p in projects" :key="p.id" :label="p.title" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="成果描述">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="交付成果的简要描述" />
        </el-form-item>
        <el-form-item label="技术栈">
          <div class="flex flex-wrap gap-2 mb-2">
            <el-tag v-for="(t, i) in techList" :key="i" closable size="small" @close="techList.splice(i, 1)">{{ t }}</el-tag>
          </div>
          <div class="flex gap-2">
            <el-input v-model="newTag" placeholder="输入技术名称，回车添加" size="small" style="width:200px" @keyup.enter="addTag" />
            <el-button size="small" @click="addTag">添加</el-button>
          </div>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" placeholder="备注（可选）" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave" :loading="saving">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { deliverablesApi } from '@/api/modules/deliverables'
import { projectsApi } from '@/api/modules/projects'
import type { ProjectDeliverable, CompactProject } from '@/types'

// 状态字典
const STATUS_MAP: Record<string, { label: string; color: string }> = {
  draft: { label: '草稿', color: '#94a3b8' },
  in_progress: { label: '进行中', color: '#38bdf8' },
  released: { label: '已上线', color: '#22c55e' },
  maintaining: { label: '维护中', color: '#f59e0b' },
  deprecated: { label: '已下线', color: '#ef4444' }
}

function statusLabel(s: string) { return STATUS_MAP[s]?.label || s }
function statusColor(s: string) { return STATUS_MAP[s]?.color || '#64748b' }
function splitTags(tags: string): string[] {
  return (tags || '').split(',').map(t => t.trim()).filter(Boolean)
}
function projectName(projectId: string | null) {
  return projects.value.find(p => p.id === projectId)?.title || ''
}

// 列表数据
const list = ref<ProjectDeliverable[]>([])
const projects = ref<CompactProject[]>([])
const viewMode = ref<'card' | 'list'>('card')

// 筛选
const filterStatus = ref('')
const filterProjectId = ref('')
const keyword = ref('')

const filtered = computed(() =>
  list.value.filter(d => {
    if (filterStatus.value && d.status !== filterStatus.value) return false
    if (filterProjectId.value && d.projectId !== filterProjectId.value) return false
    if (keyword.value.trim()) {
      const kw = keyword.value.trim().toLowerCase()
      if (!d.name.toLowerCase().includes(kw) && !d.description.toLowerCase().includes(kw)) return false
    }
    return true
  })
)

async function loadData() {
  const [dRes, pRes] = await Promise.all([deliverablesApi.getAll(), projectsApi.getAll()])
  list.value = dRes.data
  projects.value = pRes.data
}

// 弹窗表单
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const techList = ref<string[]>([])
const newTag = ref('')

const form = reactive({
  id: '', name: '', version: '', description: '', status: 'draft',
  url: '', coverImage: '', completedAt: '', owner: '',
  remark: '', sortOrder: 0, isPublic: true, projectId: null as string | null
})

const rules: FormRules = {
  id: [{ required: true, message: '请输入编号', trigger: 'blur' }],
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
  status: [{ required: true, message: '请选择状态', trigger: 'change' }]
}

function addTag() {
  const t = newTag.value.trim()
  if (t && !techList.value.includes(t)) techList.value.push(t)
  newTag.value = ''
}

function openDialog(item?: ProjectDeliverable) {
  if (item) {
    isEdit.value = true
    form.id = item.id; form.name = item.name; form.version = item.version
    form.description = item.description; form.status = item.status
    form.url = item.url; form.coverImage = item.coverImage
    form.completedAt = item.completedAt; form.owner = item.owner
    form.remark = item.remark; form.sortOrder = item.sortOrder
    form.isPublic = item.isPublic; form.projectId = item.projectId
    techList.value = splitTags(item.techTags)
  } else {
    isEdit.value = false
    form.id = ''; form.name = ''; form.version = ''; form.description = ''
    form.status = 'draft'; form.url = ''; form.coverImage = ''
    form.completedAt = ''; form.owner = ''; form.remark = ''
    form.sortOrder = 0; form.isPublic = true; form.projectId = null
    techList.value = []
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const payload = {
      id: form.id, name: form.name, version: form.version,
      description: form.description, status: form.status,
      url: form.url.trim(), coverImage: form.coverImage.trim(),
      techTags: techList.value.join(','),
      completedAt: form.completedAt || '', owner: form.owner,
      remark: form.remark, sortOrder: form.sortOrder,
      isPublic: form.isPublic, projectId: form.projectId || null
    }
    if (isEdit.value) {
      await deliverablesApi.update(form.id, payload)
    } else {
      await deliverablesApi.create(payload)
    }
    ElMessage.success(isEdit.value ? '更新成功' : '创建成功')
    dialogVisible.value = false
    await loadData()
  } catch {
    // 错误已在拦截器处理
  } finally { saving.value = false }
}

async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定删除该成品记录？', '确认', { type: 'warning' })
  await deliverablesApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
/* 卡片封面 */
.cover-box {
  position: relative;
  height: 130px;
  background: rgba(6, 182, 212, 0.04);
  border-bottom: 1px solid rgba(6, 182, 212, 0.08);
}
.cover-placeholder { font-size: 40px; color: #164e63; }
.status-chip {
  position: absolute; top: 8px; left: 8px;
  font-size: 10px; line-height: 1; padding: 4px 8px;
  border-radius: 9999px; color: #051424; font-weight: 600;
}
.private-chip {
  position: absolute; top: 8px; right: 8px;
  width: 20px; height: 20px; border-radius: 6px;
  background: rgba(2, 6, 23, 0.65); color: #94a3b8;
  display: flex; align-items: center; justify-content: center;
}
.ver-tag {
  font-size: 10px; padding: 1px 6px; border-radius: 9999px;
  background: rgba(6, 182, 212, 0.08); color: #22d3ee;
  font-family: 'JetBrains Mono', monospace; white-space: nowrap;
}
.skill-tag {
  font-size: 10px; padding: 1px 8px; border-radius: 9999px;
  background: rgba(6, 182, 212, 0.08); color: #22d3ee;
}
.thumb {
  width: 44px; height: 32px; border-radius: 4px; object-fit: cover;
  border: 1px solid rgba(6, 182, 212, 0.1);
}
.thumb-empty { background: rgba(6, 182, 212, 0.04); }

/* 视图切换 */
.view-toggle-btn {
  width: 32px; height: 28px; border-radius: 6px; border: none;
  background: transparent; color: #64748b; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.2s;
}
.view-toggle-btn:hover { color: #c8d6e5; }
.view-toggle-btn.active { background: rgba(6, 182, 212, 0.15); color: #06b6d4; }

/* 暗色表格 */
:deep(.deliverable-table) {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: rgba(6, 182, 212, 0.06);
  --el-table-border-color: rgba(6, 182, 212, 0.1);
  --el-table-border-color-lighter: rgba(6, 182, 212, 0.08);
  --el-table-text-color: #c8d6e5;
  --el-table-header-text-color: #94a3b8;
  --el-table-row-hover-bg-color: rgba(6, 182, 212, 0.05);
}
</style>
