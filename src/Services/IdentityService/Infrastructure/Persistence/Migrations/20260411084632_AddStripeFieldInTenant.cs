using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeFieldInTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StripeChargeEnabled",
                table: "Tenants",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StripeConnectedAccountId",
                table: "Tenants",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "StripePayoutsEnabled",
                table: "Tenants",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeChargeEnabled",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "StripeConnectedAccountId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "StripePayoutsEnabled",
                table: "Tenants");
        }
    }
}
