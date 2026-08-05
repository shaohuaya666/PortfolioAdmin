<script setup lang="ts">
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import type { ElTree } from 'element-plus'
import { rolesApi } from '@/api/modules/roles'
import type { RoleItem, MenuItem } from '@/types'

const list = ref<RoleItem[]>([])
const menuTree = ref<MenuItem[]>([])
const dialogVisible = ref(false)
const menuVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const treeRef = ref<InstanceType<typeof ElTree>>()
const editId = ref(0)
const roleIdForMenu = ref(0)

// 防止 @check 事件递归触发
let skipCheck = false

const form = reactive({ name: '', description: '' })
const rules: FormRules = {
  name: [{ required: true, message: '请输入角色名', trigger: 'blur' }],
}

async function loadData() {
  const res = await rolesApi.getAll()
  list.value = res.data
}

function openDialog(item?: RoleItem) {
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

// 递归设置节点及所有子孙的勾选状态
function setCheckedDeep(node: MenuItem, checked: boolean) {
  const tree = treeRef.value
  if (!tree) return
  tree.setChecked(node.id, checked, false)
  if (node.children?.length) {
    for (const child of node.children) {
      setCheckedDeep(child, checked)
    }
  }
}

// 在树中查找父节点
function findParentInTree(nodes: MenuItem[], childId: number): MenuItem | null {
  for (const node of nodes) {
    if (node.children?.some(c => c.id === childId)) return node
    if (node.children?.length) {
      const found = findParentInTree(node.children, childId)
      if (found) return found
    }
  }
  return null
}

function handleCheck(
  data: MenuItem,
  { checkedKeys }: { checkedKeys: number[]; checkedNodes: any[]; halfCheckedKeys: number[]; halfCheckedNodes: any[] }
) {
  if (skipCheck) return
  const tree = treeRef.value
  if (!tree) return

  const isChecked = checkedKeys.includes(data.id)

  skipCheck = true

  if (data.type !== 'action') {
    // 菜单节点：勾选/取消时同步所有子节点
    if (data.children?.length) {
      setCheckedDeep(data, isChecked)
    }
  } else if (isChecked) {
    // action 被勾选 → 自动勾选父级菜单（有操作权限隐含需要看到菜单）
    const parent = findParentInTree(menuTree.value, data.id)
    if (parent && !checkedKeys.includes(parent.id)) {
      tree.setChecked(parent.id, true, false)
    }
  }
  // action 被取消 → 不取消父级菜单（关键：菜单可见性独立于操作权限）

  skipCheck = false
}

async function openMenuDialog(role: RoleItem) {
  roleIdForMenu.value = role.id
  const res = await rolesApi.getMenus(role.id)
  const data = res.data
  menuTree.value = data.menuTree || []
  const checkedIds = new Set(data.menuIds)
  menuVisible.value = true
  await nextTick()

  // 先设所有叶子节点，再补父节点（setCheckedKeys 不触发 @check）
  treeRef.value?.setCheckedKeys([])

  skipCheck = true
  // 遍历树：标记叶子节点，父菜单有任意子节点被选中就勾上
  syncTreeFromIds(menuTree.value, checkedIds)
  skipCheck = false
}

// 根据已保存的 ID 集合同步树状态（自底向上，子节点有勾选则自动勾选父菜单）
function syncTreeFromIds(nodes: MenuItem[], checkedIds: Set<number>): boolean {
  const tree = treeRef.value
  if (!tree) return false

  let anyChildChecked = false
  for (const node of nodes) {
    let childChecked = false
    if (node.children?.length) {
      childChecked = syncTreeFromIds(node.children, checkedIds)
      anyChildChecked = anyChildChecked || childChecked
    }
    if (checkedIds.has(node.id)) {
      tree.setChecked(node.id, true, false)
      anyChildChecked = true
    }
    // 关键：子节点有勾选则自动勾选父菜单（菜单可见性独立于操作权限）
    if (childChecked && !checkedIds.has(node.id)) {
      tree.setChecked(node.id, true, false)
      anyChildChecked = true
    }
  }
  return anyChildChecked
}

async function handleSaveMenu() {
  const checkedIds = treeRef.value?.getCheckedKeys() as number[] || []
  saving.value = true
  try {
    await rolesApi.assignMenus({ roleId: roleIdForMenu.value, menuIds: checkedIds })
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

onMounted(loadData)
</script>

<template>
  <div class="page-container">
    <div class="page-header">
      <h2 class="text-xl font-bold tracking-tight" style="font-family:'Outfit',sans-serif;">角色管理</h2>
    </div>
    <div class="card-glow">
      <div class="flex justify-between items-center mb-4">
        <span class="text-xs text-[#64748b]">共 {{ list.length }} 个角色</span>
        <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500" v-permission="'roles:create'">
          <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">add</span>
          新增角色
        </el-button>
      </div>
      <div class="space-y-2">
        <div v-for="item in list" :key="item.id"
          class="flex items-center justify-between p-3 rounded-lg border border-[#1e293b] bg-[#0f172a]/50 group hover:border-cyan-500/30 transition-colors">
          <div class="flex items-center gap-3">
            <span class="material-symbols-outlined text-cyan-400 text-xl">shield</span>
            <div>
              <span class="text-sm font-semibold text-[#c8d6e5]">{{ item.name }}</span>
              <span v-if="item.description" class="text-xs text-[#64748b] ml-2">{{ item.description }}</span>
            </div>
          </div>
          <div class="flex gap-1 opacity-0 group-hover:opacity-100 shrink-0">
            <el-button size="small" text @click="openDialog(item)" v-permission="'roles:edit'">编辑</el-button>
            <el-button size="small" text class="!text-cyan-400" @click="openMenuDialog(item)">菜单权限</el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)" v-permission="'roles:delete'">删除</el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑角色' : '新增角色'" width="430px" top="15vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" placeholder="如: 管理员" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" placeholder="角色描述" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave" :loading="saving">保存</el-button>
      </template>
    </el-dialog>

    <!-- 菜单权限分配弹窗 -->
    <el-dialog v-model="menuVisible" title="菜单权限分配" width="520px" top="12vh">
      <div class="menu-tree-wrapper">
        <el-tree
          ref="treeRef"
          :data="menuTree"
          show-checkbox
          node-key="id"
          default-expand-all
          :props="{ children: 'children', label: 'name' }"
          :check-strictly="true"
          highlight-current
          @check="handleCheck"
        >
          <template #default="{ data }">
            <span class="tree-node-label">
              <span>{{ data.name }}</span>
              <span v-if="data.type === 'action'" class="permission-badge">{{ data.permissionCode }}</span>
            </span>
          </template>
        </el-tree>
      </div>
      <template #footer>
        <el-button @click="menuVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveMenu" :loading="saving">保存权限</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container {
  max-width: 680px;
  margin: 0 auto;
}
.menu-tree-wrapper {
  max-height: 420px;
  overflow-y: auto;
  background: #0b1120;
  border: 1px solid #1e293b;
  border-radius: 8px;
  padding: 12px;
}
::deep(.menu-tree-wrapper) {
  background: transparent;
}
::deep(.menu-tree-wrapper .el-tree) {
  background: transparent;
}
::deep(.menu-tree-wrapper .el-tree-node__content) {
  height: 32px;
}
::deep(.menu-tree-wrapper .el-tree-node__label) {
  color: #c8d6e5;
  font-size: 13px;
}
.tree-node-label { display: inline-flex; align-items: center; gap: 6px; }
.permission-badge { font-size: 11px; color: #f59e0b; background: rgba(245, 158, 11, 0.1); padding: 0 4px; border-radius: 3px; font-family: monospace; }
</style>
