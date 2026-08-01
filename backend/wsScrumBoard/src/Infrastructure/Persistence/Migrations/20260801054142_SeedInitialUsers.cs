using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "email", "name", "normalized_email", "password_hash" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "admin@scrumboard.local", "Administrator", "ADMIN@SCRUMBOARD.LOCAL", "AQAAAAIAAYagAAAAEOjwVSt9xPqj1MoHEqB6JWKKgecbqXFMJbIwl60PACRF1QcbrpDD+TZqUYW6erV45Q==" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "user@scrumboard.local", "Project User", "USER@SCRUMBOARD.LOCAL", "AQAAAAIAAYagAAAAEMS79DE2c4ZV0b3b9Ts8n5GN5mgPzcZ/dUj3jacSAZBO7p3ezMudh+CMtBiQuSOTqg==" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));
        }
    }
}
