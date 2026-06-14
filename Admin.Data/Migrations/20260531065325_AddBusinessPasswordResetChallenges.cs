using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessPasswordResetChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessPasswordResetChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OtpCodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPasswordResetChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessPasswordResetChallenges_BusinessRegistrations_Busin~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPasswordResetChallenges_BusinessRegistrationId",
                table: "BusinessPasswordResetChallenges",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPasswordResetChallenges_BusinessRegistrationId_IsUs~",
                table: "BusinessPasswordResetChallenges",
                columns: new[] { "BusinessRegistrationId", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPasswordResetChallenges_ExpiresAt",
                table: "BusinessPasswordResetChallenges",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessPasswordResetChallenges");
        }
    }
}
