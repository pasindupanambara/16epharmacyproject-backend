using Microsoft.EntityFrameworkCore.Migrations;

namespace E_Pharmacy.Migrations
{
    public partial class editsix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pharmacy_id",
                table: "Order",
                newName: "PharmacyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PharmacyId",
                table: "Order",
                newName: "Pharmacy_id");
        }
    }
}
