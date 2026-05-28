using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddProjectRoleOwnerUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperOwnerUserId",
                table: "AppProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOwnerUserId",
                table: "AppProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TesterOwnerUserId",
                table: "AppProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperOwnerUserId",
                table: "AppProjectIssues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOwnerUserId",
                table: "AppProjectIssues",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TesterOwnerUserId",
                table: "AppProjectIssues",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_DeveloperOwnerUserId",
                table: "AppProjectTasks",
                column: "DeveloperOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_ProductOwnerUserId",
                table: "AppProjectTasks",
                column: "ProductOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectTasks_TesterOwnerUserId",
                table: "AppProjectTasks",
                column: "TesterOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_DeveloperOwnerUserId",
                table: "AppProjectIssues",
                column: "DeveloperOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_ProductOwnerUserId",
                table: "AppProjectIssues",
                column: "ProductOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_TesterOwnerUserId",
                table: "AppProjectIssues",
                column: "TesterOwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppProjectTasks_DeveloperOwnerUserId",
                table: "AppProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectTasks_ProductOwnerUserId",
                table: "AppProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectTasks_TesterOwnerUserId",
                table: "AppProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectIssues_DeveloperOwnerUserId",
                table: "AppProjectIssues");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectIssues_ProductOwnerUserId",
                table: "AppProjectIssues");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectIssues_TesterOwnerUserId",
                table: "AppProjectIssues");

            migrationBuilder.DropColumn(
                name: "DeveloperOwnerUserId",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "ProductOwnerUserId",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "TesterOwnerUserId",
                table: "AppProjectTasks");

            migrationBuilder.DropColumn(
                name: "DeveloperOwnerUserId",
                table: "AppProjectIssues");

            migrationBuilder.DropColumn(
                name: "ProductOwnerUserId",
                table: "AppProjectIssues");

            migrationBuilder.DropColumn(
                name: "TesterOwnerUserId",
                table: "AppProjectIssues");
        }
    }
}
