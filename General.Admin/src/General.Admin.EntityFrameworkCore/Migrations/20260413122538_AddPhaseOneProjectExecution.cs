using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseOneProjectExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectMilestones",
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
                    table.PrimaryKey("PK_AppPhaseOneProjectMilestones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectRaidItems",
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
                    table.PrimaryKey("PK_AppPhaseOneProjectRaidItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjects",
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
                    table.PrimaryKey("PK_AppPhaseOneProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneProjectTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActualEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualWorkHours = table.Column<double>(type: "REAL", nullable: true),
                    BlockReason = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    EstimatedWorkHours = table.Column<double>(type: "REAL", nullable: true),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlannedEndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TaskCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneProjectTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectMilestones_OwnerUserId_Status",
                table: "AppPhaseOneProjectMilestones",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectMilestones_ProjectId_PlannedCompletionDate",
                table: "AppPhaseOneProjectMilestones",
                columns: new[] { "ProjectId", "PlannedCompletionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectRaidItems_OwnerUserId_Status",
                table: "AppPhaseOneProjectRaidItems",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectRaidItems_ProjectId_Type_Status",
                table: "AppPhaseOneProjectRaidItems",
                columns: new[] { "ProjectId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjects_ManagerUserId_Status",
                table: "AppPhaseOneProjects",
                columns: new[] { "ManagerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjects_OrganizationUnitId_Status",
                table: "AppPhaseOneProjects",
                columns: new[] { "OrganizationUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjects_ProjectCode",
                table: "AppPhaseOneProjects",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectTasks_OrganizationUnitId_Status",
                table: "AppPhaseOneProjectTasks",
                columns: new[] { "OrganizationUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectTasks_OwnerUserId_Status",
                table: "AppPhaseOneProjectTasks",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectTasks_ProjectId_Status",
                table: "AppPhaseOneProjectTasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneProjectTasks_TaskCode",
                table: "AppPhaseOneProjectTasks",
                column: "TaskCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectMilestones");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectRaidItems");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjects");

            migrationBuilder.DropTable(
                name: "AppPhaseOneProjectTasks");
        }
    }
}
