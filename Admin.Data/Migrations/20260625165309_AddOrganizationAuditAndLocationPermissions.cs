using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAuditAndLocationPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultLocationId",
                table: "UserOrganizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAllLocationAccess",
                table: "UserOrganizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OrganizationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserDisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetailsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationAuditLogs_BusinessRegistrations_BusinessRegistr~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOrganizationLocationPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessLocationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganizationLocationPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOrganizationLocationPermissions_BusinessLocations_Busin~",
                        column: x => x.BusinessLocationId,
                        principalTable: "BusinessLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOrganizationLocationPermissions_UserOrganizations_UserO~",
                        column: x => x.UserOrganizationId,
                        principalTable: "UserOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_DefaultLocationId",
                table: "UserOrganizations",
                column: "DefaultLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAuditLogs_BusinessRegistrationId_CreatedAt",
                table: "OrganizationAuditLogs",
                columns: new[] { "BusinessRegistrationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAuditLogs_EntityType_EntityId",
                table: "OrganizationAuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAuditLogs_LocationId",
                table: "OrganizationAuditLogs",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAuditLogs_UserOrganizationId",
                table: "OrganizationAuditLogs",
                column: "UserOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizationLocationPermissions_BusinessLocationId",
                table: "UserOrganizationLocationPermissions",
                column: "BusinessLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizationLocationPermissions_UserOrganizationId_Busi~",
                table: "UserOrganizationLocationPermissions",
                columns: new[] { "UserOrganizationId", "BusinessLocationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserOrganizations_BusinessLocations_DefaultLocationId",
                table: "UserOrganizations",
                column: "DefaultLocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(
                """
                UPDATE "UserOrganizations"
                SET "HasAllLocationAccess" = TRUE
                WHERE "IsSuperAdmin" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOrganizations_BusinessLocations_DefaultLocationId",
                table: "UserOrganizations");

            migrationBuilder.DropTable(
                name: "OrganizationAuditLogs");

            migrationBuilder.DropTable(
                name: "UserOrganizationLocationPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserOrganizations_DefaultLocationId",
                table: "UserOrganizations");

            migrationBuilder.DropColumn(
                name: "DefaultLocationId",
                table: "UserOrganizations");

            migrationBuilder.DropColumn(
                name: "HasAllLocationAccess",
                table: "UserOrganizations");
        }
    }
}
