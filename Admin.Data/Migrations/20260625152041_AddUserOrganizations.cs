using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserOrganizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    HashedPassword = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOrganizations_BusinessRegistrations_BusinessRegistratio~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_BusinessRegistrationId_Email",
                table: "UserOrganizations",
                columns: new[] { "BusinessRegistrationId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_BusinessRegistrationId_SuperAdmin",
                table: "UserOrganizations",
                column: "BusinessRegistrationId",
                unique: true,
                filter: "\"IsSuperAdmin\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_Email",
                table: "UserOrganizations",
                column: "Email",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "UserOrganizations" (
                    "Id",
                    "BusinessRegistrationId",
                    "FirstName",
                    "LastName",
                    "Email",
                    "HashedPassword",
                    "IsSuperAdmin",
                    "IsEmailVerified",
                    "IsActive",
                    "CreatedAt",
                    "UpdatedAt")
                SELECT
                    gen_random_uuid(),
                    br."Id",
                    br."FirstName",
                    br."LastName",
                    LOWER(br."ContactEmail"),
                    br."HashedPassword",
                    TRUE,
                    br."IsEmailVerified",
                    TRUE,
                    br."CreatedAt",
                    br."UpdatedAt"
                FROM "BusinessRegistrations" br
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "UserOrganizations" u
                    WHERE u."BusinessRegistrationId" = br."Id"
                      AND u."IsSuperAdmin" = TRUE);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOrganizations");
        }
    }
}
