namespace General.Admin.Platform;

public class PlatformRoleAuthorizationDto
{
    public List<string> ApiBlacklist { get; set; } = [];

    public List<Guid> AccountUserIds { get; set; } = [];

    public string AccountScopeMode { get; set; } = PlatformAuthorizationDefaults.AccountScopeDataAndUsers;

    public List<Guid> CustomOrganizationUnitIds { get; set; } = [];

    public string DataScopeMode { get; set; } = PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants;

    public List<Guid> MenuIds { get; set; } = [];
}

public class PlatformRoleAuthorizationSaveInput
{
    public List<string> ApiBlacklist { get; set; } = [];

    public List<Guid> AccountUserIds { get; set; } = [];

    public string AccountScopeMode { get; set; } = PlatformAuthorizationDefaults.AccountScopeDataAndUsers;

    public List<Guid> CustomOrganizationUnitIds { get; set; } = [];

    public string DataScopeMode { get; set; } = PlatformAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants;
}

public class PlatformTenantAuthorizationDto
{
    public Guid? AdminUserId { get; set; }

    public List<string> ApiBlacklist { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public List<Guid> MenuIds { get; set; } = [];

    public string? Remark { get; set; }
}

public class PlatformTenantAuthorizationSaveInput
{
    public Guid? AdminUserId { get; set; }

    public List<string> ApiBlacklist { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public List<Guid> MenuIds { get; set; } = [];

    public string? Remark { get; set; }
}

public class PlatformExternalAccountMappingDto
{
    public DateTime BoundAt { get; set; }

    public string ExternalSource { get; set; } = string.Empty;

    public string ExternalUserId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public string? Remark { get; set; }

    public string Status { get; set; } = "active";

    public Guid UserId { get; set; }
}

public class PlatformPasswordChangeInput
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}

public class PlatformOnlineUserDto
{
    public string Browser { get; set; } = string.Empty;

    public Guid? CurrentTenantId { get; set; }

    public string Device { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public DateTime? LastAccessedAt { get; set; }

    public DateTime? SignedInAt { get; set; }

    public string? TenantName { get; set; }

    public bool CanForceLogout { get; set; } = true;

    public string UserName { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}

public class PlatformFileUploadInput
{
    public string Category { get; set; } = "default";

    public string? ParentPath { get; set; }
}

public class PlatformUpdateLogDto
{
    public string AuthorName { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public string? ImpactScope { get; set; }

    public DateTime PublishedAt { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}

public class PlatformUpdateLogSaveInput
{
    public string? ImpactScope { get; set; }

    public DateTime PublishedAt { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}

public class PlatformLogDashboardDto
{
    public List<PlatformAuditLogItemDto> AccessLogs { get; set; } = [];

    public List<PlatformAuditLogItemDto> AuditLogs { get; set; } = [];

    public List<PlatformAuditLogItemDto> ExceptionLogs { get; set; } = [];

    public List<PlatformAuditLogItemDto> OperationLogs { get; set; } = [];

    public List<PlatformLogStatItemDto> TopApis { get; set; } = [];

    public List<PlatformLogStatItemDto> TopMenus { get; set; } = [];

    public List<PlatformLogStatItemDto> TopPages { get; set; } = [];

    public List<PlatformLogStatItemDto> TopUsers { get; set; } = [];
}

public class PlatformLogStatItemDto
{
    public int Count { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
