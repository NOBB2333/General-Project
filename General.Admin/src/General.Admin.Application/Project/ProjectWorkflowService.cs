using General.Admin.Platform;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Project;

public class ProjectWorkflowService : ITransientDependency
{
    private const string ProjectChangeBusinessType = "ProjectChangeRequest";
    private const string ProjectCharterBusinessType = "ProjectCharter";

    private readonly PlatformWorkflowService _platformWorkflowService;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectChangeRequest, Guid> _projectChangeRequestRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<Project, Guid> _projectRepository;

    public ProjectWorkflowService(
        PlatformWorkflowService platformWorkflowService,
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectChangeRequest, Guid> projectChangeRequestRepository,
        IRepository<ProjectCharter, Guid> projectCharterRepository,
        IRepository<Project, Guid> projectRepository)
    {
        _platformWorkflowService = platformWorkflowService;
        _projectAccessGuard = projectAccessGuard;
        _projectChangeRequestRepository = projectChangeRequestRepository;
        _projectCharterRepository = projectCharterRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Guid> SubmitCharterApprovalAsync(Guid projectId, ProjectCharterApprovalActionInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var project = await _projectRepository.GetAsync(projectId);
        var charter = await GetCharterAsync(projectId);
        if (charter.ApprovalStatus == "已批准")
        {
            throw new BusinessException("项目章程已批准，不能重复提交。");
        }

        var activeInstance = await _platformWorkflowService.FindLatestAsync(
            PlatformWorkflowDefinitionKeys.ProjectCharterApproval,
            ProjectCharterBusinessType,
            projectId.ToString());
        if (activeInstance?.Status == PlatformWorkflowStatuses.Pending)
        {
            throw new BusinessException("项目章程已在审批中，不能重复提交。");
        }

        var instance = await _platformWorkflowService.StartAsync(new PlatformWorkflowStartInput
        {
            BusinessCode = project.ProjectCode,
            BusinessId = projectId.ToString(),
            BusinessSnapshot = BuildCharterSnapshot(charter),
            BusinessTitle = project.Name,
            BusinessType = ProjectCharterBusinessType,
            WorkflowDefinitionKey = PlatformWorkflowDefinitionKeys.ProjectCharterApproval,
            WorkflowDefinitionVersion = 1
        });
        charter.UpdateApprovalStatus("待审批");
        await _projectCharterRepository.UpdateAsync(charter, autoSave: true);
        return instance.Id;
    }

    public async Task<Guid> SubmitChangeApprovalAsync(Guid changeRequestId, ProjectWorkflowActionInput input)
    {
        var changeRequest = await _projectChangeRequestRepository.GetAsync(changeRequestId);
        await _projectAccessGuard.EnsureProjectAccessAsync(changeRequest.ProjectId);
        if (changeRequest.IsApproved())
        {
            throw new BusinessException("基线变更已批准，不能重复提交审批。");
        }

        var activeInstance = await _platformWorkflowService.FindLatestAsync(
            PlatformWorkflowDefinitionKeys.ProjectChangeApproval,
            ProjectChangeBusinessType,
            changeRequestId.ToString());
        if (activeInstance?.Status == PlatformWorkflowStatuses.Pending)
        {
            throw new BusinessException("基线变更已在审批中，不能重复提交。");
        }

        var instance = await _platformWorkflowService.StartAsync(new PlatformWorkflowStartInput
        {
            BusinessCode = changeRequest.RequestCode,
            BusinessId = changeRequestId.ToString(),
            BusinessSnapshot = BuildChangeSnapshot(changeRequest),
            BusinessTitle = changeRequest.Title,
            BusinessType = ProjectChangeBusinessType,
            WorkflowDefinitionKey = PlatformWorkflowDefinitionKeys.ProjectChangeApproval,
            WorkflowDefinitionVersion = 1
        });
        changeRequest.Submit();
        await _projectChangeRequestRepository.UpdateAsync(changeRequest, autoSave: true);
        return instance.Id;
    }

    public async Task ApproveChangeAsync(Guid changeRequestId, ProjectWorkflowActionInput input)
    {
        await CompleteChangeAsync(changeRequestId, input, approve: true);
    }

    public async Task RejectChangeAsync(Guid changeRequestId, ProjectWorkflowActionInput input)
    {
        await CompleteChangeAsync(changeRequestId, input, approve: false);
    }

    public async Task WithdrawChangeApprovalAsync(Guid changeRequestId, ProjectWorkflowActionInput input)
    {
        var changeRequest = await _projectChangeRequestRepository.GetAsync(changeRequestId);
        await _projectAccessGuard.EnsureProjectAccessAsync(changeRequest.ProjectId);
        var instance = await GetActiveChangeWorkflowAsync(changeRequestId);
        await _platformWorkflowService.WithdrawAsync(instance.Id, new PlatformApprovalActionInput
        {
            Comment = input.Comment
        });
        changeRequest.Withdraw();
        await _projectChangeRequestRepository.UpdateAsync(changeRequest, autoSave: true);
    }

    public async Task ApproveCharterAsync(Guid projectId, ProjectCharterApprovalActionInput input)
    {
        await CompleteCharterAsync(projectId, input, approve: true);
    }

    public async Task RejectCharterAsync(Guid projectId, ProjectCharterApprovalActionInput input)
    {
        await CompleteCharterAsync(projectId, input, approve: false);
    }

    public async Task WithdrawCharterApprovalAsync(Guid projectId, ProjectCharterApprovalActionInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var charter = await GetCharterAsync(projectId);
        var instance = await GetActiveCharterWorkflowAsync(projectId);
        await _platformWorkflowService.WithdrawAsync(instance.Id, new PlatformApprovalActionInput
        {
            Comment = input.Comment
        });
        charter.UpdateApprovalStatus("草稿");
        await _projectCharterRepository.UpdateAsync(charter, autoSave: true);
    }

    private async Task CompleteCharterAsync(Guid projectId, ProjectCharterApprovalActionInput input, bool approve)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var charter = await GetCharterAsync(projectId);
        var instance = await GetActiveCharterWorkflowAsync(projectId);
        if (approve)
        {
            await _platformWorkflowService.ApproveAsync(instance.Id, new PlatformApprovalActionInput
            {
                Comment = input.Comment
            });
            charter.UpdateApprovalStatus("已批准");
        }
        else
        {
            await _platformWorkflowService.RejectAsync(instance.Id, new PlatformApprovalActionInput
            {
                Comment = input.Comment
            });
            charter.UpdateApprovalStatus("退回");
        }

        await _projectCharterRepository.UpdateAsync(charter, autoSave: true);
    }

    private async Task<ProjectCharter> GetCharterAsync(Guid projectId)
    {
        var charter = (await _projectCharterRepository.GetListAsync(item => item.ProjectId == projectId)).FirstOrDefault();
        if (charter is null)
        {
            throw new BusinessException("请先保存项目章程摘要后再提交审批。");
        }

        return charter;
    }

    private async Task<PlatformWorkflowInstance> GetActiveCharterWorkflowAsync(Guid projectId)
    {
        var instance = await _platformWorkflowService.FindLatestAsync(
            PlatformWorkflowDefinitionKeys.ProjectCharterApproval,
            ProjectCharterBusinessType,
            projectId.ToString());
        if (instance is null || instance.Status != PlatformWorkflowStatuses.Pending)
        {
            throw new BusinessException("当前项目章程没有审批中的流程。");
        }

        return instance;
    }

    private async Task<PlatformWorkflowInstance> GetActiveChangeWorkflowAsync(Guid changeRequestId)
    {
        var instance = await _platformWorkflowService.FindLatestAsync(
            PlatformWorkflowDefinitionKeys.ProjectChangeApproval,
            ProjectChangeBusinessType,
            changeRequestId.ToString());
        if (instance is null || instance.Status != PlatformWorkflowStatuses.Pending)
        {
            throw new BusinessException("当前基线变更没有审批中的流程。");
        }

        return instance;
    }

    private async Task CompleteChangeAsync(Guid changeRequestId, ProjectWorkflowActionInput input, bool approve)
    {
        var changeRequest = await _projectChangeRequestRepository.GetAsync(changeRequestId);
        await _projectAccessGuard.EnsureProjectAccessAsync(changeRequest.ProjectId);
        var instance = await GetActiveChangeWorkflowAsync(changeRequestId);
        if (approve)
        {
            await _platformWorkflowService.ApproveAsync(instance.Id, new PlatformApprovalActionInput
            {
                Comment = input.Comment
            });
            changeRequest.Approve();
        }
        else
        {
            await _platformWorkflowService.RejectAsync(instance.Id, new PlatformApprovalActionInput
            {
                Comment = input.Comment
            });
            changeRequest.Reject();
        }

        await _projectChangeRequestRepository.UpdateAsync(changeRequest, autoSave: true);
    }

    private static string BuildCharterSnapshot(ProjectCharter charter)
    {
        return string.Join(
            Environment.NewLine,
            $"目标：{charter.Objectives}",
            $"范围：{charter.ScopeBoundary}",
            $"成功标准：{charter.SuccessCriteria}");
    }

    private static string BuildChangeSnapshot(ProjectChangeRequest changeRequest)
    {
        return string.Join(
            Environment.NewLine,
            $"变更编号：{changeRequest.RequestCode}",
            $"变更类型：{changeRequest.ChangeType}",
            $"变更原因：{changeRequest.Reason}",
            $"影响范围：{changeRequest.ImpactScope}",
            $"预期结果：{changeRequest.ExpectedResult}");
    }
}
