# Agent 工作指南

本仓库是 `General Admin` 企业内部项目管理平台，不是通用后台模板。开发目标是围绕项目全过程管理，把项目执行、经营管理和平台治理沉淀成可私有化交付的 ABP 模块化单体系统，并在易用、高效的前提下吸收 PMP 流程规范。

## 项目定位

- 核心用户：PM、PMO、项目成员、经营管理人员、平台管理员。
- 核心主线：平台治理、项目执行、经营管理。
- 当前优先级：项目管理闭环最小可用、PMP 流程规范落地、经营链与项目执行联动、代码结构持续治理。
- 交付重心：Web 是主交付端；桌面端只是 Avalonia + WebView 壳层预留。
- 不要把本项目做成低代码平台、客户门户、供应商门户、完整财务系统或纯展示型后台。

## 上下文读取规则

优先按用户给定 goal 和本次任务范围读取上下文，不要求每次把 `.doc` 全部读完。目标是少读但读准。

- 任务只涉及局部 bug、样式、字段映射或小范围交互：优先读相关源文件、DTO/API、调用点和测试，不必先读总文档。
- 任务涉及业务流程、模块边界、菜单结构、权限、数据模型或跨前后端联动：读取相关 `.doc` 文档，再读代码事实。
- 任务涉及项目管理流程、PMP 对标、四期建设、AI/实时/工作流等演进能力：读取 `.doc/04-四期建设/` 中对应 PRD 或待办清单。
- 任务涉及整体方向、范围取舍或模块归属：读取 `.doc/01-产品范围与实施路线.md`、`.doc/02-业务模块与管理规则.md`、`.doc/03-技术架构与交付方案.md`。
- 永远要读将要修改的源文件、契约 DTO、前后端 API 调用点和相关测试。

**CodeGraph 优先原则**：本仓库已启用 CodeGraph 代码智能索引（`.codegraph/`）。读代码、查符号、找调用关系、评估影响面时，优先使用 CodeGraph 工具：`codegraph_explore` 用于通用查询和流程理解，`codegraph_node` 用于读取单个文件或精确定位符号。CodeGraph 返回的源码和依赖关系与 Read 等效，但更快、更完整。只有 CodeGraph 未覆盖或需要编辑具体文件时，才回退到 Read/Edit/Write。

如果文档与代码不一致，以当前代码事实为准，并在回复中说明差异。不要只按 PRD 想象新增不存在的字段、接口或页面。

## 目录边界

```text
General.Admin/                         后端 ABP 模块化单体
  src/General.Admin.Domain.Shared/     枚举、常量、本地化、共享定义
  src/General.Admin.Domain/            实体、聚合、领域规则
  src/General.Admin.Application.Contracts/ DTO、应用服务接口、权限定义
  src/General.Admin.Application/       应用服务、查询聚合、写侧编排
  src/General.Admin.EntityFrameworkCore/ EF Core 映射和迁移
  src/General.Admin.HttpApi*/          API 暴露、Host、客户端
  test/                                xUnit 测试

General.Web/                           Vue 3 + Vben Admin 前端
  apps/web-general/src/api/core/        后端 API 封装
  apps/web-general/src/views/project/   项目执行区（含项目列表、详情、任务、RAID、与我相关、PM/PMO 视图）
  apps/web-general/src/views/business/  经营管理区
  apps/web-general/src/views/platform/  平台中心（通用治理页面，不含业务驾驶舱）
  apps/web-general/src/composables/     复杂页面逻辑复用
  .codegraph/                           代码智能索引（由 CodeGraph 工具维护，不要手动编辑）

General.Desktop/                       Avalonia 桌面壳
.doc/                                  产品、业务、PMP 对标和分期文档
Script/                                构建、打包、部署脚本
```

## 业务建模原则

- 先补结构化数据模型，再补页面体验。PMP 能力不能只靠前端文案和临时字段表达。
- 项目执行区以项目详情为中心，围绕章程、WBS、任务依赖、里程碑、成员角色、RAID、质量、变更、基线、EVM、工时和文档形成闭环。
- 经营管理区以经营总览、项目经营、报表中心为中心，合同、采购、预算、应收、发票、验收、预警要能和项目进度、变更、质量、收尾联动。
- 平台中心只承接通用治理能力：用户、组织、角色、菜单、租户、文件、审计、通知、监控、调度、开放接口等。
- AI、Webhook、SignalR、工作流、导入引擎等通用基础能力优先放 Platform；项目助手放 Project；经营分析放 Business。
- 指标口径由后端定义。健康分、SPI/CPI、预警、利润、回款、逾期等不要在多个前端页面重复拼装。
- 金额、利润、成本、回款等敏感数据必须考虑权限和数据范围。

## 后端约定

- 遵循 ABP DDD 分层：实体和领域规则进 Domain；DTO/接口/权限进 Contracts；应用编排进 Application；EF 映射和迁移进 EntityFrameworkCore。
- 实体通常继承 `FullAuditedAggregateRoot<Guid>`，保持私有 setter 或受控修改方法，避免把实体退化成任意 set 的数据袋。
- 应用服务使用 ABP 依赖注入约定，局部服务可实现 `ITransientDependency`。
- Project 写侧优先复用现有服务边界：`ProjectGovernanceService`、`ProjectPlanService`、`ProjectQualityService`、`ProjectResourceService`、`ProjectRiskService`、`ProjectLifecycleService`、`ProjectPerformanceService`、`ProjectAccessGuard`、`ProjectContextLoader`。
- Business 写侧优先复用 `BusinessMutationService` 和 `BusinessContextLoader`；查询聚合在继续拆分时按报表、预警、映射、明细边界拆，不做无业务意义的搬文件。
- 修改查询时注意 N+1。优先使用 Context/Loader 模式一次性加载项目、经营链、成员、任务、里程碑、风险、质量、预算等关联数据。
- 权限统一走权限定义和 AccessGuard，不要在控制器、前端或零散方法里复制判断。
- Schema 变更必须同步 EF Core 映射、DTO、应用服务、前端类型和迁移。当前迁移同时维护 SQLite 与 PostgreSQL，不要只改一个数据库提供方。
- 文件上传、路径、外部 URL、Webhook 签名、OpenAPI 密钥等功能必须做输入校验和安全边界。

## 前端约定

- 主应用在 `General.Web/apps/web-general`，技术栈为 Vue 3、TypeScript、Vben Admin、Ant Design Vue。
- API 封装放 `src/api/core/*.ts`，类型使用 `namespace XxxApi {}`，保持与后端 DTO 字段一致。
- 业务页面按 `views/project`、`views/business`、`views/platform` 分区放置，不要把业务驾驶舱塞进平台中心。
  - `views/platform/todos/index.vue` 是一期历史占位页，当前“与我相关/我的待办”入口在 `views/project/my-related/index.vue`。
- 大页面要拆 composable 和局部组件。项目详情、项目经营这类高复杂页面不要继续把请求、状态、表单、表格、图表和权限逻辑全部堆在一个 Vue 文件里。
- 复用 Vben 和 Ant Design Vue 已有模式。表格、抽屉、弹窗、表单、Tabs、权限按钮要跟现有页面保持一致。
- 前端只做交互和展示聚合，不私自定义业务指标口径；需要新指标时先补后端接口。
- 新增路由、菜单、权限时，同时检查后端菜单/权限种子、前端路由、API 权限点和页面可见性。

## PMP 流程参考规则

PMP 是流程和口径参考，不是照搬架构的硬约束。所有项目管理能力都应先服务易用性、执行效率和真实管理闭环，再用 PMP 概念校准数据完整性。

- 启动：项目章程、商业论证、目标、范围边界、成功标准、审批状态要结构化，但页面入口应保持轻量，避免让立项变成填表负担。
- 规划：WBS、任务依赖、基线、里程碑、预算、资源角色、质量标准要可追踪；复杂度应按项目规模渐进显示。
- 执行：任务、工时、成员、文档、采购、验收必须能产生真实业务记录，优先减少 PM 和成员的重复录入。
- 监控：RAID、变更影响、质量检查、EVM 快照、SPI/CPI、经营预警要有后端口径；前端负责解释和行动入口。
- 收尾：项目归档、合同收尾、验收确认、文档归档要形成闭环，不能只改项目状态。
- 不新增“概念页”冒充能力。页面必须对应真实实体、接口、权限和可验证的数据流。

## 常用命令

后端：

```bash
cd General.Admin
dotnet build General.Admin.slnx
dotnet test test/
dotnet run --project src/General.Admin.DbMigrator
dotnet run --project src/General.Admin.HttpApi.Host
```

前端：

```bash
cd General.Web
pnpm install
pnpm dev:general
pnpm dev:general-platform
pnpm build:general
pnpm build:general-platform
pnpm lint
pnpm test:unit
```

完整打包：

```bash
sh Script/package-all.sh
```

## 验证要求

- 后端逻辑或契约变更：至少运行 `dotnet build General.Admin.slnx`，能运行时再跑相关 `dotnet test`。
- 前端类型、API 或页面变更：至少运行 `pnpm lint` 或对应包的 typecheck/build；本仓库前端代码审查以 oxlint/Vben lint 链路为准，不按旧 ESLint 规则另起一套；用户体验改动要在浏览器中验证关键路径。
- 数据库模型变更：检查 SQLite/PostgreSQL 迁移、DbContext 映射、快照、DbMigrator。
- 跨前后端改动：同时验证接口字段、权限、错误处理、空数据、分页、排序和枚举映射。
- 如果测试无法运行，必须说明原因和未覆盖风险。

## 语言规范

- 代码注释、变量命名、业务术语优先使用中文。
- 字典值、状态标签、页面标题等用户可见内容必须使用中文。
- API 文档、DTO 描述使用中文。
- 提交信息（commit message）使用中文。
- 英文仅用于技术标识符（类名、方法名、变量名、枚举值）和第三方库 API。

## 代码风格

- C#：公共成员 PascalCase，私有字段 camelCase；保持 async/await；优先清晰的领域方法和应用服务编排。
- TypeScript/Vue：2 空格缩进、单引号、严格类型；代码审查走 oxlint + Stylelint + Vben 工具链；避免 `any` 扩散；复杂状态抽到 composable。
- 不做无关格式化、无关重命名、无关依赖升级。
- 不提交密钥、`.env`、本地数据库、构建产物、日志和 IDE 私有配置。
- 保留用户已有改动。工作区可能是脏的，除非明确要求，不要回滚或覆盖他人修改。

## 提交规范

提交格式：

```text
type(scope): description
```

常用类型：`feat`、`fix`、`docs`、`style`、`refactor`、`perf`、`test`、`build`、`ci`、`chore`、`revert`、`types`、`release`。

提交应保持单一目的。功能、重构、格式化、迁移和测试尽量分开，便于审查和回滚。
