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

    <div class="space-y-3">
      <div v-for="item in list" :key="item.id" class="card-panel flex items-center gap-5 group">
        <div class="user-avatar">
          <span class="material-symbols-outlined">person</span>
        </div>
        <div class="flex-1 min-w-0">
          <span class="text-sm text-[#c8d6e5] font-semibold">{{ item.username }}</span>
        </div>
        <div class="shrink-0">
          <span class="tag-cyan">{{ item.roleName }}</span>
        </div>
        <div class="shrink-0 text-xs font-mono text-[#64748b] w-36 text-right">{{ new Date(item.createdAt).toLocaleString('zh-CN') }}</div>
        <div class="flex gap-1 opacity-0 group-hover:opacity-100 shrink-0">
          <el-button size="small" text @click="openDialog(item)">编辑</el-button>
          <el-button size="small" text class="!text-yellow-400" @click="openResetPwd(item.id)">重置密码</el-button>
          <el-button size="small" text class="!text-red-400" @click="handleDelete(item.id)">
            <span class="material-symbols-outlined text-sm">delete</span>
          </el-button>
        </div>
      </div>
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

<style scoped>
.user-avatar {
  width: 40px; height: 40px; border-radius: 10px;
  background: rgba(6, 182, 212, 0.08);
  display: flex; align-items: center; justify-content: center;
  color: #06b6d4; flex-shrink: 0;
}
.user-avatar .material-symbols-outlined { font-size: 20px; }
</style>
