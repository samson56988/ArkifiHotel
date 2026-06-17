using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateGuestBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PendingGuestStayUnique",
                table: "Bookings",
                columns: new[] { "BusinessRegistrationId", "RoomId", "GuestEmail", "CheckInDate", "CheckOutDate" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_BusinessExternalReference",
                table: "BookingPayments",
                columns: new[] { "BusinessRegistrationId", "ExternalReference" },
                unique: true,
                filter: "\"ExternalReference\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_PendingGuestStayUnique",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_BookingPayments_BusinessExternalReference",
                table: "BookingPayments");
        }
    }
}
