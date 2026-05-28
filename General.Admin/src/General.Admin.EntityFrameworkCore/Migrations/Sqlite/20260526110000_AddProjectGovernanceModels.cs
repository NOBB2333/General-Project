using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddProjectGovernanceModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BaselineEndTime",
                table: "AppProjectTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BaselineStartTime",
                table: "AppProjectTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentTaskId",
                table: "AppProjectTasks",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "AppProjectTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WbsCode",
                table: "AppProjectTasks",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContingencyPlan",
                table: "AppProjectRaidItems",
                type: "TEXT",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Impact",
                table: "AppProjectRaidItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Probability",
                table: "AppProjectRaidItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResponseStrategy",
                table: "AppProjectRaidItems",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RiskScore",
                table: "AppProjectRaidItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TriggerCondition",
                table: "AppProjectRaidItems",
                type: "TEXT",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AppProjectAcceptanceCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    MilestoneId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppProjectAcceptanceCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectCharters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Assumptions = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    BusinessCase = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Constraints = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Objectives = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScopeBoundary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    SuccessCriteria = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
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
                    table.PrimaryKey("PK_AppProjectCharters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectMemberRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectMemberId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_AppProjectMemberRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectQualityChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppProjectQualityChecklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectQualityInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FindingSummary = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    InspectionResult = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    InspectionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InspectorUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QualityChecklistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RectificationRequirement = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
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
                    table.PrimaryKey("PK_AppProjectQualityInspections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProjectTaskDependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DependencyType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    PredecessorTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SuccessorTaskId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppProjectTaskDependencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProjectId_ParentTaskId_SortOrder",
                table: "AppProjectTasks",
                columns: new[] { "ProjectId", "ParentTaskId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProjectId_WbsCode",
                table: "AppProjectTasks",
                columns: new[] { "ProjectId", "WbsCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectRaidItems_ProjectId_RiskScore",
                table: "AppProjectRaidItems",
                columns: new[] { "ProjectId", "RiskScore" });

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
                name: "IX_AppProjectCharters_ApprovalStatus",
                table: "AppProjectCharters",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectCharters_ProjectId",
                table: "AppProjectCharters",
                column: "ProjectId",
                unique: true);

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
                name: "IX_AppProjectTaskDependencies_ProjectId_PredecessorTaskId_SuccessorTaskId",
                table: "AppProjectTaskDependencies",
                columns: new[] { "ProjectId", "PredecessorTaskId", "SuccessorTaskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTaskDependencies_ProjectId_SuccessorTaskId",
                table: "AppProjectTaskDependencies",
                columns: new[] { "ProjectId", "SuccessorTaskId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppProjectAcceptanceCriteria");

            migrationBuilder.DropTable(
                name: "AppProjectCharters");

            migrationBuilder.DropTable(
                name: "AppProjectMemberRoles");

            migrationBuilder.DropTable(
                name: "AppProjectQualityChecklists");

            migrationBuilder.DropTable(
                name: "AppProjectQualityInspections");

            migrationBuilder.DropTable(
                name: "AppProjectTaskDependencies");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectTasks_ProjectId_ParentTaskId_SortOrder",
                table: "AppProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectTasks_ProjectId_WbsCode",
                table: "AppProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectRaidItems_ProjectId_RiskScore",
                table: "AppProjectRaidItems");

            migrationBuilder.DropColumn(
                name: "BaselineEndTime",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "BaselineStartTime",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "WbsCode",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "ContingencyPlan",
                table: "AppProjectRaidItems");

            migrationBuilder.DropColumn(
                name: "Impact",
                table: "AppProjectRaidItems");

            migrationBuilder.DropColumn(
                name: "Probability",
                table: "AppProjectRaidItems");

            migrationBuilder.DropColumn(
                name: "ResponseStrategy",
                table: "AppProjectRaidItems");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "AppProjectRaidItems");

            migrationBuilder.DropColumn(
                name: "TriggerCondition",
                table: "AppProjectRaidItems");
        }
    }
}
