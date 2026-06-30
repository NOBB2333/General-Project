using System.Diagnostics;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

public class IntegrationDispatchService : ITransientDependency
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationDeliveryRecord, Guid> _deliveryRecordRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IntegrationRateLimitService _rateLimitService;
    private readonly IEnumerable<IIntegrationMessageSender> _messageSenders;

    public IntegrationDispatchService(
        IRepository<IntegrationConnector, Guid> connectorRepository,
        IRepository<IntegrationDeliveryRecord, Guid> deliveryRecordRepository,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IntegrationRateLimitService rateLimitService,
        IEnumerable<IIntegrationMessageSender> messageSenders)
    {
        _connectorRepository = connectorRepository;
        _deliveryRecordRepository = deliveryRecordRepository;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _rateLimitService = rateLimitService;
        _messageSenders = messageSenders;
    }

    public async Task<IntegrationMessageSendResult> SendTestMessageAsync(Guid connectorId, IntegrationMessageSendTestInput input)
    {
        return await SendMessageAsync(new IntegrationMessageSendInput
        {
            Body = input.Body,
            BusinessModule = "Integration",
            BusinessKey = "SendTest",
            ConnectorId = connectorId,
            MessageType = input.MessageType,
            SourceType = "ManualTest",
            TargetId = input.TargetId,
            TargetType = input.TargetType
        });
    }

    public async Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input)
    {
        await _rateLimitService.CheckMessageSendAsync(input);
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var record = new IntegrationDeliveryRecord(
            _guidGenerator.Create(),
            input.ConnectorId,
            connector.ConnectorCode,
            connector.ProviderType,
            input.MessageType,
            input.TargetType,
            input.TargetId,
            input.Body,
            BuildRequestJson(input),
            _currentTenant.Id,
            input.SourceType,
            input.BusinessModule,
            input.BusinessKey,
            input.MaxRetryCount);
        await _deliveryRecordRepository.InsertAsync(record, autoSave: true);

        var sender = _messageSenders.FirstOrDefault(x =>
            x.ConnectorCode.Equals(connector.ConnectorCode, StringComparison.OrdinalIgnoreCase));
        if (sender == null)
        {
            record.MarkSending();
            record.MarkFailed(
                $"连接器 {connector.DisplayName} 暂未实现消息发送适配器。",
                "{}",
                0);
            await _deliveryRecordRepository.UpdateAsync(record, autoSave: true);
            return new IntegrationMessageSendResult
            {
                DeliveryRecordId = record.Id,
                Message = record.ErrorMessage,
                Status = record.Status,
                Success = false
            };
        }

        return await SendRecordedMessageAsync(record, input, sender);
    }

    public async Task<IntegrationMessageSendResult> RetryAsync(Guid deliveryRecordId)
    {
        var record = await _deliveryRecordRepository.GetAsync(deliveryRecordId);
        if (!record.CanRetry())
        {
            throw new UserFriendlyException("当前投递记录状态不允许重试。");
        }

        var connector = await _connectorRepository.GetAsync(record.ConnectorId);
        var sender = _messageSenders.FirstOrDefault(x =>
            x.ConnectorCode.Equals(connector.ConnectorCode, StringComparison.OrdinalIgnoreCase));
        if (sender == null)
        {
            throw new UserFriendlyException($"连接器 {connector.DisplayName} 暂未实现消息发送适配器。");
        }

        var input = ParseRequest(record.RequestJson, record);
        await _rateLimitService.CheckMessageSendAsync(input);
        return await SendRecordedMessageAsync(record, input, sender);
    }

    private async Task<IntegrationMessageSendResult> SendRecordedMessageAsync(
        IntegrationDeliveryRecord record,
        IntegrationMessageSendInput input,
        IIntegrationMessageSender sender)
    {
        var stopwatch = Stopwatch.StartNew();
        record.MarkSending();
        await _deliveryRecordRepository.UpdateAsync(record, autoSave: true);

        try
        {
            var result = await sender.SendMessageAsync(input);
            stopwatch.Stop();
            record.MarkSucceeded(
                result.ExternalMessageId,
                JsonSerializer.Serialize(result),
                (int)stopwatch.ElapsedMilliseconds);
            await _deliveryRecordRepository.UpdateAsync(record, autoSave: true);

            result.DeliveryRecordId = record.Id;
            result.Status = record.Status;
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            record.MarkFailed(
                ex is UserFriendlyException ? ex.Message : $"消息投递失败：{ex.Message}",
                JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name }),
                (int)stopwatch.ElapsedMilliseconds);
            await _deliveryRecordRepository.UpdateAsync(record, autoSave: true);
            return new IntegrationMessageSendResult
            {
                DeliveryRecordId = record.Id,
                Message = record.ErrorMessage,
                Status = record.Status,
                Success = false
            };
        }
    }

    private static string BuildRequestJson(IntegrationMessageSendInput input)
    {
        return JsonSerializer.Serialize(new
        {
            input.Body,
            input.BusinessKey,
            input.BusinessModule,
            input.ConnectorId,
            input.MaxRetryCount,
            input.MessageType,
            input.SourceType,
            input.TargetId,
            input.TargetType
        });
    }

    private static IntegrationMessageSendInput ParseRequest(string requestJson, IntegrationDeliveryRecord record)
    {
        try
        {
            return JsonSerializer.Deserialize<IntegrationMessageSendInput>(requestJson) ?? BuildFallbackInput(record);
        }
        catch (JsonException)
        {
            return BuildFallbackInput(record);
        }
    }

    private static IntegrationMessageSendInput BuildFallbackInput(IntegrationDeliveryRecord record)
    {
        return new IntegrationMessageSendInput
        {
            Body = record.PayloadPreview,
            BusinessKey = record.BusinessKey,
            BusinessModule = record.BusinessModule,
            ConnectorId = record.ConnectorId,
            MaxRetryCount = record.MaxRetryCount,
            MessageType = record.MessageType,
            SourceType = record.SourceType,
            TargetId = record.TargetId,
            TargetType = record.TargetType
        };
    }
}
