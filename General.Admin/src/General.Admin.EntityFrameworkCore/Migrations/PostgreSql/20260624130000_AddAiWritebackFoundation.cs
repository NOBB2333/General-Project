using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624130000_AddAiWritebackFoundation")]
    public partial class AddAiWritebackFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiWritebackRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessModule = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetEntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TargetEntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Summary = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    DiffPreviewJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_AppAiWritebackRequests", x => x.Id);
                    table.ForeignKey(name: "FK_AppAiWritebackRequests_AppAiConversations_ConversationId", column: x => x.ConversationId, principalTable: "AppAiConversations", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_AppAiWritebackRequests_AppAiMessages_MessageId", column: x => x.MessageId, principalTable: "AppAiMessages", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_AppAiWritebackRequests_BusinessModule_ActionCode_Status", table: "AppAiWritebackRequests", columns: new[] { "BusinessModule", "ActionCode", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppAiWritebackRequests_ConversationId", table: "AppAiWritebackRequests", column: "ConversationId");
            migrationBuilder.CreateIndex(name: "IX_AppAiWritebackRequests_MessageId", table: "AppAiWritebackRequests", column: "MessageId");
            migrationBuilder.CreateIndex(name: "IX_AppAiWritebackRequests_ProjectId_CreationTime", table: "AppAiWritebackRequests", columns: new[] { "ProjectId", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppAiWritebackRequests_TenantId_Status_CreationTime", table: "AppAiWritebackRequests", columns: new[] { "TenantId", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiWritebackRequests");
        }
    }
}
