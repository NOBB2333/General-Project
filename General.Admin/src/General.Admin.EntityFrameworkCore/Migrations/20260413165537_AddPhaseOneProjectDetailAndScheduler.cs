using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseOneProjectDetailAndScheduler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectCycles",
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
                    table.PrimaryKey("PK_AppPhaseOneProjectCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectDocuments",
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
                    table.PrimaryKey("PK_AppPhaseOneProjectDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Level = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneProjectIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectMembers",
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
                    table.PrimaryKey("PK_AppPhaseOneProjectMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectWorklogs",
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
                    table.PrimaryKey("PK_AppPhaseOneProjectWorklogs", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectCycles_OwnerUserId_Status",
                table: "AppPhaseOneProjectCycles",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectCycles_ProjectId_Type_Status",
                table: "AppPhaseOneProjectCycles",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectDocuments_OwnerUserId_Status",
                table: "AppPhaseOneProjectDocuments",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectDocuments_ProjectId_Category",
                table: "AppPhaseOneProjectDocuments",
                columns: new[] { "ProjectId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectIssues_OwnerUserId_Status",
                table: "AppPhaseOneProjectIssues",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectIssues_ProjectId_Type_Status",
                table: "AppPhaseOneProjectIssues",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectMembers_ProjectId_UserId",
                table: "AppPhaseOneProjectMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectMembers_UserId_LeaveDate",
                table: "AppPhaseOneProjectMembers",
                columns: new[] { "UserId", "LeaveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectWorklogs_ProjectId_WorkDate",
                table: "AppPhaseOneProjectWorklogs",
                columns: new[] { "ProjectId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectWorklogs_UserId_WeekStartDate",
                table: "AppPhaseOneProjectWorklogs",
                columns: new[] { "UserId", "WeekStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformScheduledJobs_JobKey",
                table: "AppPlatformScheduledJobs",
                column: "JobKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectCycles");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectDocuments");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectIssues");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectMembers");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectWorklogs");

            migrationBuilder.DropTable(
                name: "AppPlatformScheduledJobs");
        }
    }
}
