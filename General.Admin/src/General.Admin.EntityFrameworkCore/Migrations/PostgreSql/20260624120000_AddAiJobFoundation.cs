using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624120000_AddAiJobFoundation")]
    public partial class AddAiJobFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PromptTemplateCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AssistantCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContextScopeJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputTargetJson = table.Column<string>(type: "TEXT", nullable: false),
                    SchedulerJobKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastRunSummary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
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
                    table.PrimaryKey("PK_AppAiJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiJobs_AppAiModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AppAiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppAiJobRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AiJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    JobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RunMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InputSnapshotJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputDraftJson = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    DraftItemCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppAiJobRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiJobRuns_AppAiJobs_AiJobId",
                        column: x => x.AiJobId,
                        principalTable: "AppAiJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobRuns_AiJobId_StartedTime",
                table: "AppAiJobRuns",
                columns: new[] { "AiJobId", "StartedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobRuns_JobCode_StartedTime",
                table: "AppAiJobRuns",
                columns: new[] { "JobCode", "StartedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobRuns_Status_StartedTime",
                table: "AppAiJobRuns",
                columns: new[] { "Status", "StartedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobRuns_TenantId_JobType_StartedTime",
                table: "AppAiJobRuns",
                columns: new[] { "TenantId", "JobType", "StartedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobs_JobType_IsEnabled",
                table: "AppAiJobs",
                columns: new[] { "JobType", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobs_ModelId",
                table: "AppAiJobs",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobs_SchedulerJobKey_IsEnabled",
                table: "AppAiJobs",
                columns: new[] { "SchedulerJobKey", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiJobs_TenantId_JobCode",
                table: "AppAiJobs",
                columns: new[] { "TenantId", "JobCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiJobRuns");

            migrationBuilder.DropTable(
                name: "AppAiJobs");
        }
    }
}
