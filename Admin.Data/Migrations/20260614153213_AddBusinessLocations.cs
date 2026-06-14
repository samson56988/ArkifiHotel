using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "PropertyFacilities",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessLocations_BusinessRegistrations_BusinessRegistratio~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_LocationId",
                table: "Rooms",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilities_LocationId",
                table: "PropertyFacilities",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLocations_BusinessRegistrationId",
                table: "BusinessLocations",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLocations_BusinessRegistrationId_Name",
                table: "BusinessLocations",
                columns: new[] { "BusinessRegistrationId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyFacilities_BusinessLocations_LocationId",
                table: "PropertyFacilities",
                column: "LocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_BusinessLocations_LocationId",
                table: "Rooms",
                column: "LocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyFacilities_BusinessLocations_LocationId",
                table: "PropertyFacilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_BusinessLocations_LocationId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "BusinessLocations");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_LocationId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_PropertyFacilities_LocationId",
                table: "PropertyFacilities");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "PropertyFacilities");
        }
    }
}
