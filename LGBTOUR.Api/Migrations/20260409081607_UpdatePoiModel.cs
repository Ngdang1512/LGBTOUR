using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGBTOUR.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePoiModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TranslatedName",
                table: "Narrations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranslatedName",
                table: "Narrations");
        }
    }
}
