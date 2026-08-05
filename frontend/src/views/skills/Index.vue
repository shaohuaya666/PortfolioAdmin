<template>
  <div>
    <h2 class="page-title">技术栈矩阵</h2>
    <p class="page-subtitle">管理技能分类与标签</p>

    <div class="grid grid-cols-2 gap-6">
      <div class="card-panel">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-[rgba(6,182,212,0.08)]">
          <h3 class="text-[#c8d6e5] font-semibold text-sm flex items-center gap-2">
            <span class="material-symbols-outlined text-cyan-500 text-sm">folder</span> 技能分类
          </h3>
          <el-button size="small" class="add-btn" @click="openCategoryDialog()" v-permission="'skills:create'">+ 添加</el-button>
        </div>
        <div v-for="cat in categories" :key="cat.id" class="category-item group">
          <div class="flex-1">
            <div class="text-sm text-[#c8d6e5] font-medium">{{ cat.title }}</div>
            <div class="text-xs text-[#64748b] mt-0.5">{{ cat.tags?.length || 0 }} 个标签</div>
          </div>
          <div class="flex gap-1 opacity-0 group-hover:opacity-100">
            <el-button size="small" text @click="openCategoryDialog(cat)" v-permission="'skills:edit'">
              <span class="material-symbols-outlined text-xs">edit</span>
            </el-button>
            <el-button size="small" text class="!text-red-400" @click="handleDeleteCategory(cat.id)" v-permission="'skills:delete'">
              <span class="material-symbols-outlined text-xs">delete</span>
            </el-button>
          </div>
        </div>
      </div>

      <div class="card-panel">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-[rgba(6,182,212,0.08)]">
          <h3 class="text-[#c8d6e5] font-semibold text-sm flex items-center gap-2">
            <span class="material-symbols-outlined text-cyan-500 text-sm">sell</span> 技术标签
          </h3>
          <el-button size="small" class="add-btn" @click="openTagDialog()" v-permission="'skills:tags_create'">+ 添加</el-button>
        </div>
        <div class="mb-3">
          <el-select v-model="filterCategoryId" placeholder="按分类筛选" size="small" clearable style="width:100%" @change="loadTags">
            <el-option v-for="cat in categories" :key="cat.id" :label="cat.title" :value="cat.id" />
          </el-select>
        </div>
        <div class="flex flex-wrap gap-2">
          <span v-for="tag in tags" :key="tag.id" class="tag-cyan cursor-pointer" :class="{ 'tag-cyan-core': tag.isCore }" @click="openTagDialog(tag)">
            {{ tag.name }}
            <el-button size="small" text style="color:#ef4444;padding:0;margin-left:4px" @click.stop="handleDeleteTag(tag.id)" v-permission="'skills:tags_delete'">
              <span class="material-symbols-outlined text-xs">close</span>
            </el-button>
          </span>
          <span v-if="tags.length === 0" class="text-xs text-[#64748b]">暂无标签</span>
        </div>
      </div>
    </div>

    <!-- 分类弹窗 -->
    <el-dialog v-model="catDialogVisible" :title="catIsEdit ? '编辑分类' : '新增分类'" width="420px" top="15vh">
      <el-form :model="catForm" label-width="60px">
        <el-form-item label="名称"><el-input v-model="catForm.title" placeholder="分类名称" /></el-form-item>
        <el-form-item label="颜色"><el-input v-model="catForm.themeColor" placeholder="primary / secondary" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="catDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveCategory" :loading="catSaving">保存</el-button>
      </template>
    </el-dialog>

    <!-- 标签弹窗 -->
    <el-dialog v-model="tagDialogVisible" :title="tagIsEdit ? '编辑标签' : '新增标签'" width="420px" top="15vh">
      <el-form :model="tagForm" label-width="60px">
        <el-form-item label="名称"><el-input v-model="tagForm.name" placeholder="标签名称" /></el-form-item>
        <el-form-item label="分类">
          <el-select v-model="tagForm.skillCategoryId" placeholder="选择分类" style="width:100%">
            <el-option v-for="cat in categories" :key="cat.id" :label="cat.title" :value="cat.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="核心"><el-switch v-model="tagForm.isCore" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="tagDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSaveTag" :loading="tagSaving">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { skillCategoriesApi, tagsApi } from '@/api/modules/skills'
import type { SkillCategory, TagInfo } from '@/types'

const categories = ref<SkillCategory[]>([])
const tags = ref<TagInfo[]>([])
const filterCategoryId = ref<number | ''>('')

const catDialogVisible = ref(false)
const catIsEdit = ref(false)
const catSaving = ref(false)
const catForm = reactive({ id: 0, title: '', themeColor: 'primary' })

const tagDialogVisible = ref(false)
const tagIsEdit = ref(false)
const tagSaving = ref(false)
const tagForm = reactive({ id: 0, name: '', isCore: false, skillCategoryId: 0 })

async function loadCategories() {
  const res = await skillCategoriesApi.getAll()
  categories.value = res.data
}

async function loadTags() {
  const cid = filterCategoryId.value || undefined
  const res = await tagsApi.getAll(cid)
  tags.value = res.data
}

function openCategoryDialog(cat?: SkillCategory) {
  if (cat) {
    catIsEdit.value = true
    catForm.id = cat.id; catForm.title = cat.title; catForm.themeColor = cat.themeColor
  } else {
    catIsEdit.value = false
    catForm.id = 0; catForm.title = ''; catForm.themeColor = 'primary'
  }
  catDialogVisible.value = true
}

async function handleSaveCategory() {
  catSaving.value = true
  try {
    if (catIsEdit.value) {
      await skillCategoriesApi.update(catForm.id, { title: catForm.title, themeColor: catForm.themeColor })
      ElMessage.success('更新成功')
    } else {
      await skillCategoriesApi.create({ title: catForm.title, themeColor: catForm.themeColor })
      ElMessage.success('创建成功')
    }
    catDialogVisible.value = false
    await loadCategories()
  } finally { catSaving.value = false }
}

async function handleDeleteCategory(id: number) {
  await ElMessageBox.confirm('删除分类将同时删除其下所有标签', '确认', { type: 'warning' })
  await skillCategoriesApi.delete(id)
  ElMessage.success('删除成功')
  await loadCategories()
  await loadTags()
}

function openTagDialog(tag?: TagInfo) {
  if (tag) {
    tagIsEdit.value = true
    tagForm.id = tag.id; tagForm.name = tag.name; tagForm.isCore = tag.isCore; tagForm.skillCategoryId = tag.skillCategoryId
  } else {
    tagIsEdit.value = false
    tagForm.id = 0; tagForm.name = ''; tagForm.isCore = false
    tagForm.skillCategoryId = filterCategoryId.value ? Number(filterCategoryId.value) : (categories.value[0]?.id || 0)
  }
  tagDialogVisible.value = true
}

async function handleSaveTag() {
  tagSaving.value = true
  try {
    if (tagIsEdit.value) {
      await tagsApi.update(tagForm.id, { name: tagForm.name, isCore: tagForm.isCore, skillCategoryId: tagForm.skillCategoryId })
      ElMessage.success('更新成功')
    } else {
      await tagsApi.create({ name: tagForm.name, isCore: tagForm.isCore, skillCategoryId: tagForm.skillCategoryId })
      ElMessage.success('创建成功')
    }
    tagDialogVisible.value = false
    await loadTags()
    await loadCategories()
  } finally { tagSaving.value = false }
}

async function handleDeleteTag(id: number) {
  await ElMessageBox.confirm('确定删除该标签？', '确认', { type: 'warning' })
  await tagsApi.delete(id)
  ElMessage.success('删除成功')
  await loadTags()
  await loadCategories()
}

onMounted(async () => { await loadCategories(); await loadTags() })
</script>

<style scoped>
.category-item {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  border-radius: 8px;
  transition: background 0.2s;
}
.category-item:hover { background: rgba(6, 182, 212, 0.04); }
.add-btn {
  background: rgba(6, 182, 212, 0.2) !important;
  border-color: rgba(6, 182, 212, 0.3) !important;
  color: #22d3ee !important;
  font-size: 12px !important;
}
</style>
