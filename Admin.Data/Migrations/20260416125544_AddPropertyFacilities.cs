using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyFacilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFacilities_BusinessRegistrations_BusinessRegistrati~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyFacilityImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyFacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFacilityImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFacilityImages_PropertyFacilities_PropertyFacilityId",
                        column: x => x.PropertyFacilityId,
                        principalTable: "PropertyFacilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilities_BusinessRegistrationId",
                table: "PropertyFacilities",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilities_BusinessRegistrationId_Name",
                table: "PropertyFacilities",
                columns: new[] { "BusinessRegistrationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFacilityImages_PropertyFacilityId",
                table: "PropertyFacilityImages",
                column: "PropertyFacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyFacilityImages");

            migrationBuilder.DropTable(
                name: "PropertyFacilities");
        }
    }
}
