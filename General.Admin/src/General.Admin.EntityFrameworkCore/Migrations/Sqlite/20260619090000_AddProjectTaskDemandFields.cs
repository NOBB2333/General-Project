using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using General.Admin.EntityFrameworkCore;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260619090000_AddProjectTaskDemandFields")]
    public partial class AddProjectTaskDemandFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppProjectTasks",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsOutOfContract",
                table: "AppProjectTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AppProjectTaskParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppProjectTaskParticipants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "IsOutOfContract",
                table: "AppProjectTasks");
        }
    }
}
