using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260624123000_AddAiUsageFoundation")]
    public partial class AddAiUsageFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiUsageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    JobId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModelCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ToolCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Scenario = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PromptTemplateCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    CostAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    RequestPreview = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ResponsePreview = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    AuditSummary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    PromptInjectionRiskScore = table.Column<int>(type: "INTEGER", nullable: false),
                    PromptInjectionSignalsJson = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppAiUsageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiUsageRecords_AppAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AppAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppAiUsageRecords_AppAiJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "AppAiJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppAiUsageRecords_AppAiModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AppAiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppAiUsageRecords_AppAiProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AppAiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_ConversationId", table: "AppAiUsageRecords", column: "ConversationId");
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_JobId", table: "AppAiUsageRecords", column: "JobId");
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_PromptTemplateCode_CreationTime", table: "AppAiUsageRecords", columns: new[] { "PromptTemplateCode", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_ProjectId_CreationTime", table: "AppAiUsageRecords", columns: new[] { "ProjectId", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_ProviderId_ModelId_CreationTime", table: "AppAiUsageRecords", columns: new[] { "ProviderId", "ModelId", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_Source_Status_CreationTime", table: "AppAiUsageRecords", columns: new[] { "Source", "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_TenantId_CreationTime", table: "AppAiUsageRecords", columns: new[] { "TenantId", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_ToolCode_CreationTime", table: "AppAiUsageRecords", columns: new[] { "ToolCode", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiUsageRecords_UserId_CreationTime", table: "AppAiUsageRecords", columns: new[] { "UserId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiUsageRecords");
        }
    }
}
