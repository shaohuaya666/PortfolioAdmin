<template>
  <div>
    <div class="flex items-center justify-between mb-6">
      <div>
        <h2 class="page-title">用户管理</h2>
        <p class="page-subtitle !mb-0">管理后台用户账号与角色分配</p>
      </div>
      <el-button type="primary" @click="openDialog()" class="!bg-cyan-500 !border-cyan-500">
        <span class="material-symbols-outlined text-sm mr-1" style="font-size:16px;vertical-align:middle;">person_add</span>
        新增用户
      </el-button>
    </div>

    <div class="card-panel overflow-x-auto">
      <el-table :data="list" style="width: 100%" size="small" class="data-table" row-style="background: transparent;">
        <el-table-column type="index" label="#" width="50" />
        <el-table-column prop="username" label="用户名" width="160">
          <template #default="{ row }">
            <span class="font-medium text-cyan-400">{{ row.username }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="roleName" label="角色" width="140">
          <template #default="{ row }">
            <span class="tag-cyan">{{ row.roleName }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180">
          <template #default="{ row }">
            <span class="text-xs font-mono text-slate-400">{{ new Date(row.createdAt).toLocaleString('zh-CN') }}</span>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" text @click="openDialog(row)">编辑</el-button>
            <el-button size="small" text class="!text-yellow-400" @click="openResetPwd(row.id)">重置密码</el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDelete(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 新增/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑用户' : '新增用户'" width="440px" top="15vh">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="用户名" prop="username">
          <el-input v-model="form.username" placeholder="请输入用户名" />
        </el-form-item>
        <el-form-item v-if="!isEdit" label="密码" prop="password">
          <el-input v-model="form.password" type="password" show-password placeholder="至少6位" />
        </el-form-item>
        <el-form-item label="角色" prop="roleId">
          <el-select v-model="form.roleId" class="!w-full" placeholder="请选择角色">
            <el-option v-for="r in roleOptions" :key="r.id" :label="r.name" :value="r.id" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave" :loading="saving">保存</el-button>
      </template>
    </el-dialog>

    <!-- 重置密码弹窗 -->
    <el-dialog v-model="resetVisible" title="重置密码" width="380px" top="20vh">
      <el-form :model="resetForm" :rules="resetRules" label-width="80px">
        <el-form-item label="新密码" prop="newPassword">
          <el-input v-model="resetForm.newPassword" type="password" show-password placeholder="至少6位" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="resetVisible = false">取消</el-button>
        <el-button type="primary" @click="handleResetPwd" :loading="saving">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { usersApi } from '@/api/modules/users'
import { rolesApi } from '@/api/modules/roles'
import type { UserDto, Role } from '@/types'

const list = ref<UserDto[]>([])
const roleOptions = ref<Role[]>([])
const dialogVisible = ref(false)
const resetVisible = ref(false)
const isEdit = ref(false)
const saving = ref(false)
const formRef = ref<FormInstance>()
const editId = ref(0)
const resetUserId = ref(0)

const form = reactive({ username: '', password: '', roleId: 0 as number })
const rules: FormRules = {
  username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, min: 6, message: '密码至少6位', trigger: 'blur' }],
  roleId: [{ required: true, message: '请选择角色', trigger: 'change' }],
}
const resetForm = reactive({ newPassword: '' })
const resetRules: FormRules = {
  newPassword: [{ required: true, min: 6, message: '密码至少6位', trigger: 'blur' }],
}

async function loadData() {
  const [userRes, roleRes] = await Promise.all([usersApi.getAll(), rolesApi.getAll()])
  list.value = userRes.data
  roleOptions.value = roleRes.data
}

function openDialog(item?: UserDto) {
  if (item) {
    isEdit.value = true
    editId.value = item.id
    form.username = item.username
    form.roleId = item.roleId
    form.password = ''
  } else {
    isEdit.value = false
    editId.value = 0
    form.username = ''
    form.password = ''
    form.roleId = roleOptions.value[0]?.id || 0
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (isEdit.value) {
      await usersApi.update(editId.value, { username: form.username, roleId: form.roleId })
      ElMessage.success('更新成功')
    } else {
      await usersApi.create({ username: form.username, password: form.password, roleId: form.roleId })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadData()
  } finally { saving.value = false }
}

function openResetPwd(id: number) {
  resetUserId.value = id
  resetForm.newPassword = ''
  resetVisible.value = true
}

async function handleResetPwd() {
  if (!resetForm.newPassword || resetForm.newPassword.length < 6) {
    ElMessage.warning('密码至少6位')
    return
  }
  saving.value = true
  try {
    await usersApi.resetPassword(resetUserId.value, { newPassword: resetForm.newPassword })
    ElMessage.success('密码已重置')
    resetVisible.value = false
  } finally { saving.value = false }
}

async function handleDelete(id: number) {
  await ElMessageBox.confirm('确定删除？', '确认', { type: 'warning' })
  await usersApi.delete(id)
  ElMessage.success('删除成功')
  await loadData()
}

onMounted(loadData)
</script>
