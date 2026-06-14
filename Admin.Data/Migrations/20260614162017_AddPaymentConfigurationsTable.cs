using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentConfigurationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "BusinessRegistrations");

            migrationBuilder.DropColumn(
                name: "PaymentSecretProtected",
                table: "BusinessRegistrations");

            migrationBuilder.CreateTable(
                name: "PaymentConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gateway = table.Column<int>(type: "integer", nullable: false),
                    EncryptedJson = table.Column<string>(type: "character varying(8192)", maxLength: 8192, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentConfigurations_BusinessRegistrations_BusinessRegistr~",
                        column: x => x.BusinessRegistrationId,
                        principalTable: "BusinessRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentConfigurations_BusinessRegistrationId",
                table: "PaymentConfigurations",
                column: "BusinessRegistrationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentConfigurations");

            migrationBuilder.AddColumn<int>(
                name: "PaymentProvider",
                table: "BusinessRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentSecretProtected",
                table: "BusinessRegistrations",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: true);
        }
    }
}
