using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddProjectIssueQualityLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcceptanceCriteriaId",
                table: "AppProjectIssues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QualityChecklistId",
                table: "AppProjectIssues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_AcceptanceCriteriaId",
                table: "AppProjectIssues",
                column: "AcceptanceCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProjectIssues_QualityChecklistId",
                table: "AppProjectIssues",
                column: "QualityChecklistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppProjectIssues_AcceptanceCriteriaId",
                table: "AppProjectIssues");

            migrationBuilder.DropIndex(
                name: "IX_AppProjectIssues_QualityChecklistId",
                table: "AppProjectIssues");

            migrationBuilder.DropColumn(
                name: "AcceptanceCriteriaId",
                table: "AppProjectIssues");

            migrationBuilder.DropColumn(
                name: "QualityChecklistId",
                table: "AppProjectIssues");
        }
    }
}
