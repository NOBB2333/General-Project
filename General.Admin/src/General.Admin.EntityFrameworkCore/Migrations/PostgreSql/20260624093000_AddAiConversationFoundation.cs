using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Mode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContextType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContextId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ContextSnapshotJson = table.Column<string>(type: "TEXT", nullable: false),
                    LastMessageSummary = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastMessageTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MessageCount = table.Column<int>(type: "integer", nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false),
                    ArchivedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    SequenceNo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false),
                    CitationsJson = table.Column<string>(type: "TEXT", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
