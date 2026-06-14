using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class BookingConfirmationCodeUniquePerBusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_ConfirmationCode",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BusinessRegistrationId_ConfirmationCode",
                table: "Bookings",
                columns: new[] { "BusinessRegistrationId", "ConfirmationCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_BusinessRegistrationId_ConfirmationCode",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ConfirmationCode",
                table: "Bookings",
                column: "ConfirmationCode",
                unique: true);
        }
    }
}
