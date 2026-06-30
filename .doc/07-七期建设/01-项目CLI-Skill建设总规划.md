# 七期建设总规划：项目 CLI Skill 化

> 版本：v1.0  
> 状态：首版已落地  
> 定位：能力封装层，不新增 ABP 业务模块，不实现 MCP runtime

## 1. 建设背景

五期 AI 模块解决系统内部的 AI 会话、Provider、MCP、工具注册、记忆和自动化任务。六期外部系统集成解决飞书、钉钉、企业微信等外部系统连接。

七期要解决的是另一类问题：把 General Admin 已经具备的项目管理能力，用 AI 容易理解、容易复用、容易审计的方式沉淀下来。

OpenCowork 的 `Skill` 设计可以作为参考。它的 Skill 本质是给 Agent 的可复用操作说明，通常不是新工具，而是告诉 Agent 如何组合已有工具完成某类任务。General Admin 七期采用这个思路：把项目 CLI/API/页面上下文的操作方式写成通用 `SKILL.md`，方便 OpenCowork、Codex 或内部 AI 调用。

本期不做：

- 不新增 ABP 业务模块
- 不新增 MCP Server
- 不做通用 Skill 市场
- 不绕过现有权限和审批
- 不把 Skill 当作可以直接执行的特权工具

## 2. 模块定位

显示名称：`项目 CLI Skill 化`

后端模块：不新增

前端页面：不新增正式业务页面

建议目录：

```text
.doc/07-七期建设/
  01-项目CLI-Skill建设总规划.md
  02-项目CLI-Skill执行清单.md
  03-Skill包索引.md
  skills/
    general-admin-project/SKILL.md

docs/skills/
  general-admin-project/SKILL.md
  general-admin-report/SKILL.md
  general-admin-risk/SKILL.md
  general-admin-meeting/SKILL.md
  general-admin-business/SKILL.md
  general-admin-import-check/SKILL.md
  general-admin-ops/SKILL.md
```

当前已新增轻量 CLI：

```text
Script/general-admin
```

该 CLI 面向 AI Skill、本地诊断和受控 API 调用，使用 Python 标准库实现，不引入新的运行时依赖。

## 3. 设计原则

1. Skill 是操作说明，不是新工具协议。
2. Skill 面向 OpenCowork、Codex 和内部 AI 保持通用结构。
3. Skill 不保存密钥，不携带 Token，不写死环境地址。
4. Skill 只能调用已有 CLI/API/页面流程，不绕过权限。
5. 涉及写操作必须要求人工确认。
6. 涉及合同、预算、利润、回款等敏感数据必须提示权限裁剪。
7. Skill 输出必须可审查、可复盘、可复制到项目文档或报告中。
8. 如果未来需要更稳定的工具调用，再从 Skill 升级到 MCP 或内部 Tool Registry。

## 4. OpenCowork Skill 参考

OpenCowork 的 Skill 通常包含：

- frontmatter：`name`、`description`、`compatibility`
- 使用场景
- 前置条件
- 操作步骤
- 命令示例
- 决策指南
- 边界和错误恢复
- 脚本或引用文件说明

General Admin 的 Skill 也应保持类似结构：

```markdown
---
name: general-admin-project
description: 查询、分析和汇总 General Admin 项目信息。
compatibility: Requires General Admin CLI or authenticated API access.
---

# General Admin Project

## When to use this skill

## Preconditions

## Commands

## Workflow

## Safety rules

## Output format
```

## 5. 首批 Skill 包

### 5.1 项目查询 Skill

用途：

- 查询项目概览
- 查询项目详情
- 查询 WBS、任务、排期、里程碑
- 查询成员、工时、健康度、EVM

典型输出：

- 项目摘要
- 项目状态说明
- 待处理事项
- 风险提示

### 5.2 项目报告 Skill

用途：

- 生成项目周报
- 生成项目月报
- 生成 PMO 汇总报告
- 生成项目复盘草稿

要求：

- 使用后端定义的指标口径
- 输出引用数据来源
- 不编造 PV、EV、AC、SPI、CPI

### 5.3 风险分析 Skill

用途：

- 分析 RAID
- 识别高风险项
- 汇总风险到期项
- 生成风险应对建议

要求：

- 风险分值以系统已有概率、影响和后端规则为准
- AI 只能解释和建议，不能替代风险等级口径

### 5.4 会议纪要 Skill

用途：

- 整理会议纪要
- 提取行动项
- 生成风险/问题草稿
- 关联项目、任务和负责人

要求：

- 行动项必须保留责任人和截止时间
- 写回项目对象前必须人工确认

### 5.5 经营分析 Skill

用途：

- 分析合同、预算、采购、发票、回款、验收
- 解释经营预警
- 生成回款催收建议
- 生成经营月报草稿

要求：

- 敏感金额和利润必须按权限裁剪
- 不把经营分析当成完整财务核算

### 5.6 数据导入检查 Skill

用途：

- 检查导入文件字段
- 对比字段字典
- 提示缺失字段
- 生成导入前检查报告

要求：

- 不直接写库
- 不自动覆盖已有数据

### 5.7 部署运维 Skill

用途：

- 检查后端构建
- 检查前端构建
- 检查 DbMigrator
- 检查日志中心和定时任务状态

要求：

- 不执行破坏性命令
- 不清理生产数据
- 不输出密钥

## 6. CLI 命令约定

本期已提供轻量 CLI，Skill 按稳定命令风格编写，并与 `Script/general-admin` 对齐。

命令风格：

```bash
general-admin project get --id <project-id>
general-admin project list --status active
general-admin project report weekly --project-id <project-id>
general-admin project risk list --project-id <project-id>
general-admin project meeting summarize --project-id <project-id> --file <meeting.md>
general-admin business overview --project-id <project-id>
general-admin ops scheduler records --job-key <job-key>
```

输出建议：

- 默认输出 JSON，便于 AI 读取
- 增加 `--format markdown` 用于报告草稿
- 所有写操作增加 `--dry-run` 和 `--confirm`
- 所有命令返回清晰错误码和中文错误说明

运行前提：

```bash
export GENERAL_ADMIN_BASE_URL=http://localhost:44315
export GENERAL_ADMIN_TOKEN=<current-user-token>
```

当前 CLI 覆盖：

- `project list/get/dashboard/report/risk/wbs/task/schedule/milestone/member/worklog/evm/health/meeting`
- `pmo report/risk`
- `business overview/project/warning/receivable/report`
- `integration connectors/delivery/webhook/sync`
- `ops build/scheduler/audit-log`
- `import inspect/map-fields/validate`
- `skill list/validate/pack/install`

安全约定：

- 认证信息只从环境变量或命令参数读取，不写入 Skill 包。
- 默认只读；写操作必须显式传入 `--confirm`。
- `--dry-run` 可放在主命令前，也可放在子命令后，方便人和 AI 使用。
- 本地文件读取用于会议纪要、导入检查等数据获取；Bash/构建命令只用于受控诊断，不执行破坏性操作。

## 7. 安全边界

Skill 必须写明：

- 当前用户必须已登录或已配置受控访问凭据
- 只读取用户有权限访问的数据
- 写操作必须先生成草稿或 dry-run
- 变更、审批、合同、预算、利润相关操作必须人工确认
- 不把 Token、AppSecret、数据库连接串写入 Skill
- 不建议在生产环境执行删除、覆盖、批量修改类命令

## 8. 分阶段交付

| 阶段 | 主题 | 核心交付 |
|---|---|---|
| P7-0 | Skill 规范 | 通用 SKILL.md 模板、命令风格、输出格式、安全边界 |
| P7-1 | 项目查询 | 项目查询 Skill、示例命令、输出样例 |
| P7-2 | 项目报告 | 周报、月报、PMO 汇总报告 Skill |
| P7-3 | 风险会议 | 风险分析、会议纪要、行动项 Skill |
| P7-4 | 经营分析 | 经营预警、回款、合同预算分析 Skill |
| P7-5 | 数据导入与运维 | 导入检查、构建检查、调度记录检查 Skill |
| P7-6 | 验收与分发 | Skill 包索引、版本说明、示例任务、交付说明 |

## 9. 验收标准

- 至少完成 7 个首批 Skill 包规划。
- 每个 Skill 都有 `name`、`description`。
- 每个 Skill 都包含使用场景、前置条件、操作步骤、安全规则和输出格式。
- 每个 Skill 都有至少 2 个命令示例。
- 写操作 Skill 必须包含 dry-run 和人工确认说明。
- 敏感数据 Skill 必须包含权限裁剪说明。
- Skill 包不要求新增后端 API，不要求新增 MCP，不要求新增页面。
- `Script/general-admin skill validate` 能校验全部 Skill。
- `Script/general-admin skill pack --dry-run` 能生成打包计划。
