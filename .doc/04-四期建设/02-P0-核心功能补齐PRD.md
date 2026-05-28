# P0 - 核心功能补齐 PRD

> 版本：v1.0
> 日期：2026-05-21
> 预估周期：1-2 个月
> 目标：把项目管理系统从"只能看"变成"能用起来"

---

## 1. 修订说明

P0 聚焦于当前系统中 **已有数据结构但缺少操作功能** 的核心模块。这些功能是项目管理系统的最基本能力，不做完就无法真正使用。

---

## 2. 需求总览

| 编号 | 需求 | 归属模块 | 优先级 | 预估工时 |
|------|------|----------|--------|----------|
| P0-01 | 任务 CRUD 与状态流转 | Project | P0-Critical | 5-8 人天 |
| P0-02 | 项目编辑与归档 | Project | P0-Critical | 3-5 人天 |
| P0-03 | 里程碑 CRUD | Project | P0-High | 3-4 人天 |
| P0-04 | 项目成员管理 | Project | P0-High | 3-4 人天 |
| P0-05 | 工时录入与管理 | Project | P0-High | 3-5 人天 |
| P0-06 | 服务拆分重构 | Project | P0-High | 3-5 人天 |
| P0-07 | RAID 项 CRUD | Project | P0-Medium | 3-4 人天 |
| P0-08 | 经营实体 CRUD 补齐 | Business | P0-Medium | 5-8 人天 |

---

## 3. P0-01 任务 CRUD 与状态流转

### 3.1 问题描述

当前系统中 `ProjectTask` 实体已定义完整（含标题、描述、优先级、状态、负责人、截止日期、标签、Story Points 等字段），`ProjectService.GetTaskListAsync` 可以查询任务列表，但 **无法创建、编辑、删除任务，也无法进行状态流转**。

这是项目管理系统最核心的功能缺失。没有任务 CRUD，整个项目管理就是空壳。

**竞品参考**：
- Linear：任务创建快捷键 `C`，状态流转 `Shift+S`，键盘优先体验
- Jira：任务支持自定义工作流，状态转换有权限和条件校验
- Plane：任务支持多视图（列表、看板、甘特图）统一操作

### 3.2 改进建议

补充完整的任务生命周期管理能力：

| 能力 | 说明 |
|------|------|
| 创建任务 | 填写标题、描述、优先级、负责人、截止日期、所属里程碑、标签 |
| 编辑任务 | 修改所有字段，支持部分更新 |
| 删除任务 | 软删除，进入回收站 |
| 状态流转 | 待办 → 进行中 → 已完成 → 已关闭，支持回退 |
| 批量操作 | 批量分配、批量修改状态、批量删除 |
| 任务关联 | 父子任务、关联任务、阻塞关系 |

### 3.3 方案设计

#### 状态机定义

```
待办(Todo) ──> 进行中(InProgress) ──> 已完成(Done) ──> 已关闭(Closed)
    │                │                      │
    │                ├──> 已阻塞(Blocked) ──┘
    │                │
    └────────────────┴──> 已取消(Cancelled)
```

状态转换规则：

| 当前状态 | 可转换为 | 条件 |
|----------|----------|------|
| 待办 | 进行中、已取消 | 无 |
| 进行中 | 已完成、已阻塞、已取消 | 无 |
| 已阻塞 | 进行中、已取消 | 阻塞原因必须填写 |
| 已完成 | 已关闭、进行中 | 重新打开需填写原因 |
| 已关闭 | 进行中 | 仅 PM/Admin 可操作 |
| 已取消 | 待办 | 仅 PM/Admin 可操作 |

#### 权限设计

| 操作 | 权限 | 默认角色 |
|------|------|----------|
| 查看任务 | Project.Task.View | 所有项目成员 |
| 创建任务 | Project.Task.Create | PM、PMO、Admin |
| 编辑任务 | Project.Task.Edit | PM、任务负责人 |
| 删除任务 | Project.Task.Delete | PM、Admin |
| 状态流转 | Project.Task.Transition | 任务负责人、PM |
| 批量操作 | Project.Task.Batch | PM、Admin |

### 3.4 技术实现方案

#### 后端

**Domain 层变更**：

```csharp
// ProjectTask 实体新增方法
public class ProjectTask : FullAuditedAggregateRoot<Guid>
{
    // 现有属性保持不变...

    // 新增：状态流转方法
    public void TransitionTo(ProjectTaskStatus newStatus, string? reason = null)
    {
        ValidateTransition(newStatus);
        Status = newStatus;
        if (newStatus == ProjectTaskStatus.Done)
            CompletedTime = Clock.Now;
    }

    // 新增：分配负责人
    public void AssignTo(Guid userId)
    {
        AssigneeId = userId;
    }

    // 新增：设置父子关系
    public void SetParent(Guid? parentTaskId)
    {
        ParentTaskId = parentTaskId;
    }

    private void ValidateTransition(ProjectTaskStatus newStatus)
    {
        var allowed = GetAllowedTransitions();
        if (!allowed.Contains(newStatus))
            throw new BusinessException("InvalidTaskTransition");
    }
}
```

**Application 层变更**：

```csharp
// 新增 TaskAppService（从 ProjectService 中拆出）
public class TaskAppService : ApplicationService
{
    // 创建任务
    Task<TaskDto> CreateAsync(CreateTaskInput input);

    // 编辑任务
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskInput input);

    // 删除任务（软删除）
    Task DeleteAsync(Guid id);

    // 状态流转
    Task<TaskDto> TransitionAsync(Guid id, TransitionTaskInput input);

    // 批量操作
    Task BatchTransitionAsync(BatchTransitionInput input);
    Task BatchAssignAsync(BatchAssignInput input);
    Task BatchDeleteAsync(BatchDeleteInput input);

    // 查询
    Task<PagedResultDto<TaskDto>> GetListAsync(TaskListInput input);
    Task<TaskDto> GetAsync(Guid id);
}
```

**Controller 层变更**：

```
POST   /api/app/task                    # 创建
PUT    /api/app/task/{id}               # 编辑
DELETE /api/app/task/{id}               # 删除
PUT    /api/app/task/{id}/transition    # 状态流转
POST   /api/app/task/batch-transition   # 批量流转
POST   /api/app/task/batch-assign       # 批量分配
GET    /api/app/task                    # 列表
GET    /api/app/task/{id}               # 详情
```

**数据库迁移**：

```csharp
// 如需新增字段
migrationBuilder.AddColumn<DateTime>(
    name: "CompletedTime",
    table: "AppProjectTask",
    nullable: true);

migrationBuilder.AddColumn<Guid>(
    name: "ParentTaskId",
    table: "AppProjectTask",
    nullable: true);

migrationBuilder.CreateIndex(
    name: "IX_AppProjectTask_ParentTaskId",
    table: "AppProjectTask",
    column: "ParentTaskId");
```

#### 前端

**新增页面组件**：

```
views/project/task/
├── TaskCreateDrawer.vue       # 创建任务抽屉
├── TaskEditDrawer.vue         # 编辑任务抽屉
├── TaskDetailDrawer.vue       # 任务详情抽屉（含状态流转按钮）
├── TaskTransitionDialog.vue   # 状态流转确认对话框
└── components/
    ├── TaskPriorityTag.vue    # 优先级标签
    ├── TaskStatusTag.vue      # 状态标签
    └── TaskAssigneeSelect.vue # 负责人选择器
```

**API 变更** (`api/core/task.ts`)：

```typescript
export const taskApi = {
  create: (data: CreateTaskInput) => request.post('/api/app/task', data),
  update: (id: string, data: UpdateTaskInput) => request.put(`/api/app/task/${id}`, data),
  delete: (id: string) => request.delete(`/api/app/task/${id}`),
  transition: (id: string, data: TransitionTaskInput) => request.put(`/api/app/task/${id}/transition`, data),
  getList: (params: TaskListInput) => request.get('/api/app/task', { params }),
  getDetail: (id: string) => request.get(`/api/app/task/${id}`),
};
```

---

## 4. P0-02 项目编辑与归档

### 4.1 问题描述

当前 `ProjectService` 支持创建项目和查看详情，但 **无法编辑项目信息（名称、描述、时间范围、负责人等），也无法归档或关闭项目**。

**竞品参考**：
- OpenProject：项目设置页可编辑所有元数据，支持项目归档和删除
- Linear：项目有完整生命周期（Planned → In Progress → Completed → Cancelled）
- Plane：项目支持 Modules 和 Cycles 的灵活组织

### 4.2 改进建议

| 能力 | 说明 |
|------|------|
| 编辑项目 | 修改名称、描述、时间范围、负责人、优先级、标签 |
| 归档项目 | 项目完成后归档，从活跃列表移除，保留数据可查 |
| 关闭项目 | 项目取消时关闭，需填写原因 |
| 删除项目 | 软删除，进入回收站，30天后自动清理 |
| 项目设置 | 配置项目成员权限、通知规则、自定义字段 |

### 4.3 方案设计

#### 项目状态机

```
规划中(Planning) ──> 进行中(Active) ──> 已完成(Completed) ──> 已归档(Archived)
    │                    │
    │                    ├──> 已暂停(Paused) ──> 进行中(Active)
    │                    │
    └────────────────────┴──> 已关闭(Closed)
```

### 4.4 技术实现方案

#### 后端

```csharp
// ProjectService 新增方法
public class ProjectService : ApplicationService
{
    // 现有方法保持不变...

    // 编辑项目
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectInput input);

    // 归档项目
    Task ArchiveAsync(Guid id);

    // 关闭项目
    Task CloseAsync(Guid id, string reason);

    // 删除项目（软删除）
    Task DeleteAsync(Guid id);

    // 从归档恢复
    Task RestoreAsync(Guid id);
}
```

**Controller**：

```
PUT    /api/app/project/{id}             # 编辑
PUT    /api/app/project/{id}/archive     # 归档
PUT    /api/app/project/{id}/close       # 关闭
DELETE /api/app/project/{id}             # 删除
PUT    /api/app/project/{id}/restore     # 恢复
```

**前端**：

```
views/project/projects/
├── ProjectEditDrawer.vue      # 编辑项目抽屉
├── ProjectSettings.vue        # 项目设置页
└── ProjectArchiveDialog.vue   # 归档确认对话框
```

---

## 5. P0-03 里程碑 CRUD

### 5.1 问题描述

`ProjectMilestone` 实体已定义（名称、描述、截止日期、状态等），但 **无法创建、编辑、删除里程碑**。里程碑是项目阶段管理的核心标记，没有它就无法进行阶段性管控。

**竞品参考**：
- OpenProject：里程碑与甘特图深度集成，可在时间线上直接拖拽调整
- Jira：Version/Milestone 支持 Release 管理
- Plane：Milestones 支持进度百分比自动计算

### 5.2 改进建议

| 能力 | 说明 |
|------|------|
| 创建里程碑 | 名称、描述、截止日期、关联任务 |
| 编辑里程碑 | 修改所有属性 |
| 完成里程碑 | 标记完成，自动检查关联任务是否全部完成 |
| 删除里程碑 | 软删除，关联任务解除关联 |
| 里程碑进度 | 自动计算关联任务的完成百分比 |

### 5.3 技术实现方案

#### 后端

```csharp
// 新增 MilestoneAppService
public class MilestoneAppService : ApplicationService
{
    Task<MilestoneDto> CreateAsync(CreateMilestoneInput input);
    Task<MilestoneDto> UpdateAsync(Guid id, UpdateMilestoneInput input);
    Task DeleteAsync(Guid id);
    Task CompleteAsync(Guid id);
    Task<PagedResultDto<MilestoneDto>> GetListAsync(MilestoneListInput input);
    Task<MilestoneDto> GetAsync(Guid id);
}
```

**里程碑进度自动计算**：

```csharp
public async Task<MilestoneProgressDto> GetProgressAsync(Guid milestoneId)
{
    var tasks = await _taskRepository.GetListByMilestoneAsync(milestoneId);
    return new MilestoneProgressDto
    {
        TotalTasks = tasks.Count,
        CompletedTasks = tasks.Count(t => t.Status == ProjectTaskStatus.Done),
        ProgressPercent = tasks.Any()
            ? (double)tasks.Count(t => t.Status == ProjectTaskStatus.Done) / tasks.Count * 100
            : 0,
        IsOverdue = tasks.Any(t => t.Deadline < Clock.Now && t.Status != ProjectTaskStatus.Done)
    };
}
```

**Controller**：

```
POST   /api/app/milestone
PUT    /api/app/milestone/{id}
DELETE /api/app/milestone/{id}
PUT    /api/app/milestone/{id}/complete
GET    /api/app/milestone
GET    /api/app/milestone/{id}
GET    /api/app/milestone/{id}/progress
```

---

## 6. P0-04 项目成员管理

### 6.1 问题描述

`ProjectMember` 实体已定义（项目ID、用户ID、角色等），但 **无法动态添加或移除项目成员**。项目成员管理是权限控制和任务分配的前提。

**竞品参考**：
- Linear：项目成员可按角色（Lead、Member）管理，支持 @提及
- Jira：项目角色（Administrator、Developer、Viewer）细粒度控制
- Plane：项目成员支持邀请链接和批量导入

### 6.2 改进建议

| 能力 | 说明 |
|------|------|
| 添加成员 | 从用户列表中选择，指定项目角色 |
| 移除成员 | 移除后该用户不再参与项目 |
| 修改角色 | 变更成员在项目中的角色 |
| 成员列表 | 查看项目成员列表，含角色和加入时间 |
| 批量添加 | 批量添加多个成员到项目 |

### 6.3 技术实现方案

#### 后端

```csharp
// 新增 MemberAppService
public class MemberAppService : ApplicationService
{
    Task AddMemberAsync(Guid projectId, AddMemberInput input);
    Task RemoveMemberAsync(Guid projectId, Guid userId);
    Task UpdateMemberRoleAsync(Guid projectId, Guid userId, UpdateMemberRoleInput input);
    Task<List<MemberDto>> GetMembersAsync(Guid projectId);
    Task BatchAddMembersAsync(Guid projectId, BatchAddMemberInput input);
}
```

**业务规则**：
- 项目创建者自动成为项目负责人（PM）
- 项目至少保留一名负责人
- 移除负责人时必须先指定新负责人
- 成员变更时发布领域事件 `ProjectMemberChangedEvent`

**Controller**：

```
GET    /api/app/project/{projectId}/member
POST   /api/app/project/{projectId}/member
DELETE /api/app/project/{projectId}/member/{userId}
PUT    /api/app/project/{projectId}/member/{userId}/role
POST   /api/app/project/{projectId}/member/batch
```

---

## 7. P0-05 工时录入与管理

### 7.1 问题描述

`ProjectWorklog` 实体已定义（项目ID、任务ID、用户ID、日期、工时数、描述等），当前支持工时统计查看，但 **无法录入工时**。工时数据是经营分析和资源管理的基础。

**竞品参考**：
- OpenProject：工时追踪最完善，支持日历视图录入、批量录入、工时审批
- Jira：原生时间追踪，支持剩余时间估算
- Toggl Track：专业时间追踪工具，支持计时器模式

### 7.2 改进建议

| 能力 | 说明 |
|------|------|
| 录入工时 | 选择任务、日期、工时数、描述 |
| 编辑工时 | 修改已录入的工时记录 |
| 删除工时 | 删除错误的工时记录 |
| 工时日历 | 日历视图展示每日工时分布 |
| 工时统计 | 按项目/任务/人员/时间维度统计 |
| 工时审批 | PM 审批团队成员的工时（可选） |

### 7.3 技术实现方案

#### 后端

```csharp
// 新增 WorklogAppService
public class WorklogAppService : ApplicationService
{
    Task<WorklogDto> CreateAsync(CreateWorklogInput input);
    Task<WorklogDto> UpdateAsync(Guid id, UpdateWorklogInput input);
    Task DeleteAsync(Guid id);
    Task<PagedResultDto<WorklogDto>> GetListAsync(WorklogListInput input);
    Task<List<WorklogDailySummaryDto>> GetDailySummaryAsync(WorklogDailySummaryInput input);
    Task<WorklogProjectSummaryDto> GetProjectSummaryAsync(Guid projectId, DateTime from, DateTime to);
}
```

**输入校验**：

```csharp
public class CreateWorklogInput
{
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }       // 可选，关联到具体任务
    public DateTime Date { get; set; }       // 工时日期
    public decimal Hours { get; set; }       // 工时数（精度 0.5h）
    public string? Description { get; set; } // 工作描述

    // 校验规则
    // - Hours 范围 0.5 ~ 24
    // - Date 不能是未来日期（可配置）
    // - 同一用户同一天总工时不能超过 24h
}
```

**Controller**：

```
POST   /api/app/worklog
PUT    /api/app/worklog/{id}
DELETE /api/app/worklog/{id}
GET    /api/app/worklog
GET    /api/app/worklog/daily-summary
GET    /api/app/worklog/project-summary/{projectId}
```

**前端**：

```
views/project/worklog/
├── WorklogEntry.vue           # 工时录入表单
├── WorklogCalendar.vue        # 日历视图录入
├── WorklogList.vue            # 工时列表
└── WorklogSummary.vue         # 工时统计
```

---

## 8. P0-06 服务拆分重构

### 8.1 问题描述

当前 `ProjectService` 一个文件承担了项目、任务、里程碑、RAID、成员、工时等全部业务逻辑，`BusinessService` 同理。这导致：

- 文件过大，难以维护
- 职责不清，违反单一职责原则
- 测试困难，无法独立测试各子模块
- 并行开发冲突频繁

### 8.2 改进建议

将 `ProjectService` 拆分为 5 个独立的 AppService：

| 拆分后服务 | 职责 | 从 ProjectService 中提取的方法 |
|------------|------|-------------------------------|
| ProjectAppService | 项目生命周期管理 | GetWorkspaceAsync, GetDetailAsync, GetProjectListAsync, CreateAsync, GetMyRelatedAsync |
| TaskAppService | 任务管理 | GetTaskListAsync（扩展为完整 CRUD） |
| MilestoneAppService | 里程碑管理 | 新增 |
| MemberAppService | 成员管理 | 新增 |
| WorklogAppService | 工时管理 | 新增 |
| RaidAppService | RAID 管理 | GetRaidListAsync（扩展为完整 CRUD） |

将 `BusinessService` 拆分为 3 个独立的 AppService：

| 拆分后服务 | 职责 |
|------------|------|
| ContractAppService | 合同管理 |
| ProcurementAppService | 采购管理 |
| BusinessReportAppService | 经营报表 |

### 8.3 技术实现方案

#### 重构步骤

```
Step 1: 创建新的 AppService 文件（空壳）
Step 2: 逐个方法从 ProjectService 迁移到对应的新 Service
Step 3: 更新 Controller 的依赖注入
Step 4: 更新前端 API 调用路径（如有变化）
Step 5: 运行测试确保无回归
Step 6: 删除 ProjectService 中已迁移的方法
```

#### 注意事项

- API 路径保持不变（`/api/app/project/*`、`/api/app/task/*` 等），避免前端大面积改动
- 保持向后兼容，不改变现有 DTO 结构
- 每个新 Service 独立注入依赖，不相互引用

---

## 9. P0-07 RAID 项 CRUD

### 9.1 问题描述

`ProjectRaidItem` 实体已定义（类型：Risk/Assumption/Issue/Dependency，标题、描述、影响、概率、责任人、状态等），当前仅支持列表查看，无法创建和编辑 RAID 项。

### 9.2 改进建议

| 能力 | 说明 |
|------|------|
| 创建 RAID 项 | 选择类型（R/A/I/D），填写标题、描述、影响、概率、责任人 |
| 编辑 RAID 项 | 修改所有属性 |
| 状态管理 | Open → In Progress → Mitigated → Closed |
| 风险矩阵 | 按影响×概率展示风险矩阵 |
| 关联任务 | RAID 项可关联到具体任务 |

### 9.3 技术实现方案

```csharp
public class RaidAppService : ApplicationService
{
    Task<RaidDto> CreateAsync(CreateRaidInput input);
    Task<RaidDto> UpdateAsync(Guid id, UpdateRaidInput input);
    Task DeleteAsync(Guid id);
    Task<PagedResultDto<RaidDto>> GetListAsync(RaidListInput input);
    Task<RaidDto> GetAsync(Guid id);
    Task<List<RaidMatrixDto>> GetRiskMatrixAsync(Guid projectId);
}
```

---

## 10. P0-08 经营实体 CRUD 补齐

### 10.1 问题描述

Business 模块的 6 个实体（合同、采购、应收、预算执行、业务链、预测历史）均已定义数据结构，BusinessService 提供了总览和报表查询，但 **各实体的创建、编辑、删除操作缺失**。

### 10.2 改进建议

为每个经营实体补齐标准 CRUD：

| 实体 | 操作 | 特殊逻辑 |
|------|------|----------|
| BusinessContract | 创建/编辑/删除/状态流转 | 合同状态：草稿→审批中→已签署→执行中→已完成→已关闭 |
| BusinessProcurement | 创建/编辑/删除/状态流转 | 关联合同，采购金额不能超过合同金额 |
| BusinessReceivable | 创建/编辑/删除/标记已收 | 回款关联合同，自动计算未回款金额 |
| BusinessBudgetExecution | 创建/编辑/删除 | 预算执行关联预算项，超额预警 |

### 10.3 技术实现方案

每个实体对应一个独立的 AppService：

```csharp
public class ContractAppService : ApplicationService
{
    Task<ContractDto> CreateAsync(CreateContractInput input);
    Task<ContractDto> UpdateAsync(Guid id, UpdateContractInput input);
    Task DeleteAsync(Guid id);
    Task<ContractDto> TransitionStatusAsync(Guid id, TransitionContractStatusInput input);
    Task<PagedResultDto<ContractDto>> GetListAsync(ContractListInput input);
    Task<ContractDto> GetAsync(Guid id);
}
```

---

## 11. P0 阶段数据库迁移清单

| 迁移 | 内容 | 影响表 |
|------|------|--------|
| AddTaskParentAndCompletedTime | 新增 ParentTaskId、CompletedTime | AppProjectTask |
| AddMilestoneProgress | 新增 ProgressPercent（计算字段或视图） | AppProjectMilestone |
| AddWorklogConstraints | 新增工时校验约束 | AppProjectWorklog |
| AddProjectStatusAndArchive | 新增项目状态枚举和归档时间 | AppProject |
| AddRaidRelations | 新增 RAID 与任务关联表 | AppProjectRaidItem |
| AddContractStatus | 新增合同状态字段 | AppBusinessContract |

---

## 12. P0 验收标准

| 验收项 | 标准 |
|--------|------|
| 任务 CRUD | 可创建、编辑、删除任务，状态流转正常 |
| 项目编辑 | 可修改项目所有属性，归档/关闭/恢复正常 |
| 里程碑 | 可创建、编辑、删除里程碑，进度自动计算 |
| 成员管理 | 可添加/移除成员，角色变更生效 |
| 工时录入 | 可录入/编辑/删除工时，日历视图和统计正常 |
| 服务拆分 | ProjectService 拆分为 5 个服务，原有功能无回归 |
| RAID | 可创建/编辑 RAID 项，风险矩阵展示正常 |
| 经营 CRUD | 合同/采购/应收/预算可创建编辑删除 |
| 权限控制 | 所有操作受权限控制，角色越权操作被拒绝 |
| 测试覆盖 | 新增服务的核心方法有单元测试 |
