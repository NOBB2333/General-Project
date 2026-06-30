---
name: general-admin-project
description: 查询、分析和汇总 General Admin 项目信息，包括项目概览、项目详情、WBS、任务、排期、里程碑、成员、工时、健康度和 EVM。
---

# General Admin Project

## When To Use This Skill

Use this skill when the user asks to:

- 查询项目列表或某个项目详情。
- 汇总项目当前状态。
- 查看 WBS、任务、排期、里程碑。
- 分析成员、工时、健康度、PV、EV、AC、SPI、CPI。
- 生成项目状态摘要、待处理事项或风险提示。
- 为周报、PMO 汇总或项目复盘准备项目上下文。

Do not use this skill for:

- 审批项目章程、变更或基线。
- 修改合同、预算、回款或利润数据。
- 绕过 General Admin 权限直接读数据库。
- 直接发送飞书、钉钉、企业微信消息。

## Preconditions

Before running project commands, confirm:

- The user has authenticated access to General Admin.
- General Admin CLI or authenticated API access is available.
- The target tenant, project, or environment is clear.
- The current user has project view permission.
- Any sensitive business fields are allowed for the current user.
- Any write action must be requested as a dry-run draft first.

If the project identifier is missing, ask for one of:

- project id
- project code
- project name
- current project context from the active page or conversation

## Commands

Use this command style when a General Admin CLI exists:

```bash
general-admin project list --status active --format json
general-admin project get --id <project-id> --format json
general-admin project wbs list --project-id <project-id> --format json
general-admin project task list --project-id <project-id> --status active --format json
general-admin project schedule list --project-id <project-id> --format json
general-admin project milestone list --project-id <project-id> --format json
general-admin project member list --project-id <project-id> --format json
general-admin project worklog summary --project-id <project-id> --format json
general-admin project evm snapshot --project-id <project-id> --latest --format json
general-admin project health explain --project-id <project-id> --format markdown
```

If only authenticated API access is available, use the equivalent API wrapper. Do not call private database tables directly unless the user explicitly asks for a local diagnostic and confirms the target database.

## Workflow

1. Identify the project context.
2. Query the project overview first.
3. Query details only for the requested scope.
4. For planning questions, include WBS, tasks, dependencies, schedules and milestones.
5. For execution questions, include active tasks, overdue tasks, blockers, issues and worklogs.
6. For PMO questions, include health, RAID count, EVM, SPI/CPI and pending governance actions.
7. Summarize in Chinese with source fields and concrete dates.
8. If data is missing, say which command or permission is missing instead of inventing values.

## Output Format

For a project status summary, use:

```markdown
## 项目摘要

- 项目：
- 状态：
- PM：
- 当前排期：
- 健康度：
- SPI / CPI：

## 当前进展

- ...

## 待处理事项

- ...

## 风险提示

- ...

## 数据来源

- project get:
- evm snapshot:
- task list:
```

For JSON handoff to another tool, use:

```json
{
  "projectId": "",
  "projectName": "",
  "status": "",
  "healthScore": null,
  "spi": null,
  "cpi": null,
  "activeTasks": 0,
  "overdueTasks": 0,
  "blockedTasks": 0,
  "openRisks": 0,
  "pendingActions": []
}
```

## Safety Rules

- Read only by default.
- Do not update project status, charter, baseline, task, risk or business data from this skill.
- Do not expose hidden business amounts, profit, contract or receivable data unless permission is explicit.
- Do not treat AI explanation as the source of health score, SPI or CPI; use system values.
- Do not invent project IDs, dates, owners, progress or EVM values.
- If a write-back is requested, produce a draft and require `--dry-run` first.
- If a confirmed write-back is later supported, require `--confirm` and human approval.

## Error Recovery

- If the project is not found, retry with project code or exact project name.
- If permission is denied, report the missing permission and stop.
- If EVM data is empty, say no snapshot exists and suggest generating or checking performance snapshots.
- If task or WBS data is empty, distinguish between “no data” and “no permission”.
- If CLI is unavailable, fall back to authenticated API access or ask for exported JSON.

## Acceptance Checklist

- [x] Project overview can be queried.
- [x] Project detail can be queried.
- [x] WBS, tasks, schedules and milestones are covered.
- [x] Members, worklogs, health and EVM are covered.
- [x] At least two command examples are present.
- [x] Output formats include Markdown and JSON.
- [x] Permission and missing-data behavior is defined.
- [x] Write-back is explicitly blocked unless dry-run and confirmation are available.
