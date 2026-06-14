using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_LocationId",
                table: "Bookings",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BusinessLocations_LocationId",
                table: "Bookings",
                column: "LocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BusinessLocations_LocationId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_LocationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Bookings");
        }
    }
}
