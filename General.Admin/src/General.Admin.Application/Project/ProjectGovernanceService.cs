using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using General.Admin.Business;

namespace General.Admin.Project;

public class ProjectGovernanceService : ITransientDependency
{
    private static readonly HashSet<string> BaselineChangeTypes = ["范围", "进度", "成本"];

    private readonly ICurrentUser _currentUser;
    private readonly ProjectPerformanceService _projectPerformanceService;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectBaseline, Guid> _projectBaselineRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _businessBudgetExecutionRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<ProjectChangeImpact, Guid> _projectChangeImpactRepository;
    private readonly IRepository<ProjectChangeRequest, Guid> _projectChangeRequestRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;

    public ProjectGovernanceService(
        ICurrentUser currentUser,
        ProjectPerformanceService projectPerformanceService,
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectBaseline, Guid> projectBaselineRepository,
        IRepository<BusinessBudgetExecution, Guid> businessBudgetExecutionRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<ProjectChangeImpact, Guid> projectChangeImpactRepository,
        IRepository<ProjectChangeRequest, Guid> projectChangeRequestRepository,
        IRepository<ProjectCharter, Guid> projectCharterRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository)
    {
        _currentUser = currentUser;
        _projectPerformanceService = projectPerformanceService;
        _projectAccessGuard = projectAccessGuard;
        _projectBaselineRepository = projectBaselineRepository;
        _businessBudgetExecutionRepository = businessBudgetExecutionRepository;
        _businessContractRepository = businessContractRepository;
        _projectChangeImpactRepository = projectChangeImpactRepository;
        _projectChangeRequestRepository = projectChangeRequestRepository;
        _projectCharterRepository = projectCharterRepository;
        _projectRepository = projectRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectTaskRepository = projectTaskRepository;
    }

    public async Task<Guid> CreateBaselineAsync(Guid projectId, ProjectBaselineSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var existingBaselines = await _projectBaselineRepository.GetListAsync(item => item.ProjectId == projectId);
        if (existingBaselines.Count > 0 && !input.CreatedFromChangeRequestId.HasValue)
        {
            throw new BusinessException("后续基线版本必须关联已批准的范围、进度或成本基线变更。");
        }

        if (input.CreatedFromChangeRequestId.HasValue)
        {
            var changeRequest = await _projectChangeRequestRepository.GetAsync(input.CreatedFromChangeRequestId.Value);
            if (changeRequest.ProjectId != projectId || !changeRequest.IsApproved())
            {
                throw new BusinessException("基线变更必须关联当前项目已批准的范围、进度或成本基线变更。");
            }

            EnsureBaselineChangeType(changeRequest.ChangeType);
        }

        if (input.IsActive && existingBaselines.Count > 0)
        {
            foreach (var existingBaseline in existingBaselines.Where(item => item.IsActive))
            {
                existingBaseline.Deactivate();
            }

            await _projectBaselineRepository.UpdateManyAsync(existingBaselines.Where(item => !item.IsActive).ToList());
        }

        var baseline = new ProjectBaseline(
            Guid.NewGuid(),
            projectId,
            existingBaselines.Count == 0 ? 1 : existingBaselines.Max(item => item.VersionNo) + 1,
            input.EffectiveDate ?? DateTime.Today,
            input.BudgetAtCompletion,
            input.ScopeBaselineSummary.Trim(),
            input.ScheduleBaselineSummary.Trim(),
            input.CostBaselineSummary.Trim(),
            input.IsActive,
            input.CreatedFromChangeRequestId);
        await _projectBaselineRepository.InsertAsync(baseline, autoSave: true);
        return baseline.Id;
    }

    public async Task<Guid> CreateChangeRequestAsync(Guid projectId, ProjectChangeRequestSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var applicantUserId = input.ApplicantUserId ?? _currentUser.Id;
        if (!applicantUserId.HasValue)
        {
            throw new BusinessException("基线变更申请人不能为空。");
        }

        var requestCode = string.IsNullOrWhiteSpace(input.RequestCode)
            ? await GenerateChangeRequestCodeAsync(projectId)
            : input.RequestCode.Trim();
        if (await _projectChangeRequestRepository.AnyAsync(item => item.RequestCode == requestCode))
        {
            throw new BusinessException("基线变更编号已存在。");
        }

        var changeType = input.ChangeType.Trim();
        EnsureBaselineChangeType(changeType);
        var changeRequest = new ProjectChangeRequest(
            Guid.NewGuid(),
            projectId,
            requestCode,
            input.Title.Trim(),
            changeType,
            input.Reason.Trim(),
            input.ImpactScope.Trim(),
            input.ExpectedResult.Trim(),
            applicantUserId.Value,
            "待评审");
        await _projectChangeRequestRepository.InsertAsync(changeRequest, autoSave: true);

        var impacts = new List<ProjectChangeImpact>();
        foreach (var impact in input.Impacts)
        {
            await ValidateChangeImpactAsync(projectId, impact);
            impacts.Add(new ProjectChangeImpact(
                Guid.NewGuid(),
                projectId,
                changeRequest.Id,
                impact.ImpactType.Trim(),
                ProjectPriorities.Normalize(impact.Severity),
                impact.Analysis.Trim(),
                impact.RelatedTaskId,
                impact.RelatedMilestoneId,
                impact.RelatedBusinessCode,
                impact.RelatedBudgetCode,
                impact.RelatedContractCode,
                impact.EstimatedCostDelta,
                impact.EstimatedScheduleDeltaDays));
        }

        if (impacts.Count > 0)
        {
            await _projectChangeImpactRepository.InsertManyAsync(impacts, autoSave: true);
        }

        return changeRequest.Id;
    }

    public async Task<Guid> CreatePerformanceSnapshotAsync(Guid projectId, ProjectPerformanceSnapshotSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        return await _projectPerformanceService.CreateSnapshotAsync(projectId, input);
    }

    public async Task<Guid> SaveCharterAsync(Guid projectId, ProjectCharterSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var approvalStatus = NormalizeCharterApprovalStatus(input.ApprovalStatus);
        var existing = (await _projectCharterRepository.GetListAsync(item => item.ProjectId == projectId)).FirstOrDefault();
        if (existing == null)
        {
            var charter = new ProjectCharter(
                Guid.NewGuid(),
                projectId,
                input.BusinessCase,
                input.Objectives,
                input.ScopeBoundary,
                input.Assumptions,
                input.Constraints,
                input.SuccessCriteria,
                approvalStatus);
            await _projectCharterRepository.InsertAsync(charter, autoSave: true);
            return charter.Id;
        }

        if (existing.ApprovalStatus == "已批准")
        {
            throw new BusinessException("已批准的项目章程摘要不能直接修改，请创建新版本并重新提交审批。");
        }

        existing.Update(
            input.BusinessCase,
            input.Objectives,
            input.ScopeBoundary,
            input.Assumptions,
            input.Constraints,
            input.SuccessCriteria,
            existing.ApprovalStatus is "待审批" or "已批准" ? existing.ApprovalStatus : approvalStatus);
        await _projectCharterRepository.UpdateAsync(existing, autoSave: true);
        return existing.Id;
    }

    private static string NormalizeCharterApprovalStatus(string approvalStatus)
    {
        var status = string.IsNullOrWhiteSpace(approvalStatus) ? "草稿" : approvalStatus.Trim();
        return status is "已批准" or "待审批" ? "草稿" : status;
    }

    private async Task<string> GenerateChangeRequestCodeAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var count = await _projectChangeRequestRepository.CountAsync(item => item.ProjectId == projectId);
        return $"CHG-{project.ProjectCode}-{count + 1:000}";
    }

    private async Task ValidateChangeImpactAsync(Guid projectId, ProjectChangeImpactSaveInput input)
    {
        if (!ProjectPriorities.IsSupported(input.Severity))
        {
            throw new BusinessException("基线变更影响等级只支持 P0、P1、P2、P3。");
        }

        if (input.RelatedTaskId.HasValue
            && !await _projectTaskRepository.AnyAsync(item => item.Id == input.RelatedTaskId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("基线变更影响关联的任务必须属于当前项目。");
        }

        if (input.RelatedMilestoneId.HasValue
            && !await _projectMilestoneRepository.AnyAsync(item => item.Id == input.RelatedMilestoneId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("基线变更影响关联的里程碑必须属于当前项目。");
        }

        if (!string.IsNullOrWhiteSpace(input.RelatedBudgetCode)
            && !await _businessBudgetExecutionRepository.AnyAsync(item =>
                item.ProjectId == projectId
                && item.BudgetCode == input.RelatedBudgetCode.Trim()))
        {
            throw new BusinessException("基线变更影响关联的预算编码必须属于当前项目。");
        }

        if (!string.IsNullOrWhiteSpace(input.RelatedContractCode)
            && !await _businessContractRepository.AnyAsync(item =>
                item.ProjectId == projectId
                && item.ContractCode == input.RelatedContractCode.Trim()))
        {
            throw new BusinessException("基线变更影响关联的合同编码必须属于当前项目。");
        }
    }

    private static void EnsureBaselineChangeType(string changeType)
    {
        if (!BaselineChangeTypes.Contains(changeType))
        {
            throw new BusinessException("项目审批只用于范围、进度、成本基线变更；合同、预算、需求细节调整请在对应业务域处理。");
        }
    }
}
