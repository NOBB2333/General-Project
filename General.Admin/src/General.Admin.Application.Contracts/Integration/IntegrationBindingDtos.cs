using System.ComponentModel.DataAnnotations;

namespace General.Admin.Integration;

public class IntegrationExternalAccountDto
{
    public string AvatarUrl { get; set; } = string.Empty;

    public Guid ConnectorId { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public string DepartmentExternalId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ExternalUnionId { get; set; } = string.Empty;

    public string ExternalUserId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public DateTime? LastSyncedTime { get; set; }

    public string Mobile { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string RawJson { get; set; } = "{}";

    public string Status { get; set; } = string.Empty;
}

public class IntegrationExternalAccountQueryInput
{
    public Guid? ConnectorId { get; set; }

    [MaxLength(64)]
    public string ConnectorCode { get; set; } = string.Empty;

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "All";
}

public class IntegrationExternalAccountSaveInput
{
    [MaxLength(512)]
    public string AvatarUrl { get; set; } = string.Empty;

    public Guid ConnectorId { get; set; }

    [MaxLength(256)]
    public string DepartmentExternalId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string ExternalUserId { get; set; } = string.Empty;

    [MaxLength(256)]
    public string ExternalUnionId { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(32_000)]
    public string RawJson { get; set; } = "{}";

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationBindingStatus.Pending;
}

public class IntegrationExternalAccountBatchInput
{
    [Required]
    public Guid ConnectorId { get; set; }

    public List<IntegrationExternalAccountSaveInput> Accounts { get; set; } = [];
}

public class IntegrationBatchResultDto
{
    public int CreatedCount { get; set; }

    public int UpdatedCount { get; set; }
}

public class IntegrationUserBindingDto
{
    public DateTime? BoundTime { get; set; }

    public Guid ConnectorId { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public string ConflictReason { get; set; } = string.Empty;

    public Guid ExternalAccountId { get; set; }

    public string ExternalDisplayName { get; set; } = string.Empty;

    public string ExternalUserId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? InternalUserId { get; set; }

    public string InternalUserName { get; set; } = string.Empty;

    public string MatchType { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class IntegrationUserBindingQueryInput
{
    public Guid? ConnectorId { get; set; }

    [MaxLength(64)]
    public string ConnectorCode { get; set; } = string.Empty;

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "All";
}

public class IntegrationUserBindingSaveInput
{
    [Required]
    public Guid ExternalAccountId { get; set; }

    [Required]
    public Guid InternalUserId { get; set; }

    [MaxLength(32)]
    public string MatchType { get; set; } = IntegrationBindingMatchTypes.Manual;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;
}

public class IntegrationSelfBindingInput
{
    [Required]
    public Guid ExternalAccountId { get; set; }

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;
}

public class IntegrationBindingResolveInput
{
    public Guid? InternalUserId { get; set; }

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationBindingStatus.Bound;
}

public class IntegrationAutoMatchInput
{
    [Required]
    public Guid ConnectorId { get; set; }
}

public class IntegrationAutoMatchResultDto
{
    public int BoundCount { get; set; }

    public int ConflictCount { get; set; }

    public int PendingCount { get; set; }

    public int ScannedCount { get; set; }
}

public class IntegrationDepartmentBindingDto
{
    public Guid ConnectorId { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public string ConflictReason { get; set; } = string.Empty;

    public string ExternalDepartmentId { get; set; } = string.Empty;

    public string ExternalDepartmentName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? InternalOrganizationUnitId { get; set; }

    public string InternalOrganizationUnitName { get; set; } = string.Empty;

    public string MatchType { get; set; } = string.Empty;

    public string ParentExternalDepartmentId { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class IntegrationDepartmentBindingQueryInput
{
    public Guid? ConnectorId { get; set; }

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "All";
}

public class IntegrationDepartmentBindingSaveInput
{
    public Guid ConnectorId { get; set; }

    [Required]
    [MaxLength(256)]
    public string ExternalDepartmentId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string ExternalDepartmentName { get; set; } = string.Empty;

    public Guid? InternalOrganizationUnitId { get; set; }

    [MaxLength(32)]
    public string MatchType { get; set; } = IntegrationBindingMatchTypes.Manual;

    [MaxLength(256)]
    public string ParentExternalDepartmentId { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationBindingStatus.Bound;
}

public class IntegrationGroupBindingDto
{
    public Guid ConnectorId { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public string ExternalGroupId { get; set; } = string.Empty;

    public string ExternalGroupName { get; set; } = string.Empty;

    public string GroupType { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InternalObjectId { get; set; } = string.Empty;

    public string InternalObjectName { get; set; } = string.Empty;

    public string InternalObjectType { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public string RobotCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class IntegrationGroupBindingQueryInput
{
    public Guid? ConnectorId { get; set; }

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "All";
}

public class IntegrationGroupBindingSaveInput
{
    public Guid ConnectorId { get; set; }

    [Required]
    [MaxLength(256)]
    public string ExternalGroupId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string ExternalGroupName { get; set; } = string.Empty;

    [MaxLength(32)]
    public string GroupType { get; set; } = "Chat";

    [MaxLength(128)]
    public string InternalObjectId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string InternalObjectName { get; set; } = string.Empty;

    [MaxLength(64)]
    public string InternalObjectType { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(128)]
    public string RobotCode { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationBindingStatus.Bound;
}

public class IntegrationIdMappingDto
{
    public Guid ConnectorId { get; set; }

    public string ConnectorCode { get; set; } = string.Empty;

    public string ExternalObjectId { get; set; } = string.Empty;

    public string ExternalObjectType { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InternalObjectId { get; set; } = string.Empty;

    public string InternalObjectType { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class IntegrationIdMappingQueryInput
{
    public Guid? ConnectorId { get; set; }

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "All";
}

public class IntegrationIdMappingSaveInput
{
    public Guid ConnectorId { get; set; }

    [Required]
    [MaxLength(256)]
    public string ExternalObjectId { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string ExternalObjectType { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string InternalObjectId { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string InternalObjectType { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Source { get; set; } = "Manual";

    [MaxLength(32)]
    public string Status { get; set; } = IntegrationBindingStatus.Bound;
}
