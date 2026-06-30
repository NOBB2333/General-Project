using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624150000_AddIntegrationDeliveryRecords")]
    public partial class AddIntegrationDeliveryRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIntegrationDeliveryRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    BusinessKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BusinessModule = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeadLetterReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DurationMilliseconds = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ExternalMessageId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastAttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxRetryCount = table.Column<int>(type: "integer", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NextRetryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PayloadPreview = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResponseJson = table.Column<string>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationDeliveryRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationDeliveryRecords_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDeliveryRecords_BusinessModule_BusinessKey_CreationTime", table: "AppIntegrationDeliveryRecords", columns: new[] { "BusinessModule", "BusinessKey", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDeliveryRecords_ConnectorCode_CreationTime", table: "AppIntegrationDeliveryRecords", columns: new[] { "ConnectorCode", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDeliveryRecords_ConnectorId_Status_CreationTime", table: "AppIntegrationDeliveryRecords", columns: new[] { "ConnectorId", "Status", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDeliveryRecords_NextRetryTime_Status", table: "AppIntegrationDeliveryRecords", columns: new[] { "NextRetryTime", "Status" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDeliveryRecords_TargetType_TargetId_CreationTime", table: "AppIntegrationDeliveryRecords", columns: new[] { "TargetType", "TargetId", "CreationTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationDeliveryRecords_TenantId_Status_CreationTime", table: "AppIntegrationDeliveryRecords", columns: new[] { "TenantId", "Status", "CreationTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppIntegrationDeliveryRecords");
        }
    }
}
