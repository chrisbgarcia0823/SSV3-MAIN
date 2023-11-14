using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Models;

namespace ShopSuppliesV3.Data
{
    public class ShopSuppliesV3Context : DbContext
    {
        public ShopSuppliesV3Context (DbContextOptions<ShopSuppliesV3Context> options)
            : base(options)
        {
        }

        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_AREATBL> SSv3_GAL_AREATBL { get; set; } = default!;
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_USERTBL> SSv3_GAL_USERTBL { get; set; } = default!;
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_ITEMTBL>? SSv3_GAL_ITEMTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_SUPPLIERTBL>? SSv3_GAL_SUPPLIERTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_ITEM_CATEGORYTBL>? SSv3_ITEM_CATEGORYTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_BUTBL>? SSv3_GAL_BUTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_UMTBL>? SSv3_GAL_UMTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_INCOMMINGTBL>? SSv3_GAL_INCOMMINGTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_ACCESSTYPETBL>? SSv3_GAL_ACCESSTYPETBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_INVENTORYTBL>? SSv3_GAL_INVENTORYTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_ITEM_REQUESTTBL>? SSv3_ITEM_REQUESTTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_OUTGOINGTBL>? SSv3_GAL_OUTGOINGTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_ADJUSTMENTTBL>? SSv3_GAL_ADJUSTMENTTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_INCOMING_HISTORYTBL>? SSv3_GAL_INCOMING_HISTORYTBL { get; set; }
        public DbSet<ShopSuppliesV3.Models.SSv3_GAL_APPROVALSTBL>? SSv3_GAL_APPROVALSTBL { get; set; } //For the approvals of the request.
    }
}
