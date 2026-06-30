using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624170000_AddIntegrationNotificationRules")]
    public partial class AddIntegrationNotificationRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIntegrationMessageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    CardJson = table.Column<string>(type: "TEXT", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PreviewVariablesJson = table.Column<string>(type: "TEXT", nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TitleTemplate = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    VariableSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationMessageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationNotificationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConditionJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EventName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastTriggeredTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QuietEndTime = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    QuietStartTime = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RateLimitMaxCount = table.Column<int>(type: "integer", nullable: false),
                    RateLimitWindowMinutes = table.Column<int>(type: "integer", nullable: false),
                    RecipientRuleJson = table.Column<string>(type: "TEXT", nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationNotificationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationNotificationRules_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppIntegrationNotificationRules_AppIntegrationMessageTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "AppIntegrationMessageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationMessageTemplates_MessageType_Status", table: "AppIntegrationMessageTemplates", columns: new[] { "MessageType", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationMessageTemplates_Status_CreationTime", table: "AppIntegrationMessageTemplates", columns: new[] { "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationMessageTemplates_TenantId_TemplateCode", table: "AppIntegrationMessageTemplates", columns: new[] { "TenantId", "TemplateCode" }, unique: true);

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationNotificationRules_ConnectorId_Status", table: "AppIntegrationNotificationRules", columns: new[] { "ConnectorId", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationNotificationRules_EventCode_Status", table: "AppIntegrationNotificationRules", columns: new[] { "EventCode", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationNotificationRules_Status_CreationTime", table: "AppIntegrationNotificationRules", columns: new[] { "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationNotificationRules_TemplateId_Status", table: "AppIntegrationNotificationRules", columns: new[] { "TemplateId", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationNotificationRules_TenantId_RuleCode", table: "AppIntegrationNotificationRules", columns: new[] { "TenantId", "RuleCode" }, unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AppIntegrationNotificationRules");
            migrationBuilder.DropTable(name: "AppIntegrationMessageTemplates");
        }
    }
}
