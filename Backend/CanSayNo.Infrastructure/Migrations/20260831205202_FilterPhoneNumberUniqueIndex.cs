using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanSayNo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterPhoneNumberUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_PhoneNumber",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true,
                filter: "\"TenantId\" IS NULL AND \"PhoneNumber\" IS NOT NULL AND \"PhoneNumber\" <> '' AND NOT \"IsDeleted\"");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_PhoneNumber",
                table: "Users",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL AND \"PhoneNumber\" IS NOT NULL AND \"PhoneNumber\" <> '' AND NOT \"IsDeleted\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_PhoneNumber",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true,
                filter: "\"TenantId\" IS NULL AND \"PhoneNumber\" IS NOT NULL AND NOT \"IsDeleted\"");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_PhoneNumber",
                table: "Users",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true,
                filter: "\"TenantId\" IS NOT NULL AND \"PhoneNumber\" IS NOT NULL AND NOT \"IsDeleted\"");
        }
    }
}
