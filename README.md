# PortfolioAdmin 个人作品集后台管理系统

前后端分离的个人作品集（简历数据）管理后台。后端基于 **.NET 8 Web API** 提供 RESTful 接口，前端基于 **Vue 3 + TypeScript + Element Plus** 构建深色主题管理界面，用于维护个人核心优势、技术栈、项目、工作经历等作品集数据，并内置 **RBAC 权限体系**（用户 / 角色 / 菜单 + 按钮级权限码）。
  
## 功能特性

- **内容管理（作品集数据）**
  - 仪表盘：数据统计总览
  - 核心优势：个人亮点展示管理
  - 技术栈矩阵：技能分类 + 技能标签两级维护
  - 项目管理：项目 + 项目技术栈关联
  - 工作经历：经历 + 成就 / 亮点关联
  - 技能诊断：技能评估记录管理
- **RBAC 权限体系**
  - 用户管理：增删改查、重置密码、分配角色
  - 角色管理：角色维护、菜单 / 按钮权限分配
  - 菜单管理：菜单与操作按钮（`Type=menu/action`）及权限码配置
  - 登录后返回**动态菜单树 + 权限码列表**，前端侧边栏动态渲染，按钮级权限由 `v-permission` 指令控制，接口层由 `[RequirePermission]` 特性二次校验
- **认证与安全**
  - JWT Bearer 认证（令牌 8 小时有效，前后端分离无状态）
  - 登录滑块验证码（先验证获取凭据，登录成功后才消耗，避免输错密码重复滑动）
  - 密码 PBKDF2-SHA256 加盐哈希存储
  - 登录后支持修改密码

## 技术栈

| 端 | 技术 |
| --- | --- |
| 后端 | .NET 8 / ASP.NET Core Web API / EF Core 8 + Pomelo (MySQL) / JWT Bearer / Swashbuckle (Swagger) |
| 前端 | Vue 3.4 / TypeScript 5.5 / Vite 5 / Element Plus 2.7 / Pinia 2 / Vue Router 4 / TailwindCSS 3 / Axios |
| 数据库 | MySQL 8（库名 `BlazorPortfolio`） |

## 目录结构

```
PortfolioAdmin/
├── backend/                        # 后端服务
│   ├── PortfolioAdmin.Api/
│   │   ├── Attributes/             # RequirePermissionAttribute 权限码校验
│   │   ├── Controllers/            # 15 个 API 控制器（Auth/Captcha/Dashboard/Users/Roles/Menus/RoleMenus/内容 CRUD）
│   │   ├── Data/                   # PortfolioDbContext（EF Core，反射自动注册实体与级联关系）
│   │   ├── DTOs/                   # 入参出参（AuthDTOs/CaptchaDTOs/RequestDTOs/RoleMenuDTOs）
│   │   ├── Middleware/             # IJwtService / JwtHelper（JWT 生成与解析）
│   │   ├── Models/                 # 实体（内容 8 张表 + RBAC 4 张表）
│   │   ├── Services/               # 服务接口与实现（IAuthService/IUserService/IRoleMenuService/IDashboardService/IPortfolioService/ICaptchaService）
│   │   ├── Utils/                  # PasswordHelper（PBKDF2）、CaptchaService（滑块验证码）
│   │   ├── Program.cs              # 启动入口（CORS/JWT/Swagger/DbContext/服务注册）
│   │   ├── appsettings.json        # 数据库连接串、JWT 密钥、令牌时效配置
│   │   └── PortfolioAdmin.Api.csproj
│   └── init_data.sql               # RBAC 增量初始化脚本（建表 + 默认数据）
└── frontend/                       # 前端工程（Vite）
    ├── src/
    │   ├── api/                    # request.ts（axios 封装）+ modules/（按模块划分 10 个）
    │   ├── components/             # SliderCaptcha.vue 滑块验证码组件
    │   ├── directives/             # permission.ts（v-permission 按钮权限指令）
    │   ├── layout/                 # MainLayout.vue（动态侧边栏菜单 + 面包屑 + 修改密码）
    │   ├── router/                 # 路由与登录守卫
    │   ├── stores/                 # Pinia（auth：token/用户/菜单/权限码，localStorage 持久化）
    │   ├── styles/                 # 全局样式
    │   ├── types/                  # TS 类型定义
    │   ├── views/                  # login + 9 个业务页面（Index.vue）
    │   ├── App.vue
    │   └── main.ts
    ├── vite.config.ts              # @ 别名、dev 端口 5173、/api 代理到 5273
    ├── tailwind.config.js
    ├── tsconfig.json
    └── package.json
```

## 功能模块

### 页面菜单（登录后按角色动态渲染）

| 菜单 | 路由 | 说明 |
| --- | --- | --- |
| 仪表盘 | `/dashboard` | 数据统计总览 |
| 核心优势 | `/advantages` | 个人优势亮点 |
| 技术栈矩阵 | `/skills` | 技能分类 + 标签（Tags） |
| 项目管理 | `/projects` | 项目 + 项目技术栈（ProjectSkills） |
| 工作经历 | `/work-history` | 经历 + 成就（Achievements） |
| 技能诊断 | `/diagnostics` | 技能诊断记录 |
| 用户管理 | `/users` | 用户 CRUD / 重置密码 / 分配角色 |
| 角色管理 | `/roles` | 角色维护 / 菜单权限分配 |
| 菜单管理 | `/menus` | 菜单与按钮（权限码）维护 |

### 权限模型

- 菜单分两级：`Type=menu`（页面菜单，进入页面需 `xx:view`）与 `Type=action`（操作按钮，如 `projects:create`）
- 登录时后端按角色返回**菜单树**与**权限码列表**；前端 `v-permission="'projects:create'"` 控制按钮显隐
- 接口层使用 `[RequirePermission("projects:create")]` 做二次鉴权（`Order=100`，在认证过滤器之后执行），未授权返回 403

## 数据库

库名：`BlazorPortfolio`（MySQL 8）。共 12 张表：

- 作品集内容（8）：`Advantages`、`SkillCategories`、`Tags`、`WorkHistories`、`Achievements`、`CompactProjects`、`ProjectSkills`、`SkillDiagnostics`
- RBAC（4）：`Users`、`Roles`、`Menus`、`RoleMenus`

表结构由 EF Core 实体（`Models/`）通过 DataAnnotation + `OnModelCreating` 反射自动注册映射；`backend/init_data.sql` 负责 RBAC 的增量初始化：为 `Users` 补 `RoleId` 列、创建 `Roles` / `Menus` / `RoleMenus` 表、写入「超级管理员」角色、9 个页面菜单 + 操作按钮权限码并全量授权。

## 主要接口

基础路径 `http://localhost:5273/api`，Swagger：`http://localhost:5273/swagger`

| 分组 | 控制器 | 接口 |
| --- | --- | --- |
| 认证 | `AuthController` | `POST /auth/login`（携带 captchaId）、`POST /auth/change-password` |
| 验证码 | `CaptchaController` | `GET /captcha/generate`、`POST /captcha/verify` |
| RBAC | `UsersController` | 用户 CRUD / 重置密码 / 分配角色 |
| | `RolesController` | 角色 CRUD |
| | `MenusController` | 菜单 / 按钮 CRUD |
| | `RoleMenusController` | 角色授权（勾选菜单） |
| 内容 | `AdvantagesController` / `SkillCategoriesController` / `TagsController` / `CompactProjectsController` / `ProjectSkillsController` / `WorkHistoriesController` / `AchievementsController` / `SkillDiagnosticsController` | 各模块 CRUD（需对应权限码） |
| 统计 | `DashboardController` | 仪表盘统计数据 |

## 快速开始

### 环境要求

- .NET 8 SDK
- Node.js 18+
- MySQL 8


### 1. 启动后端

```bash
cd backend/PortfolioAdmin.Api
# 修改 appsettings.json 中的 ConnectionStrings:DefaultConnection
dotnet run
```

- API：`http://localhost:5273`
- Swagger 文档：`http://localhost:5273/swagger`

### 2. 启动前端

```bash
cd frontend
npm install
npm run dev
```

- 管理后台：`http://localhost:5173`（`/api` 已代理到后端 `5273`）

### 默认账号

| 用户名 | 密码 | 角色 |
| --- | --- | --- |
| `audience‌` | `123456` | 看客（拥有查看权限） |

## 说明

- 前端采用深色主题（侧边栏 `#061829`、主色青 `#06b6d4`），图标使用 Material Symbols Outlined 字体，需联网加载 Google Fonts
- 后端 JSON 输出统一 camelCase，循环引用自动忽略
- JWT 默认有效期 480 分钟，过期或未携带 Token 访问受保护接口返回 401，前端自动跳转登录页
