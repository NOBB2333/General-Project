# 仓库指南

## 项目结构与模块组织

全栈企业管理平台：

- **`General.Admin/`** - 后端 (.NET ABP)
  - `src/` - 源代码
  - `test/` - 单元测试
- **`General.Web/`** - 前端 (Vue 3 + Vben Admin)
  - `apps/web-general/` - 主应用
  - `packages/` - 共享工具
- **`General.Desktop/`** - 桌面客户端 (Avalonia)
- **`Script/`** - 构建/部署脚本
- **`.doc/`** - 项目文档

## 构建、测试与开发命令

### 后端
```bash
# 数据库迁移
dotnet run --project src/General.Admin.DbMigrator

# 运行后端
dotnet run --project src/General.Admin.HttpApi.Host

# 运行测试
dotnet test test/
```

### 前端
```bash
cd General.Web
pnpm install

# 开发
pnpm dev:general
pnpm dev:general-platform

# 构建
pnpm build:general
pnpm build:general-platform

# 代码检查
pnpm lint
```

### 完整构建
```bash
sh Script/package-all.sh
```

## 代码风格与命名规范

### 后端 (.NET)
- 遵循ABP框架约定
- 公共方法PascalCase，私有方法camelCase
- 公共API使用XML文档

### 前端 (Vue/TypeScript)
- 2空格缩进，单引号
- ESLint + Stylelint + oxlint
- TypeScript严格模式

## 测试指南

- **后端**: xUnit，位于 `General.Admin/test/`
- **前端**: Vitest (`pnpm test:unit`)

## 提交与PR规范

### 提交
格式: `type(scope): description`
类型: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`, `types`, `release`

范围: 项目名, `style`, `lint`, `ci`, `dev`, `deploy`, `other`

### PR要求
- 关联Issue
- UI变更需截图
- 确保测试通过

## 架构概述

前后端分离，可选桌面客户端：
1. **后端**: ABP单体架构 (SQLite/PostgreSQL/MySQL)
2. **前端**: Vue 3 SPA, pnpm monorepo
3. **桌面端**: Avalonia壳 + WebView

## 安全提示

- 切勿提交密钥
- 使用环境变量
- 正确配置CORS
- 生产环境设置AuthServer权威
