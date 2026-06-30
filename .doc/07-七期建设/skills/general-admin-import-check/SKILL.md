---
name: general-admin-import-check
description: 检查 General Admin 导入文件字段、字段字典、缺失字段和导入前风险，不直接写库或覆盖数据。
---

# General Admin Import Check

## When To Use This Skill

Use this skill when the user asks to:

- 检查 CSV 或 Excel 导入文件。
- 对比字段字典。
- 查找缺失字段或多余字段。
- 生成导入前检查报告。
- 检查项目、经营或平台数据导入风险。

Do not use this skill to import, overwrite or delete data.

## Preconditions

- The import file path is available.
- Local file access and the General Admin field dictionary or import template are available.
- The target import type is clear.
- The expected field dictionary or template is available.
- The user understands this is a validation-only workflow.

## Commands

```bash
general-admin import inspect --type project-task --file <tasks.csv> --format markdown
general-admin import inspect --type business-receivable --file <receivables.xlsx> --format markdown
general-admin import map-fields --type project-task --file <tasks.csv> --format json
general-admin import validate --type project-task --file <tasks.csv> --strict --format markdown
```

## Workflow

1. Identify import type.
2. Inspect file headers and row count.
3. Compare headers with expected fields.
4. Report missing, extra and ambiguous fields.
5. Sample a few rows for empty values and obvious format issues.
6. Generate a pre-import report.
7. Stop before any write operation.

## Output Format

```markdown
# 导入前检查报告

## 文件概览

- 文件：
- 类型：
- 行数：
- 字段数：

## 字段检查

| 字段 | 状态 | 说明 |
|---|---|---|

## 风险提示

- ...

## 建议处理

- ...
```

## Safety Rules

- 不直接写库。
- 不自动覆盖已有数据。
- 不自动创建项目、合同、任务或回款记录。
- 不输出文件中的敏感数据样本，除非用户明确要求。
- 如果需要正式导入，必须切换到专门导入流程并人工确认。

## Error Recovery

- If file encoding fails, retry UTF-8-SIG or ask for converted file.
- If template is missing, ask for target import type or field dictionary.
- If duplicate columns exist, report them and stop strict validation.

## Acceptance Checklist

- [x] CSV check command is present.
- [x] Excel check command is present.
- [x] Field mapping output is defined.
- [x] Missing field report is defined.
- [x] No database write is allowed.
- [x] No overwrite is allowed.
