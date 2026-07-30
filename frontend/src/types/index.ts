export interface Advantage {
  id: string
  num: string
  title: string
  desc: string
}

export interface SkillCategory {
  id: number
  title: string
  themeColor: string
  tags: TagInfo[]
}

export interface TagInfo {
  id: number
  name: string
  isCore: boolean
  skillCategoryId: number
}

export interface WorkHistory {
  id: string
  company: string
  role: string
  period: string
  desc: string
  isCurrent: boolean
  achievements: Achievement[]
}

export interface Achievement {
  id: number
  description: string
  workHistoryId: string
}

export interface CompactProject {
  id: string
  title: string
  type: string
  year: string
  desc: string
  skills: ProjectSkill[]
}

export interface ProjectSkill {
  id: number
  name: string
  projectId: string
}

export interface SkillDiagnostic {
  id: number
  tagName: string
  desc: string
  stat: string
  status: string
}

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  username: string
  expiresAt: string
}

export interface DashboardStats {
  projectCount: number
  skillCount: number
  workYearCount: number
  diagnosticCount: number
}
