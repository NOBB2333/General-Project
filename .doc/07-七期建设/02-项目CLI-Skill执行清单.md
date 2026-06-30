# 七期项目 CLI Skill 执行清单

> 用法：每完成一项后把 `[ ]` 改为 `[x]`。  
> 原则：只做 Skill 包和操作说明，不新增业务模块，不实现 MCP runtime。

## P7-0 Skill 规范基线

- [x] 确认七期名称为 `项目 CLI Skill 化`。
- [x] 确认七期不新增 ABP 业务模块。
- [x] 确认七期不实现 MCP Server。
- [x] 确认七期不新增正式业务页面。
- [x] 确认 Skill 面向 OpenCowork、Codex 和内部 AI 保持通用结构。
- [x] 定义 General Admin Skill frontmatter 标准：只保留 `name`、`description`。
- [x] 定义兼容性说明位置：放入 Preconditions，不放入 frontmatter。
- [x] 定义 Skill 正文章节标准：使用场景、前置条件、命令、流程、安全规则、输出格式。
- [x] 定义 CLI 命令命名规范。
- [x] 定义 JSON 输出规范。
- [x] 定义 Markdown 输出规范。
- [x] 定义 dry-run 和 confirm 约定。
- [x] 定义敏感字段权限裁剪说明模板。
- [x] 定义错误恢复说明模板。
- [x] 新增轻量 CLI 入口 `Script/general-admin`。
- [x] CLI 支持 `GENERAL_ADMIN_BASE_URL` 和 `GENERAL_ADMIN_TOKEN` 环境变量。
- [x] CLI 默认只读，写操作必须显式 `--confirm`。
- [x] CLI 支持 `--dry-run`，且兼容全局参数放在子命令后的写法。

## P7-1 项目查询 Skill

- [x] 新增 `general-admin-project/SKILL.md`。
- [x] 描述项目概览查询场景。
- [x] 描述项目详情查询场景。
- [x] 描述 WBS、任务、排期、里程碑查询场景。
- [x] 描述成员、工时、健康度、EVM 查询场景。
- [x] 增加项目列表命令示例。
- [x] 增加项目详情命令示例。
- [x] 增加任务和 WBS 命令示例。
- [x] 增加 EVM 查询命令示例。
- [x] 增加项目摘要输出格式。
- [x] 增加无权限或项目不存在错误处理。

## P7-2 项目报告 Skill

- [x] 新增 `general-admin-report/SKILL.md`。
- [x] 描述项目周报生成场景。
- [x] 描述项目月报生成场景。
- [x] 描述 PMO 汇总报告场景。
- [x] 描述项目复盘草稿场景。
- [x] 增加周报命令示例。
- [x] 增加月报命令示例。
- [x] 增加 PMO 汇总命令示例。
- [x] 增加 Markdown 输出示例。
- [x] 明确 PV、EV、AC、SPI、CPI 只能使用系统口径。
- [x] 明确报告写回前必须人工确认。

## P7-3 风险分析 Skill

- [x] 新增 `general-admin-risk/SKILL.md`。
- [x] 描述 RAID 查询场景。
- [x] 描述高风险识别场景。
- [x] 描述风险到期项汇总场景。
- [x] 描述风险应对建议生成场景。
- [x] 增加风险列表命令示例。
- [x] 增加风险矩阵命令示例。
- [x] 增加风险摘要输出示例。
- [x] 明确风险等级以系统规则为准。
- [x] 明确 AI 只能解释和建议，不能替代风险评分。

## P7-4 会议纪要 Skill

- [x] 新增 `general-admin-meeting/SKILL.md`。
- [x] 描述会议纪要整理场景。
- [x] 描述行动项提取场景。
- [x] 描述风险/问题草稿生成场景。
- [x] 描述会议关联项目和任务场景。
- [x] 增加会议文件输入命令示例。
- [x] 增加行动项输出示例。
- [x] 增加生成风险草稿示例。
- [x] 明确行动项必须包含责任人和截止时间。
- [x] 明确写回项目对象前必须人工确认。

## P7-5 经营分析 Skill

- [x] 新增 `general-admin-business/SKILL.md`。
- [x] 描述经营概览查询场景。
- [x] 描述合同、预算、采购、发票、回款、验收分析场景。
- [x] 描述经营预警解释场景。
- [x] 描述回款催收建议场景。
- [x] 增加经营概览命令示例。
- [x] 增加回款预警命令示例。
- [x] 增加经营月报输出示例。
- [x] 明确敏感金额按权限裁剪。
- [x] 明确利润字段默认不外发。
- [x] 明确经营分析不替代财务核算。

## P7-6 数据导入检查 Skill

- [x] 新增 `general-admin-import-check/SKILL.md`。
- [x] 描述导入文件字段检查场景。
- [x] 描述字段字典对比场景。
- [x] 描述缺失字段提示场景。
- [x] 描述导入前检查报告场景。
- [x] 增加 CSV 检查命令示例。
- [x] 增加 Excel 检查命令示例。
- [x] 增加字段映射输出示例。
- [x] 明确不直接写库。
- [x] 明确不自动覆盖已有数据。

## P7-7 部署运维 Skill

- [x] 新增 `general-admin-ops/SKILL.md`。
- [x] 描述后端构建检查场景。
- [x] 描述前端构建检查场景。
- [x] 描述 DbMigrator 检查场景。
- [x] 描述日志中心检查场景。
- [x] 描述定时任务状态检查场景。
- [x] 增加后端构建命令示例。
- [x] 增加前端构建命令示例。
- [x] 增加定时任务执行记录查询示例。
- [x] 明确不执行破坏性命令。
- [x] 明确不清理生产数据。
- [x] 明确不输出密钥。

## P7-8 Skill 包索引与分发

- [x] 新增 Skill 包索引文档。
- [x] 为每个 Skill 增加版本号。
- [x] 为每个 Skill 增加适用角色。
- [x] 为每个 Skill 增加前置权限说明。
- [x] 为每个 Skill 增加示例任务。
- [x] 为每个 Skill 增加验收清单。
- [x] 增加 OpenCowork 使用说明。
- [x] 增加 Codex 使用说明。
- [x] 增加内部 AI 使用说明。
- [x] 增加后续升级 MCP 的判断条件。
- [x] CLI 支持 Skill 列表查看。
- [x] CLI 支持 Skill 结构校验。
- [x] CLI 支持 Skill 打包 dry-run。
- [x] CLI 支持 Skill 安装到目标目录。

## P7-8.5 CLI 命令实现

- [x] 实现项目列表和项目详情查询命令。
- [x] 实现项目 WBS、任务、排期、里程碑、成员查询命令。
- [x] 实现项目工时汇总命令。
- [x] 实现项目 EVM 快照命令。
- [x] 实现项目健康度解释命令。
- [x] 实现项目周报、月报、复盘草稿命令。
- [x] 实现 PMO 汇总报告草稿命令。
- [x] 实现 PMO 风险汇总命令。
- [x] 实现会议纪要、行动项、风险/问题草稿命令。
- [x] 实现经营概览、项目经营、经营预警、回款分析和经营月报草稿命令。
- [x] 实现 Integration 连接器、投递日志、Webhook 模拟接收和同步任务命令。
- [x] 实现运维构建 dry-run、调度查询和审计日志摘要命令。
- [x] 实现导入文件检查、字段映射和校验命令。

## P7-9 验收与交付

- [x] 7 个首批 Skill 均已完成。
- [x] 每个 Skill 都有 frontmatter。
- [x] 每个 Skill frontmatter 只包含 `name`、`description`。
- [x] 每个 Skill 都有使用场景。
- [x] 每个 Skill 都有前置条件。
- [x] 每个 Skill 都有命令示例。
- [x] 每个 Skill 都有安全规则。
- [x] 每个 Skill 都有输出格式。
- [x] 写操作 Skill 都包含 dry-run 和 confirm 说明。
- [x] 敏感数据 Skill 都包含权限裁剪说明。
- [x] Skill 包不要求新增后端 API。
- [x] Skill 包不要求新增 MCP。
- [x] Skill 包不要求新增页面。
- [x] `python3 Script/general-admin skill validate` 校验通过。
- [x] `python3 Script/general-admin skill pack --dry-run` 校验通过。
- [x] `python3 Script/general-admin import map-fields` 本地 CSV 示例校验通过。

> 说明：七期 CLI 是受控能力封装层，不是新的业务模块，也不是 MCP runtime。后续如果某些 Skill 高频、输入输出稳定、需要强审计和幂等，再升级为 MCP 或内部 Tool Registry。
