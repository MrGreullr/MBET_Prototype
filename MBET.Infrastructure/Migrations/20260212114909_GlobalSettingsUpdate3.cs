using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBET.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalSettingsUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowFreshDropsSection",
                table: "GlobalSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowFreshDropsSection",
                table: "GlobalSettings");
        }
    }
}
