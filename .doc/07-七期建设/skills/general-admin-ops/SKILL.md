---
name: general-admin-ops
description: 检查 General Admin 后端构建、前端构建、DbMigrator、日志中心和定时任务状态，不执行破坏性命令。
---

# General Admin Ops

## When To Use This Skill

Use this skill when the user asks to:

- 检查后端构建。
- 检查前端构建。
- 检查 DbMigrator。
- 检查日志中心。
- 检查定时任务状态和执行记录。
- 准备部署前自检报告。

Do not use this skill to clean production data, reset repositories, delete migrations or expose secrets.

## Preconditions

- The target environment is clear: local, test, staging or production.
- Local repository access or authenticated operations API access is available.
- The user has permission to run diagnostics.
- Destructive operations are out of scope unless explicitly confirmed by the user.
- Any command that changes tracked files must be avoided unless the user asks for implementation.

## Commands

```bash
cd General.Admin && dotnet build General.Admin.slnx
cd General.Admin && dotnet test test/
cd General.Admin && dotnet run --project src/General.Admin.DbMigrator -- --dry-run
cd General.Web && pnpm build:general
general-admin ops audit-log summary --since 24h --format json
general-admin ops scheduler records --job-key <job-key> --format json
general-admin ops scheduler dashboard --format json
```

## Workflow

1. Confirm target environment.
2. Run non-destructive diagnostics.
3. Summarize build, test, migration and scheduler status.
4. Report failed commands with exact command and key error.
5. Do not auto-fix unless user asks for implementation.

## Output Format

```markdown
# 运维检查报告

## 环境

- 目标环境：
- 检查时间：

## 构建与测试

- 后端：
- 前端：
- 测试：

## 数据库迁移

- DbMigrator：

## 日志与定时任务

- 日志中心：
- 定时任务：

## 风险与建议

- ...
```

## Safety Rules

- 不执行 `git reset --hard`。
- 不执行 `git checkout --` 回滚用户改动。
- 不执行生产数据清理。
- 不输出密钥、连接串、Token 或 AppSecret。
- 不运行会重写 repo 文件的 formatter/codegen，除非用户明确要求。
- 不提交 Git change 或 staged change。

## Error Recovery

- If build fails, report the first meaningful compile error and affected project.
- If DbMigrator fails, report provider and connection source without exposing secrets.
- If scheduler records are empty, distinguish no records from permission/API failure.

## Acceptance Checklist

- [x] Backend build check is covered.
- [x] Frontend build check is covered.
- [x] DbMigrator check is covered.
- [x] Log center check is covered.
- [x] Scheduler records check is covered.
- [x] Destructive commands are prohibited.
- [x] Secrets are prohibited from output.
