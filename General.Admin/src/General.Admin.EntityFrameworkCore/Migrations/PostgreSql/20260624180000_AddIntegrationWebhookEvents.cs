using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624180000_AddIntegrationWebhookEvents")]
    public partial class AddIntegrationWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIntegrationWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BusinessKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DedupeKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EventId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EventType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HeadersJson = table.Column<string>(type: "TEXT", nullable: false),
                    NormalizedPayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RawBody = table.Column<string>(type: "TEXT", nullable: false),
                    RawPayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResolvedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    SignatureStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceIp = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationWebhookEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationWebhookEvents_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationWebhookEvents_BusinessKey", table: "AppIntegrationWebhookEvents", column: "BusinessKey");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationWebhookEvents_ConnectorId_Status_CreationTime", table: "AppIntegrationWebhookEvents", columns: new[] { "ConnectorId", "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationWebhookEvents_EventId", table: "AppIntegrationWebhookEvents", column: "EventId");
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationWebhookEvents_ProviderType_EventType_CreationTime", table: "AppIntegrationWebhookEvents", columns: new[] { "ProviderType", "EventType", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationWebhookEvents_TenantId_DedupeKey", table: "AppIntegrationWebhookEvents", columns: new[] { "TenantId", "DedupeKey" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationWebhookEvents_TenantId_Status_CreationTime", table: "AppIntegrationWebhookEvents", columns: new[] { "TenantId", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppIntegrationWebhookEvents");
        }
    }
}
