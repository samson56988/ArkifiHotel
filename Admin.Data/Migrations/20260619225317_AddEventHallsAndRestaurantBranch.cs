using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventHallsAndRestaurantBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantMenuSettings_BusinessRegistrationId",
                table: "RestaurantMenuSettings");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantMenuCategories_BusinessRegistrationId_Section_Name",
                table: "RestaurantMenuCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "RestaurantMenuSettings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "RestaurantMenuCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "RestaurantMenuSettings" s
                SET "LocationId" = (
                    SELECT l."Id"
                    FROM "BusinessLocations" l
                    WHERE l."BusinessRegistrationId" = s."BusinessRegistrationId"
                    ORDER BY l."Name"
                    LIMIT 1
                )
                WHERE s."LocationId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE "RestaurantMenuCategories" c
                SET "LocationId" = (
                    SELECT l."Id"
                    FROM "BusinessLocations" l
                    WHERE l."BusinessRegistrationId" = c."BusinessRegistrationId"
                    ORDER BY l."Name"
                    LIMIT 1
                )
                WHERE c."LocationId" IS NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM "RestaurantMenuCategories"
                WHERE "LocationId" IS NULL;
                """);

            migrationBuilder.Sql("""
                DELETE FROM "RestaurantMenuSettings"
                WHERE "LocationId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "RestaurantMenuSettings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LocationId",
                table: "RestaurantMenuCategories",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EventHalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RentalPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHalls_BusinessLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "BusinessLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventHalls_BusinessRegistrations_BusinessRegistrationId",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventHallImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventHallId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHallImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHallImages_EventHalls_EventHallId",
                        column: x => x.EventHallId,
                        principalTable: "EventHalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventHallRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventHallId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GuestEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    GuestPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EventEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHallRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventHallRequests_BusinessLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "BusinessLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventHallRequests_BusinessRegistrations_BusinessRegistratio~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventHallRequests_EventHalls_EventHallId",
                        column: x => x.EventHallId,
                        principalTable: "EventHalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuSettings_BusinessRegistrationId_LocationId",
                table: "RestaurantMenuSettings",
                columns: new[] { "BusinessRegistrationId", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuSettings_LocationId",
                table: "RestaurantMenuSettings",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuCategories_BusinessRegistrationId_LocationId_~",
                table: "RestaurantMenuCategories",
                columns: new[] { "BusinessRegistrationId", "LocationId", "Section", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuCategories_LocationId",
                table: "RestaurantMenuCategories",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHallImages_EventHallId",
                table: "EventHallImages",
                column: "EventHallId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHallRequests_BusinessRegistrationId",
                table: "EventHallRequests",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHallRequests_CreatedAt",
                table: "EventHallRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EventHallRequests_EventHallId",
                table: "EventHallRequests",
                column: "EventHallId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHallRequests_LocationId",
                table: "EventHallRequests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHalls_BusinessRegistrationId",
                table: "EventHalls",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_EventHalls_BusinessRegistrationId_LocationId_Name",
                table: "EventHalls",
                columns: new[] { "BusinessRegistrationId", "LocationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_EventHalls_LocationId",
                table: "EventHalls",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantMenuCategories_BusinessLocations_LocationId",
                table: "RestaurantMenuCategories",
                column: "LocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantMenuSettings_BusinessLocations_LocationId",
                table: "RestaurantMenuSettings",
                column: "LocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantMenuCategories_BusinessLocations_LocationId",
                table: "RestaurantMenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantMenuSettings_BusinessLocations_LocationId",
                table: "RestaurantMenuSettings");

            migrationBuilder.DropTable(
                name: "EventHallImages");

            migrationBuilder.DropTable(
                name: "EventHallRequests");

            migrationBuilder.DropTable(
                name: "EventHalls");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantMenuSettings_BusinessRegistrationId_LocationId",
                table: "RestaurantMenuSettings");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantMenuSettings_LocationId",
                table: "RestaurantMenuSettings");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantMenuCategories_BusinessRegistrationId_LocationId_~",
                table: "RestaurantMenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantMenuCategories_LocationId",
                table: "RestaurantMenuCategories");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "RestaurantMenuSettings");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "RestaurantMenuCategories");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuSettings_BusinessRegistrationId",
                table: "RestaurantMenuSettings",
                column: "BusinessRegistrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuCategories_BusinessRegistrationId_Section_Name",
                table: "RestaurantMenuCategories",
                columns: new[] { "BusinessRegistrationId", "Section", "Name" });
        }
    }
}
