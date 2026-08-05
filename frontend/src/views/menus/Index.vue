<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">菜单管理</h2>
        <p class="page-subtitle !mb-0">管理侧边栏菜单结构与排序</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500" v-permission="'menus:create'">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
        新增菜单/按钮
      </el-button>
    </div>

    <div class="space-y-2">
      <div v-for="item in flatList" :key="item.id"
        class="card-panel flex items-center gap-4 group"
        :style="{ marginLeft: (item._level * 24) + 'px', borderLeftWidth: item._level > 0 ? '0px' : '1px', borderLeftColor: item._level > 0 ? 'transparent' : undefined }">
        <div class="menu-icon">
          <span class="material-symbols-outlined">{{ item.icon || 'circle' }}</span>
        </div>
        <div class="flex-1 min-w-0">
          <span class="text-sm font-semibold" :class="item.parentId === 0 ? 'text-[#22d3ee]' : 'text-[#c8d6e5]'">{{ item.name }}</span>
          <span v-if="item.type === 'action'" class="text-xs text-[#f59e0b] ml-2 px-1.5 py-0.5 rounded border border-[#f59e0b]/30">按钮</span>
          <span v-else-if="item.parentId !== 0" class="text-xs text-[#64748b] ml-2">└ 子菜单</span>
        </div>
        <div class="shrink-0">
          <span class="text-xs font-mono text-[#64748b]">{{ item.path || (item.type === 'action' ? item.permissionCode : '(父级分组)') }}</span>
        </div>
        <div class="shrink-0 text-xs font-mono text-[#64748b] w-12 text-center">#{{ item.sort }}</div>
        <div class="flex gap-1 opacity-0 group-hover:opacity-100 shrink-0">
          <el-button size="small" text @click="openDialog(item)" v-permission="'menus:edit'">编辑</el-button>
          <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)" v-permission="'menus:delete'">删除</el-button>
        </div>
      </div>
    </div>

    <!-- 弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑' : '新增'" width="460px" top="15vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="类型" prop="type">
          <el-radio-group v-model="form.type">
            <el-radio value="menu">菜单</el-radio>
            <el-radio value="action">按钮</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" :placeholder="form.type === 'action' ? '如: 新增项目' : '如: 系统管理'" />
        </el-form-item>
        <el-form-item label="路径" prop="path" v-if="form.type === 'menu'">
          <el-input v-model="form.path" placeholder="如: /users，父级留空" />
        </el-form-item>
        <el-form-item label="权限码" prop="permissionCode">
          <el-input v-model="form.permissionCode" placeholder="如: projects:create" />
        </el-form-item>
        <el-form-item label="图标" v-if="form.type === 'menu'">
          <el-input v-model="form.icon" placeholder="Material Icons 名称" />
        </el-form-item>
        <el-form-item label="父菜单" prop="parentId">
          <el-select v-model="form.parentId" class="!w-full" placeholder="顶级">
            <el-option label="顶级" :value="0" />
            <el-option v-for="m in parentOptions" :key="m.id" :label="m.name" :value="m.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="排序" prop="sort">
          <el-input-number v-model="form.sort" :min="0" class="!w-full" />
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
import { menusApi, type MenuCreateRequest } from '@/api/modules/menus'
import type { MenuItem } from '@/types'

interface FlatMenu extends MenuItem {
  _level: number
}

const treeData = ref<MenuItem[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const editId = ref(0)

const form = reactive<MenuCreateRequest>({ name: '', path: '', icon: '', parentId: 0, sort: 0, type: 'menu', permissionCode: '' })
const rules: FormRules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }],
  type: [{ required: true, message: '请选择类型', trigger: 'change' }],
}

const parentOptions = computed(() => {
  return treeData.value.filter(m => m.parentId === 0 && m.type !== 'action').map(m => ({ id: m.id, name: m.name }))
})

// 扁平化树用于表格展示
function flattenTree(nodes: MenuItem[], level: number): FlatMenu[] {
  const result: FlatMenu[] = []
  for (const node of nodes) {
    result.push({ ...node, _level: level })
    if (node.children && node.children.length > 0) {
      result.push(...flattenTree(node.children, level + 1))
    }
  }
  return result
}

const flatList = computed(() => flattenTree(treeData.value, 0))

async function loadData() {
  const res = await menusApi.getAll()
  treeData.value = res.data
}

function openDialog(item?: MenuItem) {
  if (item) {
    isEdit.value = true
    editId.value = item.id
    form.name = item.name
    form.path = item.path || ''
    form.icon = item.icon || ''
    form.parentId = item.parentId
    form.sort = item.sort
    form.type = item.type || 'menu'
    form.permissionCode = item.permissionCode || ''
  } else {
    isEdit.value = false
    editId.value = 0
    form.name = ''
    form.path = ''
    form.icon = ''
    form.parentId = 0
    form.sort = 0
    form.type = 'menu'
    form.permissionCode = ''
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const data: MenuCreateRequest = {
      name: form.name,
      path: form.path,
      icon: form.icon || undefined,
      parentId: form.parentId,
      sort: form.sort,
      type: form.type,
      permissionCode: form.permissionCode || undefined
    }
    if (isEdit.value) {
      await menusApi.update(editId.value, data)
      ElMessage.success('更新成功')
    } else {
      await menusApi.create(data)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

async function handleDelete(id: number) {
  await ElMessageBox.confirm('确定删除？子菜单将一并删除', '确认', { type: 'warning' })
  await menusApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>

<style scoped>
.menu-icon {
  width: 36px; height: 36px; border-radius: 8px;
  background: rgba(6, 182, 212, 0.08);
  display: flex; align-items: center; justify-content: center;
  color: #06b6d4; flex-shrink: 0;
}
.menu-icon .material-symbols-outlined { font-size: 18px; }
</style>
