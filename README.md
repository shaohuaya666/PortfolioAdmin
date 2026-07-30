# PortfolioAdmin

个人作品集管理后台 — 一个全栈的个人作品集/简历数据管理系统，提供可视化的后台管理界面，方便管理个人技能、项目经历、工作履历等数据。

## 技术栈

### 前端
- **Vue 3** + **TypeScript** + **Vite**
- **Element Plus** — 企业级 UI 组件库
- **Pinia** — 状态管理
- **Vue Router 4** — 路由管理
- **TailwindCSS** — 原子化 CSS 框架
- **Axios** — HTTP 请求库

### 后端
- **.NET 8** Web API (C#)
- **Entity Framework Core** — ORM
- **MySQL** (Pomelo.EntityFrameworkCore.MySql)
- **JWT** 认证
- **Swagger** API 文档

## 功能模块

| 模块 | 说明 |
|------|------|
| 仪表盘 | 数据总览，关键指标一目了然 |
| 核心优势 | 管理个人核心优势/亮点展示 |
| 技术栈矩阵 | 技能分类（主题色）+ 标签管理（核心技能标记） |
| 项目管理 | 作品项目录入，关联技术栈标签 |
| 工作经历 | 履历管理，每家公司的角色、成就维护 |
| 技能诊断 | 技能评估与状态跟踪 |

## 项目结构

```
PortfolioAdmin/
├── backend/                          # 后端 .NET 8 Web API
│   └── PortfolioAdmin.Api/
│       ├── Controllers/              # API 控制器
│       │   ├── AchievementsController.cs
│       │   ├── AdvantagesController.cs
│       │   ├── AuthController.cs
│       │   ├── CompactProjectsController.cs
│       │   ├── DashboardController.cs
│       │   ├── ProjectSkillsController.cs
│       │   ├── SkillCategoriesController.cs
│       │   ├── SkillDiagnosticsController.cs
│       │   ├── TagsController.cs
│       │   └── WorkHistoriesController.cs
│       ├── Models/                   # 数据模型
│       │   └── PortfolioData.cs
│       ├── Data/                     # DbContext
│       ├── DTOs/                     # 数据传输对象
│       ├── Services/                 # 业务服务
│       ├── Middleware/               # 中间件 (JWT)
│       ├── Program.cs                # 应用入口
│       └── appsettings.json          # 配置文件
│
├── frontend/                         # 前端 Vue 3 + Vite
│   └── src/
│       ├── api/                      # API 请求封装
│       ├── views/                    # 页面组件
│       │   ├── dashboard/            # 仪表盘
│       │   ├── advantages/           # 核心优势
│       │   ├── skills/               # 技术栈矩阵
│       │   ├── projects/             # 项目管理
│       │   ├── workHistory/          # 工作经历
│       │   ├── diagnostics/          # 技能诊断
│       │   └── login/                # 登录
│       ├── router/                   # 路由配置
│       ├── stores/                   # Pinia 状态
│       ├── layout/                   # 布局组件
│       ├── components/               # 公共组件
│       ├── types/                    # TypeScript 类型
│       └── styles/                   # 全局样式
│
└── README.md
```

## 快速开始

### 环境要求

- **Node.js** >= 18
- **.NET SDK** >= 8.0
- **MySQL** >= 8.0

### 后端启动

```bash
# 进入后端目录
cd backend/PortfolioAdmin.Api

# 修改数据库连接字符串 (appsettings.json)
# "DefaultConnection": "server=localhost;port=3306;database=blazor_portfolio;user=root;password=yourpassword"

# 运行 EF 迁移（如需自动建表可在代码中启用 EnsureCreated）
dotnet ef database update

# 启动后端服务（默认 http://localhost:5000）
dotnet run

# Swagger 文档地址：http://localhost:5000/swagger
```

### 前端启动

```bash
# 进入前端目录
cd frontend

# 安装依赖
npm install

# 启动开发服务器（默认 http://localhost:5173）
npm run dev

# 生产构建
npm run build
```

### 默认账号

| 用户名 | 密码 |
|--------|------|
| admin | admin123 |

> 可在 `appsettings.json` → `AdminUser` 节点中修改。

## 数据库

数据库名称：`blazor_portfolio`

核心数据表：

| 表名 | 说明 |
|------|------|
| advantages | 核心优势 |
| skill_categories | 技能分类 |
| tag_infos | 技能标签 |
| work_histories | 工作经历 |
| achievements | 工作成就 |
| compact_projects | 项目信息 |
| project_skills | 项目关联技能 |
| skill_diagnostics | 技能诊断 |

## API 概览

后端提供 RESTful API，通过 JWT Bearer Token 认证。

- `POST /api/auth/login` — 登录获取 Token
- `GET/POST/PUT/DELETE /api/advantages` — 核心优势 CRUD
- `GET/POST/PUT/DELETE /api/SkillCategories` — 技能分类 CRUD
- `GET/POST/PUT/DELETE /api/Tags` — 标签管理 CRUD
- `GET/POST/PUT/DELETE /api/CompactProjects` — 项目管理 CRUD
- `GET/POST/PUT/DELETE /api/WorkHistories` — 工作经历 CRUD
- `GET/POST/PUT/DELETE /api/Achievements` — 成就管理 CRUD
- `GET/POST/PUT/DELETE /api/SkillDiagnostics` — 技能诊断 CRUD
- `GET /api/Dashboard` — 仪表盘数据

> 完整 API 文档请启动后端后访问 `http://localhost:5000/swagger`

## License

MIT
