using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbpAuditLogExcelFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogExcelFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationName = table.Column<string>(type: "TEXT", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TenantName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ImpersonatorUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ImpersonatorUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ImpersonatorTenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ImpersonatorTenantName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    HttpMethod = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Exceptions = table.Column<string>(type: "TEXT", nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpBackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationName = table.Column<string>(type: "TEXT", maxLength: 96, nullable: true),
                    JobName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    JobArgs = table.Column<string>(type: "TEXT", maxLength: 1048576, nullable: false),
                    TryCount = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NextTryTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastTryTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsAbandoned = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Priority = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)15),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBackgroundJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpClaimTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Required = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsStatic = table.Column<bool>(type: "INTEGER", nullable: false),
                    Regex = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RegexDescription = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ValueType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpClaimTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatureGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatureGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ParentName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAvailableToHost = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedProviders = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatureValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatureValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpLinkUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceTenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpLinkUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpOrganizationUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 95, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpOrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnits_AbpOrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AbpOrganizationUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissionGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissionGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ResourceName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ManagementPermissionName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ParentName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MultiTenancySide = table.Column<byte>(type: "INTEGER", nullable: false),
                    Providers = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    StateCheckers = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpResourcePermissionGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ResourceName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ResourceKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpResourcePermissionGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsStatic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSecurityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ApplicationName = table.Column<string>(type: "TEXT", maxLength: 96, nullable: true),
                    Identity = table.Column<string>(type: "TEXT", maxLength: 96, nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TenantName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSecurityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Device = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DeviceInfo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IpAddresses = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    SignedIn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSettingDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "INTEGER", nullable: false),
                    Providers = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    IsInherited = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettingDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IsExternal = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ShouldChangePasswordOnNextLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    EntityVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPasswordChangeTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastSignInTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Leaved = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessBudgetExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    BudgetCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExecutedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    VarianceAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessBudgetExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessChains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChainCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Stage = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessChains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CounterpartyName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsRevenueContract = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SignDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessForecastHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Metric = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RelatedCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessForecastHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessProcurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    LinkedContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcurementCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SignDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Stage = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SupplierName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessProcurements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessReceivables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReceivedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReceivableCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessReceivables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppExternalAccountMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExternalSource = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExternalUserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExternalAccountMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppCode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Component = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Redirect = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    PermissionCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Link = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    AffixTab = table.Column<bool>(type: "INTEGER", nullable: false),
                    KeepAlive = table.Column<bool>(type: "INTEGER", nullable: false),
                    HideInBreadcrumb = table.Column<bool>(type: "INTEGER", nullable: false),
                    HideInMenu = table.Column<bool>(type: "INTEGER", nullable: false),
                    HideInTab = table.Column<bool>(type: "INTEGER", nullable: false),
                    MenuVisibleWithForbidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FileKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ParentPath = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    StorageLocation = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CronExpression = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    JobKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastRunResult = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NextRunTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeveloperOwnerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Level = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductOwnerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    RequirementTitle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TesterOwnerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AllowHistoricalRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleNames = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedCompletionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMilestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectRaidItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedResolveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectRaidItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BudgetTotalAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    ContractTotalAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    IsKeyProject = table.Column<bool>(type: "INTEGER", nullable: false),
                    ManagerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ProjectCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectSource = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ProjectType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SponsorUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActualEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualWorkHours = table.Column<double>(type: "REAL", nullable: true),
                    BlockReason = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    ContractClause = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DeveloperOwnerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    EstimatedWorkHours = table.Column<double>(type: "REAL", nullable: true),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ProductOwnerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TaskCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TesterOwnerName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectWorklogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Hours = table.Column<double>(type: "REAL", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectWorklogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRequestAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionSummary = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    BrowserInfo = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ClientIpAddress = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExecutionDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    HasException = table.Column<bool>(type: "INTEGER", nullable: false),
                    HttpMethod = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    MenuTitle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TenantName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApiBlacklist = table.Column<string>(type: "TEXT", nullable: false),
                    AccountScopeMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AllowedUserIds = table.Column<string>(type: "TEXT", nullable: false),
                    CustomOrganizationUnitIds = table.Column<string>(type: "TEXT", nullable: false),
                    DataScopeMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoleAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MenuId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoleMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTenantAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ApiBlacklist = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MenuIds = table.Column<string>(type: "TEXT", nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTenantAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUpdateLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImpactScope = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUpdateLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmployeeNo = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExternalSource = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExternalUserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastSeenBrowser = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    LastSeenDevice = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    LastSeenIpAddress = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ForceLogoutAfter = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    ClientType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayNames = table.Column<string>(type: "TEXT", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "TEXT", nullable: true),
                    Permissions = table.Column<string>(type: "TEXT", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    RedirectUris = table.Column<string>(type: "TEXT", nullable: true),
                    Requirements = table.Column<string>(type: "TEXT", nullable: true),
                    Settings = table.Column<string>(type: "TEXT", nullable: true),
                    FrontChannelLogoutUri = table.Column<string>(type: "TEXT", nullable: true),
                    ClientUri = table.Column<string>(type: "TEXT", nullable: true),
                    LogoUri = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Descriptions = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayNames = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    Resources = table.Column<string>(type: "TEXT", nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpAuditLogActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AuditLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServiceName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    MethodName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Parameters = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpAuditLogActions_AbpAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AbpAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpEntityChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChangeType = table.Column<byte>(type: "INTEGER", nullable: false),
                    EntityTenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    EntityTypeFullName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpEntityChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpEntityChanges_AbpAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AbpAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpOrganizationUnitRoles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpOrganizationUnitRoles", x => new { x.OrganizationUnitId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "AbpOrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnitRoles_AbpRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AbpRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpRoleClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClaimType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpRoleClaims_AbpRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AbpRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpTenantConnectionStrings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenantConnectionStrings", x => new { x.TenantId, x.Name });
                    table.ForeignKey(
                        name: "FK_AbpTenantConnectionStrings_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClaimType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpUserClaims_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserLogins",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 196, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserLogins", x => new { x.UserId, x.LoginProvider });
                    table.ForeignKey(
                        name: "FK_AbpUserLogins_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserOrganizationUnits",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserOrganizationUnits", x => new { x.OrganizationUnitId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "AbpOrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbpUserOrganizationUnits_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "BLOB", maxLength: 1024, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AbpUserPasskeys_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserPasswordHistories",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserPasswordHistories", x => new { x.UserId, x.Password });
                    table.ForeignKey(
                        name: "FK_AbpUserPasswordHistories_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AbpUserRoles_AbpRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AbpRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbpUserRoles_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AbpUserTokens_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    Scopes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbpEntityPropertyChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityChangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    OriginalValue = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PropertyName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PropertyTypeFullName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpEntityPropertyChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpEntityPropertyChanges_AbpEntityChanges_EntityChangeId",
                        column: x => x.EntityChangeId,
                        principalTable: "AbpEntityChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Payload = table.Column<string>(type: "TEXT", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReferenceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogActions_AuditLogId",
                table: "AbpAuditLogActions",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogActions_TenantId_ServiceName_MethodName_ExecutionTime",
                table: "AbpAuditLogActions",
                columns: new[] { "TenantId", "ServiceName", "MethodName", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogs_TenantId_ExecutionTime",
                table: "AbpAuditLogs",
                columns: new[] { "TenantId", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogs_TenantId_UserId_ExecutionTime",
                table: "AbpAuditLogs",
                columns: new[] { "TenantId", "UserId", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpBackgroundJobs_IsAbandoned_NextTryTime",
                table: "AbpBackgroundJobs",
                columns: new[] { "IsAbandoned", "NextTryTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpEntityChanges_AuditLogId",
                table: "AbpEntityChanges",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpEntityChanges_TenantId_EntityTypeFullName_EntityId",
                table: "AbpEntityChanges",
                columns: new[] { "TenantId", "EntityTypeFullName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpEntityPropertyChanges_EntityChangeId",
                table: "AbpEntityPropertyChanges",
                column: "EntityChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatureGroups_Name",
                table: "AbpFeatureGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatures_GroupName",
                table: "AbpFeatures",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatures_Name",
                table: "AbpFeatures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatureValues_Name_ProviderName_ProviderKey",
                table: "AbpFeatureValues",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpLinkUsers_SourceUserId_SourceTenantId_TargetUserId_TargetTenantId",
                table: "AbpLinkUsers",
                columns: new[] { "SourceUserId", "SourceTenantId", "TargetUserId", "TargetTenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnitRoles_RoleId_OrganizationUnitId",
                table: "AbpOrganizationUnitRoles",
                columns: new[] { "RoleId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnits_Code",
                table: "AbpOrganizationUnits",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnits_ParentId",
                table: "AbpOrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissionGrants_TenantId_Name_ProviderName_ProviderKey",
                table: "AbpPermissionGrants",
                columns: new[] { "TenantId", "Name", "ProviderName", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissionGroups_Name",
                table: "AbpPermissionGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissions_GroupName",
                table: "AbpPermissions",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissions_ResourceName_Name",
                table: "AbpPermissions",
                columns: new[] { "ResourceName", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpResourcePermissionGrants_TenantId_Name_ResourceName_ResourceKey_ProviderName_ProviderKey",
                table: "AbpResourcePermissionGrants",
                columns: new[] { "TenantId", "Name", "ResourceName", "ResourceKey", "ProviderName", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpRoleClaims_RoleId",
                table: "AbpRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpRoles_NormalizedName",
                table: "AbpRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_Action",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_ApplicationName",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "ApplicationName" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_Identity",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "Identity" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_UserId",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSessions_Device",
                table: "AbpSessions",
                column: "Device");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSessions_SessionId",
                table: "AbpSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSessions_TenantId_UserId",
                table: "AbpSessions",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettingDefinitions_Name",
                table: "AbpSettingDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettings_Name_ProviderName_ProviderKey",
                table: "AbpSettings",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenants_Name",
                table: "AbpTenants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenants_NormalizedName",
                table: "AbpTenants",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserClaims_UserId",
                table: "AbpUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserLogins_LoginProvider_ProviderKey",
                table: "AbpUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserOrganizationUnits_UserId_OrganizationUnitId",
                table: "AbpUserOrganizationUnits",
                columns: new[] { "UserId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserPasskeys_UserId",
                table: "AbpUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserRoles_RoleId_UserId",
                table: "AbpUserRoles",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_Email",
                table: "AbpUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_NormalizedEmail",
                table: "AbpUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_NormalizedUserName",
                table: "AbpUsers",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_UserName",
                table: "AbpUsers",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessBudgetExecutions_BudgetCode",
                table: "AppBusinessBudgetExecutions",
                column: "BudgetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessBudgetExecutions_ProjectId_SortOrder",
                table: "AppBusinessBudgetExecutions",
                columns: new[] { "ProjectId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessChains_ChainCode",
                table: "AppBusinessChains",
                column: "ChainCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessChains_ProjectId_Stage_Status",
                table: "AppBusinessChains",
                columns: new[] { "ProjectId", "Stage", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessContracts_ContractCode",
                table: "AppBusinessContracts",
                column: "ContractCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessContracts_ProjectId_IsRevenueContract_SignDate",
                table: "AppBusinessContracts",
                columns: new[] { "ProjectId", "IsRevenueContract", "SignDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessForecastHistories_ProjectId_Metric_CreationTime",
                table: "AppBusinessForecastHistories",
                columns: new[] { "ProjectId", "Metric", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessProcurements_ProcurementCode",
                table: "AppBusinessProcurements",
                column: "ProcurementCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessProcurements_ProjectId_SignDate",
                table: "AppBusinessProcurements",
                columns: new[] { "ProjectId", "SignDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessReceivables_ProjectId_PlannedDate",
                table: "AppBusinessReceivables",
                columns: new[] { "ProjectId", "PlannedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessReceivables_ReceivableCode",
                table: "AppBusinessReceivables",
                column: "ReceivableCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalAccountMappings_UserId_ExternalSource_ExternalUserId",
                table: "AppExternalAccountMappings",
                columns: new[] { "UserId", "ExternalSource", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_AppCode_ParentId_Order",
                table: "AppMenus",
                columns: new[] { "AppCode", "ParentId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_Name",
                table: "AppMenus",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_ParentId_Order",
                table: "AppMenus",
                columns: new[] { "ParentId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_Category_ParentPath",
                table: "AppPlatformFiles",
                columns: new[] { "Category", "ParentPath" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_FileKey",
                table: "AppPlatformFiles",
                column: "FileKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_JobKey",
                table: "AppPlatformScheduledJobs",
                column: "JobKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectCycles_OwnerUserId_Status",
                table: "AppProjectCycles",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectCycles_ProjectId_Type_Status",
                table: "AppProjectCycles",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectDocuments_OwnerUserId_Status",
                table: "AppProjectDocuments",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectDocuments_ProjectId_Category",
                table: "AppProjectDocuments",
                columns: new[] { "ProjectId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_OwnerUserId_Status",
                table: "AppProjectIssues",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_ProjectId_Type_Status",
                table: "AppProjectIssues",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMembers_ProjectId_UserId",
                table: "AppProjectMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMembers_UserId_LeaveDate",
                table: "AppProjectMembers",
                columns: new[] { "UserId", "LeaveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMilestones_OwnerUserId_Status",
                table: "AppProjectMilestones",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMilestones_ProjectId_PlannedCompletionDate",
                table: "AppProjectMilestones",
                columns: new[] { "ProjectId", "PlannedCompletionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRaidItems_OwnerUserId_Status",
                table: "AppProjectRaidItems",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRaidItems_ProjectId_Type_Status",
                table: "AppProjectRaidItems",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjects_ManagerUserId_Status",
                table: "AppProjects",
                columns: new[] { "ManagerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjects_OrganizationUnitId_Status",
                table: "AppProjects",
                columns: new[] { "OrganizationUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjects_ProjectCode",
                table: "AppProjects",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_OrganizationUnitId_Status",
                table: "AppProjectTasks",
                columns: new[] { "OrganizationUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_OwnerUserId_Status",
                table: "AppProjectTasks",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProjectId_Status",
                table: "AppProjectTasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_TaskCode",
                table: "AppProjectTasks",
                column: "TaskCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWorklogs_ProjectId_WorkDate",
                table: "AppProjectWorklogs",
                columns: new[] { "ProjectId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWorklogs_UserId_WeekStartDate",
                table: "AppProjectWorklogs",
                columns: new[] { "UserId", "WeekStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_Category_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "Category", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_ExecutionTime",
                table: "AppRequestAuditLogs",
                column: "ExecutionTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_UserName_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "UserName", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRoleAuthorizations_RoleId",
                table: "AppRoleAuthorizations",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRoleMenus_RoleId_MenuId",
                table: "AppRoleMenus",
                columns: new[] { "RoleId", "MenuId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppTenantAuthorizations_TenantId",
                table: "AppTenantAuthorizations",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUpdateLogs_PublishedAt",
                table: "AppUpdateLogs",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppUpdateLogs_Version",
                table: "AppUpdateLogs",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserProfiles_EmployeeNo",
                table: "AppUserProfiles",
                column: "EmployeeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserProfiles_UserId",
                table: "AppUserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                table: "OpenIddictApplications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScopes_Name",
                table: "OpenIddictScopes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                column: "ReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbpAuditLogActions");

            migrationBuilder.DropTable(
                name: "AbpAuditLogExcelFiles");

            migrationBuilder.DropTable(
                name: "AbpBackgroundJobs");

            migrationBuilder.DropTable(
                name: "AbpClaimTypes");

            migrationBuilder.DropTable(
                name: "AbpEntityPropertyChanges");

            migrationBuilder.DropTable(
                name: "AbpFeatureGroups");

            migrationBuilder.DropTable(
                name: "AbpFeatures");

            migrationBuilder.DropTable(
                name: "AbpFeatureValues");

            migrationBuilder.DropTable(
                name: "AbpLinkUsers");

            migrationBuilder.DropTable(
                name: "AbpOrganizationUnitRoles");

            migrationBuilder.DropTable(
                name: "AbpPermissionGrants");

            migrationBuilder.DropTable(
                name: "AbpPermissionGroups");

            migrationBuilder.DropTable(
                name: "AbpPermissions");

            migrationBuilder.DropTable(
                name: "AbpResourcePermissionGrants");

            migrationBuilder.DropTable(
                name: "AbpRoleClaims");

            migrationBuilder.DropTable(
                name: "AbpSecurityLogs");

            migrationBuilder.DropTable(
                name: "AbpSessions");

            migrationBuilder.DropTable(
                name: "AbpSettingDefinitions");

            migrationBuilder.DropTable(
                name: "AbpSettings");

            migrationBuilder.DropTable(
                name: "AbpTenantConnectionStrings");

            migrationBuilder.DropTable(
                name: "AbpUserClaims");

            migrationBuilder.DropTable(
                name: "AbpUserDelegations");

            migrationBuilder.DropTable(
                name: "AbpUserLogins");

            migrationBuilder.DropTable(
                name: "AbpUserOrganizationUnits");

            migrationBuilder.DropTable(
                name: "AbpUserPasskeys");

            migrationBuilder.DropTable(
                name: "AbpUserPasswordHistories");

            migrationBuilder.DropTable(
                name: "AbpUserRoles");

            migrationBuilder.DropTable(
                name: "AbpUserTokens");

            migrationBuilder.DropTable(
                name: "AppBusinessBudgetExecutions");

            migrationBuilder.DropTable(
                name: "AppBusinessChains");

            migrationBuilder.DropTable(
                name: "AppBusinessContracts");

            migrationBuilder.DropTable(
                name: "AppBusinessForecastHistories");

            migrationBuilder.DropTable(
                name: "AppBusinessProcurements");

            migrationBuilder.DropTable(
                name: "AppBusinessReceivables");

            migrationBuilder.DropTable(
                name: "AppExternalAccountMappings");

            migrationBuilder.DropTable(
                name: "AppMenus");

            migrationBuilder.DropTable(
                name: "AppPlatformFiles");

            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobs");

            migrationBuilder.DropTable(
                name: "AppProjectCycles");

            migrationBuilder.DropTable(
                name: "AppProjectDocuments");

            migrationBuilder.DropTable(
                name: "AppProjectIssues");

            migrationBuilder.DropTable(
                name: "AppProjectMembers");

            migrationBuilder.DropTable(
                name: "AppProjectMilestones");

            migrationBuilder.DropTable(
                name: "AppProjectRaidItems");

            migrationBuilder.DropTable(
                name: "AppProjects");

            migrationBuilder.DropTable(
                name: "AppProjectTasks");

            migrationBuilder.DropTable(
                name: "AppProjectWorklogs");

            migrationBuilder.DropTable(
                name: "AppRequestAuditLogs");

            migrationBuilder.DropTable(
                name: "AppRoleAuthorizations");

            migrationBuilder.DropTable(
                name: "AppRoleMenus");

            migrationBuilder.DropTable(
                name: "AppTenantAuthorizations");

            migrationBuilder.DropTable(
                name: "AppUpdateLogs");

            migrationBuilder.DropTable(
                name: "AppUserProfiles");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "AbpEntityChanges");

            migrationBuilder.DropTable(
                name: "AbpTenants");

            migrationBuilder.DropTable(
                name: "AbpOrganizationUnits");

            migrationBuilder.DropTable(
                name: "AbpRoles");

            migrationBuilder.DropTable(
                name: "AbpUsers");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "AbpAuditLogs");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");
        }
    }
}
