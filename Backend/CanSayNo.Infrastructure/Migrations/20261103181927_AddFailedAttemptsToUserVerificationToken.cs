using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanSayNo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFailedAttemptsToUserVerificationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "UserVerificationTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "UserVerificationTokens");
        }
    }
}
