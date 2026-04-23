using Microsoft.EntityFrameworkCore;
using General.Admin.PhaseOne;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ISettingManagementDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AdminDbContext :
    AbpDbContext<AdminDbContext>,
    IIdentityDbContext,
    ISettingManagementDbContext,
    ITenantManagementDbContext
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
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }
    public DbSet<AppMenu> AppMenus { get; set; }
    public DbSet<AppRoleMenu> AppRoleMenus { get; set; }
    public DbSet<AppRoleAuthorization> AppRoleAuthorizations { get; set; }
    public DbSet<AppTenantAuthorization> AppTenantAuthorizations { get; set; }
    public DbSet<AppUserProfile> AppUserProfiles { get; set; }
    public DbSet<AppExternalAccountMapping> AppExternalAccountMappings { get; set; }
    public DbSet<AppPlatformFile> AppPlatformFiles { get; set; }
    public DbSet<AppUpdateLog> AppUpdateLogs { get; set; }
    public DbSet<PhaseOneProject> PhaseOneProjects { get; set; }
    public DbSet<PhaseOneProjectCycle> PhaseOneProjectCycles { get; set; }
    public DbSet<PhaseOneProjectDocument> PhaseOneProjectDocuments { get; set; }
    public DbSet<PhaseOneProjectIssue> PhaseOneProjectIssues { get; set; }
    public DbSet<PhaseOneProjectMember> PhaseOneProjectMembers { get; set; }
    public DbSet<PhaseOneProjectMilestone> PhaseOneProjectMilestones { get; set; }
    public DbSet<PhaseOneProjectTask> PhaseOneProjectTasks { get; set; }
    public DbSet<PhaseOneProjectRaidItem> PhaseOneProjectRaidItems { get; set; }
    public DbSet<PhaseOneProjectWorklog> PhaseOneProjectWorklogs { get; set; }
    public DbSet<PlatformScheduledJob> PlatformScheduledJobs { get; set; }
    public DbSet<PhaseOneBusinessBudgetExecution> PhaseOneBusinessBudgetExecutions { get; set; }
    public DbSet<PhaseOneBusinessChain> PhaseOneBusinessChains { get; set; }
    public DbSet<PhaseOneBusinessContract> PhaseOneBusinessContracts { get; set; }
    public DbSet<PhaseOneBusinessForecastHistory> PhaseOneBusinessForecastHistories { get; set; }
    public DbSet<PhaseOneBusinessProcurement> PhaseOneBusinessProcurements { get; set; }
    public DbSet<PhaseOneBusinessReceivable> PhaseOneBusinessReceivables { get; set; }

    #endregion

    public AdminDbContext(DbContextOptions<AdminDbContext> options)
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

        builder.Entity<AppPlatformFile>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PlatformFiles", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.FileKey).IsRequired().HasMaxLength(256);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(256);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(256);
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.Property(x => x.ParentPath).HasMaxLength(256);
            b.Property(x => x.StorageLocation).IsRequired().HasMaxLength(512);
            b.HasIndex(x => x.FileKey).IsUnique();
            b.HasIndex(x => new { x.Category, x.ParentPath });
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

        builder.Entity<PhaseOneProject>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjects", AdminConsts.DbSchema);
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

        builder.Entity<PhaseOneProjectMilestone>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectMilestones", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.ProjectId, x.PlannedCompletionDate });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<PhaseOneProjectCycle>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectCycles", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.Type, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<PhaseOneProjectTask>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectTasks", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TaskCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Priority).IsRequired().HasMaxLength(32);
            b.Property(x => x.BlockReason).HasMaxLength(512);
            b.Property(x => x.ContractClause).HasMaxLength(256);
            b.Property(x => x.ProductOwnerName).HasMaxLength(64);
            b.Property(x => x.DeveloperOwnerName).HasMaxLength(64);
            b.Property(x => x.TesterOwnerName).HasMaxLength(64);
            b.HasIndex(x => x.TaskCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
            b.HasIndex(x => new { x.OrganizationUnitId, x.Status });
        });

        builder.Entity<PhaseOneProjectRaidItem>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectRaidItems", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Level).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.HasIndex(x => new { x.ProjectId, x.Type, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<PhaseOneProjectIssue>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectIssues", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Type).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.Level).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.RequirementTitle).HasMaxLength(256);
            b.Property(x => x.ProductOwnerName).HasMaxLength(64);
            b.Property(x => x.DeveloperOwnerName).HasMaxLength(64);
            b.Property(x => x.TesterOwnerName).HasMaxLength(64);
            b.HasIndex(x => new { x.ProjectId, x.Type, x.Status });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<PhaseOneProjectDocument>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectDocuments", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Category).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.Version).IsRequired().HasMaxLength(32);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Summary).HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.Category });
            b.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        builder.Entity<PhaseOneProjectMember>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectMembers", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoleNames).IsRequired().HasMaxLength(256);
            b.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
            b.HasIndex(x => new { x.UserId, x.LeaveDate });
        });

        builder.Entity<PhaseOneProjectWorklog>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneProjectWorklogs", AdminConsts.DbSchema);
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
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.Property(x => x.CronExpression).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.LastRunResult).HasMaxLength(256);
            b.HasIndex(x => x.JobKey).IsUnique();
        });

        builder.Entity<PhaseOneBusinessBudgetExecution>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneBusinessBudgetExecutions", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.BudgetCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Category).IsRequired().HasMaxLength(64);
            b.HasIndex(x => new { x.ProjectId, x.SortOrder });
            b.HasIndex(x => x.BudgetCode).IsUnique();
        });

        builder.Entity<PhaseOneBusinessChain>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneBusinessChains", AdminConsts.DbSchema);
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

        builder.Entity<PhaseOneBusinessContract>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneBusinessContracts", AdminConsts.DbSchema);
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

        builder.Entity<PhaseOneBusinessForecastHistory>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneBusinessForecastHistories", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ChangeType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Metric).IsRequired().HasMaxLength(64);
            b.Property(x => x.NewValue).IsRequired().HasMaxLength(64);
            b.Property(x => x.OldValue).IsRequired().HasMaxLength(64);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(256);
            b.Property(x => x.RelatedCode).HasMaxLength(64);
            b.HasIndex(x => new { x.ProjectId, x.Metric, x.CreationTime });
        });

        builder.Entity<PhaseOneBusinessProcurement>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneBusinessProcurements", AdminConsts.DbSchema);
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

        builder.Entity<PhaseOneBusinessReceivable>(b =>
        {
            b.ToTable($"{AdminConsts.DbTablePrefix}PhaseOneBusinessReceivables", AdminConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.InvoiceCode).HasMaxLength(64);
            b.Property(x => x.LinkedContractCode).HasMaxLength(64);
            b.Property(x => x.ReceivableCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.Title).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.ReceivableCode).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.PlannedDate });
        });
    }
}
