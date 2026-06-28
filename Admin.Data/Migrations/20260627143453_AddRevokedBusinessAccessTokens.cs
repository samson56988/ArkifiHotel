using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedBusinessAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevokedBusinessAccessTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Jti = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevokedBusinessAccessTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevokedBusinessAccessTokens_BusinessRegistrations_BusinessR~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevokedBusinessAccessTokens_BusinessRegistrationId",
                table: "RevokedBusinessAccessTokens",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedBusinessAccessTokens_ExpiresAtUtc",
                table: "RevokedBusinessAccessTokens",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedBusinessAccessTokens_Jti",
                table: "RevokedBusinessAccessTokens",
                column: "Jti",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevokedBusinessAccessTokens");
        }
    }
}
