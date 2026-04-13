using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGBTOUR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStopStationToPOI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStopStation",
                table: "POIs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStopStation",
                table: "POIs");
        }
    }
}
