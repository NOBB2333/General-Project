using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Platform;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Project;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class ProjectWorkflowServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<PlatformApprovalRecord, Guid> _approvalRecordRepository;
    private readonly ProjectGovernanceService _governanceService;
    private readonly ProjectLifecycleService _lifecycleService;
    private readonly IRepository<ProjectBaseline, Guid> _projectBaselineRepository;
    private readonly IRepository<ProjectChangeRequest, Guid> _projectChangeRequestRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<PlatformWorkflowInstance, Guid> _workflowInstanceRepository;
    private readonly ProjectWorkflowService _workflowService;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectWorkflowServiceTests()
    {
        _approvalRecordRepository = GetRequiredService<IRepository<PlatformApprovalRecord, Guid>>();
        _governanceService = GetRequiredService<ProjectGovernanceService>();
        _lifecycleService = GetRequiredService<ProjectLifecycleService>();
        _projectBaselineRepository = GetRequiredService<IRepository<ProjectBaseline, Guid>>();
        _projectChangeRequestRepository = GetRequiredService<IRepository<ProjectChangeRequest, Guid>>();
        _projectCharterRepository = GetRequiredService<IRepository<ProjectCharter, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _workflowInstanceRepository = GetRequiredService<IRepository<PlatformWorkflowInstance, Guid>>();
        _workflowService = GetRequiredService<ProjectWorkflowService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task SaveCharterAsync_Should_Not_Directly_Approve_Charter()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("章程状态保存测试项目");

            await _governanceService.SaveCharterAsync(
                projectId,
                CreateCharterInput("已批准"));

            var charter = await GetCharterAsync(projectId);
            charter.ApprovalStatus.ShouldBe("草稿");
        });
    }

    [Fact]
    public async Task SaveCharterAsync_Should_Allow_File_First_With_Empty_Summary()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("章程文件优先测试项目");

            await _governanceService.SaveCharterAsync(
                projectId,
                new ProjectCharterSaveInput
                {
                    ApprovalStatus = "草稿",
                    Assumptions = "   ",
                    BusinessCase = "",
                    Constraints = "   ",
                    Objectives = "",
                    ScopeBoundary = "",
                    SuccessCriteria = ""
                });

            var charter = await GetCharterAsync(projectId);
            charter.BusinessCase.ShouldBeEmpty();
            charter.Objectives.ShouldBeEmpty();
            charter.ScopeBoundary.ShouldBeEmpty();
            charter.Assumptions.ShouldBeEmpty();
            charter.Constraints.ShouldBeEmpty();
            charter.SuccessCriteria.ShouldBeEmpty();
            charter.ApprovalStatus.ShouldBe("草稿");
        });
    }

    [Fact]
    public async Task CharterApproval_Should_Submit_And_Approve_With_Records()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectWithCharterAsync("章程审批通过测试项目");

            var instanceId = await _workflowService.SubmitCharterApprovalAsync(
                projectId,
                new ProjectCharterApprovalActionInput
                {
                    Comment = "章程摘要和文件已准备好。"
                });

            var submittedCharter = await GetCharterAsync(projectId);
            submittedCharter.ApprovalStatus.ShouldBe("待审批");

            var instance = await _workflowInstanceRepository.GetAsync(instanceId);
            instance.WorkflowDefinitionKey.ShouldBe(PlatformWorkflowDefinitionKeys.ProjectCharterApproval);
            instance.BusinessType.ShouldBe("ProjectCharter");
            instance.BusinessId.ShouldBe(projectId.ToString());
            instance.Status.ShouldBe(PlatformWorkflowStatuses.Pending);
            instance.ApplicantUserId.ShouldBe(CurrentUserId);
            instance.ElsaWorkflowInstanceId.ShouldNotBeNullOrWhiteSpace();

            await _workflowService.ApproveCharterAsync(
                projectId,
                new ProjectCharterApprovalActionInput
                {
                    Comment = "同意进入执行。"
                });

            var approvedCharter = await GetCharterAsync(projectId);
            approvedCharter.ApprovalStatus.ShouldBe("已批准");

            var approvedInstance = await _workflowInstanceRepository.GetAsync(instanceId);
            approvedInstance.Status.ShouldBe(PlatformWorkflowStatuses.Approved);
            approvedInstance.CompletedTime.ShouldNotBeNull();

            var records = (await _approvalRecordRepository.GetListAsync(item => item.ApprovalInstanceId == instanceId))
                .OrderBy(item => item.CreationTime)
                .ToList();
            records.Count.ShouldBe(2);
            records[0].Action.ShouldBe(PlatformWorkflowActions.Submit);
            records[0].ToStatus.ShouldBe(PlatformWorkflowStatuses.Pending);
            records[1].Action.ShouldBe(PlatformWorkflowActions.Approve);
            records[1].FromStatus.ShouldBe(PlatformWorkflowStatuses.Pending);
            records[1].ToStatus.ShouldBe(PlatformWorkflowStatuses.Approved);
            records[1].ActorUserId.ShouldBe(CurrentUserId);
        });
    }

    [Fact]
    public async Task SaveCharterAsync_Should_Keep_Pending_Status_During_Approval()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectWithCharterAsync("审批中保存摘要测试项目");

            await _workflowService.SubmitCharterApprovalAsync(projectId, new ProjectCharterApprovalActionInput());
            await _governanceService.SaveCharterAsync(projectId, CreateCharterInput());

            var charter = await GetCharterAsync(projectId);
            charter.ApprovalStatus.ShouldBe("待审批");
        });
    }

    [Fact]
    public async Task CharterApproval_Should_Reject_And_Block_Project_InProgress()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectWithCharterAsync("章程审批驳回测试项目");
            var instanceId = await _workflowService.SubmitCharterApprovalAsync(projectId, new ProjectCharterApprovalActionInput());

            await _workflowService.RejectCharterAsync(
                projectId,
                new ProjectCharterApprovalActionInput
                {
                    Comment = "范围边界需要补充。"
                });

            var rejectedCharter = await GetCharterAsync(projectId);
            rejectedCharter.ApprovalStatus.ShouldBe("退回");
            var rejectedInstance = await _workflowInstanceRepository.GetAsync(instanceId);
            rejectedInstance.Status.ShouldBe(PlatformWorkflowStatuses.Rejected);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _lifecycleService.UpdateProjectStatusAsync(
                    projectId,
                    new ProjectStatusUpdateInput
                    {
                        Status = ProjectStatuses.InProgress
                    }));
            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("必须先完成并通过项目章程审批");
        });
    }

    [Fact]
    public async Task CharterApproval_Should_Withdraw_To_Draft()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectWithCharterAsync("章程审批撤回测试项目");
            var instanceId = await _workflowService.SubmitCharterApprovalAsync(projectId, new ProjectCharterApprovalActionInput());

            await _workflowService.WithdrawCharterApprovalAsync(
                projectId,
                new ProjectCharterApprovalActionInput
                {
                    Comment = "补充章程文件后重新提交。"
                });

            var charter = await GetCharterAsync(projectId);
            charter.ApprovalStatus.ShouldBe("草稿");
            var instance = await _workflowInstanceRepository.GetAsync(instanceId);
            instance.Status.ShouldBe(PlatformWorkflowStatuses.Withdrawn);

            var records = await _approvalRecordRepository.GetListAsync(item => item.ApprovalInstanceId == instanceId);
            records.Any(item => item.Action == PlatformWorkflowActions.Withdraw).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task UpdateProjectStatusAsync_Should_Allow_InProgress_After_Approved_Charter()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectWithCharterAsync("章程批准后进入执行测试项目");
            await _workflowService.SubmitCharterApprovalAsync(projectId, new ProjectCharterApprovalActionInput());
            await _workflowService.ApproveCharterAsync(projectId, new ProjectCharterApprovalActionInput());

            await _lifecycleService.UpdateProjectStatusAsync(
                projectId,
                new ProjectStatusUpdateInput
                {
                    Status = ProjectStatuses.InProgress
                });

            var project = await _projectRepository.GetAsync(projectId);
            project.Status.ShouldBe(ProjectStatuses.InProgress);
        });
    }

    [Fact]
    public async Task SaveCharterAsync_Should_Reject_Updating_Approved_Charter_Summary()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectWithCharterAsync("章程批准后摘要锁定测试项目");
            await _workflowService.SubmitCharterApprovalAsync(projectId, new ProjectCharterApprovalActionInput());
            await _workflowService.ApproveCharterAsync(projectId, new ProjectCharterApprovalActionInput());

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _governanceService.SaveCharterAsync(
                    projectId,
                    new ProjectCharterSaveInput
                    {
                        ApprovalStatus = "草稿",
                        Assumptions = "修改后的假设。",
                        BusinessCase = "修改后的商业论证。",
                        Constraints = "修改后的制约条件。",
                        Objectives = "修改后的目标。",
                        ScopeBoundary = "修改后的范围边界。",
                        SuccessCriteria = "修改后的成功标准。"
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("已批准的项目章程摘要不能直接修改");
            var charter = await GetCharterAsync(projectId);
            charter.ApprovalStatus.ShouldBe("已批准");
            charter.Objectives.ShouldBe("完成章程审批、记录和状态联动。");
        });
    }

    [Fact]
    public async Task UpdateProjectStatusAsync_Should_Block_InProgress_When_Only_Charter_Field_Is_Approved()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("章程字段绕过测试项目");
            await _projectCharterRepository.InsertAsync(
                new ProjectCharter(
                    Guid.NewGuid(),
                    projectId,
                    "业务论证已线下通过。",
                    "验证不能只靠字段绕过审批。",
                    "范围覆盖章程审批门禁。",
                    string.Empty,
                    string.Empty,
                    "必须有真实审批实例。",
                    "已批准"),
                autoSave: true);

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _lifecycleService.UpdateProjectStatusAsync(
                    projectId,
                    new ProjectStatusUpdateInput
                    {
                        Status = ProjectStatuses.InProgress
                    }));
            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("必须先完成并通过项目章程审批");
        });
    }

    [Fact]
    public async Task BaselineChangeApproval_Should_Submit_Approve_And_Allow_New_Baseline()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("基线变更审批通过测试项目");
            await CreateBaselineAsync(projectId);
            var changeRequestId = await CreateChangeRequestAsync(projectId, "范围");

            var instanceId = await _workflowService.SubmitChangeApprovalAsync(
                changeRequestId,
                new ProjectWorkflowActionInput
                {
                    Comment = "提交范围基线变更。"
                });

            var submittedChange = await _projectChangeRequestRepository.GetAsync(changeRequestId);
            submittedChange.Status.ShouldBe("审批中");

            await _workflowService.ApproveChangeAsync(
                changeRequestId,
                new ProjectWorkflowActionInput
                {
                    Comment = "同意调整范围基线。"
                });

            var approvedChange = await _projectChangeRequestRepository.GetAsync(changeRequestId);
            approvedChange.Status.ShouldBe("已批准");

            var instance = await _workflowInstanceRepository.GetAsync(instanceId);
            instance.WorkflowDefinitionKey.ShouldBe(PlatformWorkflowDefinitionKeys.ProjectChangeApproval);
            instance.BusinessType.ShouldBe("ProjectChangeRequest");
            instance.BusinessId.ShouldBe(changeRequestId.ToString());
            instance.Status.ShouldBe(PlatformWorkflowStatuses.Approved);

            var records = (await _approvalRecordRepository.GetListAsync(item => item.ApprovalInstanceId == instanceId))
                .OrderBy(item => item.CreationTime)
                .ToList();
            records.Select(item => item.Action).ShouldBe([PlatformWorkflowActions.Submit, PlatformWorkflowActions.Approve]);

            var newBaselineId = await CreateBaselineAsync(projectId, changeRequestId);
            var newBaseline = await _projectBaselineRepository.GetAsync(newBaselineId);
            newBaseline.VersionNo.ShouldBe(2);
            newBaseline.CreatedFromChangeRequestId.ShouldBe(changeRequestId);
        });
    }

    [Fact]
    public async Task BaselineChangeApproval_Should_Reject_And_Withdraw_To_Expected_Status()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var rejectProjectId = await CreateProjectAsync("基线变更驳回测试项目");
            var rejectedChangeId = await CreateChangeRequestAsync(rejectProjectId, "进度");
            var rejectedInstanceId = await _workflowService.SubmitChangeApprovalAsync(rejectedChangeId, new ProjectWorkflowActionInput());

            await _workflowService.RejectChangeAsync(
                rejectedChangeId,
                new ProjectWorkflowActionInput
                {
                    Comment = "进度影响未说明清楚。"
                });

            var rejectedChange = await _projectChangeRequestRepository.GetAsync(rejectedChangeId);
            rejectedChange.Status.ShouldBe("已驳回");
            var rejectedInstance = await _workflowInstanceRepository.GetAsync(rejectedInstanceId);
            rejectedInstance.Status.ShouldBe(PlatformWorkflowStatuses.Rejected);

            var withdrawProjectId = await CreateProjectAsync("基线变更撤回测试项目");
            var withdrawnChangeId = await CreateChangeRequestAsync(withdrawProjectId, "成本");
            var withdrawnInstanceId = await _workflowService.SubmitChangeApprovalAsync(withdrawnChangeId, new ProjectWorkflowActionInput());

            await _workflowService.WithdrawChangeApprovalAsync(
                withdrawnChangeId,
                new ProjectWorkflowActionInput
                {
                    Comment = "重新补充成本影响后再提交。"
                });

            var withdrawnChange = await _projectChangeRequestRepository.GetAsync(withdrawnChangeId);
            withdrawnChange.Status.ShouldBe("待评审");
            var withdrawnInstance = await _workflowInstanceRepository.GetAsync(withdrawnInstanceId);
            withdrawnInstance.Status.ShouldBe(PlatformWorkflowStatuses.Withdrawn);
        });
    }

    [Fact]
    public async Task CreateChangeRequestAsync_Should_Only_Allow_Scope_Schedule_And_Cost()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("基线变更类型约束测试项目");

            foreach (var changeType in new[] { "合同影响", "预算影响", "质量", "需求调整" })
            {
                var exception = await Should.ThrowAsync<BusinessException>(() =>
                    _governanceService.CreateChangeRequestAsync(projectId, CreateChangeRequestInput(changeType)));
                exception.Code.ShouldNotBeNull();
                exception.Code.ShouldContain("只用于范围、进度、成本基线变更");
            }

            var allowedChangeRequestId = await CreateChangeRequestAsync(projectId, "成本");
            var allowedChangeRequest = await _projectChangeRequestRepository.GetAsync(allowedChangeRequestId);
            allowedChangeRequest.ChangeType.ShouldBe("成本");
        });
    }

    [Fact]
    public async Task CreateBaselineAsync_Should_Require_Approved_Change_For_Next_Version()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("新版基线审批约束测试项目");
            await CreateBaselineAsync(projectId);
            var changeRequestId = await CreateChangeRequestAsync(projectId, "进度");

            var missingChangeException = await Should.ThrowAsync<BusinessException>(() => CreateBaselineAsync(projectId));
            missingChangeException.Code.ShouldNotBeNull();
            missingChangeException.Code.ShouldContain("后续基线版本必须关联已批准");

            var unapprovedChangeException = await Should.ThrowAsync<BusinessException>(() => CreateBaselineAsync(projectId, changeRequestId));
            unapprovedChangeException.Code.ShouldNotBeNull();
            unapprovedChangeException.Code.ShouldContain("必须关联当前项目已批准");
        });
    }

    private async Task<Guid> CreateProjectWithCharterAsync(string name)
    {
        var projectId = await CreateProjectAsync(name);
        await _governanceService.SaveCharterAsync(projectId, CreateCharterInput());
        return projectId;
    }

    private async Task<Guid> CreateProjectAsync(string name)
    {
        await EnsureCurrentUserAsync();
        var projectId = Guid.NewGuid();
        await _projectRepository.InsertAsync(
            new ProjectEntity(
                projectId,
                NewCode("PRJ"),
                name,
                null,
                "软件项目",
                "测试客户",
                Guid.NewGuid(),
                CurrentUserId,
                CurrentUserId,
                ProjectPriorities.P1,
                "待启动",
                DateTime.Today,
                DateTime.Today.AddDays(30),
                false,
                "用于验证项目章程审批。"),
            autoSave: true);
        return projectId;
    }

    private async Task<Guid> CreateBaselineAsync(Guid projectId, Guid? changeRequestId = null)
    {
        return await _governanceService.CreateBaselineAsync(
            projectId,
            new ProjectBaselineSaveInput
            {
                BudgetAtCompletion = changeRequestId.HasValue ? 150_000 : 100_000,
                CostBaselineSummary = changeRequestId.HasValue ? "调整后的成本基线。" : "初始成本基线。",
                CreatedFromChangeRequestId = changeRequestId,
                EffectiveDate = DateTime.Today,
                IsActive = true,
                ScheduleBaselineSummary = changeRequestId.HasValue ? "调整后的进度基线。" : "初始进度基线。",
                ScopeBaselineSummary = changeRequestId.HasValue ? "调整后的范围基线。" : "初始范围基线。"
            });
    }

    private async Task<Guid> CreateChangeRequestAsync(Guid projectId, string changeType)
    {
        return await _governanceService.CreateChangeRequestAsync(projectId, CreateChangeRequestInput(changeType));
    }

    private static ProjectChangeRequestSaveInput CreateChangeRequestInput(string changeType)
    {
        return new ProjectChangeRequestSaveInput
        {
            ApplicantUserId = CurrentUserId,
            ChangeType = changeType,
            ExpectedResult = "基线调整后按新计划执行。",
            ImpactScope = "影响当前项目基线，不影响合同和预算审批。",
            Impacts =
            [
                new ProjectChangeImpactSaveInput
                {
                    Analysis = "范围、进度或成本基线需要受控调整。",
                    EstimatedCostDelta = changeType == "成本" ? 50_000 : null,
                    EstimatedScheduleDeltaDays = changeType == "进度" ? 5 : null,
                    ImpactType = changeType is "范围" or "进度" or "成本" ? changeType : "范围",
                    Severity = ProjectPriorities.P1
                }
            ],
            Reason = "已批准基线需要调整。",
            Title = $"{changeType}基线变更"
        };
    }

    private static ProjectCharterSaveInput CreateCharterInput(string approvalStatus = "草稿")
    {
        return new ProjectCharterSaveInput
        {
            ApprovalStatus = approvalStatus,
            Assumptions = "客户按时提供验收环境。",
            BusinessCase = "通过项目管理平台提升项目执行透明度。",
            Constraints = "一期范围控制在项目章程审批闭环。",
            Objectives = "完成章程审批、记录和状态联动。",
            ScopeBoundary = "仅覆盖项目章程，不扩展到任意流程设计器。",
            SuccessCriteria = "章程审批通过后才允许项目进入执行中。"
        };
    }

    private async Task<ProjectCharter> GetCharterAsync(Guid projectId)
    {
        var charter = (await _projectCharterRepository.GetListAsync(item => item.ProjectId == projectId)).Single();
        return charter;
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "project.workflow.tester", "project.workflow.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }
}
