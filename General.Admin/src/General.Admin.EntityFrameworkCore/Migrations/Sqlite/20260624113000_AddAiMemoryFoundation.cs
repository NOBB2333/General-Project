using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using General.Admin.EntityFrameworkCore;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260624113000_AddAiMemoryFoundation")]
    public partial class AddAiMemoryFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiKnowledgeSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ContentText = table.Column<string>(type: "TEXT", nullable: false),
                    ContextType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContextId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LastIndexedTime = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppAiKnowledgeSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAiMemories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemoryType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ContextType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContextId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    KnowledgeSourceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TagsJson = table.Column<string>(type: "TEXT", nullable: false),
                    SourceReferencesJson = table.Column<string>(type: "TEXT", nullable: false),
                    Importance = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUsedTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UseCount = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_AppAiMemories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiMemories_AppAiKnowledgeSources_KnowledgeSourceId",
                        column: x => x.KnowledgeSourceId,
                        principalTable: "AppAiKnowledgeSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiKnowledgeSources_ContextType_ContextId_LastIndexedTime",
                table: "AppAiKnowledgeSources",
                columns: new[] { "ContextType", "ContextId", "LastIndexedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiKnowledgeSources_ProjectId_SourceType_LastIndexedTime",
                table: "AppAiKnowledgeSources",
                columns: new[] { "ProjectId", "SourceType", "LastIndexedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiKnowledgeSources_Status_LastIndexedTime",
                table: "AppAiKnowledgeSources",
                columns: new[] { "Status", "LastIndexedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiKnowledgeSources_TenantId_SourceType_SourceId",
                table: "AppAiKnowledgeSources",
                columns: new[] { "TenantId", "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMemories_ContextType_ContextId_IsEnabled",
                table: "AppAiMemories",
                columns: new[] { "ContextType", "ContextId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMemories_KnowledgeSourceId",
                table: "AppAiMemories",
                column: "KnowledgeSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMemories_MemoryType_IsEnabled_Importance",
                table: "AppAiMemories",
                columns: new[] { "MemoryType", "IsEnabled", "Importance" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMemories_OwnerUserId_IsEnabled",
                table: "AppAiMemories",
                columns: new[] { "OwnerUserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMemories_TenantId_MemoryType_CreationTime",
                table: "AppAiMemories",
                columns: new[] { "TenantId", "MemoryType", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiMemories");

            migrationBuilder.DropTable(
                name: "AppAiKnowledgeSources");
        }
    }
}
