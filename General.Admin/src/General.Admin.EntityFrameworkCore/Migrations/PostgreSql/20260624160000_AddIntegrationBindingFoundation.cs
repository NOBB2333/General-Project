using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624160000_AddIntegrationBindingFoundation")]
    public partial class AddIntegrationBindingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIntegrationExternalAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DepartmentExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalUnionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastSyncedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Mobile = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RawJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppIntegrationExternalAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationExternalAccounts_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationDepartmentBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConflictReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExternalDepartmentId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalDepartmentName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InternalOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    InternalOrganizationUnitName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MatchType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ParentExternalDepartmentId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppIntegrationDepartmentBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationDepartmentBindings_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationGroupBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalGroupId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalGroupName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    GroupType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InternalObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InternalObjectName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InternalObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    RobotCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppIntegrationGroupBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationGroupBindings_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationIdMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalObjectId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExternalObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InternalObjectId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InternalObjectType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppIntegrationIdMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationIdMappings_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationUserBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BoundTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConflictReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExternalAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalDisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    InternalUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    InternalUserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MatchType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_AppIntegrationUserBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationUserBindings_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppIntegrationUserBindings_AppIntegrationExternalAccounts_ExternalAccountId",
                        column: x => x.ExternalAccountId,
                        principalTable: "AppIntegrationExternalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationExternalAccounts_ConnectorCode_Status", table: "AppIntegrationExternalAccounts", columns: new[] { "ConnectorCode", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationExternalAccounts_ConnectorId_ExternalUserId", table: "AppIntegrationExternalAccounts", columns: new[] { "ConnectorId", "ExternalUserId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationExternalAccounts_Email", table: "AppIntegrationExternalAccounts", column: "Email");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationExternalAccounts_ExternalUnionId", table: "AppIntegrationExternalAccounts", column: "ExternalUnionId");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationExternalAccounts_Mobile", table: "AppIntegrationExternalAccounts", column: "Mobile");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationExternalAccounts_TenantId_Status_LastSyncedTime", table: "AppIntegrationExternalAccounts", columns: new[] { "TenantId", "Status", "LastSyncedTime" });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDepartmentBindings_ConnectorId_ExternalDepartmentId", table: "AppIntegrationDepartmentBindings", columns: new[] { "ConnectorId", "ExternalDepartmentId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDepartmentBindings_InternalOrganizationUnitId_ConnectorId", table: "AppIntegrationDepartmentBindings", columns: new[] { "InternalOrganizationUnitId", "ConnectorId" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDepartmentBindings_ParentExternalDepartmentId", table: "AppIntegrationDepartmentBindings", column: "ParentExternalDepartmentId");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDepartmentBindings_TenantId_Status_CreationTime", table: "AppIntegrationDepartmentBindings", columns: new[] { "TenantId", "Status", "CreationTime" });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationGroupBindings_ConnectorId_ExternalGroupId", table: "AppIntegrationGroupBindings", columns: new[] { "ConnectorId", "ExternalGroupId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationGroupBindings_GroupType_Status", table: "AppIntegrationGroupBindings", columns: new[] { "GroupType", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationGroupBindings_InternalObjectType_InternalObjectId_ConnectorId", table: "AppIntegrationGroupBindings", columns: new[] { "InternalObjectType", "InternalObjectId", "ConnectorId" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationGroupBindings_TenantId_Status_CreationTime", table: "AppIntegrationGroupBindings", columns: new[] { "TenantId", "Status", "CreationTime" });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationIdMappings_ConnectorId_InternalObjectType_InternalObjectId_ExternalObjectType_ExternalObjectId", table: "AppIntegrationIdMappings", columns: new[] { "ConnectorId", "InternalObjectType", "InternalObjectId", "ExternalObjectType", "ExternalObjectId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationIdMappings_ExternalObjectType_ExternalObjectId_ConnectorId", table: "AppIntegrationIdMappings", columns: new[] { "ExternalObjectType", "ExternalObjectId", "ConnectorId" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationIdMappings_InternalObjectType_InternalObjectId_ConnectorId", table: "AppIntegrationIdMappings", columns: new[] { "InternalObjectType", "InternalObjectId", "ConnectorId" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationIdMappings_TenantId_Status_CreationTime", table: "AppIntegrationIdMappings", columns: new[] { "TenantId", "Status", "CreationTime" });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationUserBindings_ConnectorId_Status_CreationTime", table: "AppIntegrationUserBindings", columns: new[] { "ConnectorId", "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationUserBindings_ExternalAccountId", table: "AppIntegrationUserBindings", column: "ExternalAccountId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationUserBindings_ExternalUserId", table: "AppIntegrationUserBindings", column: "ExternalUserId");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationUserBindings_InternalUserId_ConnectorId", table: "AppIntegrationUserBindings", columns: new[] { "InternalUserId", "ConnectorId" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationUserBindings_TenantId_Status_CreationTime", table: "AppIntegrationUserBindings", columns: new[] { "TenantId", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AppIntegrationUserBindings");
            migrationBuilder.DropTable(name: "AppIntegrationDepartmentBindings");
            migrationBuilder.DropTable(name: "AppIntegrationGroupBindings");
            migrationBuilder.DropTable(name: "AppIntegrationIdMappings");
            migrationBuilder.DropTable(name: "AppIntegrationExternalAccounts");
        }
    }
}
