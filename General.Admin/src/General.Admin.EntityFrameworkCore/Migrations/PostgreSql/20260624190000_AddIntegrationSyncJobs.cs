using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624190000_AddIntegrationSyncJobs")]
    public partial class AddIntegrationSyncJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIntegrationSyncJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Direction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    JobCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastRunError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LastRunStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastRunSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Mode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SyncType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationSyncJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationSyncJobs_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationSyncRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictCount = table.Column<int>(type: "integer", nullable: false),
                    ConflictJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedCount = table.Column<int>(type: "integer", nullable: false),
                    DurationMilliseconds = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FailedCount = table.Column<int>(type: "integer", nullable: false),
                    FinishedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Mode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResultJson = table.Column<string>(type: "TEXT", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    StartedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SyncJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationSyncRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationSyncRecords_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppIntegrationSyncRecords_AppIntegrationSyncJobs_SyncJobId",
                        column: x => x.SyncJobId,
                        principalTable: "AppIntegrationSyncJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncJobs_ConnectorId_IsEnabled", table: "AppIntegrationSyncJobs", columns: new[] { "ConnectorId", "IsEnabled" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncJobs_LastRunStatus_LastRunTime", table: "AppIntegrationSyncJobs", columns: new[] { "LastRunStatus", "LastRunTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncJobs_TenantId_IsEnabled_SyncType", table: "AppIntegrationSyncJobs", columns: new[] { "TenantId", "IsEnabled", "SyncType" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncJobs_TenantId_JobCode", table: "AppIntegrationSyncJobs", columns: new[] { "TenantId", "JobCode" }, unique: true);

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncRecords_ConnectorId_Status_CreationTime", table: "AppIntegrationSyncRecords", columns: new[] { "ConnectorId", "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncRecords_ProviderType_SyncType_CreationTime", table: "AppIntegrationSyncRecords", columns: new[] { "ProviderType", "SyncType", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncRecords_Status_FinishedTime", table: "AppIntegrationSyncRecords", columns: new[] { "Status", "FinishedTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncRecords_SyncJobId_CreationTime", table: "AppIntegrationSyncRecords", columns: new[] { "SyncJobId", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationSyncRecords_TenantId_Status_CreationTime", table: "AppIntegrationSyncRecords", columns: new[] { "TenantId", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppIntegrationSyncRecords");

            migrationBuilder.DropTable(
                name: "AppIntegrationSyncJobs");
        }
    }
}
