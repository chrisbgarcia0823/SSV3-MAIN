using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSuppliesV3.Migrations
{
    public partial class UpdatedApprovalsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsAdvanceOder",
                table: "SSv3_GAL_APPROVALSTBL",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdvanceOder",
                table: "SSv3_GAL_APPROVALSTBL");
        }
    }
}
