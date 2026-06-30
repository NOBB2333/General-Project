using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using General.Admin.EntityFrameworkCore;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260624100000_AddAiToolFoundation")]
    public partial class AddAiToolFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiToolDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToolCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    InputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    PermissionCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_AppAiToolDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAiToolInvocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToolCode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    InputJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputJson = table.Column<string>(type: "TEXT", nullable: false),
                    SourceReferencesJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MessageId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppAiToolInvocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiToolInvocations_AppAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AppAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppAiToolInvocations_AppAiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "AppAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiToolDefinitions_Category_IsEnabled_Sort",
                table: "AppAiToolDefinitions",
                columns: new[] { "Category", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiToolDefinitions_TenantId_ToolCode",
                table: "AppAiToolDefinitions",
                columns: new[] { "TenantId", "ToolCode" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiToolInvocations_ConversationId_CreationTime",
                table: "AppAiToolInvocations",
                columns: new[] { "ConversationId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiToolInvocations_MessageId",
                table: "AppAiToolInvocations",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiToolInvocations_TenantId_ToolCode_CreationTime",
                table: "AppAiToolInvocations",
                columns: new[] { "TenantId", "ToolCode", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiToolInvocations_ToolCode_Status_CreationTime",
                table: "AppAiToolInvocations",
                columns: new[] { "ToolCode", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiToolInvocations");

            migrationBuilder.DropTable(
                name: "AppAiToolDefinitions");
        }
    }
}
