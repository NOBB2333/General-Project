# 四期建设总 PRD

> 版本：v1.0
> 日期：2026-05-21
> 状态：规划中

---

## 1. 修订说明

四期定位为 **项目管理能力补齐与智能化升级阶段**。前三期完成了平台底座、项目执行框架和经营管理框架，四期的目标是把"看得到"变成"用得起来"，并引入 AI 和实时能力。

---

## 2. 四期建设目标

| 目标 | 说明 |
|------|------|
| 核心 PM 功能闭环 | 任务、里程碑、成员、工时等核心操作完整可用 |
| 实时协作能力 | 信号推送、在线状态、实时通知 |
| AI 能力接入 | 自然语言查询、智能报告、MCP Server |
| 外部集成能力 | Webhook、Git 集成、IM 通知 |
| 代码质量提升 | 服务拆分、领域事件、测试补全、CI/CD |

---

## 3. 分期规划

| 阶段 | 主题 | 核心交付 | 预估周期 |
|------|------|----------|----------|
| **P0** | 核心功能补齐 | 任务 CRUD、项目编辑归档、里程碑管理、成员管理、工时录入 | 1-2 个月 |
| **P1** | 竞争力提升 | 看板视图、SignalR 实时通知、文档管理、Webhook 集成、服务拆分重构 | 2-4 个月 |
| **P2** | 差异化竞争 | AI 自然语言查询、AI 报告生成、MCP Server、工作流引擎、数据导入 | 4-6 个月 |

---

## 4. 功能归属模块决策

### 4.1 归属原则

| 原则 | 说明 |
|------|------|
| 业务实体就近 | 功能属于哪个业务域就放哪个模块 |
| 平台能力下沉 | 通用能力（通知、文件、AI 基础设施）放 Platform |
| 新增模块独立 | 如果功能跨域且复杂度高，可新建独立模块 |

### 4.2 功能归属矩阵

| 功能 | 归属模块 | 理由 |
|------|----------|------|
| 任务 CRUD + 状态流转 | **Project** | 任务是项目执行的核心实体 |
| 项目编辑/归档 | **Project** | 项目生命周期管理 |
| 里程碑 CRUD | **Project** | 里程碑是项目阶段标记 |
| 成员管理 | **Project** | 成员是项目维度的角色 |
| 工时录入/统计 | **Project** | 工时关联任务和项目 |
| 看板视图 | **Project** | 看板是任务的可视化呈现 |
| 文档管理 | **Project** | 文档关联项目和任务 |
| 实时通知基础设施 | **Platform** | 通用平台能力（SignalR Hub） |
| Webhook 引擎 | **Platform** | 通用集成能力 |
| Git 集成 | **Platform** | 通用外部连接器 |
| AI 基础设施 | **Platform** | LLM 调用封装、MCP Server 框架 |
| AI 项目助手 | **Project** | 项目维度的 AI 交互 |
| AI 经营分析 | **Business** | 经营维度的 AI 分析 |
| 工作流引擎 | **Platform** | 通用工作流，被 Project/Business 复用 |
| 数据导入 | **Platform** | 通用导入引擎 |
| 合同/采购 CRUD 补齐 | **Business** | 经营管理核心实体 |
| CI/CD 流水线 | **基础设施** | 不属于业务模块，属于 DevOps |

---

## 5. 技术架构演进

### 5.1 当前架构

```
前端 (Vue 3) --HTTP--> 后端 (.NET 10 + ABP)
                         ├── Platform 模块
                         ├── Project 模块
                         └── Business 模块
```

### 5.2 四期目标架构

```
前端 (Vue 3) --HTTP/WS--> 后端 (.NET 10 + ABP)
                              ├── Platform 模块
                              │   ├── SignalR Hub（实时通知）
                              │   ├── Webhook Engine（外部集成）
                              │   ├── AI Infrastructure（LLM 封装）
                              │   ├── Workflow Engine（工作流）
                              │   └── Import Engine（数据导入）
                              ├── Project 模块
                              │   ├── TaskService（任务服务）
                              │   ├── MilestoneService（里程碑服务）
                              │   ├── MemberService（成员服务）
                              │   ├── WorklogService（工时服务）
                              │   ├── DocumentService（文档服务）
                              │   └── ProjectAiAssistant（AI 助手）
                              └── Business 模块
                                  ├── ContractService（合同服务）
                                  ├── ProcurementService（采购服务）
                                  └── BusinessAiAnalyzer（AI 分析）

AI Agent (Claude/GPT) --MCP--> MCP Server (.NET)
                                    └── 桥接 PM 系统 API
```

### 5.3 关键技术选型

| 技术 | 选型 | 用途 |
|------|------|------|
| 实时通信 | SignalR | 任务变更推送、通知、在线状态 |
| AI 流式输出 | SSE (Server-Sent Events) | AI 回答的流式渲染 |
| LLM 接入 | Semantic Kernel / 直接 HTTP | 封装 LLM API 调用 |
| MCP Server | 自研 .NET MCP Server 模块 | 暴露 PM 能力给 AI Agent |
| 看板拖拽 | vuedraggable / sortablejs | 看板列间拖拽 |
| 富文本编辑 | Tiptap | 文档编辑（如需内置文档） |
| 文件预览 | pdf.js + mammoth.js | 文档在线预览 |
| CI/CD | GitHub Actions | 自动化构建、测试、部署 |

---

## 6. 竞品参考

| 功能 | 参考产品 | 参考要点 |
|------|----------|----------|
| 任务状态流转 | Linear | 键盘优先、流畅的状态切换动画 |
| 看板视图 | Trello / Plane | 列拖拽、卡片信息密度、筛选器 |
| 甘特图交互 | OpenProject | 依赖线拖拽、里程碑标记、基线对比 |
| 实时通知 | Slack / 飞书 | 通知分级、已读/未读、快捷操作 |
| AI 查询 | Linear AI / ClickUp Brain | 自然语言转查询、上下文感知 |
| MCP Server | Plane MCP | 标准 MCP 协议、工具注册、权限控制 |
| Webhook | GitHub Webhook | 签名验证、重试机制、事件订阅 |
| 工作流 | Jira Workflow | 状态节点、转换条件、触发器 |

---

## 7. 代码质量改进计划

| 改进项 | 当前状态 | 目标状态 | 所属阶段 |
|--------|----------|----------|----------|
| 服务拆分 | ProjectService 1个文件 | 拆为 Task/Milestone/Member/Worklog/Document 5个服务 | P0 |
| 领域事件 | 未使用 | 核心业务操作发布 DomainEvent，通知/审计/报表订阅 | P1 |
| 单元测试 | 覆盖率低 | 核心服务 80%+ 覆盖率 | P1 |
| API 版本 | 无版本控制 | 引入 /api/v1/ 前缀 | P1 |
| CI/CD | 无 | GitHub Actions：lint → test → build → deploy | P1 |
| 错误处理 | 基础 ApiResponse | 统一异常码、国际化错误消息 | P0 |

---

## 8. 四期交付物清单

| 序号 | 文档 | 说明 |
|------|------|------|
| 00 | 项目全面分析报告 | 当前项目现状分析 |
| 01 | 四期建设总 PRD | 本文档 |
| 02 | P0 核心功能补齐 PRD | 任务、项目、里程碑、成员、工时 |
| 03 | P1 竞争力提升 PRD | 看板、实时通知、文档、Webhook、重构 |
| 04 | P2 差异化竞争 PRD | AI、MCP、工作流、数据导入 |

---

## 9. 风险与依赖

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| P0 功能量大 | 可能延期 | 优先做任务 CRUD，其余迭代交付 |
| SignalR 与 ABP 集成复杂度 | 技术风险 | ABP 官方有 SignalR 集成包，降低风险 |
| LLM API 稳定性和成本 | AI 功能可用性 | 支持多 LLM 后端，降级到规则引擎 |
| 前端看板性能 | 大量任务时卡顿 | 虚拟滚动 + 分页加载 |
| 数据库迁移兼容性 | 线上升级风险 | 每次迁移单独 PR，灰度发布 |
