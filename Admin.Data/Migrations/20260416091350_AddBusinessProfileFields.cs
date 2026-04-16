using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "BusinessRegistrations");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "BusinessRegistrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "BusinessRegistrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "BusinessRegistrations",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "BusinessRegistrations");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "BusinessRegistrations");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "BusinessRegistrations");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "BusinessRegistrations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
