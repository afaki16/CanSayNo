using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanSayNo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropPermissionResourceTypeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Resource_Type",
                table: "Permissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Resource_Type",
                table: "Permissions",
                columns: new[] { "Resource", "Type" },
                unique: true);
        }
    }
}
