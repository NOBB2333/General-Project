# General Admin 项目管理系统实现程度评估

> 评估日期：2026-06-15
> 评估范围：基于 `.doc` 产品规划文档，对照 `General.Admin` 后端与 `General.Web/apps/web-general` 前端代码
> 评估依据：PMP 十大知识领域、五期实施路线、四期 P0/P1/P2 PRD、当前代码事实

---

## 一、总体结论

当前系统处于 **四期建设中期**，已完成的项目管理闭环基础（P0）约 **75%～80%**：

- **一期平台中心**：已基本落地，页面、权限、菜单、文件、审计、监控、定时任务都有实现。
- **二期项目执行区**：项目列表、详情、任务、里程碑、RAID、文档、成员、工时等核心能力已具备，并在四期中补上了章程、WBS、依赖、质量、变更、基线、EVM 等 PMP 治理数据。
- **三期经营管理区**：经营总览、项目经营、报表中心已具备，合同/采购/预算/应收/发票/验收/预警统一收敛到报表中心检索和项目经营详情维护。
- **四期 AI 能力**：仅做了数据预留，未真正启动。
- **五期 OA 协同**：文档规划中，代码未涉及。

**一句话概括**："项目协作 + 经营看板 + PMP 治理数据骨架" 已经搭起来，但查询聚合服务臃肿、P1/P2 高级能力基本未动、测试和 CI/CD 薄弱。

---

## 二、各期范围与实际代码对照

### 2.1 一期：平台中心

| 规划模块 | 实际状态 | 证据 |
|---|---|---|
| 组织架构 | ✅ 已落地 | `views/platform/organization/index.vue`、`OrganizationUnitAppService` |
| 用户管理 | ✅ 已落地 | `views/platform/users/index.vue` |
| 角色权限 | ✅ 已落地 | `views/platform/roles/index.vue`、`AdminPermissionDefinitionProvider` |
| 菜单管理 | ✅ 已落地 | `views/platform/menus/index.vue` |
| 租户管理 | ✅ 已落地 | `views/platform/tenants/index.vue` |
| 文件管理 | ✅ 已落地 | `views/platform/files/index.vue`、`FileController` |
| 审计日志 | ✅ 已落地 | `views/platform/audit-logs/index.vue` |
| 系统监控 | ✅ 已落地 | `views/platform/system-monitor/index.vue` |
| 定时任务 | ✅ 已落地 | `views/platform/scheduler/index.vue`、多个 `ScheduledJobs` |
| 个人中心 | ✅ 已落地 | `views/platform/profile/index.vue` |
| 我的待办 | ⚠️ 已迁移 | 入口在 `views/project/my-related/index.vue`；`views/platform/todos/index.vue` 是一期残留占位页 |

**一期完成度：约 95%**，只剩 `platform/todos` 占位页未清理。

---

### 2.2 二期：项目执行区

| 规划模块 | 实际状态 | 证据 |
|---|---|---|
| 项目列表 | ✅ 已落地 | `views/project/projects/index.vue`（670 行） |
| 项目详情 | ✅ 已落地 | `views/project/detail/index.vue`（1171 行）+ 8 个 Tab 组件 |
| 与我相关 | ✅ 已落地 | `views/project/my-related/index.vue` |
| PM 工作台 | ✅ 已落地 | `views/project/pm-dashboard/index.vue` |
| PMO 执行总览 | ✅ 已落地 | `views/project/pmo-overview/index.vue` |
| 任务管理 | ✅ 已落地 | `ProjectTasksTab.vue`、独立 `tasks/index.vue` |
| 里程碑管理 | ✅ 已落地 | `ProjectPlanTab.vue`、`ProjectLifecycleService` |
| RAID 管理 | ✅ 已落地 | `views/project/raid/index.vue` |
| 文档管理 | ✅ 已落地 | `ProjectDocsTab.vue` |
| 成员管理 | ✅ 已落地 | `ProjectResourcesTab.vue` |
| 工时管理 | ✅ 已落地 | `ProjectWorklog` 实体、详情页展示 |

**二期完成度：约 90%**，执行区页面和功能基本齐全。

---

### 2.3 三期：经营管理区

| 规划模块 | 实际状态 | 证据 |
|---|---|---|
| 经营总览 | ✅ 已落地 | `views/business/overview/index.vue`（509 行） |
| 项目经营 | ✅ 已落地 | `views/business/projects/index.vue`（942 行）+ 6 个 Tab |
| 报表中心 | ✅ 已落地 | `views/business/reports/index.vue`（977 行） |
| 经营明细检索 | ✅ 已落地 | `business/reports/index.vue` 聚合合同/采购/预算/应收/发票/验收/预警 Tab |
| 经营明细维护 | ✅ 已落地 | `business/projects/index.vue` 在项目经营详情维护合同、采购、预算、应收、发票、验收和经营链 |
| 旧台账 URL 兼容 | ✅ 已收敛 | 独立台账 URL 自动跳转到报表中心对应页签，不再作为左侧菜单入口 |

**三期完成度：约 85%**。总览、项目经营详情和报表中心已落地，经营明细统一复用后端经营报表口径，发现问题后跳转到项目经营详情维护。后续重点从“入口缺失”转向交互提效、页面拆分、查询性能和测试覆盖。

---

### 2.4 四期：AI 交互能力

| 规划方向 | 实际状态 |
|---|---|
| 智能摘要 | ❌ 未启动 |
| 风险解释 | ❌ 未启动 |
| 周报辅助生成 | ❌ 未启动 |
| 问答能力 | ❌ 未启动 |
| MCP Server | ❌ 未启动 |

**四期完成度：约 5%**，只有数据预留。

---

### 2.5 五期：OA 办公协同

仅在 `.doc/05-五期建设/01-OA设计.md` 有文档，**代码未涉及**。

---

## 三、PMP 知识领域覆盖度

基于 `.doc/04-四期建设/05-四期建设待办清单-PMP对标.md` 核对：

| PMP 知识领域 | 当前覆盖 | 完成度 | 主要差距 |
|---|---|---|---|
| **整合管理** | 项目创建、状态、章程、变更、基线 | 78% | 章程独立编辑与状态推进已补，缺审批记录流水；项目收尾流程弱 |
| **范围管理** | 任务、里程碑、周期 | 75% | 缺需求追踪矩阵；WBS 工作包概念弱 |
| **进度管理** | 计划/实际时间、周期、依赖、基线 | 70% | 缺关键路径、浮动时间计算 |
| **成本管理** | 预算、合同、采购、回款、EVM | 70% | AC 成本归集口径未统一；缺预算版本链 |
| **质量管理** | 验收标准、检查单、检查记录 | 80% | 缺质量指标看板、缺陷密度统计 |
| **资源管理** | 成员、角色、工时、责任人 UserId | 75% | 缺资源日历、资源负荷统计 |
| **风险管理** | RAID、概率/影响、应对策略 | 80% | 缺风险矩阵专门页、应对行动闭环 |
| **沟通管理** | 周报提醒、站内通知 | 40% | 缺沟通计划、会议纪要、状态报告模板 |
| **采购管理** | 合同、采购实体 | 70% | 缺合同变更、供应商评估、合同收尾 |
| **干系人管理** | 项目成员 | 40% | 缺干系人识别、权力/利益矩阵 |

---

## 四、P0 / P1 / P2 完成度矩阵

### 4.1 P0 - 核心功能补齐（四期重点）

| 编号 | 内容 | 后端状态 | 前端状态 | 总体 |
|---|---|---|---|---|
| P0-A | 项目章程与立项闭环 | ✅ 实体/服务/状态流转校验已落地 | ✅ 立项页可维护章程、审批状态并批准后推进到执行中；审批记录流水待补 | 95% |
| P0-B | WBS 与任务依赖 | ✅ 实体/依赖/循环校验已落地 | ✅ 计划 Tab 可维护 | 95% |
| P0-C | RAID 风险管理增强 | ✅ 概率/影响/策略已落地 | ✅ 风险页可维护 | 90% |
| P0-D | 质量管理基础 | ✅ 验收/检查单/检查记录已落地 | ✅ 详情页可维护 | 90% |
| P0-E | 变更请求与影响分析 | ✅ 实体/审批/基线关联已落地 | ✅ 控制 Tab 可维护 | 90% |
| P0-F | EVM 最小模型 | ✅ 基线/快照/定时任务已落地 | ✅ 控制 Tab 展示 | 90% |
| P0-G | 项目角色与责任人结构化 | ✅ ProjectMemberRole/ UserId 已落地 | ✅ 资源 Tab 可维护 | 90% |
| P0-H | 经营链 CRUD 与执行跟踪 | ✅ 实体/创建/更新/删除、报表中心聚合检索、CSV 导出、总览/详情、台账筛选、预警筛选、利润分析导出专项测试和写侧 CRUD 集成测试已落地 | ✅ 项目经营详情支持合同/采购/预算/应收/发票/验收/经营链编辑删除，报表中心当前页签导出、预警状态/等级服务端筛选已补齐；独立台账菜单已收敛 | 90% |
| CQ-01 | 拆分 ProjectService | ✅ 读侧 ProjectQueryService、写侧 ProjectService、查询投影 ProjectProjectionMapper 已拆分 | - | 90% |
| CQ-02 | 拆分 BusinessService | ✅ 写侧、查询门面、项目/总览投影、报表查询、预警生成和报表导出构建已拆 | - | 90% |

**P0 总体完成度：约 85%**，核心数据模型和闭环已通，前端体验和代码质量治理还需收尾。

---

### 4.2 P1 - 竞争力提升

| 编号 | 内容 | 状态 |
|---|---|---|
| P1-01 | 看板视图 | ✅ 已实现（ProjectTasksTab 有看板视图） |
| P1-02 | SignalR 实时通知 | ❌ 未启动 |
| P1-03 | 项目文档管理 | ⚠️ 基础上传/列表有，缺版本管理 |
| P1-04 | Webhook 引擎 | ❌ 未启动 |
| P1-05 | Git 集成 | ❌ 未启动 |
| P1-06 | 领域事件 | ❌ 未启动 |
| P1-07 | 单元测试补全 | ⚠️ 只有 Sample 测试，核心业务缺测试 |
| P1-08 | CI/CD 流水线 | ❌ 无 `.github` 配置 |
| P1-09 | 甘特图交互增强 | ⚠️ 有甘特展示，缺拖拽/关键路径 |
| P1-10 | 通知中心增强 | ⚠️ 基础通知有，缺偏好/免打扰 |

**P1 总体完成度：约 20%**，只有看板视图和部分文档能力落地。

---

### 4.3 P2 - 差异化竞争

| 编号 | 内容 | 状态 |
|---|---|---|
| P2-01 | AI 自然语言查询 | ❌ 未启动 |
| P2-02 | AI 报告自动生成 | ❌ 未启动 |
| P2-03 | MCP Server | ❌ 未启动 |
| P2-04 | 工作流引擎 | ❌ 未启动 |
| P2-05 | 数据导入引擎 | ❌ 未启动 |
| P2-06 | AI 任务智能分配 | ❌ 未启动 |
| P2-07 | IM 集成 | ❌ 未启动 |
| P2-08 | 项目模板 | ❌ 未启动 |

**P2 总体完成度：约 0%**。

---

## 五、关键代码事实

### 5.1 后端关键服务规模

| 文件 | 行数 | 状态 |
|---|---|---|
| `ProjectService.cs` | 45 行 | 项目创建写侧服务 |
| `ProjectQueryService.cs` | 320 行 | 项目读侧查询编排，承接 workspace/detail/list/task list/RAID list/my-related |
| `ProjectProjectionMapper.cs` | 587 行 | 项目查询 DTO 投影与状态/关系辅助计算 |
| `BusinessService.cs` | 查询门面 | 加载经营上下文、解析访问权限并委托查询构建 |
| `BusinessQueryBuilder.cs` | 项目经营查询投影 | 构建 overview/project list/project detail DTO，不再承载报表中心台账列表 |
| `BusinessReportQueryBuilder.cs` | 报表查询投影 | 从查询投影拆出合同/采购/预算/应收/发票/验收/预警报表列表筛选与聚合 |
| `BusinessProjectionMapper.cs` | 经营投影映射 | 统一金额掩码、关键字匹配、项目汇总和台账 DTO 映射 |
| `BusinessWarningBuilder.cs` | 经营预警构建 | 从查询投影拆出利润/收尾/回款/SPI/CPI 预警与焦点项目生成 |
| `BusinessReportExportBuilder.cs` | 报表导出构建 | 从查询投影拆出 CSV 行构建、转义、文件名和内容类型 |
| `ProjectGovernanceService.cs` | 261 行 | 章程、基线、变更、审批、EVM 快照 |
| `ProjectPlanService.cs` | 170 行 | 任务依赖、WBS 更新、循环检测 |
| `ProjectPerformanceService.cs` | 191 行 | EVM 计算、每周快照 |
| `BusinessMutationService.cs` | 259 行 | 经营实体写入 |

### 5.2 前端关键页面规模

| 文件 | 行数 | 状态 |
|---|---|---|
| `project/detail/index.vue` | 1171 行 | 详情壳 + 8 个 Tab |
| `business/projects/index.vue` | 942 行 | 经营详情壳 + 6 个 Tab |
| `business/reports/index.vue` | 977 行 | 报表中心 |
| `project/detail/components/ProjectControlTab.vue` | 534 行 | 变更/基线/EVM 集中 |

### 5.3 测试与工程

| 项目 | 状态 |
|---|---|
| 后端测试项目 | 存在 `Domain.Tests` / `Application.Tests` / `EFCore.Tests` |
| 核心业务测试 | ❌ 基本只有 Sample 测试 |
| 前端测试 | ❌ 未看到测试配置 |
| CI/CD | ❌ 无 `.github/workflows` |

### 5.4 CodeGraph

项目已启用 CodeGraph（`.codegraph/codegraph.db`、`daemon.sock`、`daemon.pid`），用于代码智能索引和调用关系查询。

---

## 六、当前未提交改动说明

当前工作区有大量四期建设的未提交改动，与 `.doc/04-四期建设/05-四期建设待办清单-PMP对标.md` 高度对应：

- **新增域实体**：`ProjectCharter`、`ProjectBaseline`、`ProjectPerformanceSnapshot`、`ProjectChangeRequest`、`ProjectChangeImpact`、`ProjectChangeApproval`、`ProjectAcceptanceCriteria`、`ProjectQualityChecklist`、`ProjectQualityInspection`、`ProjectTaskDependency`、`ProjectMemberRole`、`BusinessInvoice`、`BusinessAcceptance`。
- **新增应用服务**：`ProjectGovernanceService`、`ProjectLifecycleService`、`ProjectPlanService`、`ProjectQualityService`、`ProjectResourceService`、`ProjectRiskService`、`ProjectPerformanceService`、`BusinessMutationService`、`BusinessContextLoader`。
- **新增迁移**：SQLite 和 PostgreSQL 各有 4 条新迁移，覆盖变更、绩效、发票、验收、外键。
- **前端调整**：`ProjectControlTab.vue`（变更/基线/EVM）、经营旧台账 URL 通过 `BusinessReportRedirect` 收敛到报表中心。

这些改动说明 **P0 第一批和第二批控制闭环正在落地或刚落地**，但尚未提交。

---

## 七、关键差距清单（按优先级）

### 高优先级（影响 P0 验收）

1. **P0-A 章程审批流**：独立立项页已支持待规划项目选择、章程维护、审批状态更新，以及“已批准章程后进入执行中”的状态推进；仍缺独立审批记录流水和统一工作流承接。
2. **P0-H 经营闭环维护**：合同/采购/预算/应收/发票/验收/预警已收敛到报表中心聚合检索和项目经营详情维护，支持 CSV 导出、报表筛选、总览聚合、项目经营详情投影、采购/回款/发票/验收/预警台账筛选专项测试；验收日期筛选已与前端“验收日期”控件对齐，预警状态和等级筛选已进入后端报表与导出口径，报表中心已补齐当前页签导出和利润分析导出口径；项目经营详情已支持合同、采购、预算、应收、发票、验收和经营链事项编辑/删除，后续重点是减少重复录入、补审批记录和真实业务语义。
3. **CQ-01 / CQ-02 服务拆分**：`ProjectService` 已收缩为项目创建写侧服务，workspace/detail/list/task list/RAID list/my-related 已拆到 `ProjectQueryService`，查询 DTO 投影已拆到 `ProjectProjectionMapper`；`BusinessService` 已拆出写侧、上下文加载、项目/总览查询投影、报表查询投影、预警生成和报表导出构建，后续只需继续关注查询性能和测试覆盖。
4. **测试覆盖**：经营查询/导出、总览/详情投影和报表台账筛选已有 18 条专项测试，经营写侧更新/删除已有 4 条 EFCore 集成测试，P0 状态机、EVM 计算、变更审批、风险评分等项目治理侧仍缺单元测试。

### 中优先级（P1 启动前）

5. **关键路径/浮动时间**：P0-B 已做依赖，但还没做进度网络分析。
6. **资源负荷/资源日历**：P0-G 已结构化角色，但还没做负荷统计。
7. **质量指标看板**：P0-D 已有检查记录，但缺少缺陷密度、通过率等质量指标。
8. **预算版本链 / 合同变更**：P0-E/H 已有关联，但版本化和前后对比未做。

### 低优先级（P2）

9. AI 能力、MCP Server、工作流引擎、IM 集成、项目模板等均未启动。

---

## 八、结论与建议

**当前系统实现程度：约 70%～75%（按五期总范围）**，如果只看四期 P0 目标，则约 **85%**。

### 建议下一步

1. **先收尾 P0**：把当前未提交改动验证并提交；按真实审批语义补章程审批记录流水；继续补经营管理区剩余测试和项目治理侧测试。
2. **补测试**：为 P0 核心状态机、EVM 计算、变更审批、WBS/依赖校验建立单元测试。
3. **建立 CI/CD**：至少配置 GitHub Actions 做后端构建 + 前端类型检查。
4. **再启动 P1**：优先做资源负荷、关键路径、质量指标看板，这些直接依赖已落地的 P0 数据。
5. **P2 暂缓**：在 P1 数据稳定前，不建议启动 AI 和工作流引擎。

当前最大的价值是：**PMP 治理闭环的数据骨架已经搭好**，下一步重点是让查询服务瘦身、补测试、补前端体验，而不是继续铺新功能。

---

> 本评估基于 2026-06-15 的代码与文档状态，后续应根据实际开发进度更新。
