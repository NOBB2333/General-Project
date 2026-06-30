# 七期 Skill 包索引

> 版本：v1.0  
> 状态：已落地  
> 用途：作为 General Admin 项目 CLI Skill 化的统一索引、角色说明、权限说明和验收入口。

## 1. 索引原则

- Skill 是 AI 可读操作说明，不是新工具协议。
- Skill 面向 OpenCowork、Codex 和内部 AI 保持通用结构。
- Skill 不保存密钥、不携带 Token、不写死环境地址。
- Skill 不新增业务模块、不新增页面、不要求新增 MCP。
- Skill 中所有写操作必须要求 dry-run 和人工确认。
- 涉及金额、利润、合同、预算、回款等敏感数据时，必须声明权限裁剪。

## 2. 通用结构

每个 Skill 必须包含 frontmatter：

```markdown
---
name: general-admin-project
description: 查询、分析和汇总 General Admin 项目信息。
---
```

frontmatter 只放 `name` 和 `description`。兼容性、权限和运行前提统一写入 `Preconditions`，避免不同 AI Runtime 对额外 YAML 字段解析不一致。

每个 Skill 正文必须包含：

- When to use this skill
- Preconditions
- Commands
- Workflow
- Safety rules
- Output format
- Acceptance checklist

## 3. 命令约定

命令命名采用：

```text
general-admin <domain> <resource/action> [options]
```

示例：

```bash
general-admin project get --id <project-id>
general-admin project list --status active --format json
general-admin project report weekly --project-id <project-id> --format markdown
general-admin project risk list --project-id <project-id> --format json
general-admin project meeting summarize --project-id <project-id> --file <meeting.md> --dry-run
general-admin business overview --project-id <project-id> --format json
general-admin ops scheduler records --job-key <job-key> --format json
```

输出规则：

- 默认输出 JSON，便于 AI 读取。
- 报告类命令允许 `--format markdown`。
- 写操作必须支持 `--dry-run`。
- 真正写回必须显式传入 `--confirm`。
- 错误输出必须包含中文错误说明和可恢复建议。

当前 CLI 入口：

```bash
python3 Script/general-admin --help
```

常用环境变量：

```bash
export GENERAL_ADMIN_BASE_URL=http://localhost:44315
export GENERAL_ADMIN_TOKEN=<current-user-token>
```

Skill 包维护命令：

```bash
python3 Script/general-admin skill list
python3 Script/general-admin skill validate
python3 Script/general-admin skill pack --dry-run
python3 Script/general-admin skill install --target ~/.codex/skills --dry-run
```

CLI 默认只读。会触发写入的命令必须加 `--confirm`；仅检查请求或草稿时使用 `--dry-run`。

## 4. 首批 Skill 包

| Skill | 版本 | 适用角色 | 前置权限 | 典型任务 |
|---|---|---|---|---|
| `general-admin-project` | v1.0 | PM、PMO、项目成员 | 项目查看权限 | 查询项目状态、任务、WBS、排期、EVM |
| `general-admin-report` | v1.0 | PM、PMO | 项目查看权限、报告查看权限 | 生成周报、月报、PMO 汇总、复盘草稿 |
| `general-admin-risk` | v1.0 | PM、PMO | 项目风险查看权限 | 分析 RAID、高风险项、风险到期项 |
| `general-admin-meeting` | v1.0 | PM、项目成员 | 会议查看权限、项目查看权限 | 整理会议纪要、提取行动项、生成风险草稿 |
| `general-admin-business` | v1.0 | 经营管理人员、PMO | 经营查看权限 | 经营预警解释、回款分析、经营月报 |
| `general-admin-import-check` | v1.0 | 平台管理员、PMO | 文件查看权限、导入权限 | 检查导入文件、字段映射、缺失字段 |
| `general-admin-ops` | v1.0 | 平台管理员 | 运维查看权限 | 构建检查、迁移检查、日志和定时任务检查 |

当前已落地的 Skill 包：

```text
.doc/07-七期建设/skills/general-admin-project/SKILL.md
.doc/07-七期建设/skills/general-admin-report/SKILL.md
.doc/07-七期建设/skills/general-admin-risk/SKILL.md
.doc/07-七期建设/skills/general-admin-meeting/SKILL.md
.doc/07-七期建设/skills/general-admin-business/SKILL.md
.doc/07-七期建设/skills/general-admin-import-check/SKILL.md
.doc/07-七期建设/skills/general-admin-ops/SKILL.md
```

当前已落地的 CLI 覆盖域：

```text
project       项目查询、报告、风险、WBS、任务、工时、EVM、健康度、会议草稿
pmo           PMO 汇总报告和风险摘要
business      经营概览、项目经营、预警、回款、经营报告草稿
integration   连接器、投递日志、Webhook 模拟接收、同步任务
ops           构建 dry-run、调度记录、审计日志摘要
import        导入文件检查、字段映射、导入校验
skill         Skill 列表、校验、打包、安装
```

## 5. OpenCowork 使用说明

OpenCowork 使用时，把 Skill 包放入用户可加载的 skills 目录，或按 OpenCowork 的 Skill 同步机制导入。

使用规则：

- 新会话开始前确认 Skill 已加载。
- 让 Agent 先读取对应 `SKILL.md`。
- 对写操作必须保持人工确认。
- 需要真实外部消息投递时，不直接在 Skill 中保存密钥，必须走六期 Integration。

## 6. Codex 使用说明

Codex 使用时，Skill 包应保持通用 `SKILL.md` 结构。

使用规则：

- 先读取 `SKILL.md` 完整内容。
- 优先使用已有 CLI/API/页面上下文。
- 不执行破坏性命令。
- 不执行 `git add`、`git commit` 或生产数据清理，除非用户明确要求。
- 写操作先生成计划或 dry-run，再等待用户确认。

## 7. 内部 AI 使用说明

内部 AI 使用时，Skill 应作为场景说明和操作模板，不作为特权工具。

使用规则：

- 内部 AI 必须继承当前用户权限。
- 内部 AI 不允许绕过项目、经营、平台权限。
- 内部 AI 不允许直接读取连接器密钥。
- 内部 AI 写回项目、风险、会议、经营对象前必须产生草稿并等待人工确认。

## 8. 升级 MCP 的判断条件

满足以下条件时，再从 Skill 升级为 MCP 或内部 Tool Registry：

- 同一 Skill 被高频调用，且步骤稳定。
- 需要结构化输入输出，而不是自然语言说明。
- 需要跨系统稳定调用，例如读取项目、写入草稿、查询投递记录。
- 需要强审计、强权限、强幂等。
- CLI 调用对 AI 来说错误率高或上下文太长。

暂不升级 MCP 的情况：

- 只是操作规范。
- 只是报告写作流程。
- 只是人工辅助检查。
- 仍需要大量人工判断。

## 9. 通用验收清单

每个 Skill 完成时必须满足：

- [x] frontmatter 完整。
- [x] 使用场景明确。
- [x] 前置条件明确。
- [x] 命令示例不少于 2 个。
- [x] 安全规则明确。
- [x] 输出格式明确。
- [x] 写操作包含 dry-run 和 confirm。
- [x] 敏感数据包含权限裁剪说明。
- [x] 不要求新增后端 API。
- [x] 不要求新增 MCP。
- [x] 不要求新增页面。
