using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStorefrontAboutImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorefrontAboutImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorefrontAboutImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorefrontAboutImages_BusinessRegistrations_BusinessRegistr~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontAboutImages_BusinessRegistrationId",
                table: "StorefrontAboutImages",
                column: "BusinessRegistrationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorefrontAboutImages");
        }
    }
}
