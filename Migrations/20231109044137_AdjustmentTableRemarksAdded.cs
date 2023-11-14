using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSuppliesV3.Migrations
{
    public partial class AdjustmentTableRemarksAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "SSv3_GAL_ADJUSTMENTTBL",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SSv3_GAL_ADJUSTMENTTBL");
        }
    }
}
