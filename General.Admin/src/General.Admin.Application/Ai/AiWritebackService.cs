using System.Text.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace General.Admin.Ai;

public class AiWritebackService : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiWritebackRequest, Guid> _requestRepository;

    public AiWritebackService(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IGuidGenerator guidGenerator,
        IRepository<AiWritebackRequest, Guid> requestRepository)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _guidGenerator = guidGenerator;
        _requestRepository = requestRepository;
    }

    public async Task<AiWritebackRequestDto> CreateRequestAsync(AiWritebackRequestCreateInput input)
    {
        var request = new AiWritebackRequest(
            _guidGenerator.Create(),
            input.BusinessModule,
            input.ActionCode,
            input.TargetEntityType,
            input.TargetEntityId,
            input.Summary,
            NormalizeJson(input.PayloadJson),
            NormalizeJson(input.DiffPreviewJson),
            _currentTenant.Id,
            input.ProjectId,
            input.ConversationId,
            input.MessageId,
            input.SourceType);
        await _requestRepository.InsertAsync(request, autoSave: true);
        return MapRequest(request);
    }

    public async Task<AiPagedResultDto<AiWritebackRequestDto>> GetRequestsAsync(AiWritebackRequestQueryInput input)
    {
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 500);
        var skipCount = Math.Max(input.SkipCount, 0);
        var queryable = await _requestRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessModule))
        {
            var businessModule = input.BusinessModule.Trim();
            queryable = queryable.Where(x => x.BusinessModule == businessModule);
        }

        if (!string.IsNullOrWhiteSpace(input.ActionCode))
        {
            var actionCode = input.ActionCode.Trim();
            queryable = queryable.Where(x => x.ActionCode == actionCode);
        }

        if (!string.IsNullOrWhiteSpace(input.TargetEntityType))
        {
            var targetEntityType = input.TargetEntityType.Trim();
            queryable = queryable.Where(x => x.TargetEntityType == targetEntityType);
        }

        if (input.ProjectId.HasValue)
        {
            queryable = queryable.Where(x => x.ProjectId == input.ProjectId.Value);
        }

        if (input.ConversationId.HasValue)
        {
            queryable = queryable.Where(x => x.ConversationId == input.ConversationId.Value);
        }

        if (input.CreatedFrom.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime >= input.CreatedFrom.Value);
        }

        if (input.CreatedTo.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime < input.CreatedTo.Value.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.Summary.Contains(keyword) ||
                x.TargetEntityId.Contains(keyword) ||
                x.PayloadJson.Contains(keyword) ||
                x.DiffPreviewJson.Contains(keyword));
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var requests = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount));

        return new AiPagedResultDto<AiWritebackRequestDto>
        {
            Items = requests
                .Select(MapRequest)
                .ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<AiWritebackRequestDto> ApproveAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Approve(_currentUser.Id);
        await _requestRepository.UpdateAsync(request, autoSave: true);
        return MapRequest(request);
    }

    public async Task<AiWritebackRequestDto> RejectAsync(Guid id, AiWritebackDecisionInput input)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Reject(input.Reason, _currentUser.Id);
        await _requestRepository.UpdateAsync(request, autoSave: true);
        return MapRequest(request);
    }

    public async Task<AiWritebackRequestDto> CancelAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Cancel();
        await _requestRepository.UpdateAsync(request, autoSave: true);
        return MapRequest(request);
    }

    private static string NormalizeJson(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        try
        {
            using var _ = JsonDocument.Parse(normalized);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("AI 写回申请的 JSON 内容格式不正确。");
        }

        return normalized;
    }

    private static AiWritebackRequestDto MapRequest(AiWritebackRequest item)
    {
        return new AiWritebackRequestDto
        {
            ActionCode = item.ActionCode,
            ApprovedTime = item.ApprovedTime,
            ApproverUserId = item.ApproverUserId,
            BusinessModule = item.BusinessModule,
            ConversationId = item.ConversationId,
            CreationTime = item.CreationTime,
            DiffPreviewJson = item.DiffPreviewJson,
            ErrorMessage = item.ErrorMessage,
            Id = item.Id,
            MessageId = item.MessageId,
            PayloadJson = item.PayloadJson,
            ProjectId = item.ProjectId,
            RejectReason = item.RejectReason,
            SourceType = item.SourceType,
            Status = item.Status,
            Summary = item.Summary,
            TargetEntityId = item.TargetEntityId,
            TargetEntityType = item.TargetEntityType
        };
    }
}
