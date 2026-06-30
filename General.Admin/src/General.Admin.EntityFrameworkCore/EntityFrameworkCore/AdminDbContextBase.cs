using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.EntityFrameworkCore;

public abstract class AdminDbContextBase<TDbContext> :
    AbpDbContext<TDbContext>,
    IIdentityDbContext,
    IPermissionManagementDbContext,
    ISettingManagementDbContext,
    ITenantManagementDbContext
    where TDbContext : DbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentityUserOrganizationUnit> UserOrganizationUnits { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; }
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; }
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; }
    public DbSet<PermissionGrant> PermissionGrants { get; set; }
    public DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }
    public DbSet<AppMenu> AppMenus { get; set; }
    public DbSet<AppRoleMenu> AppRoleMenus { get; set; }
    public DbSet<AppRoleAuthorization> AppRoleAuthorizations { get; set; }
    public DbSet<AppTenantAuthorization> AppTenantAuthorizations { get; set; }
    public DbSet<AppUserProfile> AppUserProfiles { get; set; }
    public DbSet<AppRequestAuditLog> AppRequestAuditLogs { get; set; }
    public DbSet<AppExternalAccountMapping> AppExternalAccountMappings { get; set; }
    public DbSet<AppDictData> AppDictData { get; set; }
    public DbSet<AppDictType> AppDictTypes { get; set; }
    public DbSet<AppFileStorageSource> AppFileStorageSources { get; set; }
    public DbSet<AppNotification> AppNotifications { get; set; }
    public DbSet<AppOpenApiApplication> AppOpenApiApplications { get; set; }
    public DbSet<AppPlatformFile> AppPlatformFiles { get; set; }
    public DbSet<AppScheduledJobRecord> AppScheduledJobRecords { get; set; }
    public DbSet<AppUpdateLog> AppUpdateLogs { get; set; }
    public DbSet<AppUserNotification> AppUserNotifications { get; set; }
    public DbSet<PlatformApprovalRecord> PlatformApprovalRecords { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<ProjectAcceptanceCriteria> ProjectAcceptanceCriteria { get; set; }
    public DbSet<ProjectAcceptanceRecord> ProjectAcceptanceRecords { get; set; }
    public DbSet<ProjectAcceptanceRecordCriteria> ProjectAcceptanceRecordCriteria { get; set; }
    public DbSet<ProjectAcceptanceRecordRequirement> ProjectAcceptanceRecordRequirements { get; set; }
    public DbSet<ProjectBaseline> ProjectBaselines { get; set; }
    public DbSet<ProjectChangeApproval> ProjectChangeApprovals { get; set; }
    public DbSet<ProjectChangeImpact> ProjectChangeImpacts { get; set; }
    public DbSet<ProjectChangeRequest> ProjectChangeRequests { get; set; }
    public DbSet<ProjectCharter> ProjectCharters { get; set; }
    public DbSet<ProjectSprint> ProjectSprints { get; set; }
    public DbSet<ProjectDocument> ProjectDocuments { get; set; }
    public DbSet<ProjectIssue> ProjectIssues { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<ProjectMemberRole> ProjectMemberRoles { get; set; }
    public DbSet<ProjectMeetingActionItem> ProjectMeetingActionItems { get; set; }
    public DbSet<ProjectMeetingRecord> ProjectMeetingRecords { get; set; }
    public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
    public DbSet<ProjectPerformanceSnapshot> ProjectPerformanceSnapshots { get; set; }
    public DbSet<ProjectQualityChecklist> ProjectQualityChecklists { get; set; }
    public DbSet<ProjectQualityInspection> ProjectQualityInspections { get; set; }
    public DbSet<ProjectRequirement> ProjectRequirements { get; set; }
    public DbSet<ProjectReport> ProjectReports { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<ProjectTaskDependency> ProjectTaskDependencies { get; set; }
    public DbSet<ProjectTaskParticipant> ProjectTaskParticipants { get; set; }
    public DbSet<ProjectRaidItem> ProjectRaidItems { get; set; }
    public DbSet<ProjectWbsNode> ProjectWbsNodes { get; set; }
    public DbSet<ProjectWorklog> ProjectWorklogs { get; set; }
    public DbSet<PlatformScheduledJob> PlatformScheduledJobs { get; set; }
    public DbSet<PlatformScheduledJobClusterNode> PlatformScheduledJobClusterNodes { get; set; }
    public DbSet<PlatformScheduledJobTrigger> PlatformScheduledJobTriggers { get; set; }
    public DbSet<PlatformWorkflowInstance> PlatformWorkflowInstances { get; set; }
    public DbSet<BusinessBudgetExecution> BusinessBudgetExecutions { get; set; }
    public DbSet<BusinessAcceptance> BusinessAcceptances { get; set; }
    public DbSet<BusinessChain> BusinessChains { get; set; }
    public DbSet<BusinessContract> BusinessContracts { get; set; }
    public DbSet<BusinessForecastHistory> BusinessForecastHistories { get; set; }
    public DbSet<BusinessInvoice> BusinessInvoices { get; set; }
    public DbSet<BusinessProcurement> BusinessProcurements { get; set; }
    public DbSet<BusinessReceivable> BusinessReceivables { get; set; }
    public DbSet<AiProvider> AiProviders { get; set; }
    public DbSet<AiModel> AiModels { get; set; }
    public DbSet<AiConversation> AiConversations { get; set; }
    public DbSet<AiMessage> AiMessages { get; set; }
    public DbSet<AiToolDefinition> AiToolDefinitions { get; set; }
    public DbSet<AiToolInvocation> AiToolInvocations { get; set; }
    public DbSet<AiAssistant> AiAssistants { get; set; }
    public DbSet<AiPromptTemplate> AiPromptTemplates { get; set; }
    public DbSet<AiMcpServer> AiMcpServers { get; set; }
    public DbSet<AiMcpTool> AiMcpTools { get; set; }
    public DbSet<AiMcpInvocation> AiMcpInvocations { get; set; }
    public DbSet<AiMemory> AiMemories { get; set; }
    public DbSet<AiKnowledgeSource> AiKnowledgeSources { get; set; }
    public DbSet<AiJob> AiJobs { get; set; }
    public DbSet<AiJobRun> AiJobRuns { get; set; }
    public DbSet<AiUsageRecord> AiUsageRecords { get; set; }
    public DbSet<AiWritebackRequest> AiWritebackRequests { get; set; }
    public DbSet<AiSkill> AiSkills { get; set; }
    public DbSet<AiPersona> AiPersonas { get; set; }
    public DbSet<AiCommand> AiCommands { get; set; }
    public DbSet<AiAgentDefinition> AiAgentDefinitions { get; set; }
    public DbSet<AiArtifact> AiArtifacts { get; set; }
    public DbSet<AiWebSearchConfig> AiWebSearchConfigs { get; set; }
    public DbSet<AiWebSearchRecord> AiWebSearchRecords { get; set; }
    public DbSet<IntegrationConnector> IntegrationConnectors { get; set; }
    public DbSet<IntegrationDeliveryRecord> IntegrationDeliveryRecords { get; set; }
    public DbSet<IntegrationDepartmentBinding> IntegrationDepartmentBindings { get; set; }
    public DbSet<IntegrationExternalAccount> IntegrationExternalAccounts { get; set; }
    public DbSet<IntegrationGroupBinding> IntegrationGroupBindings { get; set; }
    public DbSet<IntegrationIdMapping> IntegrationIdMappings { get; set; }
    public DbSet<IntegrationMessageTemplate> IntegrationMessageTemplates { get; set; }
    public DbSet<IntegrationNotificationRule> IntegrationNotificationRules { get; set; }
    public DbSet<IntegrationSyncJob> IntegrationSyncJobs { get; set; }
    public DbSet<IntegrationSyncRecord> IntegrationSyncRecords { get; set; }
    public DbSet<IntegrationTenantConfig> IntegrationTenantConfigs { get; set; }
    public DbSet<IntegrationUserBinding> IntegrationUserBindings { get; set; }
    public DbSet<IntegrationWebhookEvent> IntegrationWebhookEvents { get; set; }

    #endregion

    protected AdminDbContextBase(DbContextOptions<TDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<AppMenu>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}Menus", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AppCode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Path).IsRequired().HasMaxLength(256);
            b.Property(x => x.Component).HasMaxLength(256);
            b.Property(x => x.Redirect).HasMaxLength(256);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Icon).HasMaxLength(128);
            b.Property(x => x.PermissionCode).HasMaxLength(128);
            b.Property(x => x.Link).HasMaxLength(512);
            b.HasIndex(x => x.Name).IsUnique();
            b.HasIndex(x => new { x.AppCode, x.ParentId, x.Order });
            b.HasIndex(x => new { x.ParentId, x.Order });
        });

        builder.Entity<AppRoleMenu>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}RoleMenus", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.RoleId, x.MenuId }).IsUnique();
        });

        builder.Entity<AppRoleAuthorization>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}RoleAuthorizations", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.DataScopeMode).IsRequired().HasMaxLength(64);
            b.Property(x => x.AccountScopeMode).IsRequired().HasMaxLength(64);
            b.Property(x => x.CustomOrganizationUnitIds).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.AllowedUserIds).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ApiBlacklist).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => x.RoleId).IsUnique();
        });

        builder.Entity<AppTenantAuthorization>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}TenantAuthorizations", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Remark).HasMaxLength(256);
            b.Property(x => x.MenuIds).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ApiBlacklist).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => x.TenantId).IsUnique();
        });

        builder.Entity<AppUserProfile>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}UserProfiles", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EmployeeNo).HasMaxLength(64);
            b.Property(x => x.PhoneNumber).HasMaxLength(32);
            b.Property(x => x.ExternalSource).HasMaxLength(64);
            b.Property(x => x.ExternalUserId).HasMaxLength(128);
            b.Property(x => x.ForceLogoutAfter);
            b.Property(x => x.LastSeenIpAddress).HasMaxLength(64);
            b.Property(x => x.LastSeenDevice).HasMaxLength(128);
            b.Property(x => x.LastSeenBrowser).HasMaxLength(512);
            b.HasIndex(x => x.UserId).IsUnique();
            b.HasIndex(x => x.EmployeeNo).IsUnique();
        });

        builder.Entity<AppRequestAuditLog>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}RequestAuditLogs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(32);
            b.Property(x => x.HttpMethod).HasMaxLength(16);
            b.Property(x => x.Url).HasMaxLength(1024);
            b.Property(x => x.UserName).HasMaxLength(128);
            b.Property(x => x.TenantName).HasMaxLength(128);
            b.Property(x => x.HostOperatorUserName).HasMaxLength(128);
            b.Property(x => x.ImpersonatedUserName).HasMaxLength(128);
            b.Property(x => x.OperationSessionId).HasMaxLength(64);
            b.Property(x => x.ClientIpAddress).HasMaxLength(64);
            b.Property(x => x.BrowserInfo).HasMaxLength(512);
            b.Property(x => x.ActionSummary).HasMaxLength(512);
            b.Property(x => x.MenuTitle).HasMaxLength(256);
            b.Property(x => x.ExceptionMessage).HasMaxLength(1024);
            b.HasIndex(x => x.ExecutionTime);
            b.HasIndex(x => new { x.Category, x.ExecutionTime });
            b.HasIndex(x => new { x.Category, x.HttpMethod, x.ExecutionTime });
            b.HasIndex(x => new { x.HasException, x.ExecutionTime });
            b.HasIndex(x => new { x.HttpMethod, x.ExecutionTime });
            b.HasIndex(x => new { x.UserName, x.ExecutionTime });
            b.HasIndex(x => new { x.IsHostTenantOperation, x.ExecutionTime });
            b.HasIndex(x => new { x.OperationSessionId, x.ExecutionTime });
        });

        builder.Entity<AppExternalAccountMapping>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ExternalAccountMappings", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ExternalSource).IsRequired().HasMaxLength(64);
            b.Property(x => x.ExternalUserId).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(256);
            b.HasIndex(x => new { x.UserId, x.ExternalSource, x.ExternalUserId }).IsUnique();
        });

        builder.Entity<AppDictType>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}DictTypes", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Remark).HasMaxLength(256);
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.Sort);
        });

        builder.Entity<AppDictData>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}DictData", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Label).IsRequired().HasMaxLength(128);
            b.Property(x => x.Value).IsRequired().HasMaxLength(128);
            b.Property(x => x.TagColor).HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(256);
            b.HasIndex(x => new { x.DictTypeId, x.Value }).IsUnique();
            b.HasIndex(x => new { x.DictTypeId, x.IsEnabled, x.Sort });
        });

        builder.Entity<AppOpenApiApplication>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}OpenApiApplications", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AppId).IsRequired().HasMaxLength(64);
            b.Property(x => x.EncryptedSecret).IsRequired().HasMaxLength(2048);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Remark).HasMaxLength(256);
            b.Property(x => x.Scopes).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => x.AppId).IsUnique();
            b.HasIndex(x => x.IsEnabled);
        });

        builder.Entity<AppFileStorageSource>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}FileStorageSources", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ProviderName).IsRequired().HasMaxLength(32);
            b.Property(x => x.Endpoint).HasMaxLength(512);
            b.Property(x => x.RootPath).HasMaxLength(512);
            b.Property(x => x.AccessKeyId).IsRequired().HasMaxLength(256);
            b.Property(x => x.EncryptedSecret).HasMaxLength(2048);
            b.Property(x => x.BucketName).HasMaxLength(128);
            b.Property(x => x.Region).HasMaxLength(128);
            b.Property(x => x.CustomDomain).HasMaxLength(512);
            b.Property(x => x.PathTemplate).HasMaxLength(128);
            b.Property(x => x.Remark).HasMaxLength(256);
            b.HasIndex(x => x.Name).IsUnique();
            b.HasIndex(x => new { x.ProviderName, x.IsEnabled });
            b.HasIndex(x => x.IsDefault);
        });

        builder.Entity<AppPlatformFile>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformFiles", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FileKey).IsRequired().HasMaxLength(256);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(256);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(256);
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.ParentPath).HasMaxLength(256);
            b.Property(x => x.BusinessId).HasMaxLength(128);
            b.Property(x => x.BusinessType).HasMaxLength(64);
            b.Property(x => x.BucketName).HasMaxLength(128);
            b.Property(x => x.IsCurrentVersion).HasDefaultValue(true);
            b.Property(x => x.StorageLocation).IsRequired().HasMaxLength(512);
            b.Property(x => x.StorageProvider)
                .IsRequired()
                .HasMaxLength(32)
                .HasDefaultValue(PlatformFileStorageNames.Local);
            b.Property(x => x.VersionNo).HasDefaultValue(1);
            b.HasIndex(x => x.FileKey).IsUnique();
            b.HasIndex(x => x.PreviousVersionFileId);
            b.HasIndex(x => x.ReplacedByFileId);
            b.HasIndex(x => x.StorageSourceId);
            b.HasIndex(x => x.StorageProvider);
            b.HasIndex(x => new { x.BusinessType, x.BusinessId });
            b.HasIndex(x => new { x.Category, x.ParentPath });
        });

        builder.Entity<AppNotification>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}Notifications", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Content).IsRequired().HasMaxLength(1024);
            b.Property(x => x.Type).IsRequired().HasMaxLength(64);
            b.Property(x => x.Level).IsRequired().HasMaxLength(32);
            b.Property(x => x.Link).HasMaxLength(512);
            b.Property(x => x.Avatar).HasMaxLength(512);
            b.Property(x => x.RecipientMode).IsRequired().HasMaxLength(64);
            b.Property(x => x.RecipientSummary).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => new { x.Type, x.CreationTime });
            b.HasIndex(x => x.SenderUserId);
        });

        builder.Entity<AppUserNotification>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}UserNotifications", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.UserId, x.IsRemoved, x.IsRead });
            b.HasIndex(x => new { x.UserId, x.IsRemoved, x.IsRead, x.CreationTime });
            b.HasIndex(x => new { x.UserId, x.NotificationId }).IsUnique();
            b.HasIndex(x => x.NotificationId);
        });

        builder.Entity<AppScheduledJobRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ScheduledJobRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.JobKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.JobTitle).IsRequired().HasMaxLength(128);
            b.Property(x => x.TriggerKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.TriggerMode).IsRequired().HasMaxLength(16);
            b.Property(x => x.Status).IsRequired().HasMaxLength(16);
            b.Property(x => x.Result).HasMaxLength(512);
            b.Property(x => x.ErrorMessage).HasMaxLength(2048);
            b.Property(x => x.InstanceId).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.JobKey, x.StartTime });
            b.HasIndex(x => new { x.JobKey, x.TriggerKey, x.StartTime });
            b.HasIndex(x => new { x.JobKey, x.Status, x.StartTime });
        });

        builder.Entity<AppUpdateLog>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}UpdateLogs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Version).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Summary).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ImpactScope).HasMaxLength(256);
            b.HasIndex(x => x.Version).IsUnique();
            b.HasIndex(x => x.PublishedAt);
        });

        builder.Entity<ProjectEntity>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}Projects", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProjectCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ShortName).HasMaxLength(64);
            b.Property(x => x.ProjectType).HasMaxLength(64);
            b.Property(x => x.ProjectSource).HasMaxLength(64);
            b.Property(x => x.Priority).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(1024);
            b.HasIndex(x => x.ProjectCode).IsUnique();
            b.HasIndex(x => new { x.OrganizationUnitId, x.Status });
            b.HasIndex(x => new { x.ManagerUserId, x.Status });
        });

        builder.Entity<ProjectMilestone>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectMilestones", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.SprintId);
            b.HasIndex(x => new { x.ProjectId, x.PlannedCompletionDate });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
            b.HasOne<ProjectSprint>().WithMany().HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectCharter>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectCharters", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ApprovalStatus).IsRequired().HasMaxLength(32);
            b.Property(x => x.Assumptions).HasMaxLength(2048);
            b.Property(x => x.BusinessCase).IsRequired().HasMaxLength(2048);
            b.Property(x => x.Constraints).HasMaxLength(2048);
            b.Property(x => x.Objectives).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ScopeBoundary).IsRequired().HasMaxLength(2048);
            b.Property(x => x.SuccessCriteria).IsRequired().HasMaxLength(2048);
            b.HasIndex(x => x.ProjectId).IsUnique();
            b.HasIndex(x => x.ApprovalStatus);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectChangeRequest>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectChangeRequests", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ChangeType).IsRequired().HasMaxLength(32);
            b.Property(x => x.ExpectedResult).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ImpactScope).IsRequired().HasMaxLength(2048);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(2048);
            b.Property(x => x.RequestCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.RequestCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => x.ApplicantUserId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectChangeImpact>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectChangeImpacts", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Analysis).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ImpactType).IsRequired().HasMaxLength(32);
            b.Property(x => x.RelatedBusinessCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.RelatedBudgetCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.RelatedContractCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Severity).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.ProjectId, x.ProjectChangeRequestId });
            b.HasIndex(x => x.RelatedTaskId);
            b.HasIndex(x => x.RelatedMilestoneId);
            b.HasIndex(x => x.RelatedBusinessCode);
            b.HasIndex(x => x.RelatedBudgetCode);
            b.HasIndex(x => x.RelatedContractCode);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectChangeRequest>().WithMany().HasForeignKey(x => x.ProjectChangeRequestId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.RelatedTaskId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMilestone>().WithMany().HasForeignKey(x => x.RelatedMilestoneId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectChangeApproval>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectChangeApprovals", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Action).IsRequired().HasMaxLength(32);
            b.Property(x => x.Comment).HasMaxLength(1024);
            b.HasIndex(x => new { x.ProjectId, x.ProjectChangeRequestId, x.DecisionTime });
            b.HasIndex(x => x.ApproverUserId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectChangeRequest>().WithMany().HasForeignKey(x => x.ProjectChangeRequestId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectBaseline>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectBaselines", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CostBaselineSummary).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ScheduleBaselineSummary).IsRequired().HasMaxLength(2048);
            b.Property(x => x.ScopeBaselineSummary).IsRequired().HasMaxLength(2048);
            b.HasIndex(x => new { x.ProjectId, x.VersionNo }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.IsActive });
            b.HasIndex(x => x.CreatedFromChangeRequestId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectChangeRequest>().WithMany().HasForeignKey(x => x.CreatedFromChangeRequestId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectPerformanceSnapshot>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectPerformanceSnapshots", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SnapshotPeriod).IsRequired().HasMaxLength(16);
            b.HasIndex(x => new { x.ProjectId, x.SnapshotDate }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.SnapshotPeriod });
            b.HasIndex(x => x.ProjectBaselineId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectBaseline>().WithMany().HasForeignKey(x => x.ProjectBaselineId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectSprint>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectSprints", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SprintCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.SprintCode }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<ProjectTask>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectTasks", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TaskCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Priority).IsRequired().HasMaxLength(32);
            b.Property(x => x.BlockReason).HasMaxLength(512);
            b.Property(x => x.ContractClause).HasMaxLength(256);
            b.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            b.Property(x => x.ProductOwnerName).HasMaxLength(64);
            b.Property(x => x.DeveloperOwnerName).HasMaxLength(64);
            b.Property(x => x.TesterOwnerName).HasMaxLength(64);
            b.Property(x => x.WbsCode).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.SprintId);
            b.HasIndex(x => x.RequirementId);
            b.HasIndex(x => x.WbsNodeId);
            b.HasIndex(x => x.ProductOwnerUserId);
            b.HasIndex(x => x.DeveloperOwnerUserId);
            b.HasIndex(x => x.TesterOwnerUserId);
            b.HasIndex(x => x.TaskCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => new { x.ProjectId, x.ParentTaskId, x.SortOrder });
            b.HasIndex(x => new { x.ProjectId, x.WbsCode });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
            b.HasIndex(x => new { x.OrganizationUnitId, x.Status });
            b.HasOne<ProjectSprint>().WithMany().HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectRequirement>().WithMany().HasForeignKey(x => x.RequirementId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.ParentTaskId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectWbsNode>().WithMany().HasForeignKey(x => x.WbsNodeId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectTaskDependency>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectTaskDependencies", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.DependencyType).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.ProjectId, x.PredecessorTaskId, x.SuccessorTaskId }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.SuccessorTaskId });
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.PredecessorTaskId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.SuccessorTaskId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectTaskParticipant>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectTaskParticipants", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoleName).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.TaskId, x.RoleName, x.UserId }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.UserId });
            b.HasIndex(x => new { x.ProjectId, x.RoleName });
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectWbsNode>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectWbsNodes", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.WbsCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.ScopeDescription).HasMaxLength(1024);
            b.Property(x => x.DeliveryDescription).HasMaxLength(1024);
            b.Property(x => x.EstimateSummary).HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.WbsCode }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.ParentId, x.SortOrder });
            b.HasIndex(x => x.MilestoneId);
            b.HasIndex(x => x.SprintId);
            b.HasIndex(x => x.OwnerUserId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectWbsNode>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMilestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectSprint>().WithMany().HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectRequirement>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectRequirements", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AcceptanceCriteria).HasMaxLength(2048);
            b.Property(x => x.ChangeReason).HasMaxLength(1024);
            b.Property(x => x.Description).HasMaxLength(4096);
            b.Property(x => x.ImpactSummary).HasMaxLength(2048);
            b.Property(x => x.Priority).IsRequired().HasMaxLength(32);
            b.Property(x => x.RequirementCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Source).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.ProjectId, x.RequirementCode }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => new { x.ProjectId, x.WbsNodeId });
            b.HasIndex(x => x.MilestoneId);
            b.HasIndex(x => x.ReplacesRequirementId);
            b.HasIndex(x => x.BusinessOwnerUserId);
            b.HasIndex(x => x.ProductOwnerUserId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectWbsNode>().WithMany().HasForeignKey(x => x.WbsNodeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMilestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectRequirement>().WithMany().HasForeignKey(x => x.ReplacesRequirementId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectRaidItem>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectRaidItems", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Level).IsRequired().HasMaxLength(32);
            b.Property(x => x.ContingencyPlan).HasMaxLength(1024);
            b.Property(x => x.ResponseStrategy).HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.TriggerCondition).HasMaxLength(1024);
            b.HasIndex(x => new { x.ProjectId, x.Type, x.Status });
            b.HasIndex(x => new { x.ProjectId, x.RiskScore });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<ProjectIssue>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectIssues", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Level).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.RequirementTitle).HasMaxLength(256);
            b.Property(x => x.ProductOwnerName).HasMaxLength(64);
            b.Property(x => x.DeveloperOwnerName).HasMaxLength(64);
            b.Property(x => x.TesterOwnerName).HasMaxLength(64);
            b.HasIndex(x => x.AcceptanceCriteriaId);
            b.HasIndex(x => x.QualityChecklistId);
            b.HasIndex(x => x.ProductOwnerUserId);
            b.HasIndex(x => x.DeveloperOwnerUserId);
            b.HasIndex(x => x.TesterOwnerUserId);
            b.HasIndex(x => new { x.ProjectId, x.Type, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
            b.HasOne<ProjectAcceptanceCriteria>().WithMany().HasForeignKey(x => x.AcceptanceCriteriaId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectQualityChecklist>().WithMany().HasForeignKey(x => x.QualityChecklistId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectDocument>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectDocuments", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Version).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.Category });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<PlatformWorkflowInstance>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformWorkflowInstances", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BusinessCode).HasMaxLength(128);
            b.Property(x => x.BusinessId).IsRequired().HasMaxLength(64);
            b.Property(x => x.BusinessSnapshot).HasMaxLength(2048);
            b.Property(x => x.BusinessTitle).IsRequired().HasMaxLength(256);
            b.Property(x => x.BusinessType).IsRequired().HasMaxLength(64);
            b.Property(x => x.CurrentNodeName).IsRequired().HasMaxLength(64);
            b.Property(x => x.ElsaWorkflowInstanceId).HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.WorkflowDefinitionKey).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.WorkflowDefinitionKey, x.BusinessType, x.BusinessId, x.Status });
            b.HasIndex(x => x.ApplicantUserId);
            b.HasIndex(x => x.ElsaWorkflowInstanceId);
        });

        builder.Entity<PlatformApprovalRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformApprovalRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Action).IsRequired().HasMaxLength(32);
            b.Property(x => x.Comment).HasMaxLength(1024);
            b.Property(x => x.FromStatus).IsRequired().HasMaxLength(32);
            b.Property(x => x.NodeName).IsRequired().HasMaxLength(64);
            b.Property(x => x.ToStatus).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.ApprovalInstanceId);
            b.HasIndex(x => x.ActorUserId);
            b.HasOne<PlatformWorkflowInstance>().WithMany().HasForeignKey(x => x.ApprovalInstanceId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectReport>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectReports", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ReportType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.BusinessSummary).HasMaxLength(2048);
            b.Property(x => x.CompletedSummary).HasMaxLength(4096);
            b.Property(x => x.NextPlan).HasMaxLength(2048);
            b.Property(x => x.RiskIssueSummary).HasMaxLength(2048);
            b.Property(x => x.MilestoneSummary).HasMaxLength(2048);
            b.Property(x => x.HoursSummary).HasMaxLength(1024);
            b.HasIndex(x => new { x.ProjectId, x.ReportType, x.PeriodStart, x.PeriodEnd });
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => x.AuthorUserId);
            b.HasIndex(x => x.SprintId);
            b.HasIndex(x => x.AttachmentFileId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectSprint>().WithMany().HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AppPlatformFile>().WithMany().HasForeignKey(x => x.AttachmentFileId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectMeetingRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectMeetingRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Topic).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Minutes).HasMaxLength(4096);
            b.Property(x => x.Decisions).HasMaxLength(2048);
            b.Property(x => x.ParticipantUserIds).HasMaxLength(1024);
            b.HasIndex(x => new { x.ProjectId, x.MeetingTime });
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => x.FacilitatorUserId);
            b.HasIndex(x => x.AttachmentFileId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AppPlatformFile>().WithMany().HasForeignKey(x => x.AttachmentFileId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectMeetingActionItem>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectMeetingActionItems", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ActionType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.HasIndex(x => new { x.ProjectId, x.MeetingRecordId });
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => x.AssigneeUserId);
            b.HasIndex(x => x.DueDate);
            b.HasIndex(x => x.LinkedTaskId);
            b.HasIndex(x => x.LinkedRaidItemId);
            b.HasIndex(x => x.LinkedIssueId);
            b.HasIndex(x => x.LinkedRequirementId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMeetingRecord>().WithMany().HasForeignKey(x => x.MeetingRecordId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.LinkedTaskId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectRaidItem>().WithMany().HasForeignKey(x => x.LinkedRaidItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectIssue>().WithMany().HasForeignKey(x => x.LinkedIssueId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectRequirement>().WithMany().HasForeignKey(x => x.LinkedRequirementId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectMember>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectMembers", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoleNames).IsRequired().HasMaxLength(256);
            b.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
            b.HasIndex(x => new { x.UserId, x.LeaveDate });
        });

        builder.Entity<ProjectMemberRole>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectMemberRoles", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoleName).IsRequired().HasMaxLength(64);
            b.HasIndex(x => new { x.ProjectMemberId, x.RoleName }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.RoleName });
            b.HasIndex(x => new { x.ProjectId, x.UserId });
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMember>().WithMany().HasForeignKey(x => x.ProjectMemberId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectAcceptanceCriteria>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectAcceptanceCriteria", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Description).IsRequired().HasMaxLength(1024);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => x.MilestoneId);
            b.HasIndex(x => x.TaskId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMilestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectTask>().WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectAcceptanceRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectAcceptanceRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AcceptanceResult).IsRequired().HasMaxLength(32);
            b.Property(x => x.RectificationRequirement).HasMaxLength(1024);
            b.Property(x => x.Remark).HasMaxLength(1024);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.ProjectId, x.AcceptanceTime });
            b.HasIndex(x => x.AcceptanceFileId);
            b.HasIndex(x => x.MilestoneId);
            b.HasIndex(x => x.SprintId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<AppPlatformFile>().WithMany().HasForeignKey(x => x.AcceptanceFileId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMilestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectSprint>().WithMany().HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectAcceptanceRecordCriteria>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectAcceptanceRecordCriteria", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.AcceptanceRecordId, x.AcceptanceCriteriaId }).IsUnique();
            b.HasIndex(x => x.AcceptanceCriteriaId);
            b.HasIndex(x => x.ProjectId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectAcceptanceRecord>().WithMany().HasForeignKey(x => x.AcceptanceRecordId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectAcceptanceCriteria>().WithMany().HasForeignKey(x => x.AcceptanceCriteriaId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectAcceptanceRecordRequirement>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectAcceptanceRecordRequirements", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.AcceptanceRecordId, x.RequirementId }).IsUnique();
            b.HasIndex(x => x.RequirementId);
            b.HasIndex(x => x.ProjectId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectAcceptanceRecord>().WithMany().HasForeignKey(x => x.AcceptanceRecordId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectRequirement>().WithMany().HasForeignKey(x => x.RequirementId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectQualityChecklist>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectQualityChecklists", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).IsRequired().HasMaxLength(1024);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.ProjectId, x.Category });
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectQualityInspection>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectQualityInspections", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FindingSummary).HasMaxLength(1024);
            b.Property(x => x.InspectionResult).IsRequired().HasMaxLength(32);
            b.Property(x => x.RectificationRequirement).HasMaxLength(1024);
            b.HasIndex(x => new { x.ProjectId, x.InspectionTime });
            b.HasIndex(x => x.QualityChecklistId);
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectQualityChecklist>().WithMany().HasForeignKey(x => x.QualityChecklistId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProjectWorklog>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}ProjectWorklogs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Summary).IsRequired().HasMaxLength(256);
            b.HasIndex(x => new { x.ProjectId, x.WorkDate });
            b.HasIndex(x => new { x.UserId, x.WeekStartDate });
        });

        builder.Entity<PlatformScheduledJob>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformScheduledJobs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.JobKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.HandlerKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.CronExpression).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.LastRunResult).HasMaxLength(256);
            b.Property(x => x.RunningInstanceId).HasMaxLength(128);
            b.HasIndex(x => x.JobKey).IsUnique();
            b.HasIndex(x => new { x.IsEnabled, x.NextRunTime });
            b.HasIndex(x => x.LockExpirationTime);
        });

        builder.Entity<PlatformScheduledJobTrigger>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformScheduledJobTriggers", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TriggerKey).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.CronExpression).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.LastRunResult).HasMaxLength(256);
            b.HasIndex(x => new { x.JobId, x.TriggerKey }).IsUnique();
            b.HasIndex(x => new { x.IsEnabled, x.NextRunTime });
        });

        builder.Entity<PlatformScheduledJobClusterNode>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformScheduledJobClusterNodes", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.InstanceId).IsRequired().HasMaxLength(128);
            b.Property(x => x.HostName).IsRequired().HasMaxLength(128);
            b.Property(x => x.ProcessId).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(256);
            b.HasIndex(x => x.InstanceId).IsUnique();
            b.HasIndex(x => x.LastHeartbeatTime);
            b.HasIndex(x => x.Status);
        });

        builder.Entity<BusinessBudgetExecution>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessBudgetExecutions", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BudgetCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.HasIndex(x => new { x.ProjectId, x.SortOrder });
            b.HasIndex(x => x.BudgetCode).IsUnique();
        });

        builder.Entity<BusinessAcceptance>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessAcceptances", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AcceptanceCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.AcceptanceResult).IsRequired().HasMaxLength(32);
            b.Property(x => x.LinkedContractCode).HasMaxLength(64);
            b.Property(x => x.QualityInspectionCode).HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.AcceptanceCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.AcceptanceDate });
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ProjectMilestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BusinessChain>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessChains", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ChainCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.LinkedContractCode).HasMaxLength(64);
            b.Property(x => x.SourceChangeCode).HasMaxLength(64);
            b.Property(x => x.Stage).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(512);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.ChainCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.Stage, x.Status });
        });

        builder.Entity<BusinessContract>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessContracts", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContractCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.CounterpartyName).IsRequired().HasMaxLength(128);
            b.Property(x => x.ParentContractCode).HasMaxLength(64);
            b.Property(x => x.SourceChangeCode).HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.HasIndex(x => x.ContractCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.IsRevenueContract, x.SignDate });
        });

        builder.Entity<BusinessForecastHistory>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessForecastHistories", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ChangeType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Metric).IsRequired().HasMaxLength(64);
            b.Property(x => x.NewValue).IsRequired().HasMaxLength(64);
            b.Property(x => x.OldValue).IsRequired().HasMaxLength(64);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(256);
            b.Property(x => x.RelatedCode).HasMaxLength(64);
            b.HasIndex(x => new { x.ProjectId, x.Metric, x.CreationTime });
        });

        builder.Entity<BusinessInvoice>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessInvoices", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.InvoiceCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.LinkedContractCode).HasMaxLength(64);
            b.Property(x => x.ReceivableCode).HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.InvoiceCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.InvoiceDate });
            b.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BusinessProcurement>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessProcurements", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.LinkedContractCode).HasMaxLength(64);
            b.Property(x => x.ProcurementCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.SourceChangeCode).HasMaxLength(64);
            b.Property(x => x.Stage).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.SupplierName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.ProcurementCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.SignDate });
        });

        builder.Entity<BusinessReceivable>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}BusinessReceivables", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.InvoiceCode).HasMaxLength(64);
            b.Property(x => x.LinkedContractCode).HasMaxLength(64);
            b.Property(x => x.ReceivableCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.ReceivableCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.PlannedDate });
        });

        builder.Entity<AiProvider>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiProviders", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ApiKeyCipherText).HasMaxLength(2048);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Endpoint).HasMaxLength(512);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.OrganizationId).HasMaxLength(128);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            b.HasIndex(x => new { x.ProviderType, x.IsEnabled });
            b.HasIndex(x => x.IsDefault);
        });

        builder.Entity<AiModel>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiModels", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CapabilitiesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(128);
            b.Property(x => x.ModelCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.ModelType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.FallbackModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiProvider>()
                .WithMany()
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ProviderId, x.ModelCode }).IsUnique();
            b.HasIndex(x => new { x.ProviderId, x.IsDefault });
            b.HasIndex(x => new { x.ProviderId, x.IsEnabled, x.Sort });
            b.HasIndex(x => x.FallbackModelId);
            b.HasIndex(x => new { x.TenantId, x.ProviderId, x.IsEnabled });
        });

        builder.Entity<AiConversation>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiConversations", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContextId).HasMaxLength(128);
            b.Property(x => x.ContextSnapshotJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ContextType).IsRequired().HasMaxLength(32);
            b.Property(x => x.LastMessageSummary).HasMaxLength(256);
            b.Property(x => x.Mode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.CreatorId, x.Status, x.LastMessageTime });
            b.HasIndex(x => new { x.ContextType, x.ContextId });
            b.HasIndex(x => x.ModelId);
        });

        builder.Entity<AiMessage>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiMessages", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CitationsJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Content).IsRequired().HasMaxLength(16000);
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.Property(x => x.MetadataJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Role).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<AiConversation>()
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<AiProvider>()
                .WithMany()
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.ConversationId, x.SequenceNo }).IsUnique();
            b.HasIndex(x => new { x.ConversationId, x.CreationTime });
            b.HasIndex(x => new { x.Role, x.Status });
        });

        builder.Entity<AiToolDefinition>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiToolDefinitions", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.InputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.OutputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.PermissionCode).HasMaxLength(128);
            b.Property(x => x.ToolCode).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.TenantId, x.ToolCode });
            b.HasIndex(x => new { x.Category, x.IsEnabled, x.Sort });
        });

        builder.Entity<AiToolInvocation>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiToolInvocations", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.Property(x => x.InputJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.OutputJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SourceReferencesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.ToolCode).IsRequired().HasMaxLength(128);
            b.HasOne<AiConversation>()
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiMessage>()
                .WithMany()
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.ToolCode, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ConversationId, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.ToolCode, x.CreationTime });
        });

        builder.Entity<AiWritebackRequest>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiWritebackRequests", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ActionCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.BusinessModule).IsRequired().HasMaxLength(64);
            b.Property(x => x.DiffPreviewJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.Property(x => x.PayloadJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.RejectReason).HasMaxLength(512);
            b.Property(x => x.SourceType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).IsRequired().HasMaxLength(512);
            b.Property(x => x.TargetEntityId).HasMaxLength(128);
            b.Property(x => x.TargetEntityType).IsRequired().HasMaxLength(128);
            b.HasOne<AiConversation>()
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiMessage>()
                .WithMany()
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.BusinessModule, x.ActionCode, x.Status });
            b.HasIndex(x => new { x.ProjectId, x.CreationTime });
        });

        builder.Entity<AiSkill>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiSkills", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.DefaultMode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.InputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Instruction).IsRequired().HasMaxLength(16000);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.OutputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SkillCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.ToolCodesJson).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => new { x.TenantId, x.SkillCode }).IsUnique();
            b.HasIndex(x => new { x.Category, x.IsEnabled, x.Sort });
        });

        builder.Entity<AiPersona>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiPersonas", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BoundaryPrompt).HasMaxLength(8000);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.PersonaCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.PersonalityPrompt).IsRequired().HasMaxLength(8000);
            b.Property(x => x.RoleType).IsRequired().HasMaxLength(32);
            b.Property(x => x.StylePrompt).HasMaxLength(4000);
            b.HasIndex(x => new { x.TenantId, x.PersonaCode }).IsUnique();
            b.HasIndex(x => new { x.RoleType, x.IsEnabled, x.Sort });
        });

        builder.Entity<AiCommand>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiCommands", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AgentCode).HasMaxLength(128);
            b.Property(x => x.CommandCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.ExampleInput).HasMaxLength(512);
            b.Property(x => x.InputPlaceholder).HasMaxLength(256);
            b.Property(x => x.Mode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.PromptTemplateCode).HasMaxLength(128);
            b.Property(x => x.SkillCode).HasMaxLength(128);
            b.Property(x => x.SlashName).IsRequired().HasMaxLength(64);
            b.HasIndex(x => new { x.TenantId, x.CommandCode }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.SlashName });
            b.HasIndex(x => new { x.IsEnabled, x.Sort });
        });

        builder.Entity<AiAgentDefinition>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiAgentDefinitions", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AgentCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.PersonaCode).HasMaxLength(128);
            b.Property(x => x.RoleType).IsRequired().HasMaxLength(32);
            b.Property(x => x.RuntimeConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SkillCodesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SystemPrompt).IsRequired().HasMaxLength(16000);
            b.Property(x => x.ToolCodesJson).IsRequired().HasColumnType("TEXT");
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.DefaultModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.AgentCode }).IsUnique();
            b.HasIndex(x => new { x.RoleType, x.IsEnabled, x.Sort });
            b.HasIndex(x => x.DefaultModelId);
        });

        builder.Entity<AiArtifact>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiArtifacts", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ArtifactCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.ExamplePayloadJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.JsonSchema).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.RendererType).IsRequired().HasMaxLength(64);
            b.Property(x => x.SystemHint).HasMaxLength(4000);
            b.HasIndex(x => new { x.TenantId, x.ArtifactCode }).IsUnique();
            b.HasIndex(x => new { x.Category, x.RendererType, x.IsEnabled, x.Sort });
        });

        builder.Entity<AiWebSearchConfig>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiWebSearchConfigs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AllowDomainsJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ApiKeyCipherText).HasMaxLength(2048);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.DenyDomainsJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Endpoint).HasMaxLength(512);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            b.HasIndex(x => new { x.ProviderType, x.IsEnabled });
            b.HasIndex(x => x.IsDefault);
        });

        builder.Entity<AiWebSearchRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiWebSearchRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.Property(x => x.ProviderName).HasMaxLength(128);
            b.Property(x => x.Query).IsRequired().HasMaxLength(512);
            b.Property(x => x.RequestJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ResponseSummaryJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<AiWebSearchConfig>()
                .WithMany()
                .HasForeignKey(x => x.ConfigId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiConversation>()
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiMessage>()
                .WithMany()
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ConfigId, x.CreationTime });
            b.HasIndex(x => new { x.ConversationId, x.CreationTime });
        });

        builder.Entity<IntegrationConnector>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationConnectors", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CapabilitiesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConfigSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Icon).HasMaxLength(128);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.HasIndex(x => new { x.TenantId, x.ConnectorCode }).IsUnique();
            b.HasIndex(x => new { x.ProviderType, x.IsEnabled, x.Sort });
        });

        builder.Entity<IntegrationTenantConfig>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationTenantConfigs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AccessTokenCipherText).HasMaxLength(4096);
            b.Property(x => x.CallbackPath).HasMaxLength(512);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.LastTestMessage).HasMaxLength(1000);
            b.Property(x => x.LastTestStatus).IsRequired().HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.SecretCipherText).HasMaxLength(4096);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.ConnectorId }).IsUnique();
            b.HasIndex(x => new { x.ConnectorId, x.IsEnabled });
            b.HasIndex(x => new { x.LastTestStatus, x.LastTestTime });
        });

        builder.Entity<AiAssistant>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiAssistants", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AssistantCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.DefaultMode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.RoleType).IsRequired().HasMaxLength(32);
            b.Property(x => x.SystemPrompt).IsRequired().HasMaxLength(16000);
            b.Property(x => x.ToolCodesJson).IsRequired().HasColumnType("TEXT");
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.DefaultModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.AssistantCode }).IsUnique();
            b.HasIndex(x => new { x.RoleType, x.IsEnabled, x.Sort });
            b.HasIndex(x => x.DefaultModelId);
        });

        builder.Entity<AiPromptTemplate>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiPromptTemplates", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AssistantCode).HasMaxLength(64);
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.InputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Mode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.OutputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.PromptContent).IsRequired().HasMaxLength(16000);
            b.Property(x => x.Scenario).IsRequired().HasMaxLength(64);
            b.Property(x => x.TemplateCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.ToolCodesJson).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => new { x.TenantId, x.TemplateCode }).IsUnique();
            b.HasIndex(x => new { x.Category, x.IsEnabled, x.Sort });
            b.HasIndex(x => new { x.AssistantCode, x.IsEnabled, x.Sort });
        });

        builder.Entity<AiMcpServer>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiMcpServers", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ArgumentsJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Command).HasMaxLength(512);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Endpoint).HasMaxLength(512);
            b.Property(x => x.EnvironmentJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.HeadersJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.LastErrorMessage).HasMaxLength(2000);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.PermissionCode).HasMaxLength(128);
            b.Property(x => x.ProtocolVersion).IsRequired().HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.ServerCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.TransportType).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.TenantId, x.ServerCode }).IsUnique();
            b.HasIndex(x => new { x.TransportType, x.IsEnabled, x.Sort });
        });

        builder.Entity<AiMcpTool>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiMcpTools", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.InputSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.PermissionCode).HasMaxLength(128);
            b.Property(x => x.ToolName).IsRequired().HasMaxLength(128);
            b.HasOne<AiMcpServer>()
                .WithMany()
                .HasForeignKey(x => x.ServerId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ServerId, x.ToolName }).IsUnique();
            b.HasIndex(x => new { x.ServerId, x.IsEnabled, x.ToolName });
            b.HasIndex(x => new { x.TenantId, x.ToolName });
        });

        builder.Entity<AiMcpInvocation>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiMcpInvocations", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ErrorMessage).HasMaxLength(4000);
            b.Property(x => x.InputJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.OutputJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.ToolName).IsRequired().HasMaxLength(128);
            b.HasOne<AiMcpServer>()
                .WithMany()
                .HasForeignKey(x => x.ServerId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<AiMcpTool>()
                .WithMany()
                .HasForeignKey(x => x.ToolId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.ServerId, x.CreationTime });
            b.HasIndex(x => new { x.ToolId, x.CreationTime });
            b.HasIndex(x => new { x.Status, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.ToolName, x.CreationTime });
        });

        builder.Entity<AiKnowledgeSource>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiKnowledgeSources", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContentText).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ContextId).HasMaxLength(128);
            b.Property(x => x.ContextType).HasMaxLength(64);
            b.Property(x => x.MetadataJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SourceId).IsRequired().HasMaxLength(128);
            b.Property(x => x.SourceType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(4000);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.HasIndex(x => new { x.TenantId, x.SourceType, x.SourceId }).IsUnique();
            b.HasIndex(x => new { x.ContextType, x.ContextId, x.LastIndexedTime });
            b.HasIndex(x => new { x.ProjectId, x.SourceType, x.LastIndexedTime });
            b.HasIndex(x => new { x.Status, x.LastIndexedTime });
        });

        builder.Entity<AiMemory>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiMemories", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Content).IsRequired().HasMaxLength(8000);
            b.Property(x => x.ContextId).HasMaxLength(128);
            b.Property(x => x.ContextType).HasMaxLength(64);
            b.Property(x => x.MemoryType).IsRequired().HasMaxLength(32);
            b.Property(x => x.SourceReferencesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Summary).HasMaxLength(2000);
            b.Property(x => x.TagsJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.HasOne<AiKnowledgeSource>()
                .WithMany()
                .HasForeignKey(x => x.KnowledgeSourceId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.MemoryType, x.IsEnabled, x.Importance });
            b.HasIndex(x => new { x.ContextType, x.ContextId, x.IsEnabled });
            b.HasIndex(x => new { x.OwnerUserId, x.IsEnabled });
            b.HasIndex(x => x.KnowledgeSourceId);
            b.HasIndex(x => new { x.TenantId, x.MemoryType, x.CreationTime });
        });

        builder.Entity<AiJob>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiJobs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AssistantCode).HasMaxLength(64);
            b.Property(x => x.ContextScopeJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.JobCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.JobType).IsRequired().HasMaxLength(64);
            b.Property(x => x.LastRunStatus).IsRequired().HasMaxLength(32);
            b.Property(x => x.LastRunSummary).HasMaxLength(512);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.OutputTargetJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.PromptTemplateCode).HasMaxLength(128);
            b.Property(x => x.SchedulerJobKey).IsRequired().HasMaxLength(64);
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.JobCode }).IsUnique();
            b.HasIndex(x => new { x.SchedulerJobKey, x.IsEnabled });
            b.HasIndex(x => new { x.JobType, x.IsEnabled });
            b.HasIndex(x => x.ModelId);
        });

        builder.Entity<AiJobRun>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiJobRuns", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ErrorMessage).HasMaxLength(4000);
            b.Property(x => x.InputSnapshotJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.JobCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.JobName).IsRequired().HasMaxLength(128);
            b.Property(x => x.JobType).IsRequired().HasMaxLength(64);
            b.Property(x => x.OutputDraftJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.RunMode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<AiJob>()
                .WithMany()
                .HasForeignKey(x => x.AiJobId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.AiJobId, x.StartedTime });
            b.HasIndex(x => new { x.JobCode, x.StartedTime });
            b.HasIndex(x => new { x.Status, x.StartedTime });
            b.HasIndex(x => new { x.TenantId, x.JobType, x.StartedTime });
        });

        builder.Entity<AiUsageRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}AiUsageRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AuditSummary).HasMaxLength(1000);
            b.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.Property(x => x.ModelCode).HasMaxLength(128);
            b.Property(x => x.PromptInjectionSignalsJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.PromptTemplateCode).HasMaxLength(128);
            b.Property(x => x.RequestPreview).HasMaxLength(4000);
            b.Property(x => x.ResponsePreview).HasMaxLength(4000);
            b.Property(x => x.Scenario).HasMaxLength(64);
            b.Property(x => x.Source).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.ToolCode).HasMaxLength(128);
            b.HasOne<AiProvider>()
                .WithMany()
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiModel>()
                .WithMany()
                .HasForeignKey(x => x.ModelId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiConversation>()
                .WithMany()
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<AiJob>()
                .WithMany()
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.CreationTime });
            b.HasIndex(x => new { x.UserId, x.CreationTime });
            b.HasIndex(x => new { x.ProjectId, x.CreationTime });
            b.HasIndex(x => new { x.ProviderId, x.ModelId, x.CreationTime });
            b.HasIndex(x => new { x.Source, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.PromptTemplateCode, x.CreationTime });
            b.HasIndex(x => new { x.ToolCode, x.CreationTime });
            b.HasIndex(x => x.ConversationId);
            b.HasIndex(x => x.JobId);
        });

        builder.Entity<IntegrationConnector>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationConnectors", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.CapabilitiesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConfigSchemaJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Icon).HasMaxLength(128);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.HasIndex(x => new { x.TenantId, x.ConnectorCode }).IsUnique();
            b.HasIndex(x => new { x.ProviderType, x.IsEnabled, x.Sort });
        });

        builder.Entity<IntegrationTenantConfig>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationTenantConfigs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AccessTokenCipherText).HasMaxLength(4096);
            b.Property(x => x.CallbackPath).HasMaxLength(512);
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.LastTestMessage).HasMaxLength(1000);
            b.Property(x => x.LastTestStatus).IsRequired().HasMaxLength(32);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.SecretCipherText).HasMaxLength(4096);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.ConnectorId }).IsUnique();
            b.HasIndex(x => new { x.ConnectorId, x.IsEnabled });
            b.HasIndex(x => new { x.LastTestStatus, x.LastTestTime });
        });

        builder.Entity<IntegrationDeliveryRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationDeliveryRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BusinessKey).HasMaxLength(128);
            b.Property(x => x.BusinessModule).HasMaxLength(64);
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.DeadLetterReason).HasMaxLength(1000);
            b.Property(x => x.ErrorMessage).HasMaxLength(4000);
            b.Property(x => x.ExternalMessageId).HasMaxLength(256);
            b.Property(x => x.MessageType).IsRequired().HasMaxLength(32);
            b.Property(x => x.PayloadPreview).HasMaxLength(2000);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.RequestJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ResponseJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SourceType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.TargetId).IsRequired().HasMaxLength(256);
            b.Property(x => x.TargetType).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ConnectorId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ConnectorCode, x.CreationTime });
            b.HasIndex(x => new { x.TargetType, x.TargetId, x.CreationTime });
            b.HasIndex(x => new { x.NextRetryTime, x.Status });
            b.HasIndex(x => new { x.BusinessModule, x.BusinessKey, x.CreationTime });
        });

        builder.Entity<IntegrationMessageTemplate>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationMessageTemplates", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BodyTemplate).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.CardJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.MessageType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.PreviewVariablesJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.TemplateCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.TitleTemplate).HasMaxLength(256);
            b.Property(x => x.VariableSchemaJson).IsRequired().HasColumnType("TEXT");
            b.HasIndex(x => new { x.TenantId, x.TemplateCode }).IsUnique();
            b.HasIndex(x => new { x.MessageType, x.Status });
            b.HasIndex(x => new { x.Status, x.CreationTime });
        });

        builder.Entity<IntegrationNotificationRule>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationNotificationRules", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ChannelType).IsRequired().HasMaxLength(32);
            b.Property(x => x.ConditionJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConnectorCode).HasMaxLength(64);
            b.Property(x => x.EventCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.EventName).HasMaxLength(128);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ProviderType).HasMaxLength(64);
            b.Property(x => x.QuietEndTime).HasMaxLength(16);
            b.Property(x => x.QuietStartTime).HasMaxLength(16);
            b.Property(x => x.RecipientRuleJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.RuleCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.TargetType).HasMaxLength(32);
            b.HasOne<IntegrationMessageTemplate>()
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.RuleCode }).IsUnique();
            b.HasIndex(x => new { x.EventCode, x.Status });
            b.HasIndex(x => new { x.TemplateId, x.Status });
            b.HasIndex(x => new { x.ConnectorId, x.Status });
            b.HasIndex(x => new { x.Status, x.CreationTime });
        });

        builder.Entity<IntegrationWebhookEvent>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationWebhookEvents", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ActionCode).HasMaxLength(128);
            b.Property(x => x.BusinessKey).HasMaxLength(256);
            b.Property(x => x.ConnectorCode).HasMaxLength(64);
            b.Property(x => x.DedupeKey).IsRequired().HasMaxLength(256);
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            b.Property(x => x.EventId).HasMaxLength(256);
            b.Property(x => x.EventType).IsRequired().HasMaxLength(128);
            b.Property(x => x.HeadersJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.NormalizedPayloadJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.RawBody).HasColumnType("TEXT");
            b.Property(x => x.RawPayloadJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.SignatureStatus).IsRequired().HasMaxLength(32);
            b.Property(x => x.SourceIp).HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.DedupeKey }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ProviderType, x.EventType, x.CreationTime });
            b.HasIndex(x => new { x.ConnectorId, x.Status, x.CreationTime });
            b.HasIndex(x => x.EventId);
            b.HasIndex(x => x.BusinessKey);
        });

        builder.Entity<IntegrationSyncJob>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationSyncJobs", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConfigJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.CronExpression).HasMaxLength(128);
            b.Property(x => x.Direction).IsRequired().HasMaxLength(32);
            b.Property(x => x.JobCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.LastRunError).HasMaxLength(2000);
            b.Property(x => x.LastRunStatus).HasMaxLength(32);
            b.Property(x => x.LastRunSummary).HasMaxLength(2000);
            b.Property(x => x.Mode).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.SyncType).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.JobCode }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.IsEnabled, x.SyncType });
            b.HasIndex(x => new { x.ConnectorId, x.IsEnabled });
            b.HasIndex(x => new { x.LastRunStatus, x.LastRunTime });
        });

        builder.Entity<IntegrationSyncRecord>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationSyncRecords", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConflictJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ErrorMessage).HasMaxLength(4000);
            b.Property(x => x.Mode).IsRequired().HasMaxLength(32);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.RequestJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.ResultJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(2000);
            b.Property(x => x.SyncType).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationSyncJob>()
                .WithMany()
                .HasForeignKey(x => x.SyncJobId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.SyncJobId, x.CreationTime });
            b.HasIndex(x => new { x.ConnectorId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ProviderType, x.SyncType, x.CreationTime });
            b.HasIndex(x => new { x.Status, x.FinishedTime });
        });

        builder.Entity<IntegrationExternalAccount>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationExternalAccounts", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.AvatarUrl).HasMaxLength(512);
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.DepartmentExternalId).HasMaxLength(256);
            b.Property(x => x.DisplayName).HasMaxLength(128);
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.ExternalUnionId).HasMaxLength(256);
            b.Property(x => x.ExternalUserId).IsRequired().HasMaxLength(256);
            b.Property(x => x.Mobile).HasMaxLength(64);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.RawJson).IsRequired().HasColumnType("TEXT");
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ConnectorId, x.ExternalUserId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.LastSyncedTime });
            b.HasIndex(x => new { x.ConnectorCode, x.Status });
            b.HasIndex(x => x.ExternalUnionId);
            b.HasIndex(x => x.Mobile);
            b.HasIndex(x => x.Email);
        });

        builder.Entity<IntegrationUserBinding>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationUserBindings", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ConflictReason).HasMaxLength(1000);
            b.Property(x => x.ExternalDisplayName).HasMaxLength(128);
            b.Property(x => x.ExternalUserId).IsRequired().HasMaxLength(256);
            b.Property(x => x.InternalUserName).HasMaxLength(128);
            b.Property(x => x.MatchType).IsRequired().HasMaxLength(32);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<IntegrationExternalAccount>()
                .WithMany()
                .HasForeignKey(x => x.ExternalAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.ExternalAccountId).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.ConnectorId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.InternalUserId, x.ConnectorId });
            b.HasIndex(x => x.ExternalUserId);
        });

        builder.Entity<IntegrationDepartmentBinding>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationDepartmentBindings", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ConflictReason).HasMaxLength(1000);
            b.Property(x => x.ExternalDepartmentId).IsRequired().HasMaxLength(256);
            b.Property(x => x.ExternalDepartmentName).HasMaxLength(128);
            b.Property(x => x.InternalOrganizationUnitName).HasMaxLength(128);
            b.Property(x => x.MatchType).IsRequired().HasMaxLength(32);
            b.Property(x => x.ParentExternalDepartmentId).HasMaxLength(256);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ConnectorId, x.ExternalDepartmentId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.InternalOrganizationUnitId, x.ConnectorId });
            b.HasIndex(x => x.ParentExternalDepartmentId);
        });

        builder.Entity<IntegrationGroupBinding>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationGroupBindings", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ExternalGroupId).IsRequired().HasMaxLength(256);
            b.Property(x => x.ExternalGroupName).HasMaxLength(128);
            b.Property(x => x.GroupType).IsRequired().HasMaxLength(32);
            b.Property(x => x.InternalObjectId).HasMaxLength(128);
            b.Property(x => x.InternalObjectName).HasMaxLength(128);
            b.Property(x => x.InternalObjectType).HasMaxLength(64);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.RobotCode).HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ConnectorId, x.ExternalGroupId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.InternalObjectType, x.InternalObjectId, x.ConnectorId });
            b.HasIndex(x => new { x.GroupType, x.Status });
        });

        builder.Entity<IntegrationIdMapping>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}IntegrationIdMappings", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ConnectorCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ExternalObjectId).IsRequired().HasMaxLength(256);
            b.Property(x => x.ExternalObjectType).IsRequired().HasMaxLength(64);
            b.Property(x => x.InternalObjectId).IsRequired().HasMaxLength(128);
            b.Property(x => x.InternalObjectType).IsRequired().HasMaxLength(64);
            b.Property(x => x.ProviderType).IsRequired().HasMaxLength(64);
            b.Property(x => x.Remark).HasMaxLength(512);
            b.Property(x => x.Source).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasOne<IntegrationConnector>()
                .WithMany()
                .HasForeignKey(x => x.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ConnectorId, x.InternalObjectType, x.InternalObjectId, x.ExternalObjectType, x.ExternalObjectId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.CreationTime });
            b.HasIndex(x => new { x.InternalObjectType, x.InternalObjectId, x.ConnectorId });
            b.HasIndex(x => new { x.ExternalObjectType, x.ExternalObjectId, x.ConnectorId });
        });
    }
}
