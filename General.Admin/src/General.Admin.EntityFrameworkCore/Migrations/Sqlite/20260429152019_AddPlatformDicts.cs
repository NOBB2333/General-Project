using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddPlatformDicts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppDictData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DictTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "INTEGER", nullable: false),
                    TagColor = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_AppDictData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDictTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_AppDictTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDictData_DictTypeId_IsEnabled_Sort",
                table: "AppDictData",
                columns: new[] { "DictTypeId", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDictData_DictTypeId_Value",
                table: "AppDictData",
                columns: new[] { "DictTypeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDictTypes_Code",
                table: "AppDictTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDictTypes_Sort",
                table: "AppDictTypes",
                column: "Sort");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDictData");

            migrationBuilder.DropTable(
                name: "AppDictTypes");
        }
    }
}
