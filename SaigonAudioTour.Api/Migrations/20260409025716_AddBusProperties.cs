using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGBTOUR.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TicketPrice",
                table: "Tours",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalDistanceKm",
                table: "Tours",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketPrice",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "TotalDistanceKm",
                table: "Tours");
        }
    }
}
