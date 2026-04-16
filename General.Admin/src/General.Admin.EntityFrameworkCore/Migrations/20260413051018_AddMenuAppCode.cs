using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuAppCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppCode",
                table: "AppMenus",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppMenus_AppCode_ParentId_Order",
                table: "AppMenus",
                columns: new[] { "AppCode", "ParentId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppMenus_AppCode_ParentId_Order",
                table: "AppMenus");

            migrationBuilder.DropColumn(
                name: "AppCode",
                table: "AppMenus");
        }
    }
}
