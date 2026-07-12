using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moneta.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Siret = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    VatNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    address_line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    address_line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    address_postal_code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    address_city = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    address_country = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "seller_profile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LegalName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Siret = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    VatNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    address_line1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    address_line2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    address_postal_code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    address_city = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    address_country = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Iban = table.Column<string>(type: "TEXT", maxLength: 34, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seller_profile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitPriceExclVat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    vat_percentage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_lines_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_lines_InvoiceId",
                table: "invoice_lines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_Number",
                table: "invoices",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "invoice_lines");

            migrationBuilder.DropTable(
                name: "seller_profile");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "invoices");
        }
    }
}
