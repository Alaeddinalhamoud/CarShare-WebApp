using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CarShareV1.Data.Migrations
{
    public partial class _2se2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Vehicles",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "WebSiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressAPI = table.Column<string>(nullable: true),
                    GoogleAPI = table.Column<string>(nullable: true),
                    PaymentAPI = table.Column<string>(nullable: true),
                    MobileApiKey = table.Column<string>(nullable: true),
                    MobileApiSecret = table.Column<string>(nullable: true),
                    MobileWebsiteName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSiteSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebSiteSettings");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Vehicles");
        }
    }
}
