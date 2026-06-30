using System.Text;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace General.Admin.Integration;

public class IntegrationDeliveryService : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationDeliveryRecord, Guid> _deliveryRecordRepository;
    private readonly IntegrationDispatchService _dispatchService;

    public IntegrationDeliveryService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<IntegrationConnector, Guid> connectorRepository,
        IRepository<IntegrationDeliveryRecord, Guid> deliveryRecordRepository,
        IntegrationDispatchService dispatchService)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _connectorRepository = connectorRepository;
        _deliveryRecordRepository = deliveryRecordRepository;
        _dispatchService = dispatchService;
    }

    public async Task<PlatformPagedResultDto<IntegrationDeliveryRecordDto>> GetRecordsAsync(IntegrationDeliveryRecordQueryInput input)
    {
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 500);
        var skipCount = Math.Max(input.SkipCount, 0);
        var queryable = await BuildFilteredQueryAsync(input);
        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var records = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount));

        return new PlatformPagedResultDto<IntegrationDeliveryRecordDto>
        {
            Items = records.Select(MapRecord).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationDeliverySummaryDto> GetSummaryAsync(IntegrationDeliveryRecordQueryInput input)
    {
        var queryable = await BuildFilteredQueryAsync(input);
        var records = await _asyncQueryableExecuter.ToListAsync(queryable);
        var connectors = (await _connectorRepository.GetListAsync())
            .ToDictionary(x => x.Id);
        var completed = records
            .Where(x => x.Status == "Succeeded" && x.DurationMilliseconds > 0)
            .ToList();

        return new IntegrationDeliverySummaryDto
        {
            AverageDurationMilliseconds = completed.Count == 0
                ? 0
                : (int)Math.Round(completed.Average(x => x.DurationMilliseconds)),
            ConnectorRates = records
                .GroupBy(x => x.ConnectorId)
                .Select(group =>
                {
                    var total = group.Count();
                    var succeeded = group.Count(x => x.Status == "Succeeded");
                    var failed = group.Count(x => x.Status is "Failed" or "DeadLetter");
                    return new IntegrationDeliveryConnectorRateDto
                    {
                        ConnectorCode = connectors.GetValueOrDefault(group.Key)?.ConnectorCode ?? group.First().ConnectorCode,
                        DisplayName = connectors.GetValueOrDefault(group.Key)?.DisplayName ?? group.First().ConnectorCode,
                        FailedCount = failed,
                        SucceededCount = succeeded,
                        SuccessRate = total == 0 ? 0 : Math.Round(succeeded * 100m / total, 2),
                        TotalCount = total
                    };
                })
                .OrderByDescending(x => x.TotalCount)
                .ThenBy(x => x.DisplayName)
                .ToList(),
            DeadLetterCount = records.Count(x => x.Status == "DeadLetter"),
            FailedCount = records.Count(x => x.Status == "Failed"),
            PendingCount = records.Count(x => x.Status is "Pending" or "Sending"),
            SkippedCount = records.Count(x => x.Status == "Skipped"),
            SucceededCount = records.Count(x => x.Status == "Succeeded"),
            TotalCount = records.Count
        };
    }

    public async Task<IntegrationDeliveryExportResultDto> ExportRecordsAsync(IntegrationDeliveryRecordQueryInput input)
    {
        var queryable = await BuildFilteredQueryAsync(input);
        var maxResultCount = input.MaxResultCount <= 20 ? 5000 : Math.Clamp(input.MaxResultCount, 1, 5000);
        var records = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Take(maxResultCount));

        return new IntegrationDeliveryExportResultDto
        {
            Content = BuildCsv(records),
            ContentType = "text/csv;charset=utf-8",
            FileName = $"外部系统投递审计_{DateTime.Now:yyyyMMddHHmmss}.csv"
        };
    }

    public async Task<IntegrationDeliveryRecordDto> RetryAsync(Guid id)
    {
        await _dispatchService.RetryAsync(id);
        return MapRecord(await _deliveryRecordRepository.GetAsync(id));
    }

    public async Task<IntegrationDeliveryRecordDto> SkipAsync(Guid id, IntegrationDeliverySkipInput input)
    {
        var record = await _deliveryRecordRepository.GetAsync(id);
        record.MarkSkipped(input.Reason);
        await _deliveryRecordRepository.UpdateAsync(record, autoSave: true);
        return MapRecord(record);
    }

    private async Task<IQueryable<IntegrationDeliveryRecord>> BuildFilteredQueryAsync(IntegrationDeliveryRecordQueryInput input)
    {
        var queryable = await _deliveryRecordRepository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.ConnectorCode))
        {
            var connectorCode = input.ConnectorCode.Trim();
            queryable = queryable.Where(x => x.ConnectorCode == connectorCode);
        }

        if (!string.IsNullOrWhiteSpace(input.MessageType))
        {
            var messageType = input.MessageType.Trim();
            queryable = queryable.Where(x => x.MessageType == messageType);
        }

        if (!string.IsNullOrWhiteSpace(input.TargetType))
        {
            var targetType = input.TargetType.Trim();
            queryable = queryable.Where(x => x.TargetType == targetType);
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
                x.TargetId.Contains(keyword) ||
                x.PayloadPreview.Contains(keyword) ||
                x.ExternalMessageId.Contains(keyword) ||
                x.ErrorMessage.Contains(keyword) ||
                x.BusinessKey.Contains(keyword));
        }

        return queryable;
    }

    private static IntegrationDeliveryRecordDto MapRecord(IntegrationDeliveryRecord record)
    {
        return new IntegrationDeliveryRecordDto
        {
            AttemptCount = record.AttemptCount,
            BusinessKey = record.BusinessKey,
            BusinessModule = record.BusinessModule,
            ConnectorCode = record.ConnectorCode,
            ConnectorId = record.ConnectorId,
            CreationTime = record.CreationTime,
            DeadLetterReason = record.DeadLetterReason,
            DurationMilliseconds = record.DurationMilliseconds,
            ErrorMessage = record.ErrorMessage,
            ExternalMessageId = record.ExternalMessageId,
            Id = record.Id,
            LastAttemptTime = record.LastAttemptTime,
            MaxRetryCount = record.MaxRetryCount,
            MessageType = record.MessageType,
            NextRetryTime = record.NextRetryTime,
            PayloadPreview = record.PayloadPreview,
            ProviderType = record.ProviderType,
            RequestJson = record.RequestJson,
            ResponseJson = record.ResponseJson,
            SourceType = record.SourceType,
            Status = record.Status,
            TargetId = record.TargetId,
            TargetType = record.TargetType
        };
    }

    private static byte[] BuildCsv(IReadOnlyCollection<IntegrationDeliveryRecord> records)
    {
        var rows = new List<string[]>
        {
            new[]
            {
                "投递创建时间",
                "最近尝试时间",
                "连接器",
                "平台",
                "消息类型",
                "目标类型",
                "目标ID",
                "状态",
                "尝试次数",
                "最大重试",
                "业务模块",
                "业务键",
                "来源",
                "外部消息ID",
                "耗时毫秒",
                "下次重试时间",
                "错误信息",
                "死信原因",
                "内容摘要"
            }
        };

        rows.AddRange(records.Select(record => new[]
        {
            FormatDateTime(record.CreationTime),
            FormatDateTime(record.LastAttemptTime),
            record.ConnectorCode,
            record.ProviderType,
            record.MessageType,
            record.TargetType,
            record.TargetId,
            record.Status,
            record.AttemptCount.ToString(),
            record.MaxRetryCount.ToString(),
            record.BusinessModule,
            record.BusinessKey,
            record.SourceType,
            record.ExternalMessageId,
            record.DurationMilliseconds.ToString(),
            FormatDateTime(record.NextRetryTime),
            record.ErrorMessage,
            record.DeadLetterReason,
            record.PayloadPreview
        }));

        var lines = rows.Select(row => string.Join(",", row.Select(EscapeCsv)));
        var content = "\uFEFF" + string.Join(Environment.NewLine, lines);
        return Encoding.UTF8.GetBytes(content);
    }

    private static string EscapeCsv(string? value)
    {
        var text = value ?? string.Empty;
        return text.Contains(',') || text.Contains('"') || text.Contains('\r') || text.Contains('\n')
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
    }
}
