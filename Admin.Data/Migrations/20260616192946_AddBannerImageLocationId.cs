using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerImageLocationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "StorefrontBannerImages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorefrontBannerImages_LocationId",
                table: "StorefrontBannerImages",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_StorefrontBannerImages_BusinessLocations_LocationId",
                table: "StorefrontBannerImages",
                column: "LocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorefrontBannerImages_BusinessLocations_LocationId",
                table: "StorefrontBannerImages");

            migrationBuilder.DropIndex(
                name: "IX_StorefrontBannerImages_LocationId",
                table: "StorefrontBannerImages");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "StorefrontBannerImages");
        }
    }
}
