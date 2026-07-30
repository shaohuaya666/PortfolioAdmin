<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">核心优势</h2>
        <p class="page-subtitle !mb-0">管理个人核心竞争力展示</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500">
        <span class="material-symbols-outlined text-sm mr-1 align-middle" style="font-size:16px;vertical-align:middle;">add</span>
        新增优势
      </el-button>
    </div>

    <div class="grid grid-cols-2 gap-4">
      <div v-for="item in list" :key="item.id" class="card-panel flex items-start gap-4 group">
        <div class="num-badge">{{ item.num }}</div>
        <div class="flex-1 min-w-0">
          <h3 class="text-[#c8d6e5] text-sm font-semibold mb-1">{{ item.title }}</h3>
          <p class="text-[#64748b] text-xs leading-relaxed">{{ item.desc }}</p>
        </div>
        <div class="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <el-button size="small" text @click="openDialog(item)">
            <span class="material-symbols-outlined text-sm">edit</span>
          </el-button>
          <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)">
            <span class="material-symbols-outlined text-sm">delete</span>
          </el-button>
        </div>
      </div>
    </div>

    <!-- 编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑优势' : '新增优势'" width="500px" top="15vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="60px">
        <el-form-item label="编号" prop="id">
          <el-input v-model="form.id" :disabled="isEdit" placeholder="如: adv-9" />
        </el-form-item>
        <el-form-item label="序号" prop="num">
          <el-input v-model="form.num" placeholder="如: 09" />
        </el-form-item>
        <el-form-item label="标题" prop="title">
          <el-input v-model="form.title" placeholder="优势标题" />
        </el-form-item>
        <el-form-item label="描述" prop="desc">
          <el-input v-model="form.desc" type="textarea" :rows="3" placeholder="优势描述" />
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
import { advantagesApi } from '@/api/modules/advantages'
import type { Advantage } from '@/types'

const list = ref<Advantage[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive({ id: '', num: '', title: '', desc: '' })
const rules: FormRules = {
  id: [{ required: true, message: '请输入编号' }],
  num: [{ required: true, message: '请输入序号' }],
  title: [{ required: true, message: '请输入标题' }],
  desc: [{ required: true, message: '请输入描述' }],
}

async function loadData() {
  const res = await advantagesApi.getAll()
  list.value = res.data
}

function openDialog(item?: Advantage) {
  if (item) {
    isEdit.value = true
    form.id = item.id; form.num = item.num; form.title = item.title; form.desc = item.desc
  } else {
    isEdit.value = false
    form.id = ''; form.num = ''; form.title = ''; form.desc = ''
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await advantagesApi.update(form.id, { num: form.num, title: form.title, desc: form.desc })
      ElMessage.success('更新成功')
    } else {
      await advantagesApi.create(form)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

async function handleDelete(id: string) {
  await ElMessageBox.confirm('确定删除该优势？', '确认', { type: 'warning' })
  await advantagesApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.num-badge {
  font-family: 'JetBrains Mono', monospace;
  font-size: 16px;
  font-weight: 700;
  color: #06b6d4;
  opacity: 0.6;
  flex-shrink: 0;
  padding-top: 2px;
}
</style>
