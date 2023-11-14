using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSuppliesV3.Migrations
{
    public partial class ApprovalsTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SSv3_GAL_APPROVALSTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForApprovalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestorUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApproverUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNumbers = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_APPROVALSTBL", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SSv3_GAL_APPROVALSTBL");
        }
    }
}
