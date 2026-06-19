using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantMenuCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Section = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantMenuCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantMenuCategories_BusinessRegistrations_BusinessRegi~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantMenuSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NavLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    HeroEyebrow = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    HeroTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HeroSubtitle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MealsSectionTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DrinksSectionTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    HeroImageRelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HeroImageOriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantMenuSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantMenuSettings_BusinessRegistrations_BusinessRegist~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantMenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TagsJson = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImageRelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageOriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantMenuItems_RestaurantMenuCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "RestaurantMenuCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuCategories_BusinessRegistrationId",
                table: "RestaurantMenuCategories",
                column: "BusinessRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuCategories_BusinessRegistrationId_Section_Name",
                table: "RestaurantMenuCategories",
                columns: new[] { "BusinessRegistrationId", "Section", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuItems_CategoryId",
                table: "RestaurantMenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantMenuSettings_BusinessRegistrationId",
                table: "RestaurantMenuSettings",
                column: "BusinessRegistrationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantMenuItems");

            migrationBuilder.DropTable(
                name: "RestaurantMenuSettings");

            migrationBuilder.DropTable(
                name: "RestaurantMenuCategories");
        }
    }
}
