using Microsoft.EntityFrameworkCore.Migrations;

namespace E_Pharmacy.Migrations
{
    public partial class editone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Order",
                newName: "ImageName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageName",
                table: "Order",
                newName: "Image");
        }
    }
}
