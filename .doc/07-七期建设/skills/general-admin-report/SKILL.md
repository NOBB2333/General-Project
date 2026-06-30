---
name: general-admin-report
description: 基于 General Admin 项目数据生成项目周报、月报、PMO 汇总报告和项目复盘草稿。
---

# General Admin Report

## When To Use This Skill

Use this skill when the user asks to:

- 生成项目周报。
- 生成项目月报。
- 生成 PMO 多项目汇总报告。
- 生成项目复盘草稿。
- 为会议或管理汇报准备项目报告材料。

Do not use this skill to approve reports, change project status, or overwrite existing reports without human confirmation.

## Preconditions

- The user has project view permission.
- General Admin CLI or authenticated API access is available.
- The reporting period is clear.
- The target project or project set is clear.
- EVM and health data must come from General Admin system values.
- Business-sensitive fields must be included only when the user has permission.

## Commands

```bash
general-admin project report weekly --project-id <project-id> --week-start <yyyy-mm-dd> --format markdown --dry-run
general-admin project report monthly --project-id <project-id> --month <yyyy-mm> --format markdown --dry-run
general-admin pmo report summary --status active --month <yyyy-mm> --format markdown --dry-run
general-admin project report retrospective --project-id <project-id> --format markdown --dry-run
```

## Workflow

1. Resolve project and reporting period.
2. Query project overview, WBS, active tasks, overdue tasks, risks, issues, worklogs and latest EVM snapshot.
3. For PMO reports, aggregate by project status, health, SPI/CPI, risk count and overdue items.
4. Generate a Markdown draft.
5. Include data source notes.
6. Ask for human confirmation before write-back.

## Output Format

```markdown
# 项目周报

## 本期概览

- 项目：
- 周期：
- 健康度：
- SPI / CPI：

## 本期完成

- ...

## 下期计划

- ...

## 风险与问题

- ...

## 需要协调

- ...

## 数据来源

- project get:
- task list:
- evm snapshot:
```

## Safety Rules

- PV、EV、AC、SPI、CPI 只能使用系统口径，不得编造。
- 报告默认是草稿。
- 写回项目报告必须先 dry-run，再由用户确认。
- 敏感经营数据必须按权限裁剪。
- 不把 AI 推断当作项目健康分来源。

## Error Recovery

- If EVM is missing, state that no snapshot exists.
- If worklog data is missing, distinguish “no records” from “no permission”.
- If reporting period is missing, ask for a concrete week or month.

## Acceptance Checklist

- [x] Weekly report scenario is covered.
- [x] Monthly report scenario is covered.
- [x] PMO summary scenario is covered.
- [x] Retrospective draft scenario is covered.
- [x] Markdown output is defined.
- [x] EVM source constraints are explicit.
- [x] Report write-back requires human confirmation.
