<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">技能诊断</h2>
        <p class="page-subtitle !mb-0">管理技能详细诊断数据</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500" v-permission="'diagnostics:create'">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
        新增诊断
      </el-button>
    </div>

    <div class="card-panel overflow-x-auto">
      <el-table :data="list" style="width: 100%" size="small" class="diagnostic-table" row-style="background: transparent;">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="tagName" label="技能标签" width="140">
          <template #default="{ row }">
            <span class="tag-cyan">{{ row.tagName }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="desc" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="stat" label="统计数据" width="120">
          <template #default="{ row }">
            <span class="font-mono text-xs text-cyan-400">{{ row.stat }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <span class="text-xs" :class="row.status === '精通' ? 'text-green-400' : row.status === '熟练' ? 'text-cyan-400' : 'text-slate-400'">{{ row.status }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="openDialog(row)" v-permission="'diagnostics:edit'">编辑</el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDelete(row.id)" v-permission="'diagnostics:delete'">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑诊断' : '新增诊断'" width="480px" top="15vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="70px">
        <el-form-item label="标签名" prop="tagName">
          <el-input v-model="form.tagName" placeholder="如: C# / ASP.NET" />
        </el-form-item>
        <el-form-item label="描述" prop="desc">
          <el-input v-model="form.desc" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="数据" prop="stat">
          <el-input v-model="form.stat" placeholder="如: 6年 / 30+项目" />
        </el-form-item>
        <el-form-item label="状态" prop="status">
          <el-select v-model="form.status" class="!w-full">
            <el-option label="精通" value="精通" />
            <el-option label="熟练" value="熟练" />
            <el-option label="了解" value="了解" />
          </el-select>
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
import { diagnosticsApi } from '@/api/modules/diagnostics'
import type { SkillDiagnostic } from '@/types'

const list = ref<SkillDiagnostic[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive({ id: 0, tagName: '', desc: '', stat: '', status: '熟练' })
const rules: FormRules = {
  tagName: [{ required: true, message: '请输入标签名', trigger: 'blur' }],
  desc: [{ required: true, message: '请输入描述', trigger: 'blur' }],
  stat: [{ required: true, message: '请输入统计数据', trigger: 'blur' }],
  status: [{ required: true, message: '请选择状态', trigger: 'change' }],
}

async function loadData() {
  const res = await diagnosticsApi.getAll()
  list.value = res.data
}

function openDialog(item?: SkillDiagnostic) {
  if (item) {
    isEdit.value = true
    form.id = item.id; form.tagName = item.tagName; form.desc = item.desc; form.stat = item.stat; form.status = item.status
  } else {
    isEdit.value = false
    form.id = 0; form.tagName = ''; form.desc = ''; form.stat = ''; form.status = '熟练'
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await diagnosticsApi.update(form.id, { tagName: form.tagName, desc: form.desc, stat: form.stat, status: form.status })
      ElMessage.success('更新成功')
    } else {
      await diagnosticsApi.create({ tagName: form.tagName, desc: form.desc, stat: form.stat, status: form.status })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

async function handleDelete(id: number) {
  await ElMessageBox.confirm('确定删除？', '确认', { type: 'warning' })
  await diagnosticsApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.diagnostic-table {
  --el-table-bg-color: transparent;
  --el-table-tr-bg-color: transparent;
  --el-table-header-bg-color: rgba(6, 182, 212, 0.06);
  --el-table-row-hover-bg-color: rgba(6, 182, 212, 0.04);
  --el-table-border-color: rgba(6, 182, 212, 0.08);
  --el-table-header-text-color: #94a3b8;
  --el-table-text-color: #c8d6e5;
}
</style>
