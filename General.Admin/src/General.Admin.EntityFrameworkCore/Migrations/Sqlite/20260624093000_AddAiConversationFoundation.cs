using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using General.Admin.EntityFrameworkCore;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260624093000_AddAiConversationFoundation")]
    public partial class AddAiConversationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ContextType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ContextId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ContextSnapshotJson = table.Column<string>(type: "TEXT", nullable: false),
                    LastMessageSummary = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    LastMessageTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MessageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    ArchivedTime = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppAiConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiConversations_AppAiModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AppAiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppAiMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 16000, nullable: false),
                    SequenceNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ModelId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PromptTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTokens = table.Column<int>(type: "INTEGER", nullable: false),
                    CitationsJson = table.Column<string>(type: "TEXT", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_AppAiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiMessages_AppAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AppAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppAiMessages_AppAiModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "AppAiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppAiMessages_AppAiProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AppAiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiConversations_ContextType_ContextId",
                table: "AppAiConversations",
                columns: new[] { "ContextType", "ContextId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiConversations_CreatorId_Status_LastMessageTime",
                table: "AppAiConversations",
                columns: new[] { "CreatorId", "Status", "LastMessageTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiConversations_ModelId",
                table: "AppAiConversations",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMessages_ConversationId_CreationTime",
                table: "AppAiMessages",
                columns: new[] { "ConversationId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMessages_ConversationId_SequenceNo",
                table: "AppAiMessages",
                columns: new[] { "ConversationId", "SequenceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMessages_ModelId",
                table: "AppAiMessages",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMessages_ProviderId",
                table: "AppAiMessages",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMessages_Role_Status",
                table: "AppAiMessages",
                columns: new[] { "Role", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiMessages");

            migrationBuilder.DropTable(
                name: "AppAiConversations");
        }
    }
}
