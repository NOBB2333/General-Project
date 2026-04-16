# General Admin

## 一、项目是做什么的

`General Admin` 是一个综合管理平台仓库，面向企业内部经营管理、项目管理、组织与权限管理等场景，当前包含后端、前端、桌面端三部分。

项目当前主要覆盖以下能力：

- 平台基础能力：登录认证、菜单权限、角色用户、租户、组织、审计日志、系统监控、调度任务、文件管理
- 项目管理能力：项目台账、项目详情、任务、RAID、里程碑、工作台、项目空间、我的关联事项
- 经营管理能力：预算、合同、发票、回款、验收、预警、经营看板与分析类页面
- 多端接入能力：浏览器端管理后台，以及基于桌面壳的客户端承载方式

仓库目录概览：

- `General.Admin`：后端服务，基于 ABP 多层架构
- `General.Web`：前端工程，基于 Vue + Vben Admin，包含多语言能力
- `General.Desktop`：桌面端壳项目，基于 Avalonia + WebView
- `.doc`：项目建设方案、业务设计、技术设计与分期实施文档
- `build`：本地构建输出目录
- `Script`：辅助脚本
- `Logs`：运行日志目录

## 二、项目架构

项目采用前后端分离，并预留桌面壳承载方式。

### 1. 后端架构

- 目录：`General.Admin`
- 架构风格：ABP 分层单体，多层模块化设计
- 主要职责：
  - 提供认证、授权、租户、组织、菜单、角色、用户等基础能力
  - 提供项目管理、经营管理等业务接口
  - 提供数据库迁移与种子数据初始化

后端核心目录：

- `General.Admin/src`
- `General.Admin/test`

### 2. 前端架构

- 目录：`General.Web`
- 技术栈：Vue 3 + Vite + TypeScript + Vben Admin
- 管理方式：`pnpm workspace` + `turbo`
- 当前主要应用：
  - `General.Web/apps/web-general`
  - `General.Web/apps/web-general-platform`

前端负责：

- 管理后台页面与交互
- 动态路由、权限控制、多语言支持
- 对接后端接口并承载业务流程

### 3. 桌面端架构

- 目录：`General.Desktop`
- 技术栈：Avalonia + WebView
- 定位：桌面客户端壳，不重复实现业务页面

桌面端主要负责：

- 承载前端页面
- 补充通知、文件选择、下载、本地缓存、自动更新等桌面能力

### 4. 配套文档

- `.doc/01-一期建设`
- `.doc/02-二期建设`
- `.doc/03-三期建设`

这些文档用于说明项目范围、实施路线、业务规则、技术架构与阶段交付内容。

## 三、如何操作与如何启动

推荐启动顺序是：先后端，再前端，最后桌面端。

### 1. 环境要求

- `.NET SDK 10.0+`
- `Node.js 20.19+`
- `pnpm 10+`

如果本机还没有 `pnpm`，可以先安装：

```bash
npm i -g corepack
corepack enable
```

### 2. 启动后端

进入后端目录：

```bash
cd /Users/wong/Code/CsharpLang/General-admin/General.Admin
```

首次启动前，先执行数据库迁移器：

```bash
dotnet run --project src/General.Admin.DbMigrator
```

然后启动后端主服务。  
如果你使用 IDE，直接打开 `General.Admin` 解决方案并启动 Web 主项目即可。  
如果你使用命令行，请在后端目录内选择实际 Web 启动项目运行。

### 3. 启动前端

进入前端目录并安装依赖：

```bash
cd /Users/wong/Code/CsharpLang/General-admin/General.Web
pnpm install
```

启动默认管理端：

```bash
pnpm dev
```

等价命令：

```bash
pnpm dev:general
```

如果你要启动平台端：

```bash
pnpm dev:general-platform
```

前端构建命令：

```bash
pnpm build:general
pnpm build:general-platform
```

### 4. 启动桌面端

进入桌面端目录：

```bash
cd /Users/wong/Code/CsharpLang/General-admin/General.Desktop
```

启动桌面壳：

```bash
dotnet run
```

说明：

- 当前桌面端定位是壳工程
- 业务页面仍以 `General.Web` 为主
- 如果需要完整桌面联调，通常应先保证前端已可运行或已完成构建

### 5. 日常操作建议

开发联调时，建议按下面顺序操作：

1. 启动 `General.Admin.DbMigrator`，确保数据库结构和种子数据已准备完成
2. 启动后端服务
3. 启动前端 `web-general` 或 `web-general-platform`
4. 如需桌面联调，再启动 `General.Desktop`

### 6. 入口说明

- 后端说明文件：[General.Admin/README.md](/Users/wong/Code/CsharpLang/General-admin/General.Admin/README.md:1)
- 前端说明文件：[General.Web/README.md](/Users/wong/Code/CsharpLang/General-admin/General.Web/README.md:1)
- 桌面端说明文件：[General.Desktop/README.md](/Users/wong/Code/CsharpLang/General-admin/General.Desktop/README.md:1)

如果后续需要，我可以继续把这个 README 再补成“开发版”和“部署版”两套说明。
