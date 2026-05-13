using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonAudioTour.Api.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddPremiumExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PremiumExpiresAt",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PremiumExpiresAt",
                table: "Users");
        }
    }
}
