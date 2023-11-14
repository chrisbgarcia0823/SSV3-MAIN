using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopSuppliesV3.Migrations
{
    public partial class InitialCommit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SSv3_GAL_ACCESSTYPETBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_ACCESSTYPETBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_ADJUSTMENTTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    Adjustment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_ADJUSTMENTTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_AREATBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    AreaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_AREATBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_BUTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    BUname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_BUTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_INCOMING_HISTORYTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncomingId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_INCOMING_HISTORYTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_INCOMMINGTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IncommingQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalOutgoingQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TotalOnHoldQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    addedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_INCOMMINGTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_INVENTORYTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncommingItemId = table.Column<int>(type: "int", nullable: false),
                    IncommingTotalQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalOnHoldQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InRequestTotalQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OutgoingTotalQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Adjustment = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RemainingStocksToRequest = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RemainingStocksOnhand = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_INVENTORYTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_ITEMTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BUName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedByUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<int>(type: "int", nullable: true),
                    SafetyStock = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BuyerName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_ITEMTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_OUTGOINGTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    QuantityIssued = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IncommingIds = table.Column<int>(type: "int", nullable: true),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateIssued = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuerUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_OUTGOINGTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_SUPPLIERTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isActive = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_SUPPLIERTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_UMTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_UMTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_GAL_USERTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAdd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BUname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    addedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isApprover = table.Column<bool>(type: "bit", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_GAL_USERTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_ITEM_CATEGORYTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedByUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteByUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEdited = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_ITEM_CATEGORYTBL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SSv3_ITEM_REQUESTTBL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<int>(type: "int", nullable: false),
                    AreaName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BUname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestorUserId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    QuantityRequested = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateRequested = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateApproved = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApproverUserId = table.Column<int>(type: "int", nullable: true),
                    isAdvance = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSv3_ITEM_REQUESTTBL", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SSv3_GAL_ACCESSTYPETBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_ADJUSTMENTTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_AREATBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_BUTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_INCOMING_HISTORYTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_INCOMMINGTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_INVENTORYTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_ITEMTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_OUTGOINGTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_SUPPLIERTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_UMTBL");

            migrationBuilder.DropTable(
                name: "SSv3_GAL_USERTBL");

            migrationBuilder.DropTable(
                name: "SSv3_ITEM_CATEGORYTBL");

            migrationBuilder.DropTable(
                name: "SSv3_ITEM_REQUESTTBL");
        }
    }
}
