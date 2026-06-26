using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationStaffAndModulePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserOrganizations_Email",
                table: "UserOrganizations");

            migrationBuilder.AddColumn<bool>(
                name: "HasAllModuleAccess",
                table: "UserOrganizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultPassword",
                table: "UserOrganizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "UserOrganizations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserOrganizationModulePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganizationModulePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOrganizationModulePermissions_UserOrganizations_UserOrg~",
                        column: x => x.UserOrganizationId,
                        principalTable: "UserOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_BusinessRegistrationId_Username",
                table: "UserOrganizations",
                columns: new[] { "BusinessRegistrationId", "Username" },
                unique: true,
                filter: "\"Username\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizationModulePermissions_UserOrganizationId_Module~",
                table: "UserOrganizationModulePermissions",
                columns: new[] { "UserOrganizationId", "ModuleCode" },
                unique: true);

            migrationBuilder.Sql(
                """
                UPDATE "UserOrganizations"
                SET "HasAllModuleAccess" = TRUE
                WHERE "IsSuperAdmin" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOrganizationModulePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserOrganizations_BusinessRegistrationId_Username",
                table: "UserOrganizations");

            migrationBuilder.DropColumn(
                name: "HasAllModuleAccess",
                table: "UserOrganizations");

            migrationBuilder.DropColumn(
                name: "IsDefaultPassword",
                table: "UserOrganizations");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "UserOrganizations");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_Email",
                table: "UserOrganizations",
                column: "Email",
                unique: true);
        }
    }
}
