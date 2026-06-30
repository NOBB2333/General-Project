---
name: general-admin-meeting
description: 整理 General Admin 项目会议纪要、提取行动项，并生成风险、问题或任务草稿。
---

# General Admin Meeting

## When To Use This Skill

Use this skill when the user asks to:

- 整理会议纪要。
- 从会议文本中提取行动项。
- 生成风险或问题草稿。
- 将会议内容关联项目、任务和负责人。
- 为项目报告准备会议摘要。

Do not use this skill to automatically write meeting records or create risks without human confirmation.

## Preconditions

- The target project is clear.
- General Admin CLI or authenticated API access is available.
- The meeting source file or text is available.
- The user has project meeting view permission.
- If write-back is requested, use dry-run first.

## Commands

```bash
general-admin project meeting summarize --project-id <project-id> --file <meeting.md> --format markdown --dry-run
general-admin project meeting actions --project-id <project-id> --file <meeting.md> --format json --dry-run
general-admin project meeting draft-risk --project-id <project-id> --file <meeting.md> --format markdown --dry-run
general-admin project meeting draft-issue --project-id <project-id> --file <meeting.md> --format markdown --dry-run
```

## Workflow

1. Resolve project context.
2. Read meeting source.
3. Extract decisions, risks, issues and action items.
4. Normalize action items with owner, due date and related object.
5. Generate draft meeting minutes.
6. For risk or issue creation, produce a draft only.
7. Ask for human confirmation before write-back.

## Output Format

```markdown
# 会议纪要

## 基本信息

- 项目：
- 会议时间：
- 参会人：

## 关键结论

- ...

## 行动项

| 行动项 | 责任人 | 截止时间 | 关联对象 |
|---|---|---|---|

## 风险 / 问题草稿

- ...
```

## Safety Rules

- 行动项必须保留责任人和截止时间。
- 不自动创建风险、问题或任务。
- 写回项目对象前必须人工确认。
- 不从会议文本推断不存在的人员或日期。
- 不把会议纪要当成审批记录。

## Error Recovery

- If owner is missing, mark it as “待确认”.
- If due date is missing, mark it as “待确认”.
- If meeting source is unreadable, ask for a pasted text or supported file.

## Acceptance Checklist

- [x] Meeting summary scenario is covered.
- [x] Action extraction scenario is covered.
- [x] Risk or issue draft scenario is covered.
- [x] Project/task association scenario is covered.
- [x] Meeting file command example is present.
- [x] Action output format is defined.
- [x] Write-back requires human confirmation.
