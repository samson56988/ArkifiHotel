using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFreeTrialTo15Days : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: new Guid("fa000002-0000-4000-8000-000000000001"),
                column: "Description",
                value: "Try ArkifiStay with a 15-day free trial. Perfect for getting started.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: new Guid("fa000002-0000-4000-8000-000000000001"),
                column: "Description",
                value: "Try ArkifiStay with a 30-day trial. Perfect for getting started.");
        }
    }
}
