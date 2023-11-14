using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSuppliesV3.Migrations
{
    public partial class IncomingTableModify : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "SSv3_GAL_INCOMMINGTBL",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEdited",
                table: "SSv3_GAL_INCOMMINGTBL",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteByUserName",
                table: "SSv3_GAL_INCOMMINGTBL",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditByUserName",
                table: "SSv3_GAL_INCOMMINGTBL",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDeleted",
                table: "SSv3_GAL_INCOMMINGTBL");

            migrationBuilder.DropColumn(
                name: "DateEdited",
                table: "SSv3_GAL_INCOMMINGTBL");

            migrationBuilder.DropColumn(
                name: "DeleteByUserName",
                table: "SSv3_GAL_INCOMMINGTBL");

            migrationBuilder.DropColumn(
                name: "EditByUserName",
                table: "SSv3_GAL_INCOMMINGTBL");
        }
    }
}
