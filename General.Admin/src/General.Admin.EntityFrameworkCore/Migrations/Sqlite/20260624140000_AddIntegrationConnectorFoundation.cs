using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminSqliteMigrationsDbContext))]
    [Migration("20260624140000_AddIntegrationConnectorFoundation")]
    public partial class AddIntegrationConnectorFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIntegrationConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CapabilitiesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConfigSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConnectorCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsBuiltin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProviderType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
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
                    table.PrimaryKey("PK_AppIntegrationConnectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppIntegrationTenantConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccessTokenCipherText = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    CallbackPath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastTestMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    LastTestStatus = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LastTestTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    SecretCipherText = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppIntegrationTenantConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppIntegrationTenantConfigs_AppIntegrationConnectors_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "AppIntegrationConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_AppIntegrationConnectors_ProviderType_IsEnabled_Sort", table: "AppIntegrationConnectors", columns: new[] { "ProviderType", "IsEnabled", "Sort" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationConnectors_TenantId_ConnectorCode", table: "AppIntegrationConnectors", columns: new[] { "TenantId", "ConnectorCode" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationTenantConfigs_ConnectorId_IsEnabled", table: "AppIntegrationTenantConfigs", columns: new[] { "ConnectorId", "IsEnabled" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationTenantConfigs_LastTestStatus_LastTestTime", table: "AppIntegrationTenantConfigs", columns: new[] { "LastTestStatus", "LastTestTime" });
            migrationBuilder.CreateIndex(name: "IX_AppIntegrationTenantConfigs_TenantId_ConnectorId", table: "AppIntegrationTenantConfigs", columns: new[] { "TenantId", "ConnectorId" }, unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppIntegrationTenantConfigs");

            migrationBuilder.DropTable(
                name: "AppIntegrationConnectors");
        }
    }
}
