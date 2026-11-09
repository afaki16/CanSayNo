using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanSayNo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScenarioLanguage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Scenarios",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "tr");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_Language_ContextId",
                table: "Scenarios",
                columns: new[] { "Language", "ContextId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scenarios_Language_ContextId",
                table: "Scenarios");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Scenarios");
        }
    }
}
