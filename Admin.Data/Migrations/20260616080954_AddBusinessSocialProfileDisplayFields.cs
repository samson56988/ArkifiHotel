using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessSocialProfileDisplayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacebookFollowers",
                table: "BusinessSocialProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookHandle",
                table: "BusinessSocialProfiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramFollowers",
                table: "BusinessSocialProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramHandle",
                table: "BusinessSocialProfiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TikTokFollowers",
                table: "BusinessSocialProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TikTokHandle",
                table: "BusinessSocialProfiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XFollowers",
                table: "BusinessSocialProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XHandle",
                table: "BusinessSocialProfiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookFollowers",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "FacebookHandle",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "InstagramFollowers",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "InstagramHandle",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "TikTokFollowers",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "TikTokHandle",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "XFollowers",
                table: "BusinessSocialProfiles");

            migrationBuilder.DropColumn(
                name: "XHandle",
                table: "BusinessSocialProfiles");
        }
    }
}
