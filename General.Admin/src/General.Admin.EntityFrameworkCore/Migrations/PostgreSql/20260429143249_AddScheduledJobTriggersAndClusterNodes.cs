using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddScheduledJobTriggersAndClusterNodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TriggerKey",
                table: "AppScheduledJobRecords",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HandlerKey",
                table: "AppPlatformScheduledJobs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE \"AppPlatformScheduledJobs\" SET \"HandlerKey\" = \"JobKey\" WHERE \"HandlerKey\" = '' OR \"HandlerKey\" IS NULL;");

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

            migrationBuilder.CreateIndex(
                name: "IX_AppScheduledJobRecords_JobKey_TriggerKey_StartTime",
                table: "AppScheduledJobRecords",
                columns: new[] { "JobKey", "TriggerKey", "StartTime" });

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
                name: "IX_AppPlatformScheduledJobTriggers_IsEnabled_NextRunTime",
                table: "AppPlatformScheduledJobTriggers",
                columns: new[] { "IsEnabled", "NextRunTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobTriggers_JobId_TriggerKey",
                table: "AppPlatformScheduledJobTriggers",
                columns: new[] { "JobId", "TriggerKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobClusterNodes");

            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobTriggers");

            migrationBuilder.DropIndex(
                name: "IX_AppScheduledJobRecords_JobKey_TriggerKey_StartTime",
                table: "AppScheduledJobRecords");

            migrationBuilder.DropColumn(
                name: "TriggerKey",
                table: "AppScheduledJobRecords");

            migrationBuilder.DropColumn(
                name: "HandlerKey",
                table: "AppPlatformScheduledJobs");
        }
    }
}
