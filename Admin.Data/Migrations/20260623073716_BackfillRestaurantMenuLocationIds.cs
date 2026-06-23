using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillRestaurantMenuLocationIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "RestaurantMenuSettings" s
                SET "LocationId" = (
                    SELECT l."Id"
                    FROM "BusinessLocations" l
                    WHERE l."BusinessRegistrationId" = s."BusinessRegistrationId"
                    ORDER BY l."Name"
                    LIMIT 1
                )
                WHERE s."LocationId" = '00000000-0000-0000-0000-000000000000'
                   OR NOT EXISTS (
                       SELECT 1
                       FROM "BusinessLocations" l
                       WHERE l."Id" = s."LocationId"
                         AND l."BusinessRegistrationId" = s."BusinessRegistrationId"
                   );
                """);

            migrationBuilder.Sql("""
                UPDATE "RestaurantMenuCategories" c
                SET "LocationId" = (
                    SELECT l."Id"
                    FROM "BusinessLocations" l
                    WHERE l."BusinessRegistrationId" = c."BusinessRegistrationId"
                    ORDER BY l."Name"
                    LIMIT 1
                )
                WHERE c."LocationId" = '00000000-0000-0000-0000-000000000000'
                   OR NOT EXISTS (
                       SELECT 1
                       FROM "BusinessLocations" l
                       WHERE l."Id" = c."LocationId"
                         AND l."BusinessRegistrationId" = c."BusinessRegistrationId"
                   );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
