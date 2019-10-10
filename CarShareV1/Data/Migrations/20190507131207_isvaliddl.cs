using Microsoft.EntityFrameworkCore.Migrations;

namespace CarShareV1.Data.Migrations
{
    public partial class isvaliddl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "DrivingLicences",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "DrivingLicences");
        }
    }
}
