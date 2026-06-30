---
name: general-admin-risk
description: 查询和分析 General Admin 项目 RAID、风险等级、高风险项、风险到期项和风险应对建议。
---

# General Admin Risk

## When To Use This Skill

Use this skill when the user asks to:

- 查询项目 RAID。
- 识别高风险项。
- 汇总风险到期项。
- 生成风险应对建议。
- 为 PMO 治理报告准备风险摘要。

Do not use this skill to silently change risk scores, approve mitigation actions, or close risks.

## Preconditions

- The user has project risk view permission.
- General Admin CLI or authenticated API access is available.
- The target project or project set is clear.
- Risk probability, impact and score must come from General Admin.
- Suggested actions are advisory drafts only.

## Commands

```bash
general-admin project risk list --project-id <project-id> --status open --format json
general-admin project risk matrix --project-id <project-id> --format json
general-admin project risk due --project-id <project-id> --days 14 --format json
general-admin pmo risk summary --status active --format markdown
```

## Workflow

1. Resolve project or portfolio scope.
2. Query RAID items.
3. Separate risks, assumptions, issues and dependencies.
4. Identify high score, overdue and soon-due risks.
5. Summarize owner, response strategy, trigger condition and contingency plan.
6. Produce advisory mitigation suggestions without changing system data.

## Output Format

```markdown
# 风险摘要

## 高风险项

| 风险 | 分值 | 责任人 | 到期日 | 建议 |
|---|---:|---|---|---|

## 即将到期

- ...

## 应对建议

- ...

## 数据来源

- risk list:
- risk matrix:
```

## Safety Rules

- 风险等级以系统规则为准。
- AI 只能解释和建议，不能替代风险评分。
- 不自动关闭风险。
- 不自动创建风险应对行动。
- 涉及通知或升级时，必须走 Integration 或人工确认。

## Error Recovery

- If no risk records exist, say so directly.
- If risk score is missing, report missing probability or impact.
- If permission is denied, stop and report the required permission.

## Acceptance Checklist

- [x] RAID query is covered.
- [x] High-risk detection is covered.
- [x] Due-risk summary is covered.
- [x] Mitigation suggestions are covered.
- [x] Risk list command example is present.
- [x] Risk matrix command example is present.
- [x] Risk score boundary is explicit.
