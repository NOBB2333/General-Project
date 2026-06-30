# P1 - 竞争力提升 PRD

> 版本：v1.0
> 日期：2026-05-21
> 预估周期：2-4 个月（P0 完成后启动）
> 目标：从"能用"提升到"好用"，具备与主流 PM 工具竞争的基础能力

---

## 0. 路线修订说明（2026-06）

本文是四期早期 P1 PRD，保留作为体验、实时通知、文档能力和领域事件设计参考。

当前正式路线中，飞书、钉钉、企业微信、Webhook、IM 通知等外部系统集成能力不再作为 Platform 的散装能力推进，而是进入六期独立 `外部系统集成区`。本文涉及外部集成的章节仅作为需求来源参考，不再作为模块归属依据。

---

## 1. 修订说明

P1 聚焦于 **用户体验提升和系统集成能力**。在 P0 完成核心 CRUD 之后，P1 让系统具备实时协作、可视化操作、外部集成等现代 PM 工具的标配能力。

---

## 2. 需求总览

| 编号 | 需求 | 归属模块 | 优先级 | 预估工时 |
|------|------|----------|--------|----------|
| P1-01 | 看板视图（Kanban Board） | Project | P1-Critical | 8-12 人天 |
| P1-02 | SignalR 实时通知 | Platform | P1-Critical | 5-8 人天 |
| P1-03 | 项目文档管理 | Project | P1-High | 5-8 人天 |
| P1-04 | Webhook 引擎 | Platform | P1-High | 5-8 人天 |
| P1-05 | Git 集成 | Platform | P1-High | 5-8 人天 |
| P1-06 | 领域事件引入 | 全模块 | P1-High | 3-5 人天 |
| P1-07 | 单元测试补全 | 全模块 | P1-Medium | 5-8 人天 |
| P1-08 | CI/CD 流水线 | 基础设施 | P1-Medium | 2-3 人天 |
| P1-09 | 甘特图交互增强 | Project | P1-Medium | 5-8 人天 |
| P1-10 | 通知中心增强 | Platform | P1-Medium | 3-5 人天 |

---

## 3. P1-01 看板视图（Kanban Board）

### 3.1 问题描述

当前任务只有列表视图，缺少看板视图。看板是敏捷项目管理最核心的可视化工具，几乎所有主流 PM 工具（Trello、Linear、Plane、Jira、ClickUp）都以看板作为默认视图。

没有看板，用户无法直观地看到任务在各状态下的分布，也无法通过拖拽快速流转任务。

**竞品参考**：
- **Trello**：看板的鼻祖，卡片信息密度高，支持自定义列
- **Linear**：看板支持按状态/优先级/负责人分组，键盘快捷操作
- **Plane**：看板支持 Swimlane（泳道），多视图切换
- **Jira**：看板支持自定义工作流列，WIP 限制

### 3.2 改进建议

| 能力 | 说明 |
|------|------|
| 看板展示 | 按任务状态分列（待办/进行中/已完成/已关闭） |
| 拖拽流转 | 卡片在列间拖拽，自动更新任务状态 |
| 卡片信息 | 展示标题、优先级、负责人、截止日期、标签 |
| 分组切换 | 支持按状态/优先级/负责人/里程碑分组 |
| 筛选器 | 按负责人、优先级、标签、里程碑筛选 |
| Swimlane | 按负责人或优先级分泳道展示 |
| WIP 限制 | 每列设置最大任务数，超限预警 |
| 快捷操作 | 点击卡片查看详情，右键菜单快速操作 |

### 3.3 方案设计

#### 视图架构

```
TaskViewModule
├── KanbanBoard.vue              # 看板主组件
├── KanbanColumn.vue             # 看板列（对应一个状态）
├── KanbanCard.vue               # 任务卡片
├── KanbanFilter.vue             # 筛选栏
├── KanbanGroupBy.vue            # 分组切换
└── TaskListView.vue             # 列表视图（已有）
```

#### 交互流程

```
用户进入项目详情
  -> 默认展示看板视图（可切换为列表/甘特图）
  -> 看板按状态分为 4 列
  -> 拖拽卡片到目标列
  -> 前端乐观更新 UI
  -> 调用后端 transition API
  -> 后端校验状态转换规则
  -> 成功：确认更新
  -> 失败：回滚 UI，提示错误
```

### 3.4 技术实现方案

#### 前端

**核心依赖**：

```json
{
  "vuedraggable": "^4.1.0",
  "sortablejs": "^1.15.0"
}
```

**KanbanBoard.vue 核心逻辑**：

```vue
<template>
  <div class="kanban-board">
    <KanbanFilter v-model="filter" @change="refreshTasks" />
    <div class="kanban-columns">
      <KanbanColumn
        v-for="column in columns"
        :key="column.status"
        :title="column.title"
        :tasks="column.tasks"
        :wip-limit="column.wipLimit"
        @task-moved="handleTaskMoved"
        @task-click="openTaskDetail"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
// 看板数据通过 TaskAppService.GetListAsync 获取
// 拖拽通过 vuedraggable 实现
// 状态流转通过 TaskAppService.TransitionAsync 调用
// 乐观更新：先更新本地状态，失败时回滚
</script>
```

**KanbanCard.vue 卡片信息**：

```vue
<template>
  <div class="kanban-card" @click="$emit('click', task)">
    <div class="card-header">
      <TaskPriorityTag :priority="task.priority" />
      <span class="task-code">{{ task.code }}</span>
    </div>
    <div class="card-title">{{ task.title }}</div>
    <div class="card-footer">
      <TaskAssigneeAvatar :user="task.assignee" />
      <span v-if="task.deadline" class="deadline" :class="{ overdue: isOverdue }">
        {{ formatDate(task.deadline) }}
      </span>
      <div class="tags">
        <Tag v-for="tag in task.tags" :key="tag" size="small">{{ tag }}</Tag>
      </div>
    </div>
  </div>
</template>
```

#### 后端

后端无需额外接口，复用 P0 的 TaskAppService：

- `GET /api/app/task?projectId=xxx` — 获取任务列表（含状态字段）
- `PUT /api/app/task/{id}/transition` — 状态流转（拖拽时调用）

**新增查询参数支持**：

```csharp
public class TaskListInput : PagedAndSortedResultRequestDto
{
    public Guid? ProjectId { get; set; }
    public string? Keyword { get; set; }
    public ProjectTaskStatus? Status { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? MilestoneId { get; set; }
    public ProjectTaskPriority? Priority { get; set; }
    public List<string>? Tags { get; set; }
}
```

---

## 4. P1-02 SignalR 实时通知

### 4.1 问题描述

当前通知系统仅支持站内通知（数据库存储 + 前端轮询），缺少实时推送能力。用户必须手动刷新页面才能看到新通知。现代 PM 工具（Slack、飞书、Linear）都已实现毫秒级实时推送。

**竞品参考**：
- **Slack**：实时消息推送，通知分级，桌面通知
- **飞书**：实时通知 + 快捷操作按钮
- **Linear**：任务变更实时同步到所有打开的页面

### 4.2 改进建议

| 能力 | 说明 |
|------|------|
| 实时推送 | 任务变更、评论、@提及等事件实时推送到前端 |
| 通知分级 | 按紧急程度分级（紧急/普通/低优先级） |
| 已读/未读 | 实时更新未读计数 |
| 快捷操作 | 通知中包含快捷按钮（查看任务、标记已读） |
| 桌面通知 | 浏览器 Notification API 推送桌面通知 |
| 在线状态 | 显示团队成员在线/离线状态 |
| 多端同步 | 多个浏览器标签页同步通知状态 |

### 4.3 方案设计

#### 架构

```
后端事件源                  SignalR Hub              前端客户端
─────────────              ────────────              ──────────
TaskCreated     ──┐
TaskUpdated     ──┤
TaskTransitioned──┼──> NotificationHub ──> Vue 3 SignalR Client
CommentAdded    ──┤        │                      │
MemberChanged   ──┘        │                      ├── 更新通知列表
                           │                      ├── 更新未读计数
                           └── 按用户/项目分组      ├── 桌面通知
                               推送到对应客户端      └── 实时刷新页面数据
```

#### 通知事件类型

| 事件 | 触发时机 | 推送对象 |
|------|----------|----------|
| TaskCreated | 任务被创建 | 项目成员 |
| TaskUpdated | 任务信息被修改 | 任务关注者、负责人 |
| TaskTransitioned | 任务状态变更 | 任务关注者、负责人 |
| TaskAssigned | 任务被分配 | 被分配人 |
| CommentAdded | 任务有新评论 | 任务关注者 |
| MilestoneCompleted | 里程碑完成 | 项目成员 |
| ProjectArchived | 项目归档 | 项目成员 |
| MemberAdded | 新成员加入项目 | 项目成员 |
| WorklogSubmitted | 工时提交 | PM |

### 4.4 技术实现方案

#### 后端

**ABP SignalR 集成**：

```bash
# 安装 ABP SignalR 包
dotnet add package Volo.Abp.AspNetCore.SignalR
```

**SignalR Hub**：

```csharp
[Authorize]
public class NotificationHub : AbpHub
{
    private readonly IOnlineClientManager _onlineClientManager;

    // 加入项目组（接收项目级通知）
    public async Task JoinProjectGroup(Guid projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }

    // 离开项目组
    public async Task LeaveProjectGroup(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }
}
```

**通知分发服务**：

```csharp
public class RealtimeNotificationService : ITransientDependency
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IRepository<AppNotification> _notificationRepository;

    // 发送通知给指定用户
    public async Task SendToUserAsync(Guid userId, NotificationDto notification)
    {
        // 1. 持久化到数据库
        await SaveNotificationAsync(userId, notification);

        // 2. 实时推送
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    // 发送给项目组
    public async Task SendToProjectAsync(Guid projectId, NotificationDto notification)
    {
        var members = await GetProjectMembersAsync(projectId);
        foreach (var member in members)
        {
            await SaveNotificationAsync(member.UserId, notification);
        }

        await _hubContext.Clients
            .Group($"project-{projectId}")
            .SendAsync("ReceiveNotification", notification);
    }
}
```

**与领域事件集成**：

```csharp
// 任务状态变更时发送实时通知
public class TaskTransitionedEventHandler :
    IDistributedEventHandler<TaskTransitionedEto>
{
    private readonly RealtimeNotificationService _notificationService;

    public async Task HandleEventAsync(TaskTransitionedEto eventData)
    {
        await _notificationService.SendToProjectAsync(
            eventData.ProjectId,
            new NotificationDto
            {
                Type = "TaskTransitioned",
                Title = $"任务状态变更",
                Content = $"任务 {eventData.TaskTitle} 状态变更为 {eventData.NewStatus}",
                Data = new { TaskId = eventData.TaskId }
            });
    }
}
```

#### 前端

**SignalR 客户端**：

```typescript
// composables/useSignalR.ts
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export function useSignalR() {
  const connection = new HubConnectionBuilder()
    .withUrl('/hubs/notification', {
      accessTokenFactory: () => getToken()
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  // 监听通知
  connection.on('ReceiveNotification', (notification) => {
    // 更新通知列表
    notificationStore.addNotification(notification)
    // 更新未读计数
    notificationStore.incrementUnread()
    // 桌面通知
    if (Notification.permission === 'granted') {
      new Notification(notification.title, { body: notification.content })
    }
  })

  // 加入项目组
  async function joinProject(projectId: string) {
    await connection.invoke('JoinProjectGroup', projectId)
  }

  return { connection, joinProject }
}
```

**通知中心组件增强**：

```vue
<template>
  <div class="notification-center">
    <Badge :count="unreadCount">
      <BellIcon @click="togglePanel" />
    </Badge>
    <NotificationPanel
      v-if="showPanel"
      :notifications="notifications"
      @read="markAsRead"
      @read-all="markAllAsRead"
    />
  </div>
</template>
```

---

## 5. P1-03 项目文档管理

### 5.1 问题描述

`ProjectDocument` 实体已定义，但缺少文档上传、预览、版本管理等操作。项目管理中的文档（需求文档、设计文档、会议纪要等）是重要的知识资产。

**竞品参考**：
- **Notion**：块编辑器，支持富文本、表格、代码块、嵌入
- **Plane Pages**：内置文档编辑，与任务关联
- **OpenProject**：文档管理 + Wiki + 文件附件

### 5.2 改进建议

| 能力 | 说明 |
|------|------|
| 文件上传 | 支持拖拽上传、多文件上传 |
| 文件预览 | PDF、图片、Office 文档在线预览 |
| 版本管理 | 文件版本历史，可回退到旧版本 |
| 文件夹组织 | 按文件夹组织项目文档 |
| 任务关联 | 文档可关联到具体任务 |
| 权限控制 | 项目成员才能访问项目文档 |
| 全文搜索 | 搜索文档标题和内容 |

### 5.3 技术实现方案

#### 后端

```csharp
public class DocumentAppService : ApplicationService
{
    // 上传文档
    Task<DocumentDto> UploadAsync(UploadDocumentInput input);

    // 编辑文档信息
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentInput input);

    // 删除文档
    Task DeleteAsync(Guid id);

    // 获取文档列表
    Task<PagedResultDto<DocumentDto>> GetListAsync(DocumentListInput input);

    // 下载文档
    Task<FileResultDto> DownloadAsync(Guid id);

    // 上传新版本
    Task<DocumentDto> UploadNewVersionAsync(Guid id, UploadVersionInput input);

    // 获取版本历史
    Task<List<DocumentVersionDto>> GetVersionsAsync(Guid id);
}
```

**存储策略**：

```csharp
// 复用已有的 AppFileStorageSource 机制
// 支持本地存储、MinIO、阿里云 OSS
public class DocumentStorageService
{
    private readonly IFileStorageSourceService _storageSource;

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        var storageSource = await _storageSource.GetActiveSourceAsync();
        return storageSource.Provider switch
        {
            "Local" => await SaveToLocalAsync(fileStream, fileName),
            "MinIO" => await SaveToMinIOAsync(fileStream, fileName),
            "OSS" => await SaveToOSSAsync(fileStream, fileName),
            _ => throw new BusinessException("UnsupportedStorageProvider")
        };
    }
}
```

#### 前端

```
views/project/document/
├── DocumentList.vue           # 文档列表（支持列表/网格视图）
├── DocumentUpload.vue         # 上传组件（拖拽 + 点击）
├── DocumentPreview.vue        # 在线预览
├── DocumentVersionHistory.vue # 版本历史
└── components/
    ├── FileIcon.vue           # 文件类型图标
    └── FileSize.vue           # 文件大小格式化
```

---

## 6. P1-04 Webhook 引擎

### 6.1 问题描述

系统无法向外部推送事件通知。现代 PM 工具通过 Webhook 与 Git、CI/CD、IM 等系统双向集成，实现自动化工作流。例如：任务状态变更时自动发消息到企业微信群。

**竞品参考**：
- **GitHub Webhook**：签名验证、重试机制、事件订阅、Payload 规范
- **GitLab Webhook**：支持自定义 Header、SSL 验证
- **Jira Webhook**：事件粒度细，支持 JQL 过滤

### 6.2 改进建议

| 能力 | 说明 |
|------|------|
| Webhook 注册 | 用户配置目标 URL、订阅事件类型、密钥 |
| 事件推送 | 事件触发时向注册的 URL 发送 HTTP POST |
| 签名验证 | HMAC-SHA256 签名，防止伪造 |
| 重试机制 | 失败后指数退避重试（最多 5 次） |
| 事件日志 | 记录每次推送的请求/响应，便于排查 |
| 事件过滤 | 支持按项目、事件类型过滤 |

### 6.3 技术实现方案

#### 后端

**Webhook 注册**：

```csharp
public class WebhookAppService : ApplicationService
{
    Task<WebhookDto> CreateAsync(CreateWebhookInput input);
    Task<WebhookDto> UpdateAsync(Guid id, UpdateWebhookInput input);
    Task DeleteAsync(Guid id);
    Task<List<WebhookDto>> GetListAsync();
    Task<List<WebhookDeliveryDto>> GetDeliveriesAsync(Guid webhookId, DeliveryListInput input);
    Task TestWebhookAsync(Guid webhookId); // 发送测试事件
}
```

**Webhook 投递服务**：

```csharp
public class WebhookDeliveryService : ITransientDependency
{
    private readonly IRepository<WebhookRegistration> _webhookRepo;
    private readonly IRepository<WebhookDeliveryLog> _deliveryLogRepo;
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task DeliverAsync(string eventType, object payload)
    {
        var webhooks = await _webhookRepo.GetListAsync(w =>
            w.EventTypes.Contains(eventType) && w.IsActive);

        foreach (var webhook in webhooks)
        {
            _ = DeliverToWebhookAsync(webhook, eventType, payload);
            // 异步投递，不阻塞主流程
        }
    }

    private async Task DeliverToWebhookAsync(
        WebhookRegistration webhook, string eventType, object payload)
    {
        var json = JsonSerializer.Serialize(new
        {
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Data = payload
        });

        var signature = ComputeHmacSha256(json, webhook.Secret);
        var request = new HttpRequestMessage(HttpMethod.Post, webhook.Url);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Headers.Add("X-Webhook-Signature", signature);
        request.Headers.Add("X-Webhook-Event", eventType);

        // 重试逻辑（指数退避）
        for (int attempt = 0; attempt <= 5; attempt++)
        {
            try
            {
                var response = await _httpClientFactory.CreateClient()
                    .SendAsync(request);
                await LogDeliveryAsync(webhook.Id, eventType, response, attempt);
                if (response.IsSuccessStatusCode) return;
            }
            catch (Exception ex)
            {
                await LogDeliveryAsync(webhook.Id, eventType, ex, attempt);
            }

            if (attempt < 5)
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }
}
```

**支持的事件类型**：

```csharp
public static class WebhookEventTypes
{
    // 项目事件
    public const string ProjectCreated = "project.created";
    public const string ProjectUpdated = "project.updated";
    public const string ProjectArchived = "project.archived";

    // 任务事件
    public const string TaskCreated = "task.created";
    public const string TaskUpdated = "task.updated";
    public const string TaskTransitioned = "task.transitioned";
    public const string TaskAssigned = "task.assigned";
    public const string TaskDeleted = "task.deleted";

    // 里程碑事件
    public const string MilestoneCompleted = "milestone.completed";

    // 成员事件
    public const string MemberAdded = "member.added";
    public const string MemberRemoved = "member.removed";
}
```

---

## 7. P1-05 Git 集成

### 7.1 问题描述

系统无法与 Git 仓库（GitHub/GitLab/Gitea）关联。开发者的工作流通常是：在 PM 系统中看到任务 → 在 IDE 中编码 → 提交代码到 Git → 手动回到 PM 系统更新任务状态。这个手动更新环节是最大的效率损失。

**竞品参考**：
- **Linear + GitHub**：PR 关联 Issue，合并时自动关闭 Issue
- **Plane + GitHub**：Commit message 中引用 Issue ID 自动关联
- **Jira + Bitbucket**：深度集成，分支/PR/Build 状态直接显示在 Issue 上

### 7.2 改进建议

| 能力 | 说明 |
|------|------|
| 仓库关联 | 项目关联 GitHub/GitLab 仓库 |
| Commit 关联 | Commit message 中的 task ID 自动关联 |
| PR 关联 | PR 关联到任务，显示 PR 状态 |
| 自动状态流转 | PR 合并时自动将任务标记为已完成 |
| 分支创建 | 从任务直接创建 Git 分支（可选） |
| 提交日志 | 任务详情中展示关联的 Git 提交 |

### 7.3 技术实现方案

#### 架构

```
GitHub/GitLab --Webhook--> GitWebhookController
                              │
                              ├── 解析 Commit/PR 事件
                              ├── 提取任务 ID（正则匹配）
                              ├── 更新任务关联
                              └── 触发自动状态流转（如配置）
```

#### 后端

**Git 集成服务**：

```csharp
public class GitIntegrationAppService : ApplicationService
{
    // 关联仓库
    Task<LinkRepositoryAsync(Guid projectId, LinkRepositoryInput input);

    // 获取关联的仓库
    Task<List<RepositoryDto>> GetLinkedRepositoriesAsync(Guid projectId);

    // 解除关联
    Task UnlinkRepositoryAsync(Guid projectId, Guid repositoryId);
}
```

**Git Webhook 接收器**：

```csharp
[ApiController]
[Route("api/webhook/github")]
public class GitHubWebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync()
    {
        // 1. 验证签名
        var signature = Request.Headers["X-Hub-Signature-256"];
        if (!VerifySignature(Request.Body, signature))
            return Unauthorized();

        // 2. 解析事件类型
        var eventType = Request.Headers["X-GitHub-Event"];
        var payload = await DeserializePayloadAsync();

        // 3. 处理不同事件
        return eventType switch
        {
            "push" => await HandlePushAsync(payload),
            "pull_request" => await HandlePullRequestAsync(payload),
            _ => Ok()
        };
    }

    private async Task<IActionResult> HandlePushAsync(PushPayload payload)
    {
        foreach (var commit in payload.Commits)
        {
            // 从 commit message 中提取任务 ID
            // 格式：feat: PROJ-123 实现了xxx功能
            var taskIds = ExtractTaskIds(commit.Message);
            foreach (var taskId in taskIds)
            {
                await _gitService.LinkCommitAsync(taskId, commit);
            }
        }
        return Ok();
    }
}
```

**任务 ID 提取规则**：

```csharp
// 支持多种格式：
// - "PROJ-123" (标准格式)
// - "#123" (简写)
// - "task:123" (显式标记)
private static readonly Regex TaskIdPattern = new(
    @"(?:PROJ-(\d+)|#(\d+)|task:(\d+))",
    RegexOptions.IgnoreCase);
```

---

## 8. P1-06 领域事件引入

### 8.1 问题描述

当前系统中各模块之间缺少事件驱动机制。例如：任务状态变更不会自动触发通知、报表更新、审计记录等。各模块直接调用，耦合度高。

**参考**：ABP Framework 原生支持 `ILocalEventBus` 和 `IDistributedEventBus`，引入成本低。

### 8.2 改进建议

为所有核心业务操作引入领域事件：

| 业务操作 | 领域事件 | 订阅者 |
|----------|----------|--------|
| 任务创建 | TaskCreatedEto | 通知、报表、审计 |
| 任务状态变更 | TaskTransitionedEto | 通知、Webhook、Git、AI |
| 任务分配 | TaskAssignedEto | 通知（被分配人） |
| 项目创建 | ProjectCreatedEto | 通知、审计 |
| 项目归档 | ProjectArchivedEto | 通知、Webhook |
| 成员变更 | ProjectMemberChangedEto | 通知（项目组） |
| 里程碑完成 | MilestoneCompletedEto | 通知、Webhook |
| 合同签署 | ContractSignedEto | 经营报表更新 |

### 8.3 技术实现方案

#### 事件定义

```csharp
// Domain.Shared 层定义事件 DTO
public class TaskTransitionedEto
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectTaskStatus OldStatus { get; set; }
    public ProjectTaskStatus NewStatus { get; set; }
    public Guid OperatorId { get; set; }
}
```

#### 事件发布

```csharp
// TaskAppService 中发布事件
public async Task<TaskDto> TransitionAsync(Guid id, TransitionTaskInput input)
{
    var task = await _taskRepository.GetAsync(id);
    var oldStatus = task.Status;
    task.TransitionTo(input.Status, input.Reason);
    await _taskRepository.UpdateAsync(task);

    // 发布领域事件
    await _eventBus.PublishAsync(new TaskTransitionedEto
    {
        TaskId = task.Id,
        TaskTitle = task.Title,
        ProjectId = task.ProjectId,
        OldStatus = oldStatus,
        NewStatus = task.Status,
        OperatorId = CurrentUser.Id.Value
    });

    return ObjectMapper.Map<ProjectTask, TaskDto>(task);
}
```

#### 事件处理

```csharp
// 通知模块：监听任务状态变更，发送通知
public class TaskNotificationHandler :
    IDistributedEventHandler<TaskTransitionedEto>
{
    public async Task HandleEventAsync(TaskTransitionedEto eventData)
    {
        await _notificationService.SendToProjectAsync(
            eventData.ProjectId,
            new NotificationDto
            {
                Type = "TaskTransitioned",
                Title = "任务状态变更",
                Content = $"任务「{eventData.TaskTitle}」状态从 {eventData.OldStatus} 变更为 {eventData.NewStatus}"
            });
    }
}

// Webhook 模块：监听事件，推送到外部
public class TaskWebhookHandler :
    IDistributedEventHandler<TaskTransitionedEto>
{
    public async Task HandleEventAsync(TaskTransitionedEto eventData)
    {
        await _webhookService.DeliverAsync(
            WebhookEventTypes.TaskTransitioned,
            eventData);
    }
}
```

---

## 9. P1-07 单元测试补全

### 9.1 问题描述

当前测试覆盖薄弱，Project/Business 模块几乎无测试。P0 补齐 CRUD 后，必须同步补全测试，防止回归。

### 9.2 目标

| 模块 | 目标覆盖率 | 重点测试 |
|------|-----------|----------|
| TaskAppService | 80% | 创建、状态流转、权限校验、异常分支 |
| ProjectAppService | 80% | 编辑、归档、状态转换 |
| MilestoneAppService | 70% | 创建、完成、进度计算 |
| MemberAppService | 70% | 添加、移除、角色变更、边界条件 |
| WorklogAppService | 70% | 录入、校验、统计 |

### 9.3 技术实现方案

```csharp
// 使用 ABP 测试基座
public class TaskAppServiceTests : General.AdminApplicationTestBase
{
    private readonly ITaskAppService _taskAppService;

    [Fact]
    public async Task Should_Create_Task()
    {
        var task = await _taskAppService.CreateAsync(new CreateTaskInput
        {
            ProjectId = _testData.ProjectId,
            Title = "测试任务",
            Priority = ProjectTaskPriority.Medium
        });

        task.ShouldNotBeNull();
        task.Title.ShouldBe("测试任务");
        task.Status.ShouldBe(ProjectTaskStatus.Todo);
    }

    [Fact]
    public async Task Should_Throw_When_Invalid_Transition()
    {
        var task = await CreateTestTaskAsync();

        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _taskAppService.TransitionAsync(task.Id,
                new TransitionTaskInput { Status = ProjectTaskStatus.Closed });
        });
    }

    [Fact]
    public async Task Should_Not_Allow_Delete_When_Has_SubTasks()
    {
        // 测试有子任务时不能删除父任务
    }
}
```

---

## 10. P1-08 CI/CD 流水线

### 10.1 问题描述

当前无自动化构建和部署流水线，全靠手动执行脚本。容易出现"本地能跑、线上挂了"的问题。

### 10.2 方案设计

使用 GitHub Actions 实现：

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore General.Admin

      - name: Build
        run: dotnet build General.Admin --no-restore

      - name: Test
        run: dotnet test General.Admin --no-build --verbosity normal

      - name: Frontend Lint
        working-directory: General.Web
        run: |
          pnpm install
          pnpm lint

      - name: Frontend Build
        working-directory: General.Web
        run: pnpm build:general
```

---

## 11. P1-09 甘特图交互增强

### 11.1 问题描述

当前甘特图已实现基础展示，但缺少交互能力。参考 OpenProject 的甘特图，应该支持拖拽调整时间、依赖线绘制、里程碑标记等。

### 11.2 改进建议

| 能力 | 说明 |
|------|------|
| 拖拽调整 | 拖拽任务条调整开始/结束时间 |
| 依赖线 | 任务间依赖关系可视化（FS/FF/SS/SF） |
| 里程碑标记 | 菱形标记在时间线上 |
| 缩放 | 日/周/月视图切换 |
| 基线对比 | 计划 vs 实际进度对比 |
| 关键路径 | 自动标识关键路径 |

### 11.3 技术实现方案

**前端依赖**：

```json
{
  "dhtmlx-gantt": "^8.0"
  // 或
  "gantt-task-react": "^0.3"
}
```

> 选型建议：dhtmlx-gantt 功能最全但体积大；gantt-task-react 轻量但功能较少。根据实际需求选择。

---

## 12. P1-10 通知中心增强

### 12.1 问题描述

当前通知系统支持基础的站内通知，但缺少通知偏好设置、免打扰、通知聚合等高级功能。

### 12.2 改进建议

| 能力 | 说明 |
|------|------|
| 通知偏好 | 用户配置接收哪些类型的通知 |
| 免打扰 | 设置免打扰时间段 |
| 通知聚合 | 同类通知合并（"5 个任务已更新"） |
| 邮件通知 | 重要通知同步发送邮件 |
| @提及通知 | 评论中 @某人时发送通知 |
| 关注机制 | 关注任务/项目，接收相关变更通知 |

---

## 13. P1 验收标准

| 验收项 | 标准 |
|--------|------|
| 看板视图 | 拖拽流畅，状态流转正确，筛选分组正常 |
| 实时通知 | 任务变更后 1 秒内推送，多端同步 |
| 文档管理 | 上传/下载/预览正常，版本历史可查 |
| Webhook | 注册/投递/重试/签名验证正常，事件日志可查 |
| Git 集成 | Commit/PR 自动关联任务，PR 合并自动流转 |
| 领域事件 | 核心操作触发事件，通知/Webhook 正确订阅 |
| 测试覆盖 | 核心服务 80%+ 覆盖率 |
| CI/CD | PR 提交自动运行构建和测试 |
