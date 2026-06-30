using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using General.Admin.EntityFrameworkCore;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260624103000_AddAiAssistantPromptFoundation")]
    public partial class AddAiAssistantPromptFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiAssistants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssistantCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RoleType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    SystemPrompt = table.Column<string>(type: "TEXT", maxLength: 16000, nullable: false),
                    DefaultMode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DefaultModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ToolCodesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sort = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_AppAiAssistants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiAssistants_AppAiModels_DefaultModelId",
                        column: x => x.DefaultModelId,
                        principalTable: "AppAiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppAiPromptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Scenario = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AssistantCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    PromptContent = table.Column<string>(type: "TEXT", maxLength: 16000, nullable: false),
                    InputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    ToolCodesJson = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sort = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_AppAiPromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiAssistants_DefaultModelId",
                table: "AppAiAssistants",
                column: "DefaultModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiAssistants_RoleType_IsEnabled_Sort",
                table: "AppAiAssistants",
                columns: new[] { "RoleType", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiAssistants_TenantId_AssistantCode",
                table: "AppAiAssistants",
                columns: new[] { "TenantId", "AssistantCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiPromptTemplates_AssistantCode_IsEnabled_Sort",
                table: "AppAiPromptTemplates",
                columns: new[] { "AssistantCode", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiPromptTemplates_Category_IsEnabled_Sort",
                table: "AppAiPromptTemplates",
                columns: new[] { "Category", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiPromptTemplates_TenantId_TemplateCode",
                table: "AppAiPromptTemplates",
                columns: new[] { "TenantId", "TemplateCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiPromptTemplates");

            migrationBuilder.DropTable(
                name: "AppAiAssistants");
        }
    }
}
