using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBET.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalSettingsUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteFeature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    GlobalSettingsId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteFeature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteFeature_GlobalSettings_GlobalSettingsId",
                        column: x => x.GlobalSettingsId,
                        principalTable: "GlobalSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteFeature_GlobalSettingsId",
                table: "SiteFeature",
                column: "GlobalSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteFeature");
        }
    }
}
