using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Integration)]
[Route("api/app/integration")]
public class IntegrationController : ControllerBase
{
    private readonly IntegrationBindingService _bindingService;
    private readonly IntegrationConnectorService _connectorService;
    private readonly IntegrationDeliveryService _deliveryService;
    private readonly IntegrationDispatchService _dispatchService;
    private readonly IntegrationNotificationService _notificationService;
    private readonly IntegrationSyncService _syncService;
    private readonly IntegrationWebhookService _webhookService;

    public IntegrationController(
        IntegrationBindingService bindingService,
        IntegrationConnectorService connectorService,
        IntegrationDeliveryService deliveryService,
        IntegrationDispatchService dispatchService,
        IntegrationNotificationService notificationService,
        IntegrationSyncService syncService,
        IntegrationWebhookService webhookService)
    {
        _bindingService = bindingService;
        _connectorService = connectorService;
        _deliveryService = deliveryService;
        _dispatchService = dispatchService;
        _notificationService = notificationService;
        _syncService = syncService;
        _webhookService = webhookService;
    }

    [HttpGet("connectors")]
    [Authorize(AdminPermissions.Integration.ConnectorView)]
    public async Task<ActionResult<ApiResponse<List<IntegrationConnectorDto>>>> GetConnectorsAsync([FromQuery] IntegrationConnectorQueryInput input)
    {
        return ApiResponse<List<IntegrationConnectorDto>>.Ok(await _connectorService.GetConnectorsAsync(input));
    }

    [HttpGet("connectors/health")]
    [Authorize(AdminPermissions.Integration.ConnectorView)]
    public async Task<ActionResult<ApiResponse<IntegrationConnectorHealthSummaryDto>>> GetConnectorHealthAsync()
    {
        return ApiResponse<IntegrationConnectorHealthSummaryDto>.Ok(await _connectorService.GetHealthAsync());
    }

    [HttpPut("connectors/{id:guid}/tenant-config")]
    [Authorize(AdminPermissions.Integration.ConnectorManage)]
    [PlatformEndpoint("Integration.Connector.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationConnectorDto>>> SaveTenantConfigAsync(
        Guid id,
        [FromBody] IntegrationTenantConfigSaveInput input)
    {
        return ApiResponse<IntegrationConnectorDto>.Ok(await _connectorService.SaveTenantConfigAsync(id, input));
    }

    [HttpPost("connectors/{id:guid}/toggle")]
    [Authorize(AdminPermissions.Integration.ConnectorManage)]
    [PlatformEndpoint("Integration.Connector.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationConnectorDto>>> ToggleTenantConfigAsync(
        Guid id,
        [FromBody] IntegrationToggleInput input)
    {
        return ApiResponse<IntegrationConnectorDto>.Ok(await _connectorService.ToggleTenantConfigAsync(id, input.IsEnabled));
    }

    [HttpPost("connectors/{id:guid}/rotate-secret")]
    [Authorize(AdminPermissions.Integration.ConnectorRotateSecret)]
    [PlatformEndpoint("Integration.Connector.RotateSecret")]
    public async Task<ActionResult<ApiResponse<IntegrationConnectorDto>>> RotateConnectorSecretAsync(
        Guid id,
        [FromBody] IntegrationSecretRotateInput input)
    {
        return ApiResponse<IntegrationConnectorDto>.Ok(await _connectorService.RotateSecretAsync(id, input));
    }

    [HttpPost("connectors/{id:guid}/test")]
    [Authorize(AdminPermissions.Integration.ConnectorTest)]
    [PlatformEndpoint("Integration.Connector.Test")]
    public async Task<ActionResult<ApiResponse<IntegrationConnectionTestResultDto>>> TestConnectionAsync(Guid id)
    {
        return ApiResponse<IntegrationConnectionTestResultDto>.Ok(await _connectorService.TestConnectionAsync(id));
    }

    [HttpPost("connectors/{id:guid}/send-test")]
    [Authorize(AdminPermissions.Integration.ConnectorTest)]
    [PlatformEndpoint("Integration.Connector.Test")]
    public async Task<ActionResult<ApiResponse<IntegrationMessageSendResult>>> SendTestMessageAsync(
        Guid id,
        [FromBody] IntegrationMessageSendTestInput input)
    {
        return ApiResponse<IntegrationMessageSendResult>.Ok(await _dispatchService.SendTestMessageAsync(id, input));
    }

    [HttpGet("templates")]
    [Authorize(AdminPermissions.Integration.TemplateView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationMessageTemplateDto>>>> GetTemplatesAsync(
        [FromQuery] IntegrationMessageTemplateQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationMessageTemplateDto>>.Ok(await _notificationService.GetTemplatesAsync(input));
    }

    [HttpPost("templates")]
    [Authorize(AdminPermissions.Integration.TemplateManage)]
    [PlatformEndpoint("Integration.Template.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationMessageTemplateDto>>> SaveTemplateAsync(
        [FromBody] IntegrationMessageTemplateSaveInput input)
    {
        return ApiResponse<IntegrationMessageTemplateDto>.Ok(await _notificationService.SaveTemplateAsync(input));
    }

    [HttpPost("templates/render")]
    [Authorize(AdminPermissions.Integration.TemplateTest)]
    [PlatformEndpoint("Integration.Template.Test")]
    public async Task<ActionResult<ApiResponse<IntegrationTemplateRenderResultDto>>> RenderTemplateAsync(
        [FromBody] IntegrationTemplateRenderInput input)
    {
        return ApiResponse<IntegrationTemplateRenderResultDto>.Ok(await _notificationService.RenderTemplateAsync(input));
    }

    [HttpGet("notification-rules")]
    [Authorize(AdminPermissions.Integration.RuleView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationNotificationRuleDto>>>> GetNotificationRulesAsync(
        [FromQuery] IntegrationNotificationRuleQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationNotificationRuleDto>>.Ok(await _notificationService.GetRulesAsync(input));
    }

    [HttpPost("notification-rules")]
    [Authorize(AdminPermissions.Integration.RuleManage)]
    [PlatformEndpoint("Integration.Rule.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationNotificationRuleDto>>> SaveNotificationRuleAsync(
        [FromBody] IntegrationNotificationRuleSaveInput input)
    {
        return ApiResponse<IntegrationNotificationRuleDto>.Ok(await _notificationService.SaveRuleAsync(input));
    }

    [HttpPost("notification-rules/{id:guid}/toggle")]
    [Authorize(AdminPermissions.Integration.RuleManage)]
    [PlatformEndpoint("Integration.Rule.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationNotificationRuleDto>>> ToggleNotificationRuleAsync(
        Guid id,
        [FromBody] IntegrationNotificationRuleToggleInput input)
    {
        return ApiResponse<IntegrationNotificationRuleDto>.Ok(await _notificationService.ToggleRuleAsync(id, input));
    }

    [HttpPost("notification-rules/evaluate")]
    [Authorize(AdminPermissions.Integration.RuleManage)]
    [PlatformEndpoint("Integration.Rule.Evaluate")]
    public async Task<ActionResult<ApiResponse<IntegrationNotificationEvaluationResultDto>>> EvaluateNotificationRulesAsync(
        [FromBody] IntegrationNotificationEventInput input)
    {
        return ApiResponse<IntegrationNotificationEvaluationResultDto>.Ok(await _notificationService.EvaluateEventAsync(input));
    }

    [HttpPost("notification-rules/dispatch")]
    [Authorize(AdminPermissions.Integration.RuleManage)]
    [PlatformEndpoint("Integration.Rule.Dispatch")]
    public async Task<ActionResult<ApiResponse<IntegrationNotificationDispatchResultDto>>> DispatchNotificationRulesAsync(
        [FromBody] IntegrationNotificationEventInput input)
    {
        return ApiResponse<IntegrationNotificationDispatchResultDto>.Ok(await _notificationService.DispatchEventAsync(input));
    }

    [HttpGet("delivery-records")]
    [Authorize(AdminPermissions.Integration.DeliveryView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationDeliveryRecordDto>>>> GetDeliveryRecordsAsync(
        [FromQuery] IntegrationDeliveryRecordQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationDeliveryRecordDto>>.Ok(await _deliveryService.GetRecordsAsync(input));
    }

    [HttpGet("delivery-records/summary")]
    [Authorize(AdminPermissions.Integration.DeliveryView)]
    public async Task<ActionResult<ApiResponse<IntegrationDeliverySummaryDto>>> GetDeliverySummaryAsync(
        [FromQuery] IntegrationDeliveryRecordQueryInput input)
    {
        return ApiResponse<IntegrationDeliverySummaryDto>.Ok(await _deliveryService.GetSummaryAsync(input));
    }

    [HttpGet("delivery-records/export")]
    [Authorize(AdminPermissions.Integration.DeliveryExport)]
    [PlatformEndpoint("Integration.Delivery.Export")]
    public async Task<IActionResult> ExportDeliveryRecordsAsync([FromQuery] IntegrationDeliveryRecordQueryInput input)
    {
        var result = await _deliveryService.ExportRecordsAsync(input);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpPost("delivery-records/{id:guid}/retry")]
    [Authorize(AdminPermissions.Integration.DeliveryManage)]
    [PlatformEndpoint("Integration.Delivery.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationDeliveryRecordDto>>> RetryDeliveryAsync(Guid id)
    {
        return ApiResponse<IntegrationDeliveryRecordDto>.Ok(await _deliveryService.RetryAsync(id));
    }

    [HttpPost("delivery-records/{id:guid}/skip")]
    [Authorize(AdminPermissions.Integration.DeliveryManage)]
    [PlatformEndpoint("Integration.Delivery.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationDeliveryRecordDto>>> SkipDeliveryAsync(
        Guid id,
        [FromBody] IntegrationDeliverySkipInput input)
    {
        return ApiResponse<IntegrationDeliveryRecordDto>.Ok(await _deliveryService.SkipAsync(id, input));
    }

    [HttpGet("bindings/external-accounts")]
    [Authorize(AdminPermissions.Integration.BindingView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationExternalAccountDto>>>> GetExternalAccountsAsync(
        [FromQuery] IntegrationExternalAccountQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationExternalAccountDto>>.Ok(await _bindingService.GetExternalAccountsAsync(input));
    }

    [HttpPost("bindings/external-accounts")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationExternalAccountDto>>> SaveExternalAccountAsync(
        [FromBody] IntegrationExternalAccountSaveInput input)
    {
        return ApiResponse<IntegrationExternalAccountDto>.Ok(await _bindingService.SaveExternalAccountAsync(input));
    }

    [HttpPost("bindings/external-accounts/import")]
    [Authorize(AdminPermissions.Integration.BindingImport)]
    [PlatformEndpoint("Integration.Binding.Import")]
    public async Task<ActionResult<ApiResponse<IntegrationBatchResultDto>>> ImportExternalAccountsAsync(
        [FromBody] IntegrationExternalAccountBatchInput input)
    {
        return ApiResponse<IntegrationBatchResultDto>.Ok(await _bindingService.ImportExternalAccountsAsync(input));
    }

    [HttpGet("bindings/users")]
    [Authorize(AdminPermissions.Integration.BindingView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationUserBindingDto>>>> GetUserBindingsAsync(
        [FromQuery] IntegrationUserBindingQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationUserBindingDto>>.Ok(await _bindingService.GetUserBindingsAsync(input));
    }

    [HttpPost("bindings/users/auto-match")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationAutoMatchResultDto>>> AutoMatchUsersAsync(
        [FromBody] IntegrationAutoMatchInput input)
    {
        return ApiResponse<IntegrationAutoMatchResultDto>.Ok(await _bindingService.AutoMatchUsersAsync(input));
    }

    [HttpPost("bindings/users")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationUserBindingDto>>> BindUserAsync(
        [FromBody] IntegrationUserBindingSaveInput input)
    {
        return ApiResponse<IntegrationUserBindingDto>.Ok(await _bindingService.BindUserAsync(input));
    }

    [HttpPost("bindings/users/self")]
    [Authorize(AdminPermissions.Integration.BindingView)]
    public async Task<ActionResult<ApiResponse<IntegrationUserBindingDto>>> BindCurrentUserAsync(
        [FromBody] IntegrationSelfBindingInput input)
    {
        return ApiResponse<IntegrationUserBindingDto>.Ok(await _bindingService.BindCurrentUserAsync(input));
    }

    [HttpPost("bindings/users/{id:guid}/resolve")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationUserBindingDto>>> ResolveUserBindingAsync(
        Guid id,
        [FromBody] IntegrationBindingResolveInput input)
    {
        return ApiResponse<IntegrationUserBindingDto>.Ok(await _bindingService.ResolveUserBindingAsync(id, input));
    }

    [HttpGet("bindings/departments")]
    [Authorize(AdminPermissions.Integration.BindingView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationDepartmentBindingDto>>>> GetDepartmentBindingsAsync(
        [FromQuery] IntegrationDepartmentBindingQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationDepartmentBindingDto>>.Ok(await _bindingService.GetDepartmentBindingsAsync(input));
    }

    [HttpPost("bindings/departments")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationDepartmentBindingDto>>> SaveDepartmentBindingAsync(
        [FromBody] IntegrationDepartmentBindingSaveInput input)
    {
        return ApiResponse<IntegrationDepartmentBindingDto>.Ok(await _bindingService.SaveDepartmentBindingAsync(input));
    }

    [HttpGet("bindings/groups")]
    [Authorize(AdminPermissions.Integration.BindingView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationGroupBindingDto>>>> GetGroupBindingsAsync(
        [FromQuery] IntegrationGroupBindingQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationGroupBindingDto>>.Ok(await _bindingService.GetGroupBindingsAsync(input));
    }

    [HttpPost("bindings/groups")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationGroupBindingDto>>> SaveGroupBindingAsync(
        [FromBody] IntegrationGroupBindingSaveInput input)
    {
        return ApiResponse<IntegrationGroupBindingDto>.Ok(await _bindingService.SaveGroupBindingAsync(input));
    }

    [HttpGet("bindings/id-mappings")]
    [Authorize(AdminPermissions.Integration.BindingView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationIdMappingDto>>>> GetIdMappingsAsync(
        [FromQuery] IntegrationIdMappingQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationIdMappingDto>>.Ok(await _bindingService.GetIdMappingsAsync(input));
    }

    [HttpPost("bindings/id-mappings")]
    [Authorize(AdminPermissions.Integration.BindingManage)]
    [PlatformEndpoint("Integration.Binding.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationIdMappingDto>>> SaveIdMappingAsync(
        [FromBody] IntegrationIdMappingSaveInput input)
    {
        return ApiResponse<IntegrationIdMappingDto>.Ok(await _bindingService.SaveIdMappingAsync(input));
    }

    [HttpGet("sync-jobs")]
    [Authorize(AdminPermissions.Integration.SyncView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationSyncJobDto>>>> GetSyncJobsAsync(
        [FromQuery] IntegrationSyncJobQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationSyncJobDto>>.Ok(await _syncService.GetJobsAsync(input));
    }

    [HttpPost("sync-jobs")]
    [Authorize(AdminPermissions.Integration.SyncManage)]
    [PlatformEndpoint("Integration.Sync.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationSyncJobDto>>> SaveSyncJobAsync(
        [FromBody] IntegrationSyncJobSaveInput input)
    {
        return ApiResponse<IntegrationSyncJobDto>.Ok(await _syncService.SaveJobAsync(input));
    }

    [HttpPost("sync-jobs/{id:guid}/toggle")]
    [Authorize(AdminPermissions.Integration.SyncManage)]
    [PlatformEndpoint("Integration.Sync.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationSyncJobDto>>> ToggleSyncJobAsync(
        Guid id,
        [FromBody] IntegrationSyncJobToggleInput input)
    {
        return ApiResponse<IntegrationSyncJobDto>.Ok(await _syncService.ToggleJobAsync(id, input));
    }

    [HttpPost("sync-jobs/{id:guid}/run")]
    [Authorize(AdminPermissions.Integration.SyncRun)]
    [PlatformEndpoint("Integration.Sync.Run")]
    public async Task<ActionResult<ApiResponse<IntegrationSyncRecordDto>>> RunSyncJobAsync(
        Guid id,
        [FromBody] IntegrationSyncRunInput input)
    {
        return ApiResponse<IntegrationSyncRecordDto>.Ok(await _syncService.RunJobAsync(id, input));
    }

    [HttpGet("sync-records")]
    [Authorize(AdminPermissions.Integration.SyncView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationSyncRecordDto>>>> GetSyncRecordsAsync(
        [FromQuery] IntegrationSyncRecordQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationSyncRecordDto>>.Ok(await _syncService.GetRecordsAsync(input));
    }

    [HttpPost("sync-records/{id:guid}/retry")]
    [Authorize(AdminPermissions.Integration.SyncRun)]
    [PlatformEndpoint("Integration.Sync.Run")]
    public async Task<ActionResult<ApiResponse<IntegrationSyncRecordDto>>> RetrySyncRecordAsync(Guid id)
    {
        return ApiResponse<IntegrationSyncRecordDto>.Ok(await _syncService.RetryRecordAsync(id));
    }

    [HttpPost("sync-records/{id:guid}/skip")]
    [Authorize(AdminPermissions.Integration.SyncManage)]
    [PlatformEndpoint("Integration.Sync.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationSyncRecordDto>>> SkipSyncRecordAsync(
        Guid id,
        [FromBody] IntegrationSyncRecordSkipInput input)
    {
        return ApiResponse<IntegrationSyncRecordDto>.Ok(await _syncService.SkipRecordAsync(id, input));
    }

    [HttpGet("webhook-events")]
    [Authorize(AdminPermissions.Integration.WebhookView)]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<IntegrationWebhookEventDto>>>> GetWebhookEventsAsync(
        [FromQuery] IntegrationWebhookEventQueryInput input)
    {
        return ApiResponse<PlatformPagedResultDto<IntegrationWebhookEventDto>>.Ok(await _webhookService.GetEventsAsync(input));
    }

    [HttpPost("webhook-events/receive")]
    [Authorize(AdminPermissions.Integration.WebhookManage)]
    [PlatformEndpoint("Integration.Webhook.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationWebhookEventDto>>> ReceiveWebhookEventAsync(
        [FromBody] IntegrationWebhookReceiveInput input)
    {
        return ApiResponse<IntegrationWebhookEventDto>.Ok(await _webhookService.ReceiveAsync(input));
    }

    [AllowAnonymous]
    [HttpPost("webhooks/{connectorCode}/receive")]
    public async Task<IActionResult> ReceiveExternalWebhookAsync(string connectorCode)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();
        var headersJson = JsonSerializer.Serialize(Request.Headers.ToDictionary(
            item => item.Key,
            item => item.Value.ToString()));
        var input = new IntegrationWebhookReceiveInput
        {
            ConnectorCode = connectorCode,
            HeadersJson = headersJson,
            ProviderType = ResolveWebhookProviderType(connectorCode),
            RawBody = rawBody,
            RawPayloadJson = string.IsNullOrWhiteSpace(rawBody) ? "{}" : rawBody,
            SourceIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };
        var result = await _webhookService.ReceiveAsync(input);
        if (TryReadChallenge(rawBody, out var challenge))
        {
            return new JsonResult(new { challenge });
        }

        return Ok(ApiResponse<IntegrationWebhookEventDto>.Ok(result));
    }

    [HttpPost("webhook-events/{id:guid}/retry")]
    [Authorize(AdminPermissions.Integration.WebhookManage)]
    [PlatformEndpoint("Integration.Webhook.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationWebhookEventDto>>> RetryWebhookEventAsync(Guid id)
    {
        return ApiResponse<IntegrationWebhookEventDto>.Ok(await _webhookService.RetryAsync(id));
    }

    [HttpPost("webhook-events/{id:guid}/skip")]
    [Authorize(AdminPermissions.Integration.WebhookManage)]
    [PlatformEndpoint("Integration.Webhook.Manage")]
    public async Task<ActionResult<ApiResponse<IntegrationWebhookEventDto>>> SkipWebhookEventAsync(
        Guid id,
        [FromBody] IntegrationWebhookSkipInput input)
    {
        return ApiResponse<IntegrationWebhookEventDto>.Ok(await _webhookService.SkipAsync(id, input));
    }

    private static string ResolveWebhookProviderType(string connectorCode)
    {
        return connectorCode.Trim().ToLowerInvariant() switch
        {
            "dingtalk" => "DingTalk",
            "feishu" => "Feishu",
            "wecom" => "WeCom",
            _ => connectorCode
        };
    }

    private static bool TryReadChallenge(string rawBody, out string challenge)
    {
        challenge = string.Empty;
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(rawBody);
            if (document.RootElement.TryGetProperty("challenge", out var challengeElement) &&
                challengeElement.ValueKind == JsonValueKind.String)
            {
                challenge = challengeElement.GetString()?.Trim() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(challenge);
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }
}
