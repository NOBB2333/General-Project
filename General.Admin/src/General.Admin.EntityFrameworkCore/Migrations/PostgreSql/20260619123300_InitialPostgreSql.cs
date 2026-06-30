using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbpAuditLogExcelFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogExcelFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationName = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ImpersonatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImpersonatorUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ImpersonatorTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImpersonatorTenantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "integer", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ClientName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Url = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Exceptions = table.Column<string>(type: "text", nullable: true),
                    Comments = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpBackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationName = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                    JobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobArgs = table.Column<string>(type: "character varying(1048576)", maxLength: 1048576, nullable: false),
                    TryCount = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextTryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastTryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAbandoned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Priority = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)15),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBackgroundJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpClaimTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                    Regex = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RegexDescription = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpClaimTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatureGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatureGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ParentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvailableToHost = table.Column<bool>(type: "boolean", nullable: false),
                    AllowedProviders = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatureValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatureValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpLinkUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpLinkUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpOrganizationUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(95)", maxLength: 95, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityVersion = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissionGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ManagementPermissionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ParentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MultiTenancySide = table.Column<byte>(type: "smallint", nullable: false),
                    Providers = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    StateCheckers = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpResourcePermissionGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ResourceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ResourceKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpResourcePermissionGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsStatic = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    EntityVersion = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSecurityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApplicationName = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                    Identity = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                    Action = table.Column<string>(type: "character varying(96)", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSecurityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Device = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeviceInfo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IpAddresses = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SignedIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSettingDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "boolean", nullable: false),
                    Providers = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsInherited = table.Column<bool>(type: "boolean", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettingDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityVersion = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Surname = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsExternal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShouldChangePasswordOnNextLogin = table.Column<bool>(type: "boolean", nullable: false),
                    EntityVersion = table.Column<int>(type: "integer", nullable: false),
                    LastPasswordChangeTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSignInTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Leaved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessBudgetExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BudgetCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExecutedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    VarianceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessBudgetExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessChains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChainCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Stage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessChains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CounterpartyName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsRevenueContract = table.Column<bool>(type: "boolean", nullable: false),
                    ParentContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessForecastHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Metric = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NewValue = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OldValue = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RelatedCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessForecastHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessProcurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    LinkedContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SignDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Stage = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessProcurements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessReceivables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivableCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessReceivables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDictData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DictTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TagColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDictData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDictTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDictTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppExternalAccountMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExternalSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExternalAccountMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppFileStorageSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessKeyId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BucketName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CustomDomain = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EncryptedSecret = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PathTemplate = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RootPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFileStorageSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Component = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Redirect = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PermissionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Link = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    AffixTab = table.Column<bool>(type: "boolean", nullable: false),
                    KeepAlive = table.Column<bool>(type: "boolean", nullable: false),
                    HideInBreadcrumb = table.Column<bool>(type: "boolean", nullable: false),
                    HideInMenu = table.Column<bool>(type: "boolean", nullable: false),
                    HideInTab = table.Column<bool>(type: "boolean", nullable: false),
                    MenuVisibleWithForbidden = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Avatar = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Link = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RecipientMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RecipientSummary = table.Column<string>(type: "TEXT", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppOpenApiApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EncryptedSecret = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Scopes = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOpenApiApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FileKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ParentPath = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    BusinessId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BusinessType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    BucketName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PreviousVersionFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReplacedByFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    StorageLocation = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Local"),
                    StorageSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VersionNo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformScheduledJobClusterNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HostName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InstanceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastHeartbeatTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformScheduledJobClusterNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformScheduledJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    HandlerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    JobKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunResult = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LockExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RunningInstanceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformScheduledJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformScheduledJobTriggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunResult = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NextRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TriggerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformScheduledJobTriggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformWorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BusinessCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BusinessId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BusinessSnapshot = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    BusinessTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BusinessType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CompletedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentNodeName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ElsaWorkflowInstanceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WorkflowDefinitionVersion = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformWorkflowInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowHistoricalRead = table.Column<bool>(type: "boolean", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleNames = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectRaidItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Impact = table.Column<int>(type: "integer", nullable: false),
                    PlannedResolveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Probability = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContingencyPlan = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ResponseStrategy = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RiskScore = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TriggerCondition = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectRaidItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetTotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    ContractTotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsKeyProject = table.Column<bool>(type: "boolean", nullable: false),
                    ManagerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProjectCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProjectSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    ShortName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SponsorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectSprints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SprintCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectSprints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectWorklogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Hours = table.Column<double>(type: "double precision", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Summary = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectWorklogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRequestAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionSummary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BrowserInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ClientIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExecutionDuration = table.Column<int>(type: "integer", nullable: false),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExceptionMessage = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    HasException = table.Column<bool>(type: "boolean", nullable: false),
                    HostOperatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HostOperatorUserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ImpersonatedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImpersonatedUserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: true),
                    IsHostTenantOperation = table.Column<bool>(type: "boolean", nullable: false),
                    MenuTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OperationSessionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OperationTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    UserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRequestAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiBlacklist = table.Column<string>(type: "TEXT", nullable: false),
                    AccountScopeMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AllowedUserIds = table.Column<string>(type: "TEXT", nullable: false),
                    CustomOrganizationUnitIds = table.Column<string>(type: "TEXT", nullable: false),
                    DataScopeMode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoleAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MenuId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoleMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppScheduledJobRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: true),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    InstanceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Result = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TriggerKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TriggerMode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppScheduledJobRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTenantAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApiBlacklist = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MenuIds = table.Column<string>(type: "TEXT", nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTenantAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUpdateLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImpactScope = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Summary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUpdateLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExternalSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ExternalUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSeenBrowser = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    LastSeenDevice = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastSeenIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ForceLogoutAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    ClientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true),
                    FrontChannelLogoutUri = table.Column<string>(type: "text", nullable: true),
                    ClientUri = table.Column<string>(type: "text", nullable: true),
                    LogoUri = table.Column<string>(type: "text", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Descriptions = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Resources = table.Column<string>(type: "text", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpAuditLogActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MethodName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Parameters = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeType = table.Column<byte>(type: "smallint", nullable: false),
                    EntityTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    EntityTypeFullName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: true)
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
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpOrganizationUnitRoles", x => new { x.OrganizationUnitId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationU~",
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderKey = table.Column<string>(type: "character varying(196)", maxLength: 196, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserOrganizationUnits", x => new { x.OrganizationUnitId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationU~",
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
                    CredentialId = table.Column<byte[]>(type: "bytea", maxLength: 1024, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: true)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
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
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
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
                name: "AppPlatformApprovalRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NodeName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ApprovalInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPlatformApprovalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppPlatformApprovalRecords_AppPlatformWorkflowInstances_App~",
                        column: x => x.ApprovalInstanceId,
                        principalTable: "AppPlatformWorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    InvoiceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LinkedContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppBusinessInvoices_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectChangeRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpectedResult = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ImpactScope = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    RequestCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeRequests_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectCharters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Assumptions = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    BusinessCase = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Constraints = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Objectives = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeBoundary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SuccessCriteria = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectCharters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectCharters_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMeetingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Decisions = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FacilitatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Minutes = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    MeetingTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParticipantUserIds = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Topic = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMeetingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingRecords_AppPlatformFiles_AttachmentFileId",
                        column: x => x.AttachmentFileId,
                        principalTable: "AppPlatformFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingRecords_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMemberRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMemberRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectMemberRoles_AppProjectMembers_ProjectMemberId",
                        column: x => x.ProjectMemberId,
                        principalTable: "AppProjectMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMemberRoles_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectQualityChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectQualityChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectQualityChecklists_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SprintId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectMilestones_AppProjectSprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "AppProjectSprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CompletedSummary = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    SprintId = table.Column<Guid>(type: "uuid", nullable: true),
                    HoursSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    MilestoneSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    NextPlan = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RiskIssueSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectReports_AppPlatformFiles_AttachmentFileId",
                        column: x => x.AttachmentFileId,
                        principalTable: "AppPlatformFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectReports_AppProjectSprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "AppProjectSprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectReports_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbpEntityPropertyChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityChangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    OriginalValue = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    PropertyName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PropertyTypeFullName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
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
                name: "AppProjectBaselines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetAtCompletion = table.Column<decimal>(type: "numeric", nullable: false),
                    CostBaselineSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedFromChangeRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleBaselineSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ScopeBaselineSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectBaselines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectBaselines_AppProjectChangeRequests_CreatedFromCha~",
                        column: x => x.CreatedFromChangeRequestId,
                        principalTable: "AppProjectChangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectBaselines_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectChangeApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DecisionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectChangeRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectChangeApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeApprovals_AppProjectChangeRequests_ProjectC~",
                        column: x => x.ProjectChangeRequestId,
                        principalTable: "AppProjectChangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeApprovals_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectQualityInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FindingSummary = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    InspectionResult = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InspectionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InspectorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualityChecklistId = table.Column<Guid>(type: "uuid", nullable: false),
                    RectificationRequirement = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectQualityInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectQualityInspections_AppProjectQualityChecklists_Qu~",
                        column: x => x.QualityChecklistId,
                        principalTable: "AppProjectQualityChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectQualityInspections_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppBusinessAcceptances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptanceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AcceptanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptanceResult = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualityInspectionCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBusinessAcceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppBusinessAcceptances_AppProjectMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "AppProjectMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppBusinessAcceptances_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectAcceptanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptanceFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptanceResult = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AcceptanceTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RectificationRequirement = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SprintId = table.Column<Guid>(type: "uuid", nullable: true),
                    Remark = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectAcceptanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecords_AppPlatformFiles_AcceptanceFile~",
                        column: x => x.AcceptanceFileId,
                        principalTable: "AppPlatformFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecords_AppProjectMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "AppProjectMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecords_AppProjectSprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "AppProjectSprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecords_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectWbsNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SprintId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveryDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    EstimateSummary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeDescription = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WbsCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectWbsNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectWbsNodes_AppProjectMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "AppProjectMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectWbsNodes_AppProjectSprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "AppProjectSprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectWbsNodes_AppProjectWbsNodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AppProjectWbsNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectWbsNodes_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "AppProjectPerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric", nullable: false),
                    CostPerformanceIndex = table.Column<decimal>(type: "numeric", nullable: false),
                    EarnedValue = table.Column<decimal>(type: "numeric", nullable: false),
                    PlannedValue = table.Column<decimal>(type: "numeric", nullable: false),
                    ProjectBaselineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchedulePerformanceIndex = table.Column<decimal>(type: "numeric", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SnapshotPeriod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectPerformanceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectPerformanceSnapshots_AppProjectBaselines_ProjectB~",
                        column: x => x.ProjectBaselineId,
                        principalTable: "AppProjectBaselines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectPerformanceSnapshots_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptanceCriteria = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    BusinessOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    ImpactSummary = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProductOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplacesRequirementId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SizeEstimate = table.Column<double>(type: "double precision", nullable: true),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    WbsNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectRequirements_AppProjectMilestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "AppProjectMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectRequirements_AppProjectRequirements_ReplacesRequi~",
                        column: x => x.ReplacesRequirementId,
                        principalTable: "AppProjectRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectRequirements_AppProjectWbsNodes_WbsNodeId",
                        column: x => x.WbsNodeId,
                        principalTable: "AppProjectWbsNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectRequirements_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectAcceptanceRecordRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectAcceptanceRecordRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecordRequirements_AppProjectAcceptance~",
                        column: x => x.AcceptanceRecordId,
                        principalTable: "AppProjectAcceptanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecordRequirements_AppProjectRequiremen~",
                        column: x => x.RequirementId,
                        principalTable: "AppProjectRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecordRequirements_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualWorkHours = table.Column<double>(type: "double precision", nullable: true),
                    BlockReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ContractClause = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SprintId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeveloperOwnerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeveloperOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EstimatedWorkHours = table.Column<double>(type: "double precision", nullable: true),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsOutOfContract = table.Column<bool>(type: "boolean", nullable: false),
                    BaselineEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BaselineStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlannedEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProductOwnerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProductOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TaskCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TesterOwnerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TesterOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    WbsCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WbsNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectTasks_AppProjectRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "AppProjectRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectTasks_AppProjectSprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "AppProjectSprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectTasks_AppProjectTasks_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectTasks_AppProjectWbsNodes_WbsNodeId",
                        column: x => x.WbsNodeId,
                        principalTable: "AppProjectWbsNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectAcceptanceCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectAcceptanceCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceCriteria_AppProjectMilestones_Milestone~",
                        column: x => x.MilestoneId,
                        principalTable: "AppProjectMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceCriteria_AppProjectTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceCriteria_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectChangeImpacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Analysis = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProjectChangeRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelatedBusinessCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedBudgetCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedContractCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RelatedMilestoneId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstimatedCostDelta = table.Column<decimal>(type: "numeric", nullable: true),
                    EstimatedScheduleDeltaDays = table.Column<int>(type: "integer", nullable: true),
                    ImpactType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectChangeImpacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeImpacts_AppProjectChangeRequests_ProjectCha~",
                        column: x => x.ProjectChangeRequestId,
                        principalTable: "AppProjectChangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeImpacts_AppProjectMilestones_RelatedMilesto~",
                        column: x => x.RelatedMilestoneId,
                        principalTable: "AppProjectMilestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeImpacts_AppProjectTasks_RelatedTaskId",
                        column: x => x.RelatedTaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectChangeImpacts_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectTaskDependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DependencyType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PredecessorTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuccessorTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectTaskDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectTaskDependencies_AppProjectTasks_PredecessorTaskId",
                        column: x => x.PredecessorTaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectTaskDependencies_AppProjectTasks_SuccessorTaskId",
                        column: x => x.SuccessorTaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectTaskDependencies_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectTaskParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectTaskParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectTaskParticipants_AppProjectTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectTaskParticipants_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectAcceptanceRecordCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptanceCriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectAcceptanceRecordCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecordCriteria_AppProjectAcceptanceCrit~",
                        column: x => x.AcceptanceCriteriaId,
                        principalTable: "AppProjectAcceptanceCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecordCriteria_AppProjectAcceptanceReco~",
                        column: x => x.AcceptanceRecordId,
                        principalTable: "AppProjectAcceptanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectAcceptanceRecordCriteria_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeveloperOwnerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeveloperOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptanceCriteriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductOwnerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ProductOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    QualityChecklistId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequirementTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TesterOwnerName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TesterOwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectIssues_AppProjectAcceptanceCriteria_AcceptanceCri~",
                        column: x => x.AcceptanceCriteriaId,
                        principalTable: "AppProjectAcceptanceCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectIssues_AppProjectQualityChecklists_QualityCheckli~",
                        column: x => x.QualityChecklistId,
                        principalTable: "AppProjectQualityChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMeetingActionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssigneeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LinkedIssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedRaidItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedRequirementId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedTaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    MeetingRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProjectMeetingActionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingActionItems_AppProjectIssues_LinkedIssueId",
                        column: x => x.LinkedIssueId,
                        principalTable: "AppProjectIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingActionItems_AppProjectMeetingRecords_Meeti~",
                        column: x => x.MeetingRecordId,
                        principalTable: "AppProjectMeetingRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingActionItems_AppProjectRaidItems_LinkedRaid~",
                        column: x => x.LinkedRaidItemId,
                        principalTable: "AppProjectRaidItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingActionItems_AppProjectRequirements_LinkedR~",
                        column: x => x.LinkedRequirementId,
                        principalTable: "AppProjectRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingActionItems_AppProjectTasks_LinkedTaskId",
                        column: x => x.LinkedTaskId,
                        principalTable: "AppProjectTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppProjectMeetingActionItems_AppProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "AppProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogActions_AuditLogId",
                table: "AbpAuditLogActions",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogActions_TenantId_ServiceName_MethodName_Executio~",
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
                name: "IX_AbpLinkUsers_SourceUserId_SourceTenantId_TargetUserId_Targe~",
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
                name: "IX_AbpResourcePermissionGrants_TenantId_Name_ResourceName_Reso~",
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
                name: "IX_AppBusinessAcceptances_AcceptanceCode",
                table: "AppBusinessAcceptances",
                column: "AcceptanceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessAcceptances_MilestoneId",
                table: "AppBusinessAcceptances",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessAcceptances_ProjectId_AcceptanceDate",
                table: "AppBusinessAcceptances",
                columns: new[] { "ProjectId", "AcceptanceDate" });

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
                name: "IX_AppBusinessInvoices_InvoiceCode",
                table: "AppBusinessInvoices",
                column: "InvoiceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppBusinessInvoices_ProjectId_InvoiceDate",
                table: "AppBusinessInvoices",
                columns: new[] { "ProjectId", "InvoiceDate" });

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
                name: "IX_AppDictData_DictTypeId_IsEnabled_Sort",
                table: "AppDictData",
                columns: new[] { "DictTypeId", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDictData_DictTypeId_Value",
                table: "AppDictData",
                columns: new[] { "DictTypeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDictTypes_Code",
                table: "AppDictTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDictTypes_Sort",
                table: "AppDictTypes",
                column: "Sort");

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalAccountMappings_UserId_ExternalSource_ExternalUs~",
                table: "AppExternalAccountMappings",
                columns: new[] { "UserId", "ExternalSource", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppFileStorageSources_IsDefault",
                table: "AppFileStorageSources",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_AppFileStorageSources_Name",
                table: "AppFileStorageSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppFileStorageSources_ProviderName_IsEnabled",
                table: "AppFileStorageSources",
                columns: new[] { "ProviderName", "IsEnabled" });

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
                name: "IX_AppNotifications_SenderUserId",
                table: "AppNotifications",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_Type_CreationTime",
                table: "AppNotifications",
                columns: new[] { "Type", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiApplications_AppId",
                table: "AppOpenApiApplications",
                column: "AppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiApplications_IsEnabled",
                table: "AppOpenApiApplications",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformApprovalRecords_ActorUserId",
                table: "AppPlatformApprovalRecords",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformApprovalRecords_ApprovalInstanceId",
                table: "AppPlatformApprovalRecords",
                column: "ApprovalInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_BusinessType_BusinessId",
                table: "AppPlatformFiles",
                columns: new[] { "BusinessType", "BusinessId" });

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
                name: "IX_AppPlatformFiles_PreviousVersionFileId",
                table: "AppPlatformFiles",
                column: "PreviousVersionFileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_ReplacedByFileId",
                table: "AppPlatformFiles",
                column: "ReplacedByFileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_StorageProvider",
                table: "AppPlatformFiles",
                column: "StorageProvider");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_StorageSourceId",
                table: "AppPlatformFiles",
                column: "StorageSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobClusterNodes_InstanceId",
                table: "AppPlatformScheduledJobClusterNodes",
                column: "InstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobClusterNodes_LastHeartbeatTime",
                table: "AppPlatformScheduledJobClusterNodes",
                column: "LastHeartbeatTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobClusterNodes_Status",
                table: "AppPlatformScheduledJobClusterNodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_IsEnabled_NextRunTime",
                table: "AppPlatformScheduledJobs",
                columns: new[] { "IsEnabled", "NextRunTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_JobKey",
                table: "AppPlatformScheduledJobs",
                column: "JobKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_LockExpirationTime",
                table: "AppPlatformScheduledJobs",
                column: "LockExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobTriggers_IsEnabled_NextRunTime",
                table: "AppPlatformScheduledJobTriggers",
                columns: new[] { "IsEnabled", "NextRunTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobTriggers_JobId_TriggerKey",
                table: "AppPlatformScheduledJobTriggers",
                columns: new[] { "JobId", "TriggerKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformWorkflowInstances_ApplicantUserId",
                table: "AppPlatformWorkflowInstances",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformWorkflowInstances_ElsaWorkflowInstanceId",
                table: "AppPlatformWorkflowInstances",
                column: "ElsaWorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformWorkflowInstances_WorkflowDefinitionKey_Business~",
                table: "AppPlatformWorkflowInstances",
                columns: new[] { "WorkflowDefinitionKey", "BusinessType", "BusinessId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceCriteria_MilestoneId",
                table: "AppProjectAcceptanceCriteria",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceCriteria_ProjectId_Status",
                table: "AppProjectAcceptanceCriteria",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceCriteria_TaskId",
                table: "AppProjectAcceptanceCriteria",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecordCriteria_AcceptanceCriteriaId",
                table: "AppProjectAcceptanceRecordCriteria",
                column: "AcceptanceCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecordCriteria_AcceptanceRecordId_Accep~",
                table: "AppProjectAcceptanceRecordCriteria",
                columns: new[] { "AcceptanceRecordId", "AcceptanceCriteriaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecordCriteria_ProjectId",
                table: "AppProjectAcceptanceRecordCriteria",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecordRequirements_AcceptanceRecordId_R~",
                table: "AppProjectAcceptanceRecordRequirements",
                columns: new[] { "AcceptanceRecordId", "RequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecordRequirements_ProjectId",
                table: "AppProjectAcceptanceRecordRequirements",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecordRequirements_RequirementId",
                table: "AppProjectAcceptanceRecordRequirements",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecords_AcceptanceFileId",
                table: "AppProjectAcceptanceRecords",
                column: "AcceptanceFileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecords_MilestoneId",
                table: "AppProjectAcceptanceRecords",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecords_ProjectId_AcceptanceTime",
                table: "AppProjectAcceptanceRecords",
                columns: new[] { "ProjectId", "AcceptanceTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectAcceptanceRecords_SprintId",
                table: "AppProjectAcceptanceRecords",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectBaselines_CreatedFromChangeRequestId",
                table: "AppProjectBaselines",
                column: "CreatedFromChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectBaselines_ProjectId_IsActive",
                table: "AppProjectBaselines",
                columns: new[] { "ProjectId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectBaselines_ProjectId_VersionNo",
                table: "AppProjectBaselines",
                columns: new[] { "ProjectId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeApprovals_ApproverUserId",
                table: "AppProjectChangeApprovals",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeApprovals_ProjectChangeRequestId",
                table: "AppProjectChangeApprovals",
                column: "ProjectChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeApprovals_ProjectId_ProjectChangeRequestId_~",
                table: "AppProjectChangeApprovals",
                columns: new[] { "ProjectId", "ProjectChangeRequestId", "DecisionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_ProjectChangeRequestId",
                table: "AppProjectChangeImpacts",
                column: "ProjectChangeRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_ProjectId_ProjectChangeRequestId",
                table: "AppProjectChangeImpacts",
                columns: new[] { "ProjectId", "ProjectChangeRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_RelatedBudgetCode",
                table: "AppProjectChangeImpacts",
                column: "RelatedBudgetCode");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_RelatedBusinessCode",
                table: "AppProjectChangeImpacts",
                column: "RelatedBusinessCode");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_RelatedContractCode",
                table: "AppProjectChangeImpacts",
                column: "RelatedContractCode");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_RelatedMilestoneId",
                table: "AppProjectChangeImpacts",
                column: "RelatedMilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeImpacts_RelatedTaskId",
                table: "AppProjectChangeImpacts",
                column: "RelatedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeRequests_ApplicantUserId",
                table: "AppProjectChangeRequests",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeRequests_ProjectId_Status",
                table: "AppProjectChangeRequests",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectChangeRequests_RequestCode",
                table: "AppProjectChangeRequests",
                column: "RequestCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectCharters_ApprovalStatus",
                table: "AppProjectCharters",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectCharters_ProjectId",
                table: "AppProjectCharters",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectDocuments_OwnerUserId_Status",
                table: "AppProjectDocuments",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectDocuments_ProjectId_Category",
                table: "AppProjectDocuments",
                columns: new[] { "ProjectId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_AcceptanceCriteriaId",
                table: "AppProjectIssues",
                column: "AcceptanceCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_DeveloperOwnerUserId",
                table: "AppProjectIssues",
                column: "DeveloperOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_OwnerUserId_Status",
                table: "AppProjectIssues",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_ProductOwnerUserId",
                table: "AppProjectIssues",
                column: "ProductOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_ProjectId_Type_Status",
                table: "AppProjectIssues",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_QualityChecklistId",
                table: "AppProjectIssues",
                column: "QualityChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_TesterOwnerUserId",
                table: "AppProjectIssues",
                column: "TesterOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_AssigneeUserId",
                table: "AppProjectMeetingActionItems",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_DueDate",
                table: "AppProjectMeetingActionItems",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_LinkedIssueId",
                table: "AppProjectMeetingActionItems",
                column: "LinkedIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_LinkedRaidItemId",
                table: "AppProjectMeetingActionItems",
                column: "LinkedRaidItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_LinkedRequirementId",
                table: "AppProjectMeetingActionItems",
                column: "LinkedRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_LinkedTaskId",
                table: "AppProjectMeetingActionItems",
                column: "LinkedTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_MeetingRecordId",
                table: "AppProjectMeetingActionItems",
                column: "MeetingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_ProjectId_MeetingRecordId",
                table: "AppProjectMeetingActionItems",
                columns: new[] { "ProjectId", "MeetingRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingActionItems_ProjectId_Status",
                table: "AppProjectMeetingActionItems",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingRecords_AttachmentFileId",
                table: "AppProjectMeetingRecords",
                column: "AttachmentFileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingRecords_FacilitatorUserId",
                table: "AppProjectMeetingRecords",
                column: "FacilitatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingRecords_ProjectId_MeetingTime",
                table: "AppProjectMeetingRecords",
                columns: new[] { "ProjectId", "MeetingTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMeetingRecords_ProjectId_Status",
                table: "AppProjectMeetingRecords",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMemberRoles_ProjectId_RoleName",
                table: "AppProjectMemberRoles",
                columns: new[] { "ProjectId", "RoleName" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMemberRoles_ProjectId_UserId",
                table: "AppProjectMemberRoles",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectMemberRoles_ProjectMemberId_RoleName",
                table: "AppProjectMemberRoles",
                columns: new[] { "ProjectMemberId", "RoleName" },
                unique: true);

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
                name: "IX_AppProjectMilestones_SprintId",
                table: "AppProjectMilestones",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectPerformanceSnapshots_ProjectBaselineId",
                table: "AppProjectPerformanceSnapshots",
                column: "ProjectBaselineId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectPerformanceSnapshots_ProjectId_SnapshotDate",
                table: "AppProjectPerformanceSnapshots",
                columns: new[] { "ProjectId", "SnapshotDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectPerformanceSnapshots_ProjectId_SnapshotPeriod",
                table: "AppProjectPerformanceSnapshots",
                columns: new[] { "ProjectId", "SnapshotPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectQualityChecklists_ProjectId_Category",
                table: "AppProjectQualityChecklists",
                columns: new[] { "ProjectId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectQualityInspections_ProjectId_InspectionTime",
                table: "AppProjectQualityInspections",
                columns: new[] { "ProjectId", "InspectionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectQualityInspections_QualityChecklistId",
                table: "AppProjectQualityInspections",
                column: "QualityChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRaidItems_OwnerUserId_Status",
                table: "AppProjectRaidItems",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRaidItems_ProjectId_RiskScore",
                table: "AppProjectRaidItems",
                columns: new[] { "ProjectId", "RiskScore" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRaidItems_ProjectId_Type_Status",
                table: "AppProjectRaidItems",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectReports_AttachmentFileId",
                table: "AppProjectReports",
                column: "AttachmentFileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectReports_AuthorUserId",
                table: "AppProjectReports",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectReports_ProjectId_ReportType_PeriodStart_PeriodEnd",
                table: "AppProjectReports",
                columns: new[] { "ProjectId", "ReportType", "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectReports_ProjectId_Status",
                table: "AppProjectReports",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectReports_SprintId",
                table: "AppProjectReports",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_BusinessOwnerUserId",
                table: "AppProjectRequirements",
                column: "BusinessOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_MilestoneId",
                table: "AppProjectRequirements",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_ProductOwnerUserId",
                table: "AppProjectRequirements",
                column: "ProductOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_ProjectId_RequirementCode",
                table: "AppProjectRequirements",
                columns: new[] { "ProjectId", "RequirementCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_ProjectId_Status",
                table: "AppProjectRequirements",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_ProjectId_WbsNodeId",
                table: "AppProjectRequirements",
                columns: new[] { "ProjectId", "WbsNodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_ReplacesRequirementId",
                table: "AppProjectRequirements",
                column: "ReplacesRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRequirements_WbsNodeId",
                table: "AppProjectRequirements",
                column: "WbsNodeId");

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
                name: "IX_AppProjectSprints_OwnerUserId_Status",
                table: "AppProjectSprints",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectSprints_ProjectId_SprintCode",
                table: "AppProjectSprints",
                columns: new[] { "ProjectId", "SprintCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectSprints_ProjectId_Status",
                table: "AppProjectSprints",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskDependencies_PredecessorTaskId",
                table: "AppProjectTaskDependencies",
                column: "PredecessorTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskDependencies_ProjectId_PredecessorTaskId_Succ~",
                table: "AppProjectTaskDependencies",
                columns: new[] { "ProjectId", "PredecessorTaskId", "SuccessorTaskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskDependencies_ProjectId_SuccessorTaskId",
                table: "AppProjectTaskDependencies",
                columns: new[] { "ProjectId", "SuccessorTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskDependencies_SuccessorTaskId",
                table: "AppProjectTaskDependencies",
                column: "SuccessorTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskParticipants_ProjectId_RoleName",
                table: "AppProjectTaskParticipants",
                columns: new[] { "ProjectId", "RoleName" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskParticipants_ProjectId_UserId",
                table: "AppProjectTaskParticipants",
                columns: new[] { "ProjectId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskParticipants_TaskId_RoleName_UserId",
                table: "AppProjectTaskParticipants",
                columns: new[] { "TaskId", "RoleName", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_DeveloperOwnerUserId",
                table: "AppProjectTasks",
                column: "DeveloperOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_OrganizationUnitId_Status",
                table: "AppProjectTasks",
                columns: new[] { "OrganizationUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_OwnerUserId_Status",
                table: "AppProjectTasks",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ParentTaskId",
                table: "AppProjectTasks",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProductOwnerUserId",
                table: "AppProjectTasks",
                column: "ProductOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProjectId_ParentTaskId_SortOrder",
                table: "AppProjectTasks",
                columns: new[] { "ProjectId", "ParentTaskId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProjectId_Status",
                table: "AppProjectTasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProjectId_WbsCode",
                table: "AppProjectTasks",
                columns: new[] { "ProjectId", "WbsCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_RequirementId",
                table: "AppProjectTasks",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_SprintId",
                table: "AppProjectTasks",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_TaskCode",
                table: "AppProjectTasks",
                column: "TaskCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_TesterOwnerUserId",
                table: "AppProjectTasks",
                column: "TesterOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_WbsNodeId",
                table: "AppProjectTasks",
                column: "WbsNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWbsNodes_MilestoneId",
                table: "AppProjectWbsNodes",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWbsNodes_OwnerUserId",
                table: "AppProjectWbsNodes",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWbsNodes_ParentId",
                table: "AppProjectWbsNodes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWbsNodes_ProjectId_ParentId_SortOrder",
                table: "AppProjectWbsNodes",
                columns: new[] { "ProjectId", "ParentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWbsNodes_ProjectId_WbsCode",
                table: "AppProjectWbsNodes",
                columns: new[] { "ProjectId", "WbsCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectWbsNodes_SprintId",
                table: "AppProjectWbsNodes",
                column: "SprintId");

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
                name: "IX_AppRequestAuditLogs_IsHostTenantOperation_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "IsHostTenantOperation", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_OperationSessionId_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "OperationSessionId", "ExecutionTime" });

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
                name: "IX_AppScheduledJobRecords_JobKey_StartTime",
                table: "AppScheduledJobRecords",
                columns: new[] { "JobKey", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppScheduledJobRecords_JobKey_Status_StartTime",
                table: "AppScheduledJobRecords",
                columns: new[] { "JobKey", "Status", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppScheduledJobRecords_JobKey_TriggerKey_StartTime",
                table: "AppScheduledJobRecords",
                columns: new[] { "JobKey", "TriggerKey", "StartTime" });

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
                name: "IX_AppUserNotifications_NotificationId",
                table: "AppUserNotifications",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserNotifications_UserId_IsRemoved_IsRead",
                table: "AppUserNotifications",
                columns: new[] { "UserId", "IsRemoved", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserNotifications_UserId_IsRemoved_IsRead_CreationTime",
                table: "AppUserNotifications",
                columns: new[] { "UserId", "IsRemoved", "IsRead", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserNotifications_UserId_NotificationId",
                table: "AppUserNotifications",
                columns: new[] { "UserId", "NotificationId" },
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
                name: "AppBusinessAcceptances");

            migrationBuilder.DropTable(
                name: "AppBusinessBudgetExecutions");

            migrationBuilder.DropTable(
                name: "AppBusinessChains");

            migrationBuilder.DropTable(
                name: "AppBusinessContracts");

            migrationBuilder.DropTable(
                name: "AppBusinessForecastHistories");

            migrationBuilder.DropTable(
                name: "AppBusinessInvoices");

            migrationBuilder.DropTable(
                name: "AppBusinessProcurements");

            migrationBuilder.DropTable(
                name: "AppBusinessReceivables");

            migrationBuilder.DropTable(
                name: "AppDictData");

            migrationBuilder.DropTable(
                name: "AppDictTypes");

            migrationBuilder.DropTable(
                name: "AppExternalAccountMappings");

            migrationBuilder.DropTable(
                name: "AppFileStorageSources");

            migrationBuilder.DropTable(
                name: "AppMenus");

            migrationBuilder.DropTable(
                name: "AppNotifications");

            migrationBuilder.DropTable(
                name: "AppOpenApiApplications");

            migrationBuilder.DropTable(
                name: "AppPlatformApprovalRecords");

            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobClusterNodes");

            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobs");

            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobTriggers");

            migrationBuilder.DropTable(
                name: "AppProjectAcceptanceRecordCriteria");

            migrationBuilder.DropTable(
                name: "AppProjectAcceptanceRecordRequirements");

            migrationBuilder.DropTable(
                name: "AppProjectChangeApprovals");

            migrationBuilder.DropTable(
                name: "AppProjectChangeImpacts");

            migrationBuilder.DropTable(
                name: "AppProjectCharters");

            migrationBuilder.DropTable(
                name: "AppProjectDocuments");

            migrationBuilder.DropTable(
                name: "AppProjectMeetingActionItems");

            migrationBuilder.DropTable(
                name: "AppProjectMemberRoles");

            migrationBuilder.DropTable(
                name: "AppProjectPerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "AppProjectQualityInspections");

            migrationBuilder.DropTable(
                name: "AppProjectReports");

            migrationBuilder.DropTable(
                name: "AppProjectTaskDependencies");

            migrationBuilder.DropTable(
                name: "AppProjectTaskParticipants");

            migrationBuilder.DropTable(
                name: "AppProjectWorklogs");

            migrationBuilder.DropTable(
                name: "AppRequestAuditLogs");

            migrationBuilder.DropTable(
                name: "AppRoleAuthorizations");

            migrationBuilder.DropTable(
                name: "AppRoleMenus");

            migrationBuilder.DropTable(
                name: "AppScheduledJobRecords");

            migrationBuilder.DropTable(
                name: "AppTenantAuthorizations");

            migrationBuilder.DropTable(
                name: "AppUpdateLogs");

            migrationBuilder.DropTable(
                name: "AppUserNotifications");

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
                name: "AppPlatformWorkflowInstances");

            migrationBuilder.DropTable(
                name: "AppProjectAcceptanceRecords");

            migrationBuilder.DropTable(
                name: "AppProjectIssues");

            migrationBuilder.DropTable(
                name: "AppProjectMeetingRecords");

            migrationBuilder.DropTable(
                name: "AppProjectRaidItems");

            migrationBuilder.DropTable(
                name: "AppProjectMembers");

            migrationBuilder.DropTable(
                name: "AppProjectBaselines");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "AbpAuditLogs");

            migrationBuilder.DropTable(
                name: "AppProjectAcceptanceCriteria");

            migrationBuilder.DropTable(
                name: "AppProjectQualityChecklists");

            migrationBuilder.DropTable(
                name: "AppPlatformFiles");

            migrationBuilder.DropTable(
                name: "AppProjectChangeRequests");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");

            migrationBuilder.DropTable(
                name: "AppProjectTasks");

            migrationBuilder.DropTable(
                name: "AppProjectRequirements");

            migrationBuilder.DropTable(
                name: "AppProjectWbsNodes");

            migrationBuilder.DropTable(
                name: "AppProjectMilestones");

            migrationBuilder.DropTable(
                name: "AppProjects");

            migrationBuilder.DropTable(
                name: "AppProjectSprints");
        }
    }
}
