using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceAppApi.Migrations
{
    /// <inheritdoc />
    public partial class changeNewIdtoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Rename the NewId column to Id
            migrationBuilder.RenameColumn(
                name: "NewId",
                table: "RecurringInvoices",
                newName: "Id");

            // Step 2: Update the primary key to use the new Id column
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Rename the Id column back to NewId
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RecurringInvoices",
                newName: "NewId");

            // Step 2: Update the primary key to use the NewId column
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices",
                column: "NewId");
        }
    }
}
