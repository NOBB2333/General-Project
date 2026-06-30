# P2 - 差异化竞争 PRD

> 版本：v1.0
> 日期：2026-05-21
> 预估周期：4-6 个月（P1 完成后启动）
> 目标：引入 AI 能力和高级特性，形成差异化竞争优势

---

## 0. 路线修订说明（2026-06）

本文是四期早期 P2 差异化 PRD，保留作为五期 AI 和六期集成的需求来源参考。

当前正式路线已调整：

- AI 自然语言查询、AI 报告、AI 任务分配、MCP 能力统一进入五期 `AI 智能能力区`。
- IM 集成、企业微信、钉钉、飞书、Webhook 投递与回流统一进入六期 `外部系统集成区`。
- 本文中 `Project + Business`、`Platform` 等旧归属不再作为模块边界依据。

后续实现以：

- `.doc/05-五期建设/01-AI模块建设总规划.md`
- `.doc/05-五期建设/02-AI模块执行清单.md`
- `.doc/06-六期建设/01-外部系统集成模块建设总规划.md`
- `.doc/06-六期建设/02-外部系统集成执行清单.md`

为准。

---

## 1. 修订说明

P2 聚焦于 **AI 能力接入、MCP 生态和高级业务特性**。这些功能在 2026 年是项目管理系统的前沿能力，能显著提升产品竞争力。参考 Linear AI、Plane AI、ClickUp Brain 等竞品的 AI 能力设计。

---

## 2. 需求总览

| 编号 | 需求 | 归属模块 | 优先级 | 预估工时 |
|------|------|----------|--------|----------|
| P2-01 | AI 自然语言查询 | Project + Business | P2-Critical | 8-12 人天 |
| P2-02 | AI 报告自动生成 | Project + Business | P2-Critical | 8-12 人天 |
| P2-03 | MCP Server | Platform | P2-High | 10-15 人天 |
| P2-04 | 工作流引擎 | Platform | P2-High | 15-20 人天 |
| P2-05 | 数据导入引擎 | Platform | P2-High | 5-8 人天 |
| P2-06 | AI 任务智能分配 | Project | P2-Medium | 5-8 人天 |
| P2-07 | IM 集成（企业微信/钉钉/飞书） | Platform | P2-Medium | 5-8 人天 |
| P2-08 | 项目模板 | Project | P2-Medium | 3-5 人天 |

---

## 3. P2-01 AI 自然语言查询

### 3.1 问题描述

当前用户必须通过表单筛选、点击菜单等方式查询项目数据。2026 年的用户期望像和人对话一样查询系统："本月哪些任务有延期风险？"、"张三上周完成了多少 Story Points？"。

**竞品参考**：
- **Linear AI**：支持自然语言搜索 Issue，理解上下文
- **ClickUp Brain**：Ambient Answers，可查询整个工作区的数据
- **Notion AI**：自然语言查询数据库，生成筛选条件
- **Plane AI**：AI 有整个工作区的上下文感知

### 3.2 改进建议

| 能力 | 说明 |
|------|------|
| 自然语言查询 | 用户输入自然语言，AI 转化为查询并返回结果 |
| 上下文感知 | AI 了解当前用户、当前项目、历史对话 |
| 多数据源 | 可查询任务、项目、工时、合同、报表等 |
| 查询解释 | AI 展示它理解的查询意图和执行的查询条件 |
| 追问能力 | 支持基于上一轮结果继续追问 |
| 常用查询 | 预置常用查询模板（延期任务、本周计划等） |

### 3.3 方案设计

#### 交互流程

```
用户输入："本月哪些任务有延期风险？"
  │
  ▼
AI 理解意图 → 识别实体（任务）、时间范围（本月）、条件（延期风险）
  │
  ▼
AI 生成查询计划：
  - 查询本月截止日期 < 今天 + 状态 != 已完成 的任务
  - 按截止日期排序
  │
  ▼
系统执行查询 → 返回结果列表
  │
  ▼
AI 生成自然语言摘要：
  "本月共有 5 个任务存在延期风险：
   1. [PROJ-001] 用户登录模块 - 已逾期 3 天
   2. [PROJ-005] 数据导出功能 - 明天到期，状态仍为进行中
   ..."
```

#### 架构设计

```
用户输入自然语言
  │
  ▼
前端 AI Chat 组件
  │
  ▼
后端 AI Query Service
  ├── Step 1: 意图识别（调用 LLM）
  │   输入: 用户问题 + 可用工具描述
  │   输出: 查询计划（JSON）
  │
  ├── Step 2: 查询执行
  │   根据查询计划调用对应的 AppService
  │
  ├── Step 3: 结果总结（调用 LLM）
  │   输入: 原始数据 + 用户问题
  │   输出: 自然语言回答
  │
  └── Step 4: 流式输出（SSE）
      前端逐字展示 AI 回答
```

### 3.4 技术实现方案

#### 后端

**AI 基础设施（Platform 模块）**：

```csharp
// LLM 调用封装
public interface ILlmService
{
    Task<string> ChatAsync(List<ChatMessage> messages, ChatOptions? options = null);
    IAsyncEnumerable<string> ChatStreamAsync(List<ChatMessage> messages, ChatOptions? options = null);
}

// 支持多 LLM 后端
public class OpenAICompatibleLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly AiConfiguration _config;

    // 支持 OpenAI / DeepSeek / 通义千问 / 本地模型等 OpenAI 兼容接口
}
```

**AI 查询服务（Project 模块）**：

```csharp
public class AiQueryAppService : ApplicationService
{
    private readonly ILlmService _llmService;
    private readonly IServiceProvider _serviceProvider;

    // 流式查询（SSE）
    public async IAsyncEnumerable<string> QueryStreamAsync(
        string question, Guid? projectId, [EnumeratorCancellation] CancellationToken ct)
    {
        // 1. 构建工具描述
        var tools = BuildToolDescriptions();

        // 2. 调用 LLM 理解意图
        var plan = await _llmService.ChatAsync(new[]
        {
            new ChatMessage("system", GetSystemPrompt()),
            new ChatMessage("user", question)
        }, new ChatOptions { Tools = tools });

        // 3. 执行查询
        var result = await ExecuteQueryAsync(plan);

        // 4. 流式输出总结
        await foreach (var chunk in _llmService.ChatStreamAsync(new[]
        {
            new ChatMessage("system", "根据以下数据回答用户问题，用简洁的中文："),
            new ChatMessage("user", $"问题：{question}\n数据：{JsonSerializer.Serialize(result)}")
        }))
        {
            yield return chunk;
        }
    }

    // 可用工具定义（类似 Function Calling）
    private List<ToolDescription> BuildToolDescriptions()
    {
        return new List<ToolDescription>
        {
            new("query_tasks", "查询项目任务", new {
                project_id = "项目ID（可选）",
                status = "任务状态过滤",
                assignee_id = "负责人ID（可选）",
                overdue = "是否只查逾期任务",
                date_range = "日期范围"
            }),
            new("query_milestones", "查询里程碑", new { }),
            new("query_worklogs", "查询工时记录", new { }),
            new("query_contracts", "查询合同", new { }),
            new("query_project_summary", "查询项目汇总", new { })
        };
    }
}
```

**SSE 端点**：

```csharp
[ApiController]
[Route("api/app/ai")]
public class AiController : ControllerBase
{
    [HttpGet("query")]
    public async Task QueryAsync([FromQuery] string question, [FromQuery] Guid? projectId)
    {
        Response.ContentType = "text/event-stream";

        await foreach (var chunk in _aiQueryService.QueryStreamAsync(question, projectId, HttpContext.RequestAborted))
        {
            await Response.WriteAsync($"data: {chunk}\n\n");
            await Response.Body.FlushAsync();
        }
    }
}
```

#### 前端

**AI 查询组件**：

```vue
<template>
  <div class="ai-query-panel">
    <div class="chat-messages">
      <div v-for="msg in messages" :key="msg.id" :class="msg.role">
        <MarkdownRender v-if="msg.role === 'assistant'" :content="msg.content" />
        <span v-else>{{ msg.content }}</span>
      </div>
    </div>
    <div class="chat-input">
      <InputSearch
        v-model="question"
        placeholder="问我任何关于项目的问题..."
        @search="handleQuery"
        :loading="loading"
      />
      <div class="quick-queries">
        <Tag @click="quickQuery('本周有哪些任务到期？')">本周到期任务</Tag>
        <Tag @click="quickQuery('本月工时统计')">本月工时</Tag>
        <Tag @click="quickQuery('延期风险任务')">延期风险</Tag>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
async function handleQuery(question: string) {
  loading.value = true
  const assistantMsg = { role: 'assistant', content: '' }
  messages.value.push(assistantMsg)

  // SSE 流式读取
  const response = await fetch(`/api/app/ai/query?question=${encodeURIComponent(question)}`)
  const reader = response.body.getReader()
  const decoder = new TextDecoder()

  while (true) {
    const { done, value } = await reader.read()
    if (done) break
    const text = decoder.decode(value)
    // 解析 SSE data
    const lines = text.split('\n').filter(l => l.startsWith('data: '))
    for (const line of lines) {
      assistantMsg.content += line.slice(6)
    }
  }
  loading.value = false
}
</script>
```

---

## 4. P2-02 AI 报告自动生成

### 4.1 问题描述

PM 和管理层需要定期撰写周报、月报、项目总结。这些报告的数据来源分散在任务、工时、里程碑、合同等各个模块中，手工汇总耗时且容易遗漏。

**竞品参考**：
- **Linear Insights**：自动生成团队进度洞察
- **ClickUp Brain**：自动总结项目状态
- **Notion AI**：基于数据库数据生成报告

### 4.2 改进建议

| 能力 | 说明 |
|------|------|
| 周报自动生成 | 从任务变更、工时、代码提交中提取本周进展 |
| 月报自动生成 | 月度汇总，含趋势分析和风险提示 |
| 项目总结 | 项目完成后的全面总结报告 |
| 经营分析报告 | 合同、回款、预算的经营分析（Business 模块） |
| 自定义报告 | 用户指定报告维度和内容范围 |
| 报告模板 | 预置报告模板，支持自定义模板 |
| 多语言 | 基于 LLM 自动翻译 |

### 4.3 方案设计

#### 报告生成流程

```
用户选择报告类型（周报/月报/项目总结）
  │
  ▼
系统采集数据：
  ├── 本周任务变更（创建、完成、状态流转）
  ├── 本周工时记录
  ├── 里程碑进展
  ├── Git 提交和 PR（如已集成）
  ├── RAID 变更
  └── 经营数据（合同、回款等）
  │
  ▼
数据预处理和统计：
  ├── 完成率、延期率
  ├── 工时分布
  ├── 风险任务列表
  └── 趋势对比（与上周/上月）
  │
  ▼
调用 LLM 生成报告：
  输入: 统计数据 + 报告模板 + 用户角色
  输出: 结构化的自然语言报告
  │
  ▼
用户审阅和编辑
  │
  ▼
导出（PDF/Word/Markdown）
```

### 4.4 技术实现方案

#### 后端

```csharp
public class AiReportAppService : ApplicationService
{
    private readonly ILlmService _llmService;
    private readonly IRepository<ProjectTask> _taskRepo;
    private readonly IRepository<ProjectWorklog> _worklogRepo;
    private readonly IRepository<ProjectMilestone> _milestoneRepo;

    // 生成周报
    public async Task<ReportDto> GenerateWeeklyReportAsync(GenerateReportInput input)
    {
        var weekStart = GetWeekStart(input.Date ?? DateTime.Today);
        var weekEnd = weekStart.AddDays(7);

        // 1. 采集数据
        var data = await CollectReportDataAsync(input.ProjectId, weekStart, weekEnd);

        // 2. 调用 LLM 生成报告
        var report = await _llmService.ChatAsync(new[]
        {
            new ChatMessage("system", GetReportSystemPrompt(input.ReportType)),
            new ChatMessage("user", JsonSerializer.Serialize(data))
        });

        return new ReportDto
        {
            Type = input.ReportType,
            Period = $"{weekStart:yyyy-MM-dd} ~ {weekEnd:yyyy-MM-dd}",
            Content = report,
            Data = data
        };
    }

    private async Task<ReportData> CollectReportDataAsync(
        Guid projectId, DateTime from, DateTime to)
    {
        return new ReportData
        {
            // 任务统计
            TasksCreated = await _taskRepo.CountAsync(t =>
                t.ProjectId == projectId && t.CreationTime >= from && t.CreationTime < to),
            TasksCompleted = await _taskRepo.CountAsync(t =>
                t.ProjectId == projectId && t.Status == ProjectTaskStatus.Done
                && t.CompletedTime >= from && t.CompletedTime < to),
            TasksOverdue = await _taskRepo.CountAsync(t =>
                t.ProjectId == projectId && t.Deadline < DateTime.Now
                && t.Status != ProjectTaskStatus.Done),

            // 工时统计
            TotalWorklogHours = await _worklogRepo
                .Where(w => w.ProjectId == projectId && w.Date >= from && w.Date < to)
                .SumAsync(w => w.Hours),

            // 里程碑进展
            Milestones = await _milestoneRepo
                .Where(m => m.ProjectId == projectId)
                .Select(m => new { m.Name, m.Status, m.Deadline })
                .ToListAsync()
        };
    }
}
```

#### 前端

```
views/project/report/
├── AiReportGenerator.vue      # 报告生成页
├── ReportPreview.vue           # 报告预览（Markdown 渲染）
├── ReportExport.vue            # 导出（PDF/Word/Markdown）
└── ReportTemplateManager.vue   # 模板管理（可选）
```

---

## 5. P2-03 MCP Server

### 5.1 问题描述

MCP (Model Context Protocol) 是 Anthropic 推出的开放标准，让 AI Agent（如 Claude Code）通过标准协议与外部系统交互。2026 年，MCP 已成为 SaaS 产品的标配接口。

没有 MCP Server，AI Agent 就无法直接操作你的 PM 系统。用户必须在 AI 工具和 PM 系统之间来回切换。

**竞品参考**：
- **Plane MCP**：标准 MCP 协议，支持创建/查询 Issue
- **Linear MCP**：支持 Issue 管理、项目查询
- **GitHub MCP**：代码管理、PR 操作

### 5.2 改进建议

| 能力 | 说明 |
|------|------|
| MCP Server 模块 | 在 ABP 模块化架构中新增 MCP Server 模块 |
| 工具注册 | 自动从 AppService 生成 MCP 工具定义 |
| 标准协议 | 完全兼容 MCP 协议规范 |
| 认证 | 支持 API Key 和 OAuth 2.0 认证 |
| 权限控制 | MCP 调用受权限系统控制 |

### 5.3 方案设计

#### MCP 工具清单

| MCP 工具 | 说明 | 对应 API |
|----------|------|----------|
| list_projects | 列出项目 | GET /api/app/project |
| get_project | 获取项目详情 | GET /api/app/project/{id} |
| create_task | 创建任务 | POST /api/app/task |
| update_task | 更新任务 | PUT /api/app/task/{id} |
| transition_task | 任务状态流转 | PUT /api/app/task/{id}/transition |
| list_tasks | 查询任务 | GET /api/app/task |
| create_milestone | 创建里程碑 | POST /api/app/milestone |
| log_worklog | 录入工时 | POST /api/app/worklog |
| get_project_summary | 项目汇总 | GET /api/app/project/{id}/summary |
| search | 全文搜索 | GET /api/app/search |

#### 用户使用示例

```bash
# 在 Claude Code 中配置
claude mcp add general-admin https://your-domain.com/mcp --header "Authorization: Bearer xxx"

# 然后在 Claude Code 中：
> "查看当前 Sprint 的未完成任务"
> "创建一个新任务：实现用户导出功能，分配给张三"
> "把 PROJ-023 标记为已完成"
> "本周的工时统计是多少？"
```

### 5.4 技术实现方案

#### 后端

**MCP Server 模块**：

```csharp
[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class AbpMcpServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 MCP 端点
        context.Services.AddMcpServer(options =>
        {
            options.AddTools<McpProjectTools>();
            options.AddTools<McpTaskTools>();
            options.AddTools<McpMilestoneTools>();
            options.AddTools<McpWorklogTools>();
        });
    }
}
```

**MCP 工具定义**：

```csharp
public class McpTaskTools
{
    private readonly ITaskAppService _taskAppService;

    [McpTool("list_tasks", "查询项目任务列表")]
    public async Task<List<TaskDto>> ListTasksAsync(
        [McpParameter("project_id", "项目ID")] Guid projectId,
        [McpParameter("status", "状态过滤（可选）")] string? status = null,
        [McpParameter("assignee_id", "负责人ID（可选）")] Guid? assigneeId = null)
    {
        var input = new TaskListInput
        {
            ProjectId = projectId,
            Status = status != null ? Enum.Parse<ProjectTaskStatus>(status) : null,
            AssigneeId = assigneeId
        };
        var result = await _taskAppService.GetListAsync(input);
        return result.Items.ToList();
    }

    [McpTool("create_task", "创建新任务")]
    public async Task<TaskDto> CreateTaskAsync(
        [McpParameter("project_id", "项目ID")] Guid projectId,
        [McpParameter("title", "任务标题")] string title,
        [McpParameter("description", "任务描述（可选）")] string? description = null,
        [McpParameter("priority", "优先级（可选）：Low/Medium/High/Urgent")] string? priority = null)
    {
        return await _taskAppService.CreateAsync(new CreateTaskInput
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            Priority = priority != null
                ? Enum.Parse<ProjectTaskPriority>(priority)
                : ProjectTaskPriority.Medium
        });
    }

    [McpTool("transition_task", "变更任务状态")]
    public async Task<TaskDto> TransitionTaskAsync(
        [McpParameter("task_id", "任务ID")] Guid taskId,
        [McpParameter("status", "目标状态：Todo/InProgress/Done/Closed/Blocked/Cancelled")] string status)
    {
        return await _taskAppService.TransitionAsync(taskId, new TransitionTaskInput
        {
            Status = Enum.Parse<ProjectTaskStatus>(status)
        });
    }
}
```

**MCP 认证中间件**：

```csharp
public class McpAuthenticationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var apiKey = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            return;
        }

        // 验证 API Key（复用 OpenApi 模块的 AppOpenApiApplication 机制）
        var app = await ValidateApiKeyAsync(apiKey);
        if (app == null)
        {
            context.Response.StatusCode = 403;
            return;
        }

        // 设置当前用户上下文
        using (CurrentTenant.Change(app.TenantId))
        {
            await next(context);
        }
    }
}
```

---

## 6. P2-04 工作流引擎

### 6.1 问题描述

当前任务状态流转是硬编码的。不同项目、不同类型的任务可能需要不同的工作流。例如：Bug 需要经过"待验证"状态，而普通任务不需要。

**竞品参考**：
- **Jira Workflow**：最成熟的工作流引擎，支持状态节点、转换条件、触发器、验证器
- **Monday.com**：可视化自动化规则
- **Linear**：较简单，但支持自动化（如 PR 合并自动关闭 Issue）

### 6.2 改进建议

| 能力 | 说明 |
|------|------|
| 工作流定义 | 可视化定义状态节点和转换规则 |
| 条件转换 | 转换时可设置条件（如必须填写完成原因） |
| 触发器 | 状态转换时触发动作（发送通知、更新字段等） |
| 多工作流 | 不同项目/任务类型使用不同工作流 |
| 工作流模板 | 预置常用工作流模板（敏捷、瀑布、Bug 修复等） |

### 6.3 方案设计

#### 工作流数据模型

```
Workflow（工作流）
  ├── name: string
  ├── description: string
  ├── applies_to: enum (Task/Project/Contract/...)
  └── nodes: WorkflowNode[]
        ├── id: Guid
        ├── name: string (状态名称)
        ├── type: enum (Start/Normal/End)
        ├── position: {x, y} (画布坐标)
        └── transitions: WorkflowTransition[]
              ├── target_node_id: Guid
              ├── name: string (转换名称)
              ├── conditions: WorkflowCondition[]
              │     ├── field: string
              │     ├── operator: enum (Equals/NotEquals/GreaterThan/...)
              │     └── value: string
              └── actions: WorkflowAction[]
                    ├── type: enum (SendNotification/UpdateField/Webhook/...)
                    └── config: json
```

#### 预置工作流模板

**敏捷任务工作流**：
```
待办 → 进行中 → 已完成 → 已关闭
         ↓
       已阻塞 → 进行中
```

**Bug 修复工作流**：
```
新建 → 确认中 → 修复中 → 待验证 → 已关闭
                  ↓           ↓
                已阻塞      重新打开 → 修复中
```

**经营域合同状态模板（远期可选）**：

> 当前四期执行边界：Project 工作流只保留项目章程/立项批准、范围/进度/成本基线变更两个关键闸门；合同/预算审批不进入 Project 审批流。如下模板仅作为 Business 经营域远期可选状态流转参考，不作为当前项目详情体验修复目标。

```
草稿 → 审批中 → 已签署 → 执行中 → 已完成
         ↓                 ↓
       已拒绝            已暂停 → 执行中
```

### 6.4 技术实现方案

#### 后端

```csharp
// Platform 模块
public class WorkflowAppService : ApplicationService
{
    Task<WorkflowDto> CreateAsync(CreateWorkflowInput input);
    Task<WorkflowDto> UpdateAsync(Guid id, UpdateWorkflowInput input);
    Task DeleteAsync(Guid id);
    Task<List<WorkflowDto>> GetListAsync();
    Task<WorkflowDto> GetAsync(Guid id);
    Task<WorkflowValidationResult> ValidateAsync(Guid id); // 校验工作流完整性
}

// 工作流执行引擎
public class WorkflowEngine
{
    public async Task<TransitionResult> ExecuteTransitionAsync(
        Guid entityId, Guid workflowId, string targetNodeName, Dictionary<string, object>? context = null)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        var currentNode = await GetCurrentNodeAsync(entityId);
        var transition = FindTransition(workflow, currentNode, targetNodeName);

        // 1. 校验条件
        foreach (var condition in transition.Conditions)
        {
            if (!EvaluateCondition(condition, context))
                throw new BusinessException("WorkflowConditionNotMet");
        }

        // 2. 执行转换
        await UpdateEntityStatusAsync(entityId, targetNodeName);

        // 3. 执行触发动作
        foreach (var action in transition.Actions)
        {
            await ExecuteActionAsync(action, entityId, context);
        }

        return TransitionResult.Success;
    }
}
```

#### 前端

**工作流设计器**（基于 Vue Flow）：

```json
{
  "@vue-flow/core": "^1.26",
  "@vue-flow/background": "^1.3",
  "@vue-flow/controls": "^1.1"
}
```

```
views/platform/workflow/
├── WorkflowDesigner.vue        # 工作流可视化设计器
├── WorkflowNodePanel.vue       # 节点属性面板
├── WorkflowTransitionPanel.vue # 转换条件配置
├── WorkflowTemplateList.vue    # 工作流模板列表
└── WorkflowTest.vue            # 工作流测试工具
```

---

## 7. P2-05 数据导入引擎

### 7.1 问题描述

用户从其他系统迁移到本系统时，缺少数据导入能力。手动录入大量历史数据不现实。

**参考场景**：
- 从 Excel 导入项目和任务
- 从 Jira/Linear 导入数据
- 从 CSV 导入合同和回款数据

### 7.2 改进建议

| 能力 | 说明 |
|------|------|
| Excel 导入 | 支持 .xlsx/.xls 格式 |
| CSV 导入 | 支持 .csv 格式 |
| 模板下载 | 提供标准导入模板 |
| 数据校验 | 导入前校验数据格式和完整性 |
| 预览确认 | 导入前预览数据，用户确认后执行 |
| 错误报告 | 标识校验失败的行，给出错误原因 |
| 增量导入 | 支持按唯一键去重，避免重复导入 |

### 7.3 技术实现方案

#### 后端

```csharp
// Platform 模块
public class DataImportAppService : ApplicationService
{
    // 获取导入模板
    Task<FileResultDto> GetTemplateAsync(string entityType);

    // 上传并预览
    Task<ImportPreviewDto> PreviewAsync(UploadImportFileInput input);

    // 确认导入
    Task<ImportResultDto> ConfirmAsync(Guid previewId);

    // 获取导入历史
    Task<List<ImportHistoryDto>> GetHistoryAsync();
}
```

**导入流程**：

```
Step 1: 用户下载模板
Step 2: 填写数据，上传文件
Step 3: 系统解析文件，校验数据
Step 4: 返回预览（成功行 + 错误行）
Step 5: 用户确认导入
Step 6: 系统批量写入（事务保护）
Step 7: 返回导入结果
```

---

## 8. P2-06 AI 任务智能分配

### 8.1 问题描述

PM 手动分配任务时，往往不了解团队成员的当前工作负载和技能匹配度。AI 可以基于历史数据推荐最优分配方案。

### 8.2 改进建议

| 能力 | 说明 |
|------|------|
| 负载均衡建议 | 推荐当前工作量最少的成员 |
| 技能匹配 | 基于历史任务完成情况推荐 |
| 一键分配 | AI 推荐后一键确认分配 |
| 负载可视化 | 展示团队成员工作负载热力图 |

### 8.3 技术实现方案

```csharp
public class AiAssignmentAppService : ApplicationService
{
    public async Task<List<AssignmentRecommendationDto>> RecommendAsync(
        Guid projectId, Guid taskId)
    {
        var task = await _taskRepo.GetAsync(taskId);
        var members = await _memberRepo.GetListAsync(projectId);

        // 1. 获取每个成员的当前负载
        var workloads = new Dictionary<Guid, int>();
        foreach (var member in members)
        {
            workloads[member.UserId] = await _taskRepo.CountAsync(t =>
                t.ProjectId == projectId &&
                t.AssigneeId == member.UserId &&
                t.Status == ProjectTaskStatus.InProgress);
        }

        // 2. 计算推荐分数（简单启发式）
        var recommendations = members.Select(m => new AssignmentRecommendationDto
        {
            UserId = m.UserId,
            UserName = m.UserName,
            CurrentTaskCount = workloads[m.UserId],
            Score = CalculateScore(workloads[m.UserId], m.Role),
            Reason = GenerateReason(workloads[m.UserId], m.Role)
        })
        .OrderByDescending(r => r.Score)
        .Take(3)
        .ToList();

        return recommendations;
    }
}
```

---

## 9. P2-07 IM 集成

### 9.1 问题描述

国内团队大量使用企业微信、钉钉、飞书等 IM 工具。系统通知需要同步推送到这些 IM 中。

### 9.2 改进建议

| 能力 | 说明 |
|------|------|
| 企业微信机器人 | 通过 Webhook 推送通知到群 |
| 钉钉机器人 | 通过 Webhook 推送通知到群 |
| 飞书机器人 | 通过 Webhook 推送通知到群 |
| Slack 集成 | 通过 Webhook 推送通知到频道 |
| 消息卡片 | 富文本消息，含快捷操作按钮 |
| 双向操作 | 在 IM 中直接回复指令更新任务（可选） |

### 9.3 技术实现方案

```csharp
// Platform 模块
public class ImNotificationService : ITransientDependency
{
    public async Task SendToWeChatWorkAsync(string webhookUrl, ImNotificationDto notification)
    {
        var payload = new
        {
            msgtype = "markdown",
            markdown = new
            {
                content = FormatMarkdown(notification)
            }
        };
        await PostToWebhookAsync(webhookUrl, payload);
    }

    public async Task SendToDingTalkAsync(string webhookUrl, ImNotificationDto notification)
    {
        var payload = new
        {
            msgtype = "markdown",
            markdown = new
            {
                title = notification.Title,
                text = FormatMarkdown(notification)
            }
        };
        await PostToWebhookAsync(webhookUrl, payload);
    }

    public async Task SendToFeiShuAsync(string webhookUrl, ImNotificationDto notification)
    {
        // 飞书消息格式
    }
}
```

---

## 10. P2-08 项目模板

### 10.1 问题描述

每个项目都要从零开始配置任务、里程碑、成员角色，效率低。项目模板可以让 PM 一键创建标准化的项目结构。

### 10.2 改进建议

| 能力 | 说明 |
|------|------|
| 模板创建 | 从现有项目保存为模板 |
| 模板管理 | 模板的增删改查 |
| 一键创建 | 从模板创建项目，自动带入任务、里程碑、成员结构 |
| 预置模板 | 敏捷开发模板、瀑布开发模板、运维项目模板等 |
| 模板共享 | 模板在租户内共享 |

### 10.3 技术实现方案

```csharp
public class ProjectTemplateAppService : ApplicationService
{
    // 从项目创建模板
    Task<ProjectTemplateDto> CreateFromProjectAsync(CreateTemplateFromProjectInput input);

    // 从模板创建项目
    Task<ProjectDto> CreateFromTemplateAsync(CreateProjectFromTemplateInput input);

    // 模板 CRUD
    Task<List<ProjectTemplateDto>> GetListAsync();
    Task DeleteAsync(Guid id);
}
```

---

## 11. P2 验收标准

| 验收项 | 标准 |
|--------|------|
| AI 自然语言查询 | 支持中英文查询，准确率 80%+，流式输出 |
| AI 报告生成 | 周报/月报可自动生成，数据准确 |
| MCP Server | Claude Code 可通过 MCP 创建/查询任务 |
| 工作流引擎 | 可视化设计器正常工作，预置模板可用 |
| 数据导入 | Excel/CSV 导入正常，数据校验有效 |
| AI 任务分配 | 推荐合理，负载均衡有依据 |
| IM 集成 | 企业微信/钉钉/飞书消息推送正常 |
| 项目模板 | 从模板创建项目正常，任务和里程碑正确带入 |

---

## 12. P2 技术依赖清单

| 依赖 | 用途 | 版本 |
|------|------|------|
| Microsoft.SemanticKernel | LLM 编排（可选） | latest |
| 或直接 HTTP | OpenAI 兼容接口调用 | - |
| @vue-flow/core | 工作流设计器 | ^1.26 |
| SSE (ASP.NET Core) | AI 流式输出 | 内置 |
| EPPlus / ClosedXML | Excel 读写 | latest |
| SignalR (.NET) | 实时通信 | 内置 |
