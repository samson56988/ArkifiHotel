using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomWeeklyRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePricePerWeek",
                table: "Rooms",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePricePerWeek",
                table: "Rooms");
        }
    }
}
