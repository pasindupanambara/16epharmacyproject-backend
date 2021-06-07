using Microsoft.EntityFrameworkCore.Migrations;

namespace E_Pharmacy.Migrations
{
    public partial class editfive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Customer_id",
                table: "Order",
                newName: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Order",
                newName: "Customer_id");
        }
    }
}
