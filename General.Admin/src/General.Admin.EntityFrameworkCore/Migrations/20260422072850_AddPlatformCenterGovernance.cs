using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformCenterGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppExternalAccountMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BoundAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExternalSource = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExternalUserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppExternalAccountMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPlatformFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FileKey = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ParentPath = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    StorageLocation = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
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
                    table.PrimaryKey("PK_AppPlatformFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApiBlacklist = table.Column<string>(type: "TEXT", nullable: false),
                    AccountScopeMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    AllowedUserIds = table.Column<string>(type: "TEXT", nullable: false),
                    CustomOrganizationUnitIds = table.Column<string>(type: "TEXT", nullable: false),
                    DataScopeMode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppRoleAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTenantAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApiBlacklist = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MenuIds = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppTenantAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUpdateLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImpactScope = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_AppUpdateLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmployeeNo = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExternalSource = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ExternalUserId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppUserProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppExternalAccountMappings_UserId_ExternalSource_ExternalUserId",
                table: "AppExternalAccountMappings",
                columns: new[] { "UserId", "ExternalSource", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_Category_ParentPath",
                table: "AppPlatformFiles",
                columns: new[] { "Category", "ParentPath" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_FileKey",
                table: "AppPlatformFiles",
                column: "FileKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRoleAuthorizations_RoleId",
                table: "AppRoleAuthorizations",
                column: "RoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppTenantAuthorizations_TenantId",
                table: "AppTenantAuthorizations",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUpdateLogs_PublishedAt",
                table: "AppUpdateLogs",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppUpdateLogs_Version",
                table: "AppUpdateLogs",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserProfiles_UserId",
                table: "AppUserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppExternalAccountMappings");

            migrationBuilder.DropTable(
                name: "AppPlatformFiles");

            migrationBuilder.DropTable(
                name: "AppRoleAuthorizations");

            migrationBuilder.DropTable(
                name: "AppTenantAuthorizations");

            migrationBuilder.DropTable(
                name: "AppUpdateLogs");

            migrationBuilder.DropTable(
                name: "AppUserProfiles");
        }
    }
}
