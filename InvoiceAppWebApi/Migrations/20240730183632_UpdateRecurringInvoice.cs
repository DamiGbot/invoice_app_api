using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceAppApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecurringInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a new column with the desired type
            migrationBuilder.AddColumn<string>(
                name: "NewId",
                table: "RecurringInvoices",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            // Step 2: Set NewId as the primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices",
                column: "NewId");

            // Step 3: Recreate unique index on InvoiceId and RecurrenceDate
            migrationBuilder.CreateIndex(
                name: "IX_RecurringInvoices_InvoiceId_RecurrenceDate",
                table: "RecurringInvoices",
                columns: new[] { "InvoiceId", "RecurrenceDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop the new primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices");

            // Step 2: Drop the new column
            migrationBuilder.DropColumn(
                name: "NewId",
                table: "RecurringInvoices");

            // Step 3: Recreate the old Id column with the original type
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RecurringInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Step 4: Set Id as the primary key again
            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringInvoices",
                table: "RecurringInvoices",
                column: "Id");

            // Step 5: Recreate unique index on InvoiceId
            migrationBuilder.CreateIndex(
                name: "IX_RecurringInvoices_InvoiceId",
                table: "RecurringInvoices",
                column: "InvoiceId");
        }
    }
}
