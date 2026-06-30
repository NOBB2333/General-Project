using System.ComponentModel.DataAnnotations;

namespace General.Admin.Integration;

public class IntegrationDeliveryConnectorRateDto
{
    public string ConnectorCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int FailedCount { get; set; }

    public decimal SuccessRate { get; set; }

    public int SucceededCount { get; set; }

    public int TotalCount { get; set; }
}

public class IntegrationDeliveryRecordDto
{
    public int AttemptCount { get; set; }

    public string BusinessKey { get; set; } = string.Empty;

    public string BusinessModule { get; set; } = string.Empty;

    public Guid ConnectorId { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public string DeadLetterReason { get; set; } = string.Empty;

    public int DurationMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public string ExternalMessageId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public DateTime? LastAttemptTime { get; set; }

    public int MaxRetryCount { get; set; }

    public string MessageType { get; set; } = string.Empty;

    public DateTime? NextRetryTime { get; set; }

    public string PayloadPreview { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string RequestJson { get; set; } = "{}";

    public string ResponseJson { get; set; } = "{}";

    public string SourceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }
}

public class IntegrationDeliveryRecordQueryInput
{
    public string ConnectorCode { get; set; } = string.Empty;

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public string MessageType { get; set; } = string.Empty;

    public int SkipCount { get; set; }

    public string Status { get; set; } = "All";

    public string TargetType { get; set; } = string.Empty;
}

public class IntegrationDeliverySkipInput
{
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class IntegrationDeliverySummaryDto
{
    public int AverageDurationMilliseconds { get; set; }

    public IReadOnlyList<IntegrationDeliveryConnectorRateDto> ConnectorRates { get; set; } = [];

    public int DeadLetterCount { get; set; }

    public int FailedCount { get; set; }

    public int PendingCount { get; set; }

    public int SkippedCount { get; set; }

    public int SucceededCount { get; set; }

    public int TotalCount { get; set; }
}

public class IntegrationDeliveryExportResultDto
{
    public byte[] Content { get; set; } = [];

    public string ContentType { get; set; } = "text/csv";

    public string FileName { get; set; } = string.Empty;
}
