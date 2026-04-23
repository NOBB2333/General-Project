namespace General.Admin.PhaseOne;

public class PhaseOneRoleAuthorizationDto
{
    public List<string> ApiBlacklist { get; set; } = [];

    public List<Guid> AccountUserIds { get; set; } = [];

    public string AccountScopeMode { get; set; } = PhaseOneAuthorizationDefaults.AccountScopeDataAndUsers;

    public List<Guid> CustomOrganizationUnitIds { get; set; } = [];

    public string DataScopeMode { get; set; } = PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants;

    public List<Guid> MenuIds { get; set; } = [];
}

public class PhaseOneRoleAuthorizationSaveInput
{
    public List<string> ApiBlacklist { get; set; } = [];

    public List<Guid> AccountUserIds { get; set; } = [];

    public string AccountScopeMode { get; set; } = PhaseOneAuthorizationDefaults.AccountScopeDataAndUsers;

    public List<Guid> CustomOrganizationUnitIds { get; set; } = [];

    public string DataScopeMode { get; set; } = PhaseOneAuthorizationDefaults.DataScopeCurrentOrganizationAndDescendants;
}

public class PhaseOneTenantAuthorizationDto
{
    public Guid? AdminUserId { get; set; }

    public List<string> ApiBlacklist { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public List<Guid> MenuIds { get; set; } = [];

    public string? Remark { get; set; }
}

public class PhaseOneTenantAuthorizationSaveInput
{
    public Guid? AdminUserId { get; set; }

    public List<string> ApiBlacklist { get; set; } = [];

    public bool IsActive { get; set; } = true;

    public List<Guid> MenuIds { get; set; } = [];

    public string? Remark { get; set; }
}

public class PhaseOneExternalAccountMappingDto
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

public class PhaseOnePasswordChangeInput
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}

public class PhaseOneOnlineUserDto
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

public class PhaseOnePlatformFileUploadInput
{
    public string Category { get; set; } = "default";

    public string? ParentPath { get; set; }
}

public class PhaseOneUpdateLogDto
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

public class PhaseOneUpdateLogSaveInput
{
    public string? ImpactScope { get; set; }

    public DateTime PublishedAt { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}

public class PhaseOneLogDashboardDto
{
    public List<PhaseOneAuditLogItemDto> AccessLogs { get; set; } = [];

    public List<PhaseOneAuditLogItemDto> AuditLogs { get; set; } = [];

    public List<PhaseOneAuditLogItemDto> ExceptionLogs { get; set; } = [];

    public List<PhaseOneAuditLogItemDto> OperationLogs { get; set; } = [];

    public List<PhaseOneLogStatItemDto> TopApis { get; set; } = [];

    public List<PhaseOneLogStatItemDto> TopMenus { get; set; } = [];

    public List<PhaseOneLogStatItemDto> TopUsers { get; set; } = [];
}

public class PhaseOneLogStatItemDto
{
    public int Count { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
