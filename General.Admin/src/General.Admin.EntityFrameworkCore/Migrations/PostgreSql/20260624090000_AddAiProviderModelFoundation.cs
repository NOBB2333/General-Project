using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624090000_AddAiProviderModelFoundation")]
    public partial class AddAiProviderModelFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ApiKeyCipherText = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    OrganizationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
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
                    table.PrimaryKey("PK_AppAiProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppAiModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    FallbackModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModelCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ModelType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CapabilitiesJson = table.Column<string>(type: "TEXT", nullable: false),
                    ContextWindow = table.Column<int>(type: "integer", nullable: false),
                    MaxOutputTokens = table.Column<int>(type: "integer", nullable: false),
                    InputTokenPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    OutputTokenPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
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
                    table.PrimaryKey("PK_AppAiModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAiModels_AppAiModels_FallbackModelId",
                        column: x => x.FallbackModelId,
                        principalTable: "AppAiModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AppAiModels_AppAiProviders_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "AppAiProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiModels_FallbackModelId",
                table: "AppAiModels",
                column: "FallbackModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiModels_ProviderId_IsDefault",
                table: "AppAiModels",
                columns: new[] { "ProviderId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiModels_ProviderId_IsEnabled_Sort",
                table: "AppAiModels",
                columns: new[] { "ProviderId", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiModels_ProviderId_ModelCode",
                table: "AppAiModels",
                columns: new[] { "ProviderId", "ModelCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiModels_TenantId_ProviderId_IsEnabled",
                table: "AppAiModels",
                columns: new[] { "TenantId", "ProviderId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_AppAiProviders_IsDefault",
                table: "AppAiProviders",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_AppAiProviders_TenantId_Name",
                table: "AppAiProviders",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppAiProviders_ProviderType_IsEnabled",
                table: "AppAiProviders",
                columns: new[] { "ProviderType", "IsEnabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAiModels");

            migrationBuilder.DropTable(
                name: "AppAiProviders");
        }
    }
}
