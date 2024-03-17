using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceAppApi.Migrations
{
    /// <inheritdoc />
    public partial class addInvoiceTrackerToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrontendId",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrontendId",
                table: "Invoices");
        }
    }
}
