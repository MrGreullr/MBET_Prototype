using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBET.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalSettingsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableHeroOverlay",
                table: "GlobalSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FooterBio",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroCtaLink",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroCtaText",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroDescription",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroHighlightText",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroImageUrl",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroSubtitle",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                table: "GlobalSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LandingDisplayMode",
                table: "GlobalSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LandingProductCount",
                table: "GlobalSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LogoHeight",
                table: "GlobalSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowFeaturesSection",
                table: "GlobalSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableHeroOverlay",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "FooterBio",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroCtaLink",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroCtaText",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroDescription",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroHighlightText",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroImageUrl",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroSubtitle",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "LandingDisplayMode",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "LandingProductCount",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "LogoHeight",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "ShowFeaturesSection",
                table: "GlobalSettings");
        }
    }
}
