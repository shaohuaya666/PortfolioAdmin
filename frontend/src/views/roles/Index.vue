<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">角色管理</h2>
        <p class="page-subtitle !mb-0">管理角色与菜单权限分配</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
        新增角色
      </el-button>
    </div>

    <div class="card-panel overflow-x-auto">
      <el-table :data="list" style="width: 100%" size="small" class="data-table" row-style="background: transparent;">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="name" label="角色名" width="160">
          <template #default="{ row }">
            <span class="font-medium text-cyan-400">{{ row.name }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="text-xs text-slate-400">{{ row.description || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">
            <span class="text-xs font-mono text-slate-400">{{ new Date(row.createdAt).toLocaleString('zh-CN') }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="openDialog(row)">编辑</el-button>
            <el-button size="small" text class="!text-cyan-400" @click="openMenuDialog(row)">菜单权限</el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDelete(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑角色' : '新增角色'" width="440px" top="15vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="角色名" prop="name">
          <el-input v-model="form.name" placeholder="如: 编辑员" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input v-model="form.description" placeholder="可选" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave" :loading="saving">保存</el-button>
      </template>
    </el-dialog>

    <!-- 菜单权限分配弹窗 -->
    <el-dialog v-model="menuVisible" title="菜单权限分配" width="500px" top="12vh">
      <div class="menu-tree-wrapper">
        <el-tree
          ref="treeRef"
          :data="menuTree"
          show-checkbox
          node-key="id"
          default-expand-all
          :props="{ children: 'children', label: 'name' }"
          :check-strictly="false"
          highlight-current
        />
      </div>
      <template #footer>
        <el-button @click="menuVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveMenu" :loading="saving">保存权限</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import type { ElTree } from 'element-plus'
import { rolesApi } from '@/api/modules/roles'
import { menusApi } from '@/api/modules/menus'
import type { Role, MenuTreeNode } from '@/types'

const list = ref<Role[]>([])
const menuTree = ref<MenuTreeNode[]>([])
const dialogVisible = ref(false)
const menuVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const treeRef = ref<InstanceType<typeof ElTree>>()
const editId = ref(0)
const roleIdForMenu = ref(0)

const form = reactive({ name: '', description: '' })
const rules: FormRules = {
  name: [{ required: true, message: '请输入角色名', trigger: 'blur' }],
}

async function loadData() {
  const res = await rolesApi.getAll()
  list.value = res.data
}

async function prepareMenuTree() {
  const res = await menusApi.getTree()
  menuTree.value = res.data
}

function openDialog(item?: Role) {
  if (item) {
    isEdit.value = true
    editId.value = item.id
    form.name = item.name
    form.description = item.description || ''
  } else {
    isEdit.value = false
    editId.value = 0
    form.name = ''
    form.description = ''
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await rolesApi.update(editId.value, { name: form.name, description: form.description })
      ElMessage.success('更新成功')
    } else {
      await rolesApi.create({ name: form.name, description: form.description })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

async function openMenuDialog(role: Role) {
  roleIdForMenu.value = role.id
  await prepareMenuTree()
  const res = await rolesApi.getMenus(role.id)
  const checkedIds = res.data.menuIds
  menuVisible.value = true
  // 等 DOM 更新后设置选中
  await nextTick()
  treeRef.value?.setCheckedKeys(checkedIds)
}

async function handleSaveMenu() {
  const checkedIds = treeRef.value?.getCheckedKeys() as number[] || []
  const halfCheckedIds = treeRef.value?.getHalfCheckedKeys() as number[] || []
  const allIds = [...checkedIds, ...halfCheckedIds]
  saving.value = true
  try {
    await rolesApi.assignMenus({ roleId: roleIdForMenu.value, menuIds: allIds })
    ElMessage.success('权限保存成功')
    menuVisible.value = false
  } finally { saving.value = false }
}

async function handleDelete(id: number) {
  await ElMessageBox.confirm('确定删除？', '确认', { type: 'warning' })
  await rolesApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

import { nextTick } from 'vue'
onMounted(loadData)
</script>

<style scoped>
.menu-tree-wrapper {
  max-height: 400px;
  overflow-y: auto;
  padding: 8px 0;
}
:deep(.menu-tree-wrapper .el-tree) {
  background: transparent;
}
:deep(.menu-tree-wrapper .el-tree-node__content) {
  height: 32px;
}
:deep(.menu-tree-wrapper .el-tree-node__label) {
  color: #c8d6e5;
  font-size: 13px;
}
</style>
