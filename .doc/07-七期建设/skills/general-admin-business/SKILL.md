---
name: general-admin-business
description: 查询和分析 General Admin 项目经营数据，包括经营概览、合同、预算、采购、发票、回款、验收、经营预警和经营月报草稿。
---

# General Admin Business

## When To Use This Skill

Use this skill when the user asks to:

- 查询项目经营概览。
- 分析合同、预算、采购、发票、回款、验收。
- 解释经营预警。
- 生成回款催收建议。
- 生成经营月报草稿。

Do not use this skill as a full financial accounting process.

## Preconditions

- The user has business view permission.
- General Admin CLI or authenticated API access is available.
- The target project or portfolio scope is clear.
- Amount, profit and receivable fields are allowed for the user.
- Any outbound message or催收通知 must be a draft unless confirmed.

## Commands

```bash
general-admin business overview --project-id <project-id> --format json
general-admin business warning list --project-id <project-id> --format json
general-admin business receivable overdue --project-id <project-id> --format json
general-admin business report monthly --project-id <project-id> --month <yyyy-mm> --format markdown --dry-run
```

## Workflow

1. Resolve project or portfolio scope.
2. Query business overview first.
3. Query contracts, budget, procurement, invoices, receivables and acceptances only as needed.
4. Explain warning reasons using system data.
5. Produce collection suggestions or monthly report drafts.
6. Mask or omit unauthorized sensitive fields.

## Output Format

```markdown
# 经营分析摘要

## 概览

- 合同金额：
- 已回款：
- 应收逾期：
- 预算偏差：

## 预警说明

- ...

## 建议动作

- ...

## 数据来源

- business overview:
- warning list:
- receivable overdue:
```

## Safety Rules

- 敏感金额按权限裁剪。
- 利润字段默认不外发。
- 不把经营分析当成完整财务核算。
- 不自动发送催收消息。
- 不自动修改合同、预算、回款或发票记录。

## Error Recovery

- If business data is missing, say which domain is empty.
- If permission is denied, omit sensitive fields and report the permission boundary.
- If warning reason is unclear, cite the warning object instead of guessing.

## Acceptance Checklist

- [x] Business overview scenario is covered.
- [x] Contract, budget, procurement, invoice, receivable and acceptance analysis is covered.
- [x] Warning explanation is covered.
- [x] Collection suggestion is covered.
- [x] Monthly report output is defined.
- [x] Sensitive amount filtering is explicit.
- [x] Financial accounting boundary is explicit.
