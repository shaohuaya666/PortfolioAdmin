<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">工作经历</h2>
        <p class="page-subtitle !mb-0">管理工作履历与成就</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500" v-permission="'work-history:create'">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
        新增经历
      </el-button>
    </div>

    <div class="space-y-6">
      <div v-for="item in list" :key="item.id" class="card-panel">
        <!-- 头部 -->
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-3">
            <div class="w-3 h-3 rounded-full" :class="item.isCurrent ? 'bg-cyan-500' : 'bg-slate-600'"></div>
            <div>
              <h3 class="text-sm text-[#c8d6e5] font-semibold">{{ item.company }}</h3>
              <div class="text-xs text-[#06b6d4]">{{ item.role }}</div>
            </div>
            <span class="tag-cyan !text-[10px]">{{ item.period }}</span>
            <span v-if="item.isCurrent" class="tag-cyan tag-cyan-core !text-[10px]">在职</span>
          </div>
          <div class="flex gap-1">
            <el-button size="small" text @click="openDialog(item)" v-permission="'work-history:edit'">
              <span class="material-symbols-outlined text-sm">edit</span>
            </el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)" v-permission="'work-history:delete'">
              <span class="material-symbols-outlined text-sm">delete</span>
            </el-button>
          </div>
        </div>
        <p class="text-xs text-[#94a3b8] mb-4">{{ item.desc }}</p>

        <!-- 成就列表 -->
        <div>
          <div class="text-xs text-[#64748b] mb-2 flex items-center gap-2">
            <span class="material-symbols-outlined text-xs">emoji_events</span>
            工作成就
          </div>
          <div v-for="ach in item.achievements" :key="ach.id" class="flex items-center justify-between py-1.5 pl-6 group">
            <span class="text-xs text-[#94a3b8] flex-1">{{ ach.description }}</span>
            <el-button size="small" text class="!text-red-400 opacity-0 group-hover:opacity-100 !p-0" @click="handleDeleteAchievement(ach.id)" v-permission="'work-history:achievements_delete'">
              <span class="material-symbols-outlined text-xs">close</span>
            </el-button>
          </div>
          <div class="flex gap-2 pl-6 mt-2">
            <el-input v-model="achInputs[item.id]" placeholder="添加成就" size="small" class="!w-60" @keyup.enter="addAchievement(item.id)" />
            <el-button size="small" @click="addAchievement(item.id)" v-permission="'work-history:achievements_create'">添加</el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 工作经历弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑经历' : '新增经历'" width="480px" top="12vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="60px">
        <el-form-item label="编号" prop="id">
          <el-input v-model="form.id" :disabled="isEdit" placeholder="如: exp-1" />
        </el-form-item>
        <el-form-item label="公司" prop="company">
          <el-input v-model="form.company" placeholder="公司名称" />
        </el-form-item>
        <el-form-item label="职位" prop="role">
          <el-input v-model="form.role" placeholder="职位名称" />
        </el-form-item>
        <el-form-item label="时间" prop="period">
          <el-input v-model="form.period" placeholder="如: 2021.04 - 至今" />
        </el-form-item>
        <el-form-item label="描述" prop="desc">
          <el-input v-model="form.desc" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="在职">
          <el-switch v-model="form.isCurrent" />
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
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { workHistoriesApi, achievementsApi } from '@/api/modules/workHistory'
import type { WorkHistory } from '@/types'

const list = ref<WorkHistory[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const achInputs = ref<Record<string, string>>({})

const form = reactive({ id: '', company: '', role: '', period: '', desc: '', isCurrent: false })
const rules: FormRules = {
  id: [{ required: true, message: '请输入编号', trigger: 'blur' }],
  company: [{ required: true, message: '请输入公司名称', trigger: 'blur' }],
  role: [{ required: true, message: '请输入职位名称', trigger: 'blur' }],
  period: [{ required: true, message: '请输入起止时间', trigger: 'blur' }],
  desc: [{ required: true, message: '请输入工作描述', trigger: 'blur' }],
}

async function loadData() {
  const res = await workHistoriesApi.getAll()
  list.value = res.data
}

function openDialog(item?: WorkHistory) {
  if (item) {
    isEdit.value = true
    form.id = item.id; form.company = item.company; form.role = item.role
    form.period = item.period; form.desc = item.desc; form.isCurrent = item.isCurrent
  } else {
    isEdit.value = false
    form.id = ''; form.company = ''; form.role = ''; form.period = ''; form.desc = ''; form.isCurrent = false
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await workHistoriesApi.update(form.id, { company: form.company, role: form.role, period: form.period, desc: form.desc, isCurrent: form.isCurrent })
      ElMessage.success('更新成功')
    } else {
      await workHistoriesApi.create(form)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

async function handleDelete(id: string) {
  await ElMessageBox.confirm('删除经历将同时删除其下所有成就', '确认', { type: 'warning' })
  await workHistoriesApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

async function addAchievement(workHistoryId: string) {
  const desc = achInputs.value[workHistoryId]?.trim()
  if (!desc) return
  await achievementsApi.create({ description: desc, workHistoryId })
  ElMessage.success('成就已添加')
  achInputs.value[workHistoryId] = ''
  await loadData()
}

async function handleDeleteAchievement(id: number) {
  await achievementsApi.delete(id)
  ElMessage.success('已删除')
  await loadData()
}

onMounted(loadData)
</script>
