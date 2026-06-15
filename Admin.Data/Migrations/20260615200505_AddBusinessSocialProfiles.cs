using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessSocialProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessSocialProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacebookUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    InstagramUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TikTokUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    XUrl = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSocialProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessSocialProfiles_BusinessRegistrations_BusinessRegist~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSocialProfiles_BusinessRegistrationId",
                table: "BusinessSocialProfiles",
                column: "BusinessRegistrationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessSocialProfiles");
        }
    }
}
