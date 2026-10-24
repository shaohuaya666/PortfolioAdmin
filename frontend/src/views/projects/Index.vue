<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">项目管理</h2>
        <p class="page-subtitle !mb-0">维护项目经历与关联技能</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500" v-permission="'projects:create'">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
        新增项目
      </el-button>
    </div>

    <div class="space-y-4">
      <div v-for="item in list" :key="item.id" class="card-panel flex items-center gap-6 group">
        <div class="project-icon">
          <span class="material-symbols-outlined">deployed_code</span>
        </div>
        <div class="flex-1 min-w-0">
          <h3 class="text-sm text-[#c8d6e5] font-semibold">{{ item.title }}</h3>
          <p class="text-xs text-[#64748b] mt-1">{{ item.desc }}</p>
        </div>
        <div class="shrink-0">
          <span class="tag-cyan !text-[10px]">{{ item.type }}</span>
        </div>
        <div class="shrink-0 text-xs text-[#64748b] font-mono w-20 text-right">{{ item.year }}</div>
        <div class="flex flex-wrap gap-1 max-w-[200px]">
          <span v-for="skill in item.skills" :key="skill.id" class="skill-tag">{{ skill.name }}</span>
        </div>
        <div class="flex gap-1 opacity-0 group-hover:opacity-100 shrink-0">
          <el-button size="small" text @click="openDialog(item)" v-permission="'projects:edit'">
            <span class="material-symbols-outlined text-sm">edit</span>
          </el-button>
          <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)" v-permission="'projects:delete'">
            <span class="material-symbols-outlined text-sm">delete</span>
          </el-button>
        </div>
      </div>
    </div>

    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑项目' : '新增项目'" width="560px" top="10vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="70px">
        <el-form-item label="编号" prop="id">
          <el-input v-model="form.id" :disabled="isEdit" placeholder="如: new-project" />
        </el-form-item>
        <el-form-item label="名称" prop="title">
          <el-input v-model="form.title" placeholder="项目名称" />
        </el-form-item>
        <el-form-item label="类型" prop="type">
          <el-input v-model="form.type" placeholder="如: 企业系统 / 研发管理" />
        </el-form-item>
        <el-form-item label="年份" prop="year">
          <el-input v-model="form.year" placeholder="如: 2025-至今" />
        </el-form-item>
        <el-form-item label="描述" prop="desc">
          <el-input v-model="form.desc" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="技能">
          <div class="flex flex-wrap gap-2 mb-2">
            <el-tag v-for="(skill, i) in form.skills" :key="i" closable @close="form.skills.splice(i, 1)" size="small">{{ skill }}</el-tag>
          </div>
          <div class="flex gap-2">
            <el-input v-model="newSkill" placeholder="输入技能名称" size="small" style="width:150px" @keyup.enter="addSkill" />
            <el-button size="small" @click="addSkill">添加</el-button>
          </div>
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
import { projectsApi, projectSkillsApi } from '@/api/modules/projects'
import type { CompactProject } from '@/types'

const list = ref<CompactProject[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const newSkill = ref('')

const form = reactive({ id: '', title: '', type: '', year: '', desc: '', skills: [] as string[] })
const rules: FormRules = {
  id: [{ required: true, message: '请输入编号', trigger: 'blur' }],
  title: [{ required: true, message: '请输入项目名称', trigger: 'blur' }],
  type: [{ required: true, message: '请输入项目类型', trigger: 'blur' }],
  year: [{ required: true, message: '请输入年份', trigger: 'blur' }],
  desc: [{ required: true, message: '请输入项目描述', trigger: 'blur' }],
}

async function loadData() {
  const res = await projectsApi.getAll()
  list.value = res.data
}

function addSkill() {
  if (newSkill.value.trim()) {
    form.skills.push(newSkill.value.trim())
    newSkill.value = ''
  }
}

function openDialog(item?: CompactProject) {
  if (item) {
    isEdit.value = true
    form.id = item.id; form.title = item.title; form.type = item.type; form.year = item.year; form.desc = item.desc
    form.skills = item.skills?.map(s => s.name) || []
  } else {
    isEdit.value = false
    form.id = ''; form.title = ''; form.type = ''; form.year = ''; form.desc = ''
    form.skills = []
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await projectsApi.update(form.id, { title: form.title, type: form.type, year: form.year, desc: form.desc })
    } else {
      await projectsApi.create({ id: form.id, title: form.title, type: form.type, year: form.year, desc: form.desc })
    }
    const existing = isEdit.value ? (list.value.find(p => p.id === form.id)?.skills || []) : []
    const existingNames = existing.map(s => s.name)
    const newNames = form.skills
    for (const skill of existing) {
      if (!newNames.includes(skill.name)) await projectSkillsApi.delete(skill.id).catch(() => {})
    }
    for (const name of newNames) {
      if (!existingNames.includes(name)) await projectSkillsApi.create({ name, projectId: form.id })
    }
    ElMessage.success(isEdit.value ? '更新成功' : '创建成功')
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定删除该项目？', '确认', { type: 'warning' })
  await projectsApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.project-icon {
  width: 44px; height: 44px; border-radius: 10px;
  background: rgba(6, 182, 212, 0.08);
  display: flex; align-items: center; justify-content: center;
  color: #06b6d4; flex-shrink: 0;
}
.skill-tag {
  font-size: 10px; padding: 1px 8px; border-radius: 9999px;
  background: rgba(6, 182, 212, 0.08); color: #22d3ee;
}
</style>
