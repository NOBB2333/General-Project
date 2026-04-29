using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddFileStorageAndScheduledJobRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockExpirationTime",
                table: "AppPlatformScheduledJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RunningInstanceId",
                table: "AppPlatformScheduledJobs",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageProvider",
                table: "AppPlatformFiles",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "Local");

            migrationBuilder.CreateTable(
                name: "AppScheduledJobRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DurationMilliseconds = table.Column<long>(type: "INTEGER", nullable: true),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    JobKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    JobTitle = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Result = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    TriggerMode = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppScheduledJobRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_IsEnabled_NextRunTime",
                table: "AppPlatformScheduledJobs",
                columns: new[] { "IsEnabled", "NextRunTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_LockExpirationTime",
                table: "AppPlatformScheduledJobs",
                column: "LockExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_StorageProvider",
                table: "AppPlatformFiles",
                column: "StorageProvider");

            migrationBuilder.CreateIndex(
                name: "IX_AppScheduledJobRecords_JobKey_StartTime",
                table: "AppScheduledJobRecords",
                columns: new[] { "JobKey", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppScheduledJobRecords_JobKey_Status_StartTime",
                table: "AppScheduledJobRecords",
                columns: new[] { "JobKey", "Status", "StartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppScheduledJobRecords");

            migrationBuilder.DropIndex(
                name: "IX_AppPlatformScheduledJobs_IsEnabled_NextRunTime",
                table: "AppPlatformScheduledJobs");

            migrationBuilder.DropIndex(
                name: "IX_AppPlatformScheduledJobs_LockExpirationTime",
                table: "AppPlatformScheduledJobs");

            migrationBuilder.DropIndex(
                name: "IX_AppPlatformFiles_StorageProvider",
                table: "AppPlatformFiles");

            migrationBuilder.DropColumn(
                name: "LockExpirationTime",
                table: "AppPlatformScheduledJobs");

            migrationBuilder.DropColumn(
                name: "RunningInstanceId",
                table: "AppPlatformScheduledJobs");

            migrationBuilder.DropColumn(
                name: "StorageProvider",
                table: "AppPlatformFiles");
        }
    }
}
