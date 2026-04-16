using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessLoginOtpChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessLoginOtpChallenges",
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
                    table.PrimaryKey("PK_BusinessLoginOtpChallenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessLoginOtpChallenges_BusinessRegistrations_BusinessRe~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLoginOtpChallenges_BusinessRegistrationId",
                table: "BusinessLoginOtpChallenges",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLoginOtpChallenges_BusinessRegistrationId_IsUsed",
                table: "BusinessLoginOtpChallenges",
                columns: new[] { "BusinessRegistrationId", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessLoginOtpChallenges_ExpiresAt",
                table: "BusinessLoginOtpChallenges",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessLoginOtpChallenges");
        }
    }
}
