# 字典模块差距分析报告

> 日期：2026-06-13
> 背景：字典模块设计目标是替换前端硬编码，将状态、类型、优先级等可配置项统一由后端管理。当前前端大量使用硬编码，字典模块未真正发挥作用。

---

## 一、现状

### 1.1 字典模块现状

- 后端已有 `AppDictType` + `AppDictData` 实体
- 前端已有字典管理页面 `platform/dicts`
- API 已有 `getDictItemsApi(dictTypeCode)` 接口
- **种子数据只创建了 3 个字典类型**

### 1.2 种子数据现状

| 字典类型 | Code | 子项 | 前端是否使用 |
|---------|------|------|-------------|
| 项目状态 | `project_status` | 未开始/进行中/已完成 | ❌ 未使用 |
| 任务优先级 | `task_priority` | 低/中/高 | ❌ 未使用 |
| 文件分类 | `file_category` | 默认/合同/交付 | ❌ 未使用 |

### 1.3 前端硬编码现状

前端 `getDictItemsApi` 仅在字典管理页面自身使用，业务页面全部硬编码。

---

## 二、需要补齐的字典类型

### 2.1 必须放字典的（业务用户可能调整）

#### 2.1.1 `project_status` — 项目状态

- **当前状态**：字典已有，但前端未使用
- **前端硬编码位置**：
  - `views/project/projects/index.vue:124`
  - `views/project/detail/composables/useProjectDetailPage.ts:241`
- **硬编码值**：`['待规划', '进行中', '已完成', '已关闭']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 待规划 | `planning` | 10 | default |
| 进行中 | `in_progress` | 20 | processing |
| 已完成 | `completed` | 30 | success |
| 已关闭 | `closed` | 40 | default |

#### 2.1.2 `task_status` — 任务状态

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/project/tasks/index.vue:31`
  - `views/project/detail/composables/useProjectDetailPage.ts:241`
- **硬编码值**：`['未开始', '进行中', '阻塞', '已完成', '已关闭']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 未开始 | `not_started` | 10 | default |
| 进行中 | `in_progress` | 20 | processing |
| 阻塞 | `blocked` | 30 | error |
| 已完成 | `completed` | 40 | success |
| 已关闭 | `closed` | 50 | default |

#### 2.1.3 `priority` — 优先级

- **当前状态**：字典已有（`task_priority`），但前端未使用
- **前端硬编码位置**：
  - `views/project/projects/index.vue:128`
  - `views/project/detail/composables/useProjectDetailPage.ts:251`
  - 多个 Tag 组件中的颜色判断
- **硬编码值**：`['高', '中', '低']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 低 | `low` | 10 | default |
| 中 | `medium` | 20 | processing |
| 高 | `high` | 30 | error |

#### 2.1.4 `raid_type` — RAID 事项类型

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/project/raid/index.vue:30`
- **硬编码值**：`['风险', '问题', '依赖', '假设']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 风险 | `risk` | 10 | error |
| 问题 | `issue` | 20 | warning |
| 依赖 | `dependency` | 30 | blue |
| 假设 | `assumption` | 40 | default |

#### 2.1.5 `project_type` — 项目类型

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/project/projects/index.vue:129`
- **硬编码值**：`['平台建设', '业务项目', '客户交付', '内部优化']`
- **建议种子数据**：

| Label | Value | Sort |
|-------|-------|------|
| 平台建设 | `platform` | 10 |
| 业务项目 | `business` | 20 |
| 客户交付 | `delivery` | 30 |
| 内部优化 | `internal` | 40 |

#### 2.1.6 `project_source` — 项目来源

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/project/projects/index.vue:133`
- **硬编码值**：`['公司立项', '客户需求', '部门提报', '经营规划']`
- **建议种子数据**：

| Label | Value | Sort |
|-------|-------|------|
| 公司立项 | `company_init` | 10 |
| 客户需求 | `customer_demand` | 20 |
| 部门提报 | `department_submit` | 30 |
| 经营规划 | `business_plan` | 40 |

#### 2.1.7 `contract_type` — 合同类型

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/business/reports/index.vue:46`
- **硬编码值**：`['主合同', '补充合同', '分包合同', '采购合同']`
- **建议种子数据**：

| Label | Value | Sort |
|-------|-------|------|
| 主合同 | `main` | 10 |
| 补充合同 | `supplementary` | 20 |
| 分包合同 | `subcontract` | 30 |
| 采购合同 | `procurement` | 40 |

#### 2.1.8 `change_status` — 变更状态

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/project/detail/composables/useProjectDetailPage.ts:252`
- **硬编码值**：`['待评审', '评审中', '已批准', '已驳回', '已关闭']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 待评审 | `pending_review` | 10 | default |
| 评审中 | `under_review` | 20 | processing |
| 已批准 | `approved` | 30 | success |
| 已驳回 | `rejected` | 40 | error |
| 已关闭 | `closed` | 50 | default |

#### 2.1.9 `business_close_status` — 经营关闭状态

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/business/reports/index.vue:45`
- **硬编码值**：`['经营执行中', '经营收尾中', '执行关闭待回款']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 经营执行中 | `executing` | 10 | processing |
| 经营收尾中 | `closing` | 20 | warning |
| 执行关闭待回款 | `closed_pending_payment` | 30 | gold |

#### 2.1.10 `approval_status` — 审批状态

- **当前状态**：缺失
- **前端硬编码位置**：
  - `views/project/detail/composables/useProjectDetailPage.ts:248`
- **硬编码值**：`['草稿', '待审批', '已批准', '退回']`
- **建议种子数据**：

| Label | Value | Sort | TagColor |
|-------|-------|------|----------|
| 草稿 | `draft` | 10 | default |
| 待审批 | `pending` | 20 | processing |
| 已批准 | `approved` | 30 | success |
| 退回 | `returned` | 40 | error |

---

### 2.2 可以留在前端的（系统设计层面）

以下内容属于 PMP 标准术语或平台架构设计，不需要业务用户配置：

| 内容 | 前端位置 | 原因 |
|------|---------|------|
| 依赖类型 (FS/SS/FF/SF) | `useProjectDetailPage.ts:242` | PMP 标准，不会变 |
| 影响维度 (范围/进度/成本/质量/合同/预算) | `useProjectDetailPage.ts:250` | PMP 标准影响维度 |
| 质量检查结果 (通过/有条件通过/不通过/需整改) | `useProjectDetailPage.ts:255` | 质量管理标准 |
| 风险应对策略 (规避/转移/减轻/接受/升级) | `useProjectDetailPage.ts:256` | PMP 标准策略 |
| 数据范围模式 | `platform/roles/index.vue:50-54` | 平台权限模型 |
| 账号范围模式 | `platform/roles/index.vue:58-61` | 平台权限模型 |
| 菜单分区 | `platform/menus/index.vue:30-32` | 系统分区设计 |
| 回收站类型 | `platform/recycle-bin/index.vue:21-29` | 系统内部分类 |
| 存储源类型 | `platform/files/index.vue:133-135` | 技术配置 |

---

## 三、实施建议

### 3.1 种子数据补充

在 `PlatformDataSeedContributor.SeedDictionariesAsync()` 中补充上述 10 个字典类型及对应子项。

### 3.2 前端改造

1. 新增 composable `useDictOptions(dictTypeCode)` 封装字典获取逻辑
2. 业务页面将硬编码 `.map()` 替换为 `useDictOptions('xxx')`
3. Tag 颜色从字典 `tagColor` 字段读取，不再硬编码判断

### 3.3 后端改造

1. 确保 `getDictItemsApi` 接口支持按 Code 获取（当前已支持）
2. 考虑增加缓存，避免频繁查库

---

## 四、优先级

| 优先级 | 字典类型 | 理由 |
|--------|---------|------|
| P0 | `project_status`, `task_status`, `priority` | 项目管理核心状态，使用频率最高 |
| P0 | `raid_type`, `project_type`, `project_source` | 项目列表筛选必需 |
| P1 | `contract_type`, `change_status` | 经营和变更管理 |
| P1 | `business_close_status`, `approval_status` | 经营和审批流程 |
