using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBET.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalSettingsShippingFeeUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseHomeDeliveryFee",
                table: "GlobalSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePickupDeliveryFee",
                table: "GlobalSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistanceMultiplierFee",
                table: "GlobalSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EnableHomeDelivery",
                table: "GlobalSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePickupDelivery",
                table: "GlobalSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightMultiplierFee",
                table: "GlobalSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseHomeDeliveryFee",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "BasePickupDeliveryFee",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "DistanceMultiplierFee",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "EnableHomeDelivery",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "EnablePickupDelivery",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "WeightMultiplierFee",
                table: "GlobalSettings");
        }
    }
}
