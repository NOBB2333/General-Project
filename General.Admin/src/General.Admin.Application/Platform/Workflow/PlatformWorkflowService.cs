using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace General.Admin.Platform;

public class PlatformWorkflowService : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IPlatformWorkflowProvider _workflowProvider;
    private readonly IRepository<PlatformApprovalRecord, Guid> _approvalRecordRepository;
    private readonly IRepository<PlatformWorkflowInstance, Guid> _workflowInstanceRepository;

    public PlatformWorkflowService(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IPlatformWorkflowProvider workflowProvider,
        IRepository<PlatformApprovalRecord, Guid> approvalRecordRepository,
        IRepository<PlatformWorkflowInstance, Guid> workflowInstanceRepository)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _workflowProvider = workflowProvider;
        _approvalRecordRepository = approvalRecordRepository;
        _workflowInstanceRepository = workflowInstanceRepository;
    }

    public async Task<PlatformWorkflowInstance> StartAsync(PlatformWorkflowStartInput input)
    {
        var existingActive = await _workflowInstanceRepository.FirstOrDefaultAsync(item =>
            item.WorkflowDefinitionKey == input.WorkflowDefinitionKey
            && item.BusinessType == input.BusinessType
            && item.BusinessId == input.BusinessId
            && item.Status == PlatformWorkflowStatuses.Pending);
        if (existingActive is not null)
        {
            throw new BusinessException("当前业务对象已有审批中的流程。");
        }

        var elsaWorkflowInstanceId = await _workflowProvider.StartAsync(input);
        var instance = new PlatformWorkflowInstance(
            Guid.NewGuid(),
            input.WorkflowDefinitionKey.Trim(),
            input.WorkflowDefinitionVersion,
            input.BusinessType.Trim(),
            input.BusinessId.Trim(),
            input.BusinessCode.Trim(),
            input.BusinessTitle.Trim(),
            input.BusinessSnapshot.Trim(),
            input.ApplicantUserId ?? _currentUser.Id,
            elsaWorkflowInstanceId,
            _currentTenant.Id);
        await _workflowInstanceRepository.InsertAsync(instance, autoSave: true);
        await AddRecordAsync(
            instance,
            PlatformWorkflowActions.Submit,
            string.Empty,
            PlatformWorkflowStatuses.Pending,
            input.ApplicantUserId ?? _currentUser.Id,
            "提交审批");
        return instance;
    }

    public async Task ApproveAsync(Guid instanceId, PlatformApprovalActionInput input)
    {
        var instance = await GetActiveInstanceAsync(instanceId);
        var fromStatus = instance.Status;
        instance.MarkApproved();
        await AddRecordAsync(
            instance,
            PlatformWorkflowActions.Approve,
            fromStatus,
            instance.Status,
            _currentUser.Id,
            input.Comment);
        await _workflowInstanceRepository.UpdateAsync(instance, autoSave: true);
    }

    public async Task RejectAsync(Guid instanceId, PlatformApprovalActionInput input)
    {
        var instance = await GetActiveInstanceAsync(instanceId);
        var fromStatus = instance.Status;
        instance.MarkRejected();
        await AddRecordAsync(
            instance,
            PlatformWorkflowActions.Reject,
            fromStatus,
            instance.Status,
            _currentUser.Id,
            input.Comment);
        await _workflowInstanceRepository.UpdateAsync(instance, autoSave: true);
    }

    public async Task WithdrawAsync(Guid instanceId, PlatformApprovalActionInput input)
    {
        var instance = await GetActiveInstanceAsync(instanceId);
        var fromStatus = instance.Status;
        instance.MarkWithdrawn();
        await AddRecordAsync(
            instance,
            PlatformWorkflowActions.Withdraw,
            fromStatus,
            instance.Status,
            _currentUser.Id,
            input.Comment);
        await _workflowInstanceRepository.UpdateAsync(instance, autoSave: true);
    }

    public async Task<PlatformWorkflowInstance?> FindLatestAsync(string workflowDefinitionKey, string businessType, string businessId)
    {
        var instances = await _workflowInstanceRepository.GetListAsync(item =>
            item.WorkflowDefinitionKey == workflowDefinitionKey
            && item.BusinessType == businessType
            && item.BusinessId == businessId);
        return instances
            .OrderByDescending(item => item.CreationTime)
            .FirstOrDefault();
    }

    public async Task<List<PlatformApprovalRecord>> GetRecordsAsync(Guid instanceId)
    {
        var records = await _approvalRecordRepository.GetListAsync(item => item.ApprovalInstanceId == instanceId);
        return records.OrderBy(item => item.CreationTime).ToList();
    }

    private async Task AddRecordAsync(
        PlatformWorkflowInstance instance,
        string action,
        string fromStatus,
        string toStatus,
        Guid? actorUserId,
        string comment)
    {
        await _approvalRecordRepository.InsertAsync(
            new PlatformApprovalRecord(
                Guid.NewGuid(),
                instance.Id,
                action,
                string.IsNullOrWhiteSpace(fromStatus) ? "未提交" : fromStatus,
                toStatus,
                actorUserId,
                comment,
                instance.CurrentNodeName,
                _currentTenant.Id),
            autoSave: true);
    }

    private async Task<PlatformWorkflowInstance> GetActiveInstanceAsync(Guid instanceId)
    {
        var instance = await _workflowInstanceRepository.GetAsync(instanceId);
        if (!PlatformWorkflowStatuses.IsActive(instance.Status))
        {
            throw new BusinessException("当前流程已结束，不能重复处理。");
        }

        return instance;
    }
}
