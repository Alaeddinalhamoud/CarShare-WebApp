using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CarShareV1.Data.Migrations
{
    public partial class orderstb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoogleAPI",
                table: "WebSiteSettings",
                newName: "DVLAAPI");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateTime",
                table: "VehicleAvailabilities",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "VehicleAvailabilities",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "VehicleAvailabilities",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "ReturnLocationLatitue",
                table: "Reservations",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "PickUpTime",
                table: "Reservations",
                nullable: true,
                oldClrType: typeof(TimeSpan));

            migrationBuilder.AlterColumn<string>(
                name: "DropOffTime",
                table: "Reservations",
                nullable: true,
                oldClrType: typeof(TimeSpan));

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    PickUpDate = table.Column<DateTime>(nullable: false),
                    PickUpTime = table.Column<string>(nullable: true),
                    DropOffDate = table.Column<DateTime>(nullable: false),
                    DropOffTime = table.Column<string>(nullable: true),
                    TotalAmount = table.Column<double>(nullable: false),
                    IsPaid = table.Column<bool>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    PaymentMethod = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropColumn(
                name: "CreateTime",
                table: "VehicleAvailabilities");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "VehicleAvailabilities");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "VehicleAvailabilities");

            migrationBuilder.RenameColumn(
                name: "DVLAAPI",
                table: "WebSiteSettings",
                newName: "GoogleAPI");

            migrationBuilder.AlterColumn<int>(
                name: "ReturnLocationLatitue",
                table: "Reservations",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "PickUpTime",
                table: "Reservations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DropOffTime",
                table: "Reservations",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
