using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSuppliesV3.Migrations
{
    public partial class AlterApprovalsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForApprovalId",
                table: "SSv3_GAL_APPROVALSTBL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ForApprovalId",
                table: "SSv3_GAL_APPROVALSTBL",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
