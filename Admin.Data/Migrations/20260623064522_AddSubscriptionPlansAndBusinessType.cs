using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlansAndBusinessType : Migration
    {
        private static readonly Guid FreePlanId = new("fa000002-0000-4000-8000-000000000001");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    BillingInterval = table.Column<int>(type: "integer", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false, defaultValue: "NGN"),
                    YearlyDiscountPercent = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "BillingInterval", "Code", "CreatedAt", "Currency", "Description", "IsActive", "Name", "PriceAmount", "SortOrder", "Tier", "YearlyDiscountPercent" },
                values: new object[,]
                {
                    { FreePlanId, 0, "free", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NGN", "Try ArkifiStay with a 30-day trial. Perfect for getting started.", true, "Free", 0m, 0, 0, null },
                    { new Guid("fa000002-0000-4000-8000-000000000002"), 1, "pro-monthly", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NGN", "Full platform access with storefront, bookings, restaurant, and payments.", true, "Pro", 20000m, 1, 1, null },
                    { new Guid("fa000002-0000-4000-8000-000000000003"), 2, "pro-yearly", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NGN", "Pro plan billed yearly — save 20% compared to paying monthly.", true, "Pro (Yearly)", 192000m, 2, 1, 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Code",
                table: "SubscriptionPlans",
                column: "Code",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessType",
                table: "BusinessRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubscriptionExpiresAt",
                table: "BusinessRegistrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "BusinessRegistrations",
                type: "uuid",
                nullable: false,
                defaultValue: FreePlanId);

            migrationBuilder.Sql(
                """
                UPDATE "BusinessRegistrations"
                SET "SubscriptionPlanId" = 'fa000002-0000-4000-8000-000000000001',
                    "SubscriptionExpiresAt" = NOW() AT TIME ZONE 'UTC' + INTERVAL '30 days'
                WHERE "SubscriptionExpiresAt" IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessRegistrations_SubscriptionExpiresAt",
                table: "BusinessRegistrations",
                column: "SubscriptionExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessRegistrations_SubscriptionPlanId",
                table: "BusinessRegistrations",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessRegistrations_SubscriptionPlans_SubscriptionPlanId",
                table: "BusinessRegistrations",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessRegistrations_SubscriptionPlans_SubscriptionPlanId",
                table: "BusinessRegistrations");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_BusinessRegistrations_SubscriptionExpiresAt",
                table: "BusinessRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_BusinessRegistrations_SubscriptionPlanId",
                table: "BusinessRegistrations");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "BusinessRegistrations");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiresAt",
                table: "BusinessRegistrations");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "BusinessRegistrations");
        }
    }
}
