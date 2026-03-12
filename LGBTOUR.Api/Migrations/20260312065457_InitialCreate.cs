using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGBTOUR.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "POIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Lng = table.Column<double>(type: "float", nullable: false),
                    Radius = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POI_Id = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    POIId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audios_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Narrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    POI_Id = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    POIId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Narrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Narrations_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    POI_Id = table.Column<int>(type: "int", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Lng = table.Column<double>(type: "float", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    POIId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLogs_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TourPOIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourId = table.Column<int>(type: "int", nullable: false),
                    POI_Id = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPOIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourPOIs_POIs_POI_Id",
                        column: x => x.POI_Id,
                        principalTable: "POIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourPOIs_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audios_POIId",
                table: "Audios",
                column: "POIId");

            migrationBuilder.CreateIndex(
                name: "IX_Narrations_POIId",
                table: "Narrations",
                column: "POIId");

            migrationBuilder.CreateIndex(
                name: "IX_TourPOIs_POI_Id",
                table: "TourPOIs",
                column: "POI_Id");

            migrationBuilder.CreateIndex(
                name: "IX_TourPOIs_TourId",
                table: "TourPOIs",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_POIId",
                table: "UserLogs",
                column: "POIId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audios");

            migrationBuilder.DropTable(
                name: "Narrations");

            migrationBuilder.DropTable(
                name: "TourPOIs");

            migrationBuilder.DropTable(
                name: "UserLogs");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "POIs");
        }
    }
}
