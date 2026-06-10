using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickstart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokenValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Align with RefreshTokenHasher: SHA256(UTF-8 plaintext) → Base64. Requires pgcrypto.
            // Run before deploying code that expects hashed tokens; existing sessions stay valid (same string hashed).
            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS pgcrypto;
                UPDATE "RefreshTokens"
                SET "Token" = encode(digest(convert_to("Token", 'UTF8'), 'sha256'), 'base64');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot recover plaintext refresh tokens from hashes.
        }
    }
}
