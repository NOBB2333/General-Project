using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624110000_AddAiMcpFoundation")]
    public partial class AddAiMcpFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiMcpServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TransportType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Command = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ArgumentsJson = table.Column<string>(type: "TEXT", nullable: false),
                    HeadersJson = table.Column<string>(type: "TEXT", nullable: false),
                    EnvironmentJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    ProtocolVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    LastDiscoveryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
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
                    table.PrimaryKey("PK_AppAiMcpServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAiMcpTools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    InputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastDiscoveredTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AppAiMcpTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiMcpTools_AppAiMcpServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "AppAiMcpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAiMcpInvocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToolName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InputJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputJson = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppAiMcpInvocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiMcpInvocations_AppAiMcpServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "AppAiMcpServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppAiMcpInvocations_AppAiMcpTools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "AppAiMcpTools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpServers_TenantId_ServerCode",
                table: "AppAiMcpServers",
                columns: new[] { "TenantId", "ServerCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpServers_TransportType_IsEnabled_Sort",
                table: "AppAiMcpServers",
                columns: new[] { "TransportType", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpTools_ServerId_IsEnabled_ToolName",
                table: "AppAiMcpTools",
                columns: new[] { "ServerId", "IsEnabled", "ToolName" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpTools_ServerId_ToolName",
                table: "AppAiMcpTools",
                columns: new[] { "ServerId", "ToolName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpTools_TenantId_ToolName",
                table: "AppAiMcpTools",
                columns: new[] { "TenantId", "ToolName" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpInvocations_ServerId_CreationTime",
                table: "AppAiMcpInvocations",
                columns: new[] { "ServerId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpInvocations_Status_CreationTime",
                table: "AppAiMcpInvocations",
                columns: new[] { "Status", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpInvocations_TenantId_ToolName_CreationTime",
                table: "AppAiMcpInvocations",
                columns: new[] { "TenantId", "ToolName", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiMcpInvocations_ToolId_CreationTime",
                table: "AppAiMcpInvocations",
                columns: new[] { "ToolId", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiMcpInvocations");

            migrationBuilder.DropTable(
                name: "AppAiMcpTools");

            migrationBuilder.DropTable(
                name: "AppAiMcpServers");
        }
    }
}
